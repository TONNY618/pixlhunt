using System;
using System.IO;
using System.Text.Json;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Engine
{
    /// <summary>
    /// [Подсистема движка: Уровень 3 - Сервис]
    /// Статический менеджер глобального конфига. Ленивая загрузка из Config/appsettings.cfg
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
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "appsettings.cfg");
                if (!File.Exists(path))
                {
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Config", "appsettings.cfg");
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
