using System.Drawing;

namespace PixelMacroEngine.Core.Models
{
    /// <summary>
    /// Глобальные переменные текущей сессии выполнения комбо.
    /// Используются для передачи координат из условий (выше) в дочерние действия (мышь).
    /// </summary>
    public static class RuntimeVariables
    {
        public static Point? TargetPoint1 { get; set; }
        public static Point? TargetPoint2 { get; set; }
        public static bool IsRegion { get; set; }

        public static void Clear()
        {
            TargetPoint1 = null;
            TargetPoint2 = null;
            IsRegion = false;
        }
    }
}
