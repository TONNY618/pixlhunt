using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Triggers
{
    /// <summary>
    /// Оценивает прокрутку колесика мыши: срабатывает, когда в скользящем окне
    /// накоплено не менее <see cref="RequiredSteps"/> щелчков в заданном направлении.
    /// После срабатывания счётчик направления сбрасывается (защита от повторного запуска).
    /// </summary>
    public class MouseScrollTriggerEvaluator : ITriggerEvaluator
    {
        /// <summary>Направление: true — вверх, false — вниз.</summary>
        public bool ScrollUp { get; }

        /// <summary>Порог срабатывания (число щелчков в окне).</summary>
        public int RequiredSteps { get; }

        public MouseScrollTriggerEvaluator(bool scrollUp, int requiredSteps)
        {
            ScrollUp = scrollUp;
            RequiredSteps = requiredSteps > 0 ? requiredSteps : 1;
        }

        public bool Evaluate(FrameBuffer buffer)
        {
            // Таймаут бездействия между щелчками берётся из конфига
            // (AppConfigService.Instance.WhellTrigerReadIntervalMs) внутри
            // RawInputService.ConsumeWheelTicks. Захардкоженных миллисекунд здесь нет.
            return RawInputService.ConsumeWheelTicks(ScrollUp, RequiredSteps);
        }
    }
}
