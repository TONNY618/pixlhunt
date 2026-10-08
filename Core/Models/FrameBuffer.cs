using System.Drawing;

namespace PixelMacroEngine.Core.Models;

public class FrameBuffer
{
    // Заглушка: реальный метод будет читать пиксель из буфера DXGI без Bitmap.GetPixel
    public virtual Color GetPixelColor(int x, int y)
    {
        return Color.Black;
    }
}