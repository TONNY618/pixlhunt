using System;
using System.Diagnostics;
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
    private static DateTime _lastClickTime = DateTime.MinValue;
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
            var now = DateTime.UtcNow;
            if (_lastClickButton == button && (now - _lastClickTime).TotalMilliseconds < 380)
            {
                isDoubleClick = true;
            }
            _lastClickTime = now;
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
    /// Прокрутка колесика с нарастающими и затухающими интервалами между шагами.
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

            // Интервалы вращения колеса пальцем (30-60 мс)
            await Task.Delay(_random.Next(28, 58), ct).ConfigureAwait(false);
        }

        ActionLogger.LogMouse($"Scroll {(scrollUp ? "UP" : "DOWN")} x{count}", comboName: comboName);
    }

    /// <summary>
    /// Монолитное биомеханическое наведение: 
    /// 1. Баллистический бросок (размытый фокус) -> 
    /// 2. Серия резких поисковых рывков до зоны подлета (20-35 px) -> 
    /// 3. Когнитивный "щелчок в мозгу" (пауза переключения внимания 45-85 мс) -> 
    /// 4. Фирменный медленный доводчик (глубокий Ease-Out без пролетов).
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
        // ВЕТКА А: ДИСТАНЦИЯ УЖЕ БЛИЗКАЯ (<= 120 px)
        // Сразу запускаем деликатный доводчик кисти без резких бросков
        // =========================================================================
        if (distance <= 120.0)
        {
            ActionLogger.LogMouse($"Прямой доводчик к [{targetX}, {targetY}] (D={Math.Round(distance)}px)", targetX, targetY);

            await Task.Delay(_random.Next(35, 70), ct).ConfigureAwait(false);

            double shortDuration = 135.0 + Math.Sqrt(distance) * 17.5;
            await ExecuteShortPrecisionApproachAsync(start, new Point(targetX, targetY), shortDuration, ct).ConfigureAwait(false);

            await Task.Delay(_random.Next(30, 60), ct).ConfigureAwait(false);
            Point pEnd = Cursor.Position;
            ActionLogger.LogMouse($"Завершено в [{pEnd.X}, {pEnd.Y}]", pEnd.X, pEnd.Y);
            return;
        }

        // =========================================================================
        // ВЕТКА Б: СРЕДНЯЯ И ДЛИННАЯ ДИСТАНЦИЯ (> 120 px)
        // =========================================================================
        double autoSpeed = Math.Clamp(28.0 + Math.Sqrt(distance) * 2.25, 35.0, 95.0);
        ActionLogger.LogMouse($"Старт броска к [{targetX}, {targetY}] (D={Math.Round(distance)}px, авто-скорость={Math.Round(autoSpeed)})", targetX, targetY);

        await Task.Delay(_random.Next(30, 60), ct).ConfigureAwait(false);

        // 1. Баллистический бросок с размытым фокусом в сторону цели
        double scatterRatio = distance > 350.0 ? (_random.NextDouble() * 0.16 + 0.12) : 0.10;
        double scatterRadius = Math.Clamp(distance * scatterRatio, 18.0, 260.0);

        double dirAngle = Math.Atan2(totalDy, totalDx);
        double scatterAngle = dirAngle + (_random.NextDouble() * 1.4 - 0.7);

        PointF coarseTarget = new(
            (float)(targetX + Math.Cos(scatterAngle) * scatterRadius),
            (float)(targetY + Math.Sin(scatterAngle) * scatterRadius)
        );

        double ballisticDuration = Math.Clamp(Math.Sqrt(distance) * 10.5 * (100.0 / autoSpeed), 110.0, 460.0);
        await ExecuteCurvedFlightAsync(start, coarseTarget, ballisticDuration, ct).ConfigureAwait(false);

        // 2. Серия резких рывков (стягиваем курсор в ближнюю зону 20-35 px от цели)
        int maxFlicks = distance > 350.0 ? _random.Next(2, 4) : 1;

        for (int step = 1; step <= maxFlicks; step++)
        {
            ct.ThrowIfCancellationRequested();

            Point current = Cursor.Position;
            double remDx = targetX - current.X;
            double remDy = targetY - current.Y;
            double remDist = Math.Sqrt(remDx * remDx + remDy * remDy);

            // Если рывок уже привел курсор в зону доводчика (<= 30 px) — выходим к финалу!
            if (remDist <= 30.0) break;

            // Рывок с перелетом через цель для имитации поиска
            double overRatio = distance > 350.0 ? (_random.NextDouble() * 0.20 + 0.20) : 0.12;
            double latOffset = distance > 350.0 ? _random.Next(-18, 18) : _random.Next(-8, 8);
            double nX = -remDy / remDist;
            double nY = remDx / remDist;

            PointF flickTarget = new(
                (float)(targetX + (remDx / remDist) * (remDist * overRatio) + nX * latOffset),
                (float)(targetY + (remDy / remDist) * (remDist * overRatio) + nY * latOffset)
            );

            double flickDuration = Math.Clamp(remDist * 1.2 + _random.Next(35, 60), 40.0, 85.0);
            await ExecuteSnappyFlickAsync(current, flickTarget, flickDuration, ct).ConfigureAwait(false);

            await Task.Delay(_random.Next(15, 30), ct).ConfigureAwait(false);
        }

        // =========================================================================
        // ЭТАП 3: "ЩЕЛЧОК В МОЗГУ" (Пауза переключения внимания, 45-85 мс)
        // Рука остановилась после грубого подлета, взгляд сфокусировался на крестике,
        // подключается мелкая моторика пальцев для финальной доводки
        // =========================================================================
        await Task.Delay(_random.Next(45, 85), ct).ConfigureAwait(false);

        // =========================================================================
        // ЭТАП 4: ФИНАЛЬНЫЙ МЕДЛЕННЫЙ ДОВОДЧИК (Ease-Out прямо в цель)
        // =========================================================================
        Point beforeDock = Cursor.Position;
        double dockDx = targetX - beforeDock.X;
        double dockDy = targetY - beforeDock.Y;
        double dockDist = Math.Sqrt(dockDx * dockDx + dockDy * dockDy);

        if (dockDist > 1.5)
        {
            double dockDuration = 125.0 + Math.Sqrt(dockDist) * 16.5;
            await ExecuteShortPrecisionApproachAsync(beforeDock, new Point(targetX, targetY), dockDuration, ct).ConfigureAwait(false);
        }

        // Микрофиксация взгляда перед кликом
        await Task.Delay(_random.Next(30, 55), ct).ConfigureAwait(false);
        Point finalPos = Cursor.Position;
        ActionLogger.LogMouse($"Завершено в [{finalPos.X}, {finalPos.Y}]", finalPos.X, finalPos.Y);
    }

    /// <summary>
    /// Деликатная подводка для коротких дистанций (<=120 px):
    /// Уверенный старт + выраженное прогрессивное затухание скорости (Ease-Out) без пролетов.
    /// </summary>
    private static async Task ExecuteShortPrecisionApproachAsync(Point start, Point target, double durationMs, CancellationToken ct)
    {
        double totalDx = target.X - start.X;
        double totalDy = target.Y - start.Y;
        double dist = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);
        if (dist < 1.0) return;

        double normX = -totalDy / dist;
        double normY = totalDx / dist;

        // Небольшой естественный дуговой изгиб кисти (всего 1.0 - 2.5 px в сторону)
        double lateralCurve = (_random.NextDouble() * 1.5 + 1.0) * (_random.Next(2) == 0 ? 1 : -1);

        double elapsed = 0.0;
        double remainderX = 0.0;
        double remainderY = 0.0;

        while (elapsed <= durationMs && !ct.IsCancellationRequested)
        {
            double t = elapsed / durationMs;
            if (t > 1.0) t = 1.0;

            // Нелинейный закон скорости: уверенный старт (проходим первые 65% расстояния бодро),
            // а оставшиеся 35% расстояния глубоко и мягко замедляемся прямо в цель
            double progress = 1.0 - Math.Pow(1.0 - t, 2.6);

            // Мягкий боковой изгиб (затухает к финишу)
            double curveOffset = lateralCurve * Math.Sin(Math.PI * t);

            double idealX = start.X + (totalDx * progress) + (normX * curveOffset);
            double idealY = start.Y + (totalDy * progress) + (normY * curveOffset);

            // Естественный микротремор пальцев (очень деликатный, не выбивающийся из колеи)
            double tremor = Math.Sin(Math.PI * t) * (Math.Sin(elapsed * 0.05) * 0.8);
            idealX += normX * tremor;
            idealY += normY * tremor;

            Point current = Cursor.Position;
            double neededX = idealX - current.X;
            double neededY = idealY - current.Y;

            var batch = new PacketBatch();
            double stepX = neededX / 16.0;
            double stepY = neededY / 16.0;

            for (int i = 0; i < 16; i++)
            {
                // На короткой дистанции джиттер включается реже, чтобы исключить механический шум
                int jitterX = (i % 8 == 0) ? _random.Next(-1, 2) : 0;
                int jitterY = (i % 8 == 0) ? _random.Next(-1, 2) : 0;

                double exactX = stepX + remainderX + jitterX;
                double exactY = stepY + remainderY + jitterY;
                int intX = (int)Math.Round(exactX);
                int intY = (int)Math.Round(exactY);
                remainderX = exactX - intX;
                remainderY = exactY - intY;

                batch.AddMouseMove((sbyte)Math.Clamp(intX, -127, 127), (sbyte)Math.Clamp(intY, -127, 127));
            }

            if (!ArduinoHidService.Send(batch)) break;

            await Task.Delay(36, ct).ConfigureAwait(false);
            elapsed += 32.0;
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

        double elapsed = 0.0;
        double remainderX = 0.0;
        double remainderY = 0.0;

        while (elapsed <= durationMs && !ct.IsCancellationRequested)
        {
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
                int jitterX = (i % 4 == 0) ? _random.Next(-1, 2) : 0;
                int jitterY = (i % 6 == 0) ? _random.Next(-1, 2) : 0;

                double exactX = stepX + remainderX + jitterX;
                double exactY = stepY + remainderY + jitterY;
                int intX = (int)Math.Round(exactX);
                int intY = (int)Math.Round(exactY);
                remainderX = exactX - intX;
                remainderY = exactY - intY;

                batch.AddMouseMove((sbyte)Math.Clamp(intX, -127, 127), (sbyte)Math.Clamp(intY, -127, 127));
            }

            if (!ArduinoHidService.Send(batch)) break;

            await Task.Delay(36, ct).ConfigureAwait(false);
            elapsed += 32.0;
        }
    }

    /// <summary>
    /// Выполняет резкий судорожный микро-рывок (Фаза 2: лихорадочный поиск).
    /// </summary>
    private static async Task ExecuteSnappyFlickAsync(Point start, PointF target, double durationMs, CancellationToken ct)
    {
        double elapsed = 0.0;
        double remainderX = 0.0;
        double remainderY = 0.0;

        while (elapsed <= durationMs && !ct.IsCancellationRequested)
        {
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
                double exactX = stepX + remainderX;
                double exactY = stepY + remainderY;
                int intX = (int)Math.Round(exactX);
                int intY = (int)Math.Round(exactY);
                remainderX = exactX - intX;
                remainderY = exactY - intY;

                batch.AddMouseMove((sbyte)Math.Clamp(intX, -127, 127), (sbyte)Math.Clamp(intY, -127, 127));
            }

            if (!ArduinoHidService.Send(batch)) break;

            await Task.Delay(36, ct).ConfigureAwait(false);
            elapsed += 32.0;
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
