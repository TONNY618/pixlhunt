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
    /// Непрерывное биомеханическое перемещение с динамической коррекцией (без остановок).
    /// </summary>
    public static async Task MoveToAsync(int targetX, int targetY, int speedFactor, string comboName, CancellationToken ct)
    {
        Point start = Cursor.Position;
        ActionLogger.LogMouse($"Старт к [{targetX}, {targetY}]", targetX, targetY);

        if (!ArduinoHidService.IsConnected) return;

        // Быстрая реакция человека (более бодрая)
        await Task.Delay(_random.Next(60, 140), ct).ConfigureAwait(false);

        double distance = Math.Sqrt(Math.Pow(targetX - start.X, 2) + Math.Pow(targetY - start.Y, 2));
        if (distance < 2) return;

        // Ускоренный закон Фиттса
        double a = _random.NextDouble() * 0.03 + 0.05; // 50-80 мс
        double b = _random.NextDouble() * 0.03 + 0.07; // 70-100 мс/бит
        double id = Math.Log2((2.0 * distance / 30.0) + 1.0);
        double totalDurationMs = (a + b * id) * 1000.0;

        // Легкий перелет (овершут) для дистанций > 150 px
        bool willOvershoot = distance > 150 && _random.NextDouble() < 0.6;
        double overX = willOvershoot ? (targetX - start.X) * (_random.NextDouble() * 0.04 + 0.02) : 0;
        double overY = willOvershoot ? (targetY - start.Y) * (_random.NextDouble() * 0.04 + 0.02) : 0;

        double elapsed = 0;
        double remainderX = 0;
        double remainderY = 0;

        // Единый непрерывный цикл до достижения времени полета
        while (elapsed <= totalDurationMs && !ct.IsCancellationRequested)
        {
            double t = elapsed / totalDurationMs;
            if (t > 1.0) t = 1.0;

            // Асимметричный Minimum Jerk (разгон быстрее торможения)
            double u = Math.Pow(t, 0.85);
            double progress = 10 * Math.Pow(u, 3) - 15 * Math.Pow(u, 4) + 6 * Math.Pow(u, 5);

            // Выпуклость овершута в последней трети траектории
            double overshootFactor = willOvershoot ? Math.Sin(Math.PI * t) * progress : 0;

            // Идеальная точка, где мышь должна быть в данный миллисекундный срез
            double idealX = start.X + (targetX - start.X) * progress + (overX * overshootFactor);
            double idealY = start.Y + (targetY - start.Y) * progress + (overY * overshootFactor);

            // Легкий тремор руки
            double tremor = Math.Sin(elapsed * 0.05) * 1.5 * Math.Sin(Math.PI * t);
            idealX += tremor;

            // ДИНАМИЧЕСКАЯ КОРРЕКЦИЯ: смотрим реальную позицию курсора на экране
            Point current = Cursor.Position;
            double neededX = idealX - current.X;
            double neededY = idealY - current.Y;

            // Упаковываем нужное смещение в пакет из 16 микрошагов для Arduino
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

            if (!ArduinoHidService.Send(batch))
            {
                ActionLogger.LogMouse("ОШИБКА: Потеря связи.");
                break;
            }

            // ПК спит 36 мс, пока Arduino плавно аппаратно воспроизводит 16 шагов
            await Task.Delay(36, ct).ConfigureAwait(false);
            elapsed += 32.0; // Время шага Arduino (16 * 2 мс)
        }

        // Финальная фиксация взгляда/руки перед следующим действием
        await Task.Delay(_random.Next(30, 80), ct).ConfigureAwait(false);
        Point finalPos = Cursor.Position;
        ActionLogger.LogMouse($"Завершено в [{finalPos.X}, {finalPos.Y}]", finalPos.X, finalPos.Y);
    }
}
