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
/// Шаг комбо: перемещение курсора мыши к координатам из UI.
/// </summary>
public class MouseMoveStep : IComboStep
{
    public Point Target { get; }
    public int Speed { get; }

    public MouseMoveStep(Dictionary<string, string>? p)
    {
        if (p == null)
        {
            Target = Point.Empty;
            Speed = 80;
            return;
        }

        string rawCoords = p.GetValueOrDefault("textBoxXY4_1", "0, 0");
        Target = ParsePoint(rawCoords);

        if (int.TryParse(p.GetValueOrDefault("textBoxSpeedCursor", "80"), out int sp))
        {
            Speed = sp;
        }
        else
        {
            Speed = 80;
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
        if (string.IsNullOrWhiteSpace(value)) return Point.Empty;
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return Point.Empty;

        int.TryParse(parts[0], out int x);
        int.TryParse(parts[1], out int y);
        return new Point(x, y);
    }
}
