using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Triggers.Steps;

public class MouseMoveStep : IComboStep
{
    private readonly pxlhunt.UI.Forms.ComboEditorCompoziteControls.CursorMoveDto _dto;
    private static readonly Random _rnd = new();

    public MouseMoveStep(pxlhunt.UI.Forms.ComboEditorCompoziteControls.CursorMoveDto dto)
    {
        _dto = dto;
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken ct)
    {
        Point target = Point.Empty;

        if (_dto.Mode == 0) // Взять из переменных выше
        {
            if (RuntimeVariables.IsRegion && RuntimeVariables.TargetPoint1.HasValue && RuntimeVariables.TargetPoint2.HasValue)
            {
                target = GetRandomPointInRegion(RuntimeVariables.TargetPoint1.Value, RuntimeVariables.TargetPoint2.Value);
            }
            else if (RuntimeVariables.TargetPoint1.HasValue)
            {
                target = RuntimeVariables.TargetPoint1.Value;
            }
            else return; // Ошибка: данных нет
        }
        else if (_dto.Mode == 1) // Свой регион
        {
            Point p1 = ParsePoint(_dto.Point1);
            Point p2 = ParsePoint(_dto.Point2);
            if (p1.IsEmpty && p2.IsEmpty) return;
            target = GetRandomPointInRegion(p1, p2);
        }
        else // Своя точная точка (Mode 2)
        {
            target = ParsePoint(_dto.Point1);
            if (target.IsEmpty) return;
        }

        // Параметр speedFactor в MouseService игнорируется алгоритмом Фиттса, передаем 50
        await MouseService.MoveToAsync(target.X, target.Y, 50, context.ComboName ?? "Unknown", ct).ConfigureAwait(false);
    }

    private Point GetRandomPointInRegion(Point p1, Point p2)
    {
        int minX = Math.Min(p1.X, p2.X);
        int maxX = Math.Max(p1.X, p2.X);
        int minY = Math.Min(p1.Y, p2.Y);
        int maxY = Math.Max(p1.Y, p2.Y);
        
        // В будущем здесь будет нормальное распределение Гаусса
        return new Point(_rnd.Next(minX, maxX + 1), _rnd.Next(minY, maxY + 1));
    }

    private static Point ParsePoint(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Point.Empty;
        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
            return new Point(x, y);
        return Point.Empty;
    }
}
