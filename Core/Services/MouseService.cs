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
        int iterations = 0;
        const int maxIterations = 300;

        while (!ct.IsCancellationRequested && iterations < maxIterations)
        {
            iterations++;
            Point current = Cursor.Position;
            int deltaX = targetX - current.X;
            int deltaY = targetY - current.Y;

            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (distance <= 2.0)
                break;

            // Плавное замедление при приближении к цели (Ease-Out)
            // Чем ближе к цели, тем меньше шаг, чтобы не проскакивать мимо
            double stepSize = Math.Min(distance, Math.Max(2.0, (speedFactor * 0.35) * Math.Min(1.0, distance / 40.0)));

            sbyte stepX = (sbyte)Math.Clamp((int)Math.Round((deltaX / distance) * stepSize), -127, 127);
            sbyte stepY = (sbyte)Math.Clamp((int)Math.Round((deltaY / distance) * stepSize), -127, 127);

            if (stepX == 0 && deltaX != 0) stepX = (sbyte)Math.Sign(deltaX);
            if (stepY == 0 && deltaY != 0) stepY = (sbyte)Math.Sign(deltaY);

            var batch = new PacketBatch();
            batch.AddMouseMove(stepX, stepY);

            if (!ArduinoHidService.Send(batch))
                break;

            // Микропауза между шагами
            await Task.Delay(_random.Next(5, 9), ct).ConfigureAwait(false);
        }

        Point finalPos = Cursor.Position;
        ActionLogger.LogMouse($"Завершено в [{finalPos.X}, {finalPos.Y}]", finalPos.X, finalPos.Y);
    }
}
