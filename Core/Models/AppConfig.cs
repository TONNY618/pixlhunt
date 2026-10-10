namespace PixelMacroEngine.Core.Models
{
    /// <summary>
    /// [Подсистема движка: Уровень 2 - Модель]
    /// Глобальные настройки приложения, загружаемые из Config/appsettings.cfg.
    /// </summary>
    public class AppConfig
    {
        /// <summary>Кулдаун по умолчанию для мгновенных комбо (HP), мс.</summary>
        public int InstantComboDefaultCooldownMs { get; set; } = 1500;

        /// <summary>Кулдаун по умолчанию для общих комбо (Skill), мс.</summary>
        public int GlobalCooldownDefaultMs { get; set; } = 1100;

        /// <summary>Интервал захвата кадров, мс.</summary>
        public int FrameCaptureIntervalMs { get; set; } = 33;

        /// <summary>Минимальная задержка "человеческой" реакции, мс.</summary>
        public int HumanLatencyMinMs { get; set; } = 40;

        /// <summary>Максимальная задержка "человеческой" реакции, мс.</summary>
        public int HumanLatencyMaxMs { get; set; } = 120;
    }
}
