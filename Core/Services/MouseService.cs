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
    /// Биомеханическое наведение с глубоким контрастом скоростей, 2D-хаосом поиска и медленной финальной доводкой.
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
        // РЕЖИМ 1: КОРОТКАЯ ДИСТАНЦИЯ (<= 120 px) - Медленная, выверенная подводка
        // Уверенный старт (100 мс) + глубокое, выраженное замедление (ползет в точку)
        // =========================================================================
        if (distance <= 120.0)
        {
            ActionLogger.LogMouse($"Точная подводка к [{targetX}, {targetY}] (D={Math.Round(distance)}px)", targetX, targetY);

            await Task.Delay(_random.Next(35, 75), ct).ConfigureAwait(false);

            // Растянутое время для заметного человеческого замедления (350-550 мс)
            double shortDuration = 280.0 + Math.Sqrt(distance) * 26.0;
            await ExecuteShortPrecisionApproachAsync(start, new Point(targetX, targetY), shortDuration, ct).ConfigureAwait(false);

            await Task.Delay(_random.Next(35, 70), ct).ConfigureAwait(false);
            Point pEnd = Cursor.Position;
            ActionLogger.LogMouse($"Завершено в [{pEnd.X}, {pEnd.Y}]", pEnd.X, pEnd.Y);
            return;
        }

        // =========================================================================
        // РЕЖИМ 2: СРЕДНЯЯ И ДЛИННАЯ ДИСТАНЦИЯ (> 120 px)
        // Взрывной бросок -> серия разнонаправленных 2D-рывков -> пауза -> медленный доводчик
        // =========================================================================
        double autoSpeed = Math.Clamp(30.0 + Math.Sqrt(distance) * 2.3, 40.0, 95.0);
        ActionLogger.LogMouse($"Старт броска к [{targetX}, {targetY}] (D={Math.Round(distance)}px)", targetX, targetY);

        await Task.Delay(_random.Next(25, 55), ct).ConfigureAwait(false);

        // 1. ВЗРЫВНОЙ БАЛЛИСТИЧЕСКИЙ БРОСОК с размытым фокусом
        double scatterRatio = distance > 350.0 ? (_random.NextDouble() * 0.16 + 0.14) : 0.12;
        double scatterRadius = Math.Clamp(distance * scatterRatio, 25.0, 260.0);

        double dirAngle = Math.Atan2(totalDy, totalDx);
        double scatterAngle = dirAngle + (_random.NextDouble() * 1.6 - 0.8);

        PointF coarseTarget = new(
            (float)(targetX + Math.Cos(scatterAngle) * scatterRadius),
            (float)(targetY + Math.Sin(scatterAngle) * scatterRadius)
        );

        // Очень быстрый первый бросок
        double ballisticDuration = Math.Clamp(Math.Sqrt(distance) * 9.5 * (100.0 / autoSpeed), 100.0, 420.0);
        await ExecuteCurvedFlightAsync(start, coarseTarget, ballisticDuration, ct).ConfigureAwait(false);

        // 2. СЕРИЯ РАЗНОНАПРАВЛЕННЫХ 2D-РЫВКОВ (Вверх, вниз, влево, вправо вокруг цели)
        int numFlicks = distance > 350.0 ? _random.Next(3, 5) : _random.Next(2, 3);
        double lastAngle = scatterAngle;

        for (int step = 1; step <= numFlicks; step++)
        {
            ct.ThrowIfCancellationRequested();

            Point current = Cursor.Position;
            double remDx = targetX - current.X;
            double remDy = targetY - current.Y;
            double remDist = Math.Sqrt(remDx * remDx + remDy * remDy);

            // Если уже подошли вплотную (<= 25 px) — передаем управление медленному доводчику
            if (remDist <= 25.0) break;

            PointF flickTarget;
            if (step < numFlicks && remDist > 10.0)
            {
                // Затухающий радиус промаха для каждого шага
                double flickRadius = Math.Clamp(remDist * (0.65 - step * 0.12), 15.0, 90.0);
                
                // РАЗВОРОТ В 2D: каждый следующий рывок бросает курсор в противоположный квадрант
                lastAngle += Math.PI + (_random.NextDouble() * 1.8 - 0.9); // Смена направления ~180° с разбросом

                flickTarget = new PointF(
                    (float)(targetX + Math.Cos(lastAngle) * flickRadius),
                    (float)(targetY + Math.Sin(lastAngle) * flickRadius)
                );
            }
            else
            {
                flickTarget = new PointF(targetX, targetY);
            }

            // Быстрый резкий рывок (40-75 мс)
            double flickDuration = Math.Clamp(remDist * 1.1 + _random.Next(35, 55), 40.0, 80.0);
            await ExecuteSnappyFlickAsync(current, flickTarget, flickDuration, ct).ConfigureAwait(false);

            await Task.Delay(_random.Next(15, 30), ct).ConfigureAwait(false);
        }

        // =========================================================================
        // 3. КОГНИТИВНЫЙ "ЩЕЛЧОК В МОЗГУ" (Пауза переключения внимания 50-85 мс)
        // Рука замерла, глаза зафиксировали цель, подключается точный доводчик пальцев
        // =========================================================================
        await Task.Delay(_random.Next(50, 85), ct).ConfigureAwait(false);

        // =========================================================================
        // 4. МЕДЛЕННЫЙ ФИНАЛЬНЫЙ ДОВОДЧИК (Глубокий Ease-Out прямо в пиксель)
        // =========================================================================
        Point beforeDock = Cursor.Position;
        double dockDx = targetX - beforeDock.X;
        double dockDy = targetY - beforeDock.Y;
        double dockDist = Math.Sqrt(dockDx * dockDx + dockDy * dockDy);

        if (dockDist > 1.5)
        {
            // Растянутое время для мягкого, вкрадчивого причаливания к цели (320-480 мс)
            double dockDuration = 260.0 + Math.Sqrt(dockDist) * 24.0;
            await ExecuteShortPrecisionApproachAsync(beforeDock, new Point(targetX, targetY), dockDuration, ct).ConfigureAwait(false);
        }

        await Task.Delay(_random.Next(35, 65), ct).ConfigureAwait(false);
        Point finalPos = Cursor.Position;
        ActionLogger.LogMouse($"Завершено в [{finalPos.X}, {finalPos.Y}]", finalPos.X, finalPos.Y);
    }

    /// <summary>
    /// Деликатная доводка с выраженным нелинейным затуханием (Ease-Out) и высоким тремором пальцев.
    /// </summary>
    private static async Task ExecuteShortPrecisionApproachAsync(Point start, Point target, double durationMs, CancellationToken ct)
    {
        double totalDx = target.X - start.X;
        double totalDy = target.Y - start.Y;
        double dist = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);
        if (dist < 1.0) return;

        double normX = -totalDy / dist;
        double normY = totalDx / dist;
        double lateralCurve = (_random.NextDouble() * 1.8 + 0.8) * (_random.Next(2) == 0 ? 1 : -1);

        double elapsed = 0.0;
        double remainderX = 0.0;
        double remainderY = 0.0;

        while (elapsed <= durationMs && !ct.IsCancellationRequested)
        {
            double t = elapsed / durationMs;
            if (t > 1.0) t = 1.0;

            // СТЕПЕННАЯ КРИВАЯ ГЛУБОКОГО ЗАТУХАНИЯ:
            // t=0.3 -> 70% пути пройдено (уверенно)
            // t=0.6 -> 95% пути пройдено
            // t=0.6..1.0 -> последние 5% расстояния ползет медленно и аккуратно
            double progress = 1.0 - Math.Pow(1.0 - t, 3.5);

            double curveOffset = lateralCurve * Math.Sin(Math.PI * t);

            double idealX = start.X + (totalDx * progress) + (normX * curveOffset);
            double idealY = start.Y + (totalDy * progress) + (normY * curveOffset);

            // УСИЛЕННЫЙ ТРЕМОР ПАЛЬЦЕВ (рука напряжена, амплитуда 2.2 - 3.8 px, не угасает в ноль)
            double tremorAmp = 1.8 + _random.NextDouble() * 1.5;
            double tremorX = (Math.Sin(elapsed * 0.070) * 0.7 + Math.Cos(elapsed * 0.025) * 0.3) * tremorAmp;
            double tremorY = (Math.Cos(elapsed * 0.062) * 0.7 + Math.Sin(elapsed * 0.030) * 0.3) * tremorAmp;

            idealX += tremorX;
            idealY += tremorY;

            Point current = Cursor.Position;
            double neededX = idealX - current.X;
            double neededY = idealY - current.Y;

            var batch = new PacketBatch();
            double stepX = neededX / 16.0;
            double stepY = neededY / 16.0;

            for (int i = 0; i < 16; i++)
            {
                // Повышенный аппаратный джиттер на уровне 500 Гц (каждый 3-й и 4-й микрошаг)
                int jitterX = (i % 3 == 0) ? _random.Next(-1, 2) : 0;
                int jitterY = (i % 4 == 0) ? _random.Next(-1, 2) : 0;

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
