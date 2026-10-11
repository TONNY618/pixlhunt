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
        speedFactor = Math.Clamp(speedFactor, 1, 100);

        while (!ct.IsCancellationRequested)
        {
            Point current = Cursor.Position;
            int deltaX = targetX - current.X;
            int deltaY = targetY - current.Y;

            if (Math.Abs(deltaX) <= 1 && Math.Abs(deltaY) <= 1)
                break;

            int maxStep = Math.Max(2, (int)(speedFactor * 0.4));
            sbyte stepX = (sbyte)Math.Clamp(deltaX, -maxStep, maxStep);
            sbyte stepY = (sbyte)Math.Clamp(deltaY, -maxStep, maxStep);

            var batch = new PacketBatch();
            batch.AddMouseMove(stepX, stepY);
            ArduinoHidService.Send(batch);

            await Task.Delay(_random.Next(7, 12), ct).ConfigureAwait(false);
        }

        ActionLogger.LogMouse($"Move to [{targetX}, {targetY}]", targetX, targetY);
    }
}
