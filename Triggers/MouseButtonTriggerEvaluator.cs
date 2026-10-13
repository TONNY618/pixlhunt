using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers
{
    /// <summary>
    /// Оценивает состояние удержания физической кнопки мыши (ЛКМ, ПКМ, СКМ).
    /// </summary>
    public class MouseButtonTriggerEvaluator : ITriggerEvaluator
    {
        public int ButtonId { get; }

        public MouseButtonTriggerEvaluator(string buttonName)
        {
            ButtonId = buttonName switch
            {
                "R key" or "Right" => 2,
                "M key" or "Middle" => 3,
                _ => 1
            };
        }

        public bool Evaluate(FrameBuffer buffer)
        {
            // Опрос идет через глобальный сервис RawInput (без вызовов WinAPI на каждом кадре)
            return PixelMacroEngine.Core.Services.RawInputService.IsMouseButtonDown(ButtonId);
        }
    }
}
