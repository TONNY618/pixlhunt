using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelMacroEngine.Core.Input;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// Сервис управления мышью: клики, скролл и пошаговое перемещение курсора через Arduino RawHID.
/// В дальнейшем алгоритм перемещения будет дополнен кривыми Безье и законом Фиттса.
/// </summary>
public static class MouseService
{
    private static readonly Random _random = new();

    public static async Task ClickButtonAsync(byte button, bool pressDown, bool pressUp, string comboName, CancellationToken ct)
    {
        if (pressDown)
        {
            var batch = new PacketBatch();
            batch.AddMouseButton(button, isDown: true);
            ArduinoHidService.Send(batch);
            ActionLogger.LogMouse($"Button {button} DOWN", comboName: comboName);
        }

        if (pressDown && pressUp)
        {
            int holdTime = HumanizerEngine.GetKeyPressDuration();
            await Task.Delay(holdTime, ct).ConfigureAwait(false);
        }

        if (pressUp)
        {
            var batch = new PacketBatch();
            batch.AddMouseButton(button, isDown: false);
            ArduinoHidService.Send(batch);
            ActionLogger.LogMouse($"Button {button} UP", comboName: comboName);
        }

        if ((pressDown && pressUp) || (!pressDown && pressUp))
        {
            int postDelay = HumanizerEngine.GetFlightTime();
            await Task.Delay(postDelay, ct).ConfigureAwait(false);
        }
    }

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
            await Task.Delay(_random.Next(20, 45), ct).ConfigureAwait(false);
        }

        ActionLogger.LogMouse($"Scroll {(scrollUp ? "UP" : "DOWN")} x{count}", comboName: comboName);
    }

    public static async Task MoveToAsync(int targetX, int targetY, int speedFactor, string comboName, CancellationToken ct)
    {
        ActionLogger.LogMouse($"Старт движения к [{targetX}, {targetY}]", targetX, targetY);

        speedFactor = Math.Clamp(speedFactor, 1, 100);
        int maxStep = Math.Max(2, (int)(speedFactor * 0.4));
        int iterations = 0;
        const int maxIterations = 200; // Предохранитель от вечного цикла, если курсор не дошел

        while (!ct.IsCancellationRequested && iterations < maxIterations)
        {
            iterations++;
            Point current = Cursor.Position;
            int deltaX = targetX - current.X;
            int deltaY = targetY - current.Y;

            // Достигли цели (погрешность 2 пикселя)
            if (Math.Abs(deltaX) <= 2 && Math.Abs(deltaY) <= 2)
                break;

            sbyte stepX = (sbyte)Math.Clamp(deltaX, -maxStep, maxStep);
            sbyte stepY = (sbyte)Math.Clamp(deltaY, -maxStep, maxStep);

            var batch = new PacketBatch();
            batch.AddMouseMove(stepX, stepY);

            // Если отправка не удалась (плата отключилась на этом шаге) — мгновенно выходим
            if (!ArduinoHidService.Send(batch))
                break;

            await Task.Delay(_random.Next(7, 12), ct).ConfigureAwait(false);
        }

        Point finalPos = Cursor.Position;
        ActionLogger.LogMouse($"Завершено в [{finalPos.X}, {finalPos.Y}]", finalPos.X, finalPos.Y);
    }
}
