using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelMacroEngine.Core.Input;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// Сервис управления мышью: человекоподобные клики, скролл и биомеханическое перемещение курсора
/// через Arduino RawHID. Использует TrajectoryPlanner (Фиттс, minimum jerk, тремор, овершут)
/// и PreciseTimer для стабилизации системного кванта таймера Windows.
/// </summary>
public static class MouseService
{
    private static readonly Random _random = new();

    // Состояние для авто-распознавания двойных кликов
    private static long _lastClickTimestamp = 0;
    private static byte _lastClickButton = 0;
    private static readonly object _stateLock = new();

    static MouseService()
    {
        PreciseTimer.Initialize();
    }

    /// <summary>
    /// Человекоподобный клик мыши с авто-детекцией двойного клика и естественными задержками удержания.
    /// </summary>
    public static async Task ClickButtonAsync(byte button, bool pressDown, bool pressUp, string comboName, CancellationToken ct)
    {
        // Проверяем паттерн Double Click: если та же кнопка нажата быстрее чем за 380 мс
        bool isDoubleClick = false;
        lock (_stateLock)
        {
            long now = TimeService.GetTimestamp();
            if (_lastClickButton == button && _lastClickTimestamp != 0 &&
                TimeService.GetElapsedMilliseconds(_lastClickTimestamp, now) < 380)
            {
                isDoubleClick = true;
            }
            _lastClickTimestamp = now;
            _lastClickButton = button;
        }

        // Если это второй клик двойного нажатия — естественная межнажатийная пауза (80-160 мс)
        if (isDoubleClick && pressDown)
        {
            int interClickDelay = _random.Next(85, 155);
            await Task.Delay(interClickDelay, ct).ConfigureAwait(false);
        }

        if (pressDown)
        {
            var batch = new PacketBatch();
            batch.AddMouseButton(button, isDown: true);
            ArduinoHidService.Send(batch);
            ActionLogger.LogMouse($"Button {button} DOWN {(isDoubleClick ? "(Double-Click Part 2)" : "")}", comboName: comboName);
        }

        // Фаза удержания кнопки (нажатие пальцем: 60-110 мс)
        if (pressDown && pressUp)
        {
            int holdTime = _random.Next(55, 105);
            await Task.Delay(holdTime, ct).ConfigureAwait(false);
        }

        if (pressUp)
        {
            var batch = new PacketBatch();
            batch.AddMouseButton(button, isDown: false);
            ArduinoHidService.Send(batch);
            ActionLogger.LogMouse($"Button {button} UP", comboName: comboName);
        }

        // Пост-задержка после одиночного действия
        if (!isDoubleClick && ((pressDown && pressUp) || (!pressDown && pressUp)))
        {
            int flightTime = _random.Next(35, 75);
            await Task.Delay(flightTime, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Прокрутка колесика с реалистичной биомеханикой пальца: 
    /// преодоление трещотки колеса -> пик скорости -> нелинейное инерционное затухание.
    /// </summary>
    public static async Task ScrollAsync(int notches, bool scrollUp, string comboName, CancellationToken ct)
    {
        int count = Math.Clamp(notches, 1, 20);
        sbyte direction = scrollUp ? (sbyte)1 : (sbyte)-1;

        for (int i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var batch = new PacketBatch();
            batch.AddMouseMove(0, 0, direction);
            ArduinoHidService.Send(batch);

            // Нелинейный расчет интервала для текущего щелчка колесика
            int stepDelay = CalculateScrollStepDelay(i, count);
            await Task.Delay(stepDelay, ct).ConfigureAwait(false);
        }

        ActionLogger.LogMouse($"Scroll {(scrollUp ? "UP" : "DOWN")} x{count}", comboName: comboName);
    }

    /// <summary>
    /// Рассчитывает межшаговый интервал скролла по кривой кинетической энергии пальца.
    /// </summary>
    private static int CalculateScrollStepDelay(int stepIndex, int totalSteps)
    {
        if (totalSteps <= 1)
            return _random.Next(40, 75);

        // Нормализованный прогресс прокрутки (0.0 .. 1.0)
        double progress = (double)stepIndex / (totalSteps - 1);

        double baseDelay;
        if (progress < 0.28)
        {
            // ФАЗА 1: Срыв колеса с места и разгон фаланги пальца (с 38 мс вниз до 20 мс)
            double t = progress / 0.28;
            baseDelay = 38.0 - (18.0 * t);
        }
        else
        {
            // ФАЗА 2: Инерционное торможение пальца (с 20 мс вверх до 85 мс по степенной параболе)
            double t = (progress - 0.28) / 0.72;
            baseDelay = 20.0 + (65.0 * Math.Pow(t, 1.7));
        }

        // Физическая неравномерность зубьев колесика (+- 3-4 мс)
        int detentJitter = _random.Next(-4, 5);
        return Math.Clamp((int)Math.Round(baseDelay + detentJitter), 16, 110);
    }

    /// <summary>
    /// Биомеханическое наведение: спокойные короткие/средние дистанции, 
    /// передача управления доводчику в радиусе 50-75 px с паузой 60-80 мс, 
    /// и идеальное успокоение в ноль пикселей на финише (без метаний).
    /// </summary>
    public static async Task MoveToAsync(int targetX, int targetY, int speedFactor, string comboName, CancellationToken ct)
    {
        Point start = Cursor.Position;
        if (!ArduinoHidService.IsConnected) return;

        double totalDx = targetX - start.X;
        double totalDy = targetY - start.Y;
        double distance = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);
        if (distance < 2.0) return;

        // =========================================================================
        // РЕЖИМ 1: КОРОТКАЯ ДИСТАНЦИЯ (<= 140 px) - Спокойная, плавная подводка
        // Уверенный старт + выраженное глубокое замедление без перелетов и метаний
        // =========================================================================
        if (distance <= 140.0)
        {
            ActionLogger.LogMouse($"Точная подводка к [{targetX}, {targetY}] (D={Math.Round(distance)}px)", targetX, targetY);

            // Естественная задержка реакции (40-75 мс)
            await Task.Delay(_random.Next(40, 75), ct).ConfigureAwait(false);

            // Увеличенное время для по-настоящему спокойного, медленного движения (340-580 мс)
            double shortDuration = 320.0 + Math.Sqrt(distance) * 28.0;
            await ExecuteShortPrecisionApproachAsync(start, new Point(targetX, targetY), shortDuration, ct).ConfigureAwait(false);

            await Task.Delay(_random.Next(30, 60), ct).ConfigureAwait(false);
            Point pEnd = Cursor.Position;
            ActionLogger.LogMouse($"Завершено в [{pEnd.X}, {pEnd.Y}]", pEnd.X, pEnd.Y);
            return;
        }

        // =========================================================================
        // РЕЖИМ 2: СРЕДНЯЯ И ДЛИННАЯ ДИСТАНЦИЯ (> 140 px)
        // Средняя (140-350 px) - умеренная скорость; Длинная (>350 px) - взрывной бросок
        // =========================================================================
        bool isLongDistance = distance > 350.0;

        // Авто-скорость: на средней дистанции скорость умеренная (45-65), на дальней высокая (75-95)
        double autoSpeed = isLongDistance 
            ? Math.Clamp(35.0 + Math.Sqrt(distance) * 2.2, 70.0, 95.0)
            : Math.Clamp(25.0 + Math.Sqrt(distance) * 1.8, 42.0, 65.0);

        ActionLogger.LogMouse($"Старт броска к [{targetX}, {targetY}] (D={Math.Round(distance)}px, авто-скорость={Math.Round(autoSpeed)})", targetX, targetY);

        await Task.Delay(_random.Next(30, 60), ct).ConfigureAwait(false);

        // 1. БАЛЛИСТИЧЕСКИЙ БРОСОК:
        // На средней дистанции разброс минимален (20-45 px), на дальней - широкий (90-240 px)
        double scatterRatio = isLongDistance ? (_random.NextDouble() * 0.15 + 0.12) : 0.10;
        double scatterRadius = Math.Clamp(distance * scatterRatio, 20.0, 240.0);

        double dirAngle = Math.Atan2(totalDy, totalDx);
        double scatterAngle = dirAngle + (_random.NextDouble() * 1.4 - 0.7);

        PointF coarseTarget = new(
            (float)(targetX + Math.Cos(scatterAngle) * scatterRadius),
            (float)(targetY + Math.Sin(scatterAngle) * scatterRadius)
        );

        double ballisticDuration = Math.Clamp(Math.Sqrt(distance) * (isLongDistance ? 9.5 : 12.0) * (100.0 / autoSpeed), 120.0, 440.0);
        await ExecuteCurvedFlightAsync(start, coarseTarget, ballisticDuration, ct).ConfigureAwait(false);

        // 2. СЕРИЯ 2D-РЫВКОВ:
        // Стягиваем курсор в радиус 50-70 px от цели, после чего отдаем управление доводчику
        int maxFlicks = isLongDistance ? _random.Next(2, 4) : 1;
        double lastAngle = scatterAngle;

        for (int step = 1; step <= maxFlicks; step++)
        {
            ct.ThrowIfCancellationRequested();

            Point current = Cursor.Position;
            double remDx = targetX - current.X;
            double remDy = targetY - current.Y;
            double remDist = Math.Sqrt(remDx * remDx + remDy * remDy);

            // РАННЯЯ ПЕРЕДАЧА: если подошли в радиус 50-70 px — выходим к медленному доводчику!
            if (remDist <= 65.0) break;

            PointF flickTarget;
            if (step < maxFlicks && remDist > 20.0)
            {
                double flickRadius = Math.Clamp(remDist * 0.45, 20.0, 65.0);
                lastAngle += Math.PI + (_random.NextDouble() * 1.6 - 0.8);

                flickTarget = new PointF(
                    (float)(targetX + Math.Cos(lastAngle) * flickRadius),
                    (float)(targetY + Math.Sin(lastAngle) * flickRadius)
                );
            }
            else
            {
                flickTarget = new PointF(targetX, targetY);
            }

            double flickDuration = Math.Clamp(remDist * 1.1 + _random.Next(35, 55), 45.0, 85.0);
            await ExecuteSnappyFlickAsync(current, flickTarget, flickDuration, ct).ConfigureAwait(false);

            await Task.Delay(_random.Next(15, 30), ct).ConfigureAwait(false);
        }

        // =========================================================================
        // 3. КОГНИТИВНАЯ МИКРОЗАДЕРЖКА (60-80 мс)
        // Пауза фиксации внимания перед началом плавного дотягивания
        // =========================================================================
        await Task.Delay(_random.Next(60, 85), ct).ConfigureAwait(false);

        // =========================================================================
        // 4. МЕДЛЕННЫЙ ФИНАЛЬНЫЙ ДОВОДЧИК (Глубокое затухание прямо в точку)
        // =========================================================================
        Point beforeDock = Cursor.Position;
        double dockDx = targetX - beforeDock.X;
        double dockDy = targetY - beforeDock.Y;
        double dockDist = Math.Sqrt(dockDx * dockDx + dockDy * dockDy);

        if (dockDist > 1.5)
        {
            // Растянутое время для мягкого неспешного входа (320-520 мс)
            double dockDuration = 300.0 + Math.Sqrt(dockDist) * 26.0;
            await ExecuteShortPrecisionApproachAsync(beforeDock, new Point(targetX, targetY), dockDuration, ct).ConfigureAwait(false);
        }

        await Task.Delay(_random.Next(30, 55), ct).ConfigureAwait(false);
        Point finalPos = Cursor.Position;
        ActionLogger.LogMouse($"Завершено в [{finalPos.X}, {finalPos.Y}]", finalPos.X, finalPos.Y);
    }

    /// <summary>
    /// Деликатная доводка: плавное затухание скорости и монолитная фиксация в цели 
    /// без автоколебаний, паразитного джиттера и круговых метаний.
    /// </summary>
    private static async Task ExecuteShortPrecisionApproachAsync(Point start, Point target, double durationMs, CancellationToken ct)
    {
        double totalDx = target.X - start.X;
        double totalDy = target.Y - start.Y;
        double initialDist = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);
        if (initialDist < 1.0) return;

        double normX = -totalDy / initialDist;
        double normY = totalDx / initialDist;

        // Едва заметная мягкая дуга (не более 0.8 px)
        double lateralCurve = (_random.NextDouble() * 0.6 + 0.3) * (_random.Next(2) == 0 ? 1 : -1);

        double remainderX = 0.0;
        double remainderY = 0.0;

        long startTs = TimeService.GetTimestamp();

        while (!ct.IsCancellationRequested)
        {
            double elapsed = TimeService.GetElapsedMilliseconds(startTs);
            double t = Math.Clamp(elapsed / durationMs, 0.0, 1.0);

            // Замеряем физическое расстояние до цели на экране
            Point current = Cursor.Position;
            double realDx = target.X - current.X;
            double realDy = target.Y - current.Y;
            double realDist = Math.Sqrt(realDx * realDx + realDy * realDy);

            // СТОП: Если курсор уже сел в цель (погрешность <= 1.5 px) во второй половине времени —
            // МГНОВЕННО ВЫХОДИМ! Никаких лишних кругов и болтанки вокруг точки.
            if (realDist <= 1.5 && t > 0.45)
                break;

            // Глубокое нелинейное замедление (Ease-Out)
            double progress = 1.0 - Math.Pow(1.0 - t, 2.9);

            // Плавное затухание дуги
            double curveOffset = lateralCurve * Math.Sin(Math.PI * t);

            double idealX = start.X + (totalDx * progress) + (normX * curveOffset);
            double idealY = start.Y + (totalDy * progress) + (normY * curveOffset);

            // ТРЕМОР: Мягко затухает и полностью обнуляется уже к t >= 0.55
            // В финальной фазе посадки рука стабилизируется, колебания равны строго 0.0
            double tremorFade = Math.Clamp((0.55 - t) / 0.55, 0.0, 1.0);
            if (tremorFade > 0.0)
            {
                double tremor = Math.Sin(elapsed * 0.04) * 0.7 * tremorFade;
                idealX += normX * tremor;
                idealY += normY * tremor;
            }

            double neededX = idealX - current.X;
            double neededY = idealY - current.Y;

            var batch = new PacketBatch();
            double stepX = neededX / 16.0;
            double stepY = neededY / 16.0;

            for (int i = 0; i < 16; i++)
            {
                // Исключен случайный джиттер внутри микрошагов: 
                // именно он вызывал рассинхрон с обратной связью и круговую болтанку
                double exactX = stepX + remainderX;
                double exactY = stepY + remainderY;
                int intX = (int)Math.Round(exactX);
                int intY = (int)Math.Round(exactY);
                remainderX = exactX - intX;
                remainderY = exactY - intY;

                batch.AddMouseMove((sbyte)Math.Clamp(intX, -127, 127), (sbyte)Math.Clamp(intY, -127, 127));
            }

            if (!ArduinoHidService.Send(batch)) break;

            if (t >= 1.0) break;

            await Task.Delay(36, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Выполняет плавный баллистический бросок по дуге с тремором (Фаза 1).
    /// </summary>
    private static async Task ExecuteCurvedFlightAsync(Point start, PointF target, double durationMs, CancellationToken ct)
    {
        double dx = target.X - start.X;
        double dy = target.Y - start.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        if (dist < 2.0) return;

        double normX = -dy / dist;
        double normY = dx / dist;
        int sign = _random.Next(2) == 0 ? 1 : -1;
        double swerve = dist * (_random.NextDouble() * 0.12 + 0.05) * sign;

        // Контрольные точки сплайна
        PointF p0 = new(start.X, start.Y);
        PointF p1 = new((float)(start.X + dx * 0.40 + normX * swerve), (float)(start.Y + dy * 0.40 + normY * swerve));
        PointF p2 = new((float)(start.X + dx * 0.75 + normX * swerve * 0.6), (float)(start.Y + dy * 0.75 + normY * swerve * 0.6));
        PointF p3 = target;

        var spline = new System.Collections.Generic.List<PointF>
        {
            new(p0.X - (p1.X - p0.X), p0.Y - (p1.Y - p0.Y)),
            p0, p1, p2, p3,
            new(p3.X + (p3.X - p2.X), p3.Y + (p3.Y - p2.Y))
        };

        double remainderX = 0.0;
        double remainderY = 0.0;
        double brownianJitterX = 0.0;
        double brownianJitterY = 0.0;

        long startTs = TimeService.GetTimestamp();
        double elapsed = 0.0;

        while (elapsed < durationMs && !ct.IsCancellationRequested)
        {
            elapsed = TimeService.GetElapsedMilliseconds(startTs);
            double t = elapsed / durationMs;
            if (t > 1.0) t = 1.0;

            double u = Math.Pow(t, 0.75);
            double progress = 10 * Math.Pow(u, 3) - 15 * Math.Pow(u, 4) + 6 * Math.Pow(u, 5);

            PointF pos = GetCatmullRomPoint(spline, progress);

            // Двухосевой тремор
            double tremor = Math.Sin(Math.PI * t) * 1.8;
            double idealX = pos.X + (Math.Sin(elapsed * 0.06) * tremor);
            double idealY = pos.Y + (Math.Cos(elapsed * 0.05) * tremor);

            Point current = Cursor.Position;
            double neededX = idealX - current.X;
            double neededY = idealY - current.Y;

            var batch = new PacketBatch();
            double stepX = neededX / 16.0;
            double stepY = neededY / 16.0;

            for (int i = 0; i < 16; i++)
            {
                // Инерционный броуновский джиттер (Ornstein-Uhlenbeck) с затуханием 0.80
                brownianJitterX = (brownianJitterX * 0.80) + ((_random.NextDouble() * 1.6 - 0.8));
                brownianJitterY = (brownianJitterY * 0.80) + ((_random.NextDouble() * 1.6 - 0.8));

                double exactX = stepX + remainderX + brownianJitterX;
                double exactY = stepY + remainderY + brownianJitterY;
                int intX = (int)Math.Round(exactX);
                int intY = (int)Math.Round(exactY);
                remainderX = exactX - intX;
                remainderY = exactY - intY;

                batch.AddMouseMove((sbyte)Math.Clamp(intX, -127, 127), (sbyte)Math.Clamp(intY, -127, 127));
            }

            if (!ArduinoHidService.Send(batch)) break;

            await Task.Delay(36, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Выполняет резкий судорожный микро-рывок (Фаза 2: лихорадочный поиск).
    /// </summary>
    private static async Task ExecuteSnappyFlickAsync(Point start, PointF target, double durationMs, CancellationToken ct)
    {
        double remainderX = 0.0;
        double remainderY = 0.0;
        double brownianJitterX = 0.0;
        double brownianJitterY = 0.0;

        long startTs = TimeService.GetTimestamp();
        double elapsed = 0.0;

        while (elapsed < durationMs && !ct.IsCancellationRequested)
        {
            elapsed = TimeService.GetElapsedMilliseconds(startTs);
            double t = elapsed / durationMs;
            if (t > 1.0) t = 1.0;

            // Резкий агрессивный бросок
            double progress = Math.Pow(t, 0.65);

            double idealX = start.X + (target.X - start.X) * progress;
            double idealY = start.Y + (target.Y - start.Y) * progress;

            Point current = Cursor.Position;
            double neededX = idealX - current.X;
            double neededY = idealY - current.Y;

            var batch = new PacketBatch();
            double stepX = neededX / 16.0;
            double stepY = neededY / 16.0;

            for (int i = 0; i < 16; i++)
            {
                // Инерционный броуновский джиттер (Ornstein-Uhlenbeck) с затуханием 0.80
                brownianJitterX = (brownianJitterX * 0.80) + ((_random.NextDouble() * 1.6 - 0.8));
                brownianJitterY = (brownianJitterY * 0.80) + ((_random.NextDouble() * 1.6 - 0.8));

                double exactX = stepX + remainderX + brownianJitterX;
                double exactY = stepY + remainderY + brownianJitterY;
                int intX = (int)Math.Round(exactX);
                int intY = (int)Math.Round(exactY);
                remainderX = exactX - intX;
                remainderY = exactY - intY;

                batch.AddMouseMove((sbyte)Math.Clamp(intX, -127, 127), (sbyte)Math.Clamp(intY, -127, 127));
            }

            if (!ArduinoHidService.Send(batch)) break;

            await Task.Delay(36, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Сплайн Кэтмулла-Рома для непрерывной кривизны.
    /// </summary>
    private static PointF GetCatmullRomPoint(System.Collections.Generic.List<PointF> pts, double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        int sections = pts.Count - 3;
        double p = t * sections;
        int i = (int)p;
        if (i >= sections) i = sections - 1;

        double u = p - i;
        double u2 = u * u;
        double u3 = u2 * u;

        PointF p0 = pts[i];
        PointF p1 = pts[i + 1];
        PointF p2 = pts[i + 2];
        PointF p3 = pts[i + 3];

        float x = 0.5f * (float)((2 * p1.X) +
            (-p0.X + p2.X) * u +
            (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * u2 +
            (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * u3);

        float y = 0.5f * (float)((2 * p1.Y) +
            (-p0.Y + p2.Y) * u +
            (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * u2 +
            (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * u3);

        return new PointF(x, y);
    }
}
