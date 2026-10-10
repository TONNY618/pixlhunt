using System;
using System.Drawing;

namespace PixelMacroEngine.Core.Models;

/// <summary>
/// [Подсистема видеозахвата: Уровень 2 - Данные]
/// Пассивный буфер сырых BGRA-данных в оперативной памяти (Zero-Allocation).
/// Отвечает за O(1) доступ к цвету пикселей без создания объектов в куче.
/// Связан с: Наполняется из DxgiScreenCapture, управляется ScreenCaptureService.
/// </summary>
public class FrameBuffer
{
    private byte[] _data = Array.Empty<byte>();
    private int _width;
    private int _height;
    private int _stride;

    public int Width => _width;
    public int Height => _height;
    public int Stride => _stride;

    /// <summary>Сырые BGRA-данные кадра (только для чтения).</summary>
    public ReadOnlySpan<byte> Data => _data;

    /// <summary>
    /// Обновляет буфер сырыми данными из захвата.
    /// Массив переиспользуется, если размер не изменился.
    /// </summary>
    public void Update(ReadOnlySpan<byte> source, int width, int height, int stride)
    {
        int required = stride * height;
        if (_data.Length != required)
            _data = new byte[required];

        source.CopyTo(_data);
        _width = width;
        _height = height;
        _stride = stride;
    }

    /// <summary>
    /// Читает цвет пикселя напрямую из сырого буфера без аллокаций.
    /// </summary>
    public virtual Color GetPixelColor(int x, int y)
    {
        if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
            return Color.Black;

        int offset = y * _stride + x * 4;
        // Формат BGRA
        byte b = _data[offset + 0];
        byte g = _data[offset + 1];
        byte r = _data[offset + 2];
        return Color.FromArgb(r, g, b);
    }

    /// <summary>
    /// Вычисляет средний цвет прямоугольной области без аллокаций.
    /// Координаты автоматически нормализуются (min/max), выходящие за границы
    /// пиксели отбрасываются. Если область пуста — возвращает Color.Black.
    /// </summary>
    public Color GetAverageColor(int x1, int y1, int x2, int y2)
    {
        if (_data.Length == 0 || _width == 0 || _height == 0)
            return Color.Black;

        int left = Math.Min(x1, x2);
        int right = Math.Max(x1, x2);
        int top = Math.Min(y1, y2);
        int bottom = Math.Max(y1, y2);

        // Клипаем по границам буфера
        if (left < 0) left = 0;
        if (top < 0) top = 0;
        if (right >= _width) right = _width - 1;
        if (bottom >= _height) bottom = _height - 1;

        if (left > right || top > bottom)
            return Color.Black;

        long sumR = 0, sumG = 0, sumB = 0;
        long count = 0;

        for (int y = top; y <= bottom; y++)
        {
            int rowOffset = y * _stride;
            for (int x = left; x <= right; x++)
            {
                int offset = rowOffset + x * 4;
                sumB += _data[offset + 0];
                sumG += _data[offset + 1];
                sumR += _data[offset + 2];
                count++;
            }
        }

        if (count == 0) return Color.Black;

        return Color.FromArgb(
            (int)(sumR / count),
            (int)(sumG / count),
            (int)(sumB / count));
    }
}
