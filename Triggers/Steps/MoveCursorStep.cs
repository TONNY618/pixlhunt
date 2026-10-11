using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Triggers.Steps;

/// <summary>
/// Шаг комбо: плавное перемещение курсора мыши к заданным координатам.
/// </summary>
public class MoveCursorStep : IComboStep
{
    public Point Target { get; }
    public int Speed { get; }

    public MoveCursorStep(Dictionary<string, string>? parameters)
    {
        if (parameters == null)
        {
            Target = Point.Empty;
            Speed = 50;
            return;
        }

        // Парсим координаты из textBoxXY4_1 (вид: "111, 109")
        string xyRaw = parameters.GetValueOrDefault("textBoxXY4_1", "0, 0");
        Target = ParsePoint(xyRaw);

        // Парсим скорость из textBoxSpeedCursor (по умолчанию 50)
        if (int.TryParse(parameters.GetValueOrDefault("textBoxSpeedCursor", "50"), out int speed))
        {
            Speed = speed;
        }
        else
        {
            Speed = 50;
        }
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken ct)
    {
        if (Target.IsEmpty) return;

        await MouseService.MoveToAsync(Target.X, Target.Y, Speed, context.ComboName ?? "Unknown", ct)
                          .ConfigureAwait(false);
    }

    private static Point ParsePoint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Point.Empty;

        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
            return Point.Empty;

        int.TryParse(parts[0], out int x);
        int.TryParse(parts[1], out int y);
        return new Point(x, y);
    }
}
