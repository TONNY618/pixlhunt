using System;
using System.Drawing;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers;

/// <summary>
/// [Подсистема триггеров: Уровень 3 - Оценка цвета]
/// Проверяет цвет одного пикселя или средний цвет региона на соответствие ожидаемому
/// с учётом допустимого отклонения в процентах.
/// Связан с: ITriggerEvaluator, FrameBuffer.
/// </summary>
public class ColorTriggerEvaluator : ITriggerEvaluator
{
    /// <summary>Первая точка (или левый верхний угол региона).</summary>
    public Point Point1 { get; set; }

    /// <summary>Вторая точка (правый нижний угол региона).</summary>
    public Point Point2 { get; set; }

    /// <summary>Ожидаемый цвет.</summary>
    public Color ExpectedColor { get; set; }

    /// <summary>Допустимое отклонение в процентах (0..100).</summary>
    public int DeviantPercent { get; set; }

    /// <summary>true — условие "не равно", false — "равно".</summary>
    public bool IsNotEqual { get; set; }

    /// <summary>true — режим среднего цвета региона, false — один пиксель.</summary>
    public bool IsRegionAverage { get; set; }

    public ColorTriggerEvaluator()
    {
    }

    public ColorTriggerEvaluator(
        Point point1,
        Point point2,
        Color expectedColor,
        int deviantPercent,
        bool isNotEqual,
        bool isRegionAverage)
    {
        Point1 = point1;
        Point2 = point2;
        ExpectedColor = expectedColor;
        DeviantPercent = deviantPercent;
        IsNotEqual = isNotEqual;
        IsRegionAverage = isRegionAverage;
    }

    /// <inheritdoc />
    public bool Evaluate(FrameBuffer buffer)
    {
        if (buffer == null)
            return false;

        Color actual;

        if (!IsRegionAverage)
        {
            actual = buffer.GetPixelColor(Point1.X, Point1.Y);
        }
        else
        {
            actual = GetRegionAverageColor(buffer);
        }

        bool equal = ColorsMatch(actual, ExpectedColor, DeviantPercent);
        return IsNotEqual ? !equal : equal;
    }

    /// <summary>
    /// Вычисляет средний цвет пикселей в прямоугольнике между Point1 и Point2.
    /// </summary>
    private Color GetRegionAverageColor(FrameBuffer buffer)
    {
        int left = Math.Min(Point1.X, Point2.X);
        int right = Math.Max(Point1.X, Point2.X);
        int top = Math.Min(Point1.Y, Point2.Y);
        int bottom = Math.Max(Point1.Y, Point2.Y);

        // Ограничиваем пределами буфера
        if (left < 0) left = 0;
        if (top < 0) top = 0;
        if (right >= buffer.Width) right = buffer.Width - 1;
        if (bottom >= buffer.Height) bottom = buffer.Height - 1;

        if (right < left || bottom < top)
            return Color.Black;

        long sumR = 0, sumG = 0, sumB = 0;
        long count = 0;

        for (int y = top; y <= bottom; y++)
        {
            for (int x = left; x <= right; x++)
            {
                Color c = buffer.GetPixelColor(x, y);
                sumR += c.R;
                sumG += c.G;
                sumB += c.B;
                count++;
            }
        }

        if (count == 0)
            return Color.Black;

        return Color.FromArgb(
            (int)(sumR / count),
            (int)(sumG / count),
            (int)(sumB / count));
    }

    /// <summary>
    /// Сравнивает два цвета с учётом допустимого отклонения в процентах.
    /// Дистанция нормируется на максимально возможную (sqrt(3)*255 ≈ 441.67).
    /// </summary>
    private static bool ColorsMatch(Color a, Color b, int deviantPercent)
    {
        double dr = a.R - b.R;
        double dg = a.G - b.G;
        double db = a.B - b.B;

        double distance = Math.Sqrt(dr * dr + dg * dg + db * db);
        double percent = distance / 441.67 * 100.0;

        return percent <= deviantPercent;
    }
}
