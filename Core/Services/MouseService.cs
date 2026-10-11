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
    /// Биомеханическое перемещение с вариативным хаосом (S-дуги, овершуты с возвратом, боковые заходы).
    /// </summary>
    public static async Task MoveToAsync(int targetX, int targetY, int speedFactor, string comboName, CancellationToken ct)
    {
        Point start = Cursor.Position;
        ActionLogger.LogMouse($"Старт к [{targetX}, {targetY}] (скорость {speedFactor})", targetX, targetY);

        if (!ArduinoHidService.IsConnected) return;

        // Быстрая реакция человека (30-80 мс)
        await Task.Delay(_random.Next(30, 80), ct).ConfigureAwait(false);

        double totalDx = targetX - start.X;
        double totalDy = targetY - start.Y;
        double distance = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);
        if (distance < 2.0) return;

        // Нормализованные векторы: направления (dir) и перпендикуляра (norm)
        double dirX = totalDx / distance;
        double dirY = totalDy / distance;
        double normX = -dirY;
        double normY = dirX;

        // --- Учет скорости из настроек (speedFactor 1..100) и закона Фиттса ---
        speedFactor = Math.Clamp(speedFactor, 1, 100);
        double a = _random.NextDouble() * 0.02 + 0.04; // 40-60 мс
        double b = _random.NextDouble() * 0.02 + 0.05; // 50-70 мс/бит
        double id = Math.Log2((2.0 * distance / 32.0) + 1.0);
        double baseDurationMs = (a + b * id) * 1000.0;

        // Коэффициент ускорения: на скорости 80-90 полет занимает всего ~200-350 мс
        double speedModifier = 1.45 - (speedFactor / 100.0 * 0.95);
        double totalDurationMs = Math.Clamp(baseDurationMs * speedModifier, 120.0, 750.0);

        // --- Выбор сценария хаоса траектории (3 варианта) ---
        // Сценарий 0: Дуга с легким смещением (~35%)
        // Сценарий 1: "Ломаная" S-образная дуга с перегибом (~35%)
        // Сценарий 2: Перелет цели насквозь и резкий возврат назад (~30%)
        int scenario = _random.Next(3);
        int sign = _random.Next(2) == 0 ? 1 : -1;

        PointF p0 = new(start.X, start.Y);
        PointF p1;
        PointF p2;
        PointF p3 = new(targetX, targetY);

        if (scenario == 1 && distance > 80.0)
        {
            // СЦЕНАРИЙ 1: S-образная дуга (сначала улетает в одну сторону, затем доворачивает обратно)
            double h1 = distance * (_random.NextDouble() * 0.12 + 0.08) * sign;
            double h2 = distance * (_random.NextDouble() * 0.10 + 0.05) * -sign;

            p1 = new PointF((float)(start.X + totalDx * 0.30 + normX * h1),
                            (float)(start.Y + totalDy * 0.30 + normY * h1));

            p2 = new PointF((float)(start.X + totalDx * 0.70 + normX * h2),
                            (float)(start.Y + totalDy * 0.70 + normY * h2));
        }
        else if (scenario == 2 && distance > 100.0)
        {
            // СЦЕНАРИЙ 2: Перелет (проскакивает цель на 15-40 px вперед или вбок, затем одергивает назад)
            double overshootDist = Math.Clamp(distance * 0.08, 14.0, 38.0);
            double lateralOffset = _random.Next(-18, 18);

            p1 = new PointF((float)(start.X + totalDx * 0.40 + normX * lateralOffset * 0.5),
                            (float)(start.Y + totalDy * 0.40 + normY * lateralOffset * 0.5));

            // Контрольная точка вынесена ЗА цель
            p2 = new PointF((float)(targetX + dirX * overshootDist + normX * lateralOffset),
                            (float)(targetY + dirY * overshootDist + normY * lateralOffset));
        }
        else
        {
            // СЦЕНАРИЙ 0: Плавная человеческая дуга
            double curve = distance * (_random.NextDouble() * 0.06 + 0.02) * sign;

            p1 = new PointF((float)(start.X + totalDx * 0.35 + normX * curve),
                            (float)(start.Y + totalDy * 0.35 + normY * curve));

            p2 = new PointF((float)(start.X + totalDx * 0.70 + normX * curve * 0.7),
                            (float)(start.Y + totalDy * 0.70 + normY * curve * 0.7));
        }

        double elapsed = 0.0;
        double remainderX = 0.0;
        double remainderY = 0.0;

        // Непрерывный стриминг без пауз
        while (elapsed <= totalDurationMs && !ct.IsCancellationRequested)
        {
            double t = elapsed / totalDurationMs;
            if (t > 1.0) t = 1.0;

            // Асимметричный закон скорости: резкий старт, контролируемое торможение
            double u = Math.Pow(t, 0.82);
            double progress = 10 * Math.Pow(u, 3) - 15 * Math.Pow(u, 4) + 6 * Math.Pow(u, 5);

            // Кубическая интерполяция Безье для гладкого обхода контрольных точек
            double inv = 1.0 - progress;
            double b0 = inv * inv * inv;
            double b1 = 3 * inv * inv * progress;
            double b2 = 3 * inv * progress * progress;
            double b3 = progress * progress * progress;

            double idealX = b0 * p0.X + b1 * p1.X + b2 * p2.X + b3 * p3.X;
            double idealY = b0 * p0.Y + b1 * p1.Y + b2 * p2.Y + b3 * p3.Y;

            // Физиологический микротремор (затухает в конце пути)
            double tremor = (Math.Sin(elapsed * 0.06) * 1.2 + Math.Cos(elapsed * 0.02) * 0.8) * Math.Sin(Math.PI * t);
            idealX += normX * tremor;
            idealY += normY * tremor;

            // Динамический замер ошибки
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

        // Микрофиксация (30-65 мс)
        await Task.Delay(_random.Next(30, 65), ct).ConfigureAwait(false);
        Point finalPos = Cursor.Position;
        ActionLogger.LogMouse($"Завершено в [{finalPos.X}, {finalPos.Y}]", finalPos.X, finalPos.Y);
    }
}
