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
            ButtonId = ParseButtonId(buttonName);
        }

        /// <summary>
        /// Регистронезависимый и безопасный парсинг имени кнопки мыши.
        /// Неизвестное значение → 0 (не срабатывает никогда).
        /// </summary>
        private static int ParseButtonId(string? buttonName)
        {
            if (string.IsNullOrWhiteSpace(buttonName))
                return 0;

            string key = buttonName.Trim().ToLowerInvariant();

            return key switch
            {
                // ЛКМ
                "l key" or "left" or "лкм" or "1" or "lmb" or "leftbutton" => 1,
                // ПКМ
                "r key" or "right" or "пкм" or "2" or "rmb" or "rightbutton" => 2,
                // СКМ
                "m key" or "middle" or "скм" or "3" or "mmb" or "middlebutton"
                    or "wheel" or "middle click" => 3,
                _ => 0
            };
        }

        public bool Evaluate(FrameBuffer buffer)
        {
            // Неизвестная кнопка — триггер никогда не срабатывает
            if (ButtonId == 0)
                return false;

            // Опрос идет через глобальный сервис RawInput (без вызовов WinAPI на каждом кадре)
            return PixelMacroEngine.Core.Services.RawInputService.IsMouseButtonDown(ButtonId);
        }
    }
}
