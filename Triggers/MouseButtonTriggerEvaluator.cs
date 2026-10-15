using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Triggers
{
    /// <summary>
    /// Оценивает состояние физического ввода мыши:
    /// кнопки (ЛКМ, ПКМ, СКМ), наклоны колесика (WL/WR) и расширенные клавиши F13-F24.
    /// </summary>
    public class MouseButtonTriggerEvaluator : ITriggerEvaluator
    {
        public string Button { get; }

        public MouseButtonTriggerEvaluator(string button)
        {
            Button = button ?? string.Empty;
        }

        public bool Evaluate(FrameBuffer buffer)
        {
            // Расширенные клавиши F13-F24: проверяем через таблицу VK-кодов.
            if (Button.StartsWith("F") && Button.Length > 1)
            {
                int vk = ArduinoKeyMap.GetWindowsVk(Button);
                return vk > 0 && RawInputService.IsKeyDown((ushort)vk);
            }

            return Button switch
            {
                "R key" => RawInputService.IsMouseButtonDown(2),
                "M key" => RawInputService.IsMouseButtonDown(3),
                "WL" => RawInputService.ConsumeHWheel(false),
                "WR" => RawInputService.ConsumeHWheel(true),
                _ => RawInputService.IsMouseButtonDown(1)
            };
        }
    }
}
