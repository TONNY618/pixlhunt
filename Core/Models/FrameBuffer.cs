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
}
