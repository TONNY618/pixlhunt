using System;
using System.IO;
using System.Text.Json;

namespace PixelMacroEngine.Core.Models
{
    /// <summary>
    /// [Подсистема движка: Уровень 2 - Модель]
    /// Глобальные настройки приложения, загружаемые из Config/appsettings.json.
    /// </summary>
    public class AppConfig
    {
        /// <summary>Кулдаун по умолчанию для мгновенных комбо (HP), мс.</summary>
        public int InstantComboDefaultCooldownMs { get; set; } = 1500;

        /// <summary>Кулдаун по умолчанию для общих комбо (Skill), мс.</summary>
        public int GlobalCooldownDefaultMs { get; set; } = 1500;

        /// <summary>Интервал захвата кадров, мс.</summary>
        public int FrameCaptureIntervalMs { get; set; } = 33;

        /// <summary>Минимальная задержка "человеческой" реакции, мс.</summary>
        public int HumanLatencyMinMs { get; set; } = 40;

        /// <summary>Максимальная задержка "человеческой" реакции, мс.</summary>
        public int HumanLatencyMaxMs { get; set; } = 120;
    }

    /// <summary>
    /// [Подсистема движка: Уровень 3 - Сервис]
    /// Статический менеджер глобального конфига. Ленивая загрузка из Config/appsettings.json
    /// (сначала рядом с exe, затем в исходниках). При отсутствии файла — дефолтные значения.
    /// </summary>
    public static class AppConfigManager
    {
        private static AppConfig? _config;

        /// <summary>Текущий конфиг (загружается лениво при первом обращении).</summary>
        public static AppConfig Config
        {
            get
            {
                if (_config == null) Load();
                return _config!;
            }
        }

        /// <summary>Перезагружает конфиг с диска.</summary>
        public static void Load()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "appsettings.json");
                if (!File.Exists(path))
                {
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Config", "appsettings.json");
                }

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    _config = JsonSerializer.Deserialize<AppConfig>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AppConfig();
                }
                else
                {
                    _config = new AppConfig();
                }
            }
            catch
            {
                _config = new AppConfig();
            }
        }
    }
}
