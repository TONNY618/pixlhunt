using System;
using System.IO;
using System.Text.Json;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// Модель настроек приложения, читаемая из Config/appsettings.cfg.
/// </summary>
public class AppConfig
{
    public string PxlhuntNameMainForm { get; set; } = "pxlHunt";
    public int InstantComboDefaultCooldownMs { get; set; } = 1500;
    public int GlobalCooldownDefaultMs { get; set; } = 1100;
    public int FrameCaptureIntervalMs { get; set; } = 33;
    public int HumanLatencyMinMs { get; set; } = 40;
    public int HumanLatencyMaxMs { get; set; } = 120;
    public int DisplayNumberForCapteruVideoFrame { get; set; } = 0;
}

/// <summary>
/// Централизованный сервис настроек. Загружается один раз при старте приложения
/// и предоставляет доступ к конфигу через статическое свойство Instance.
/// </summary>
public static class AppConfigService
{
    public static AppConfig Instance { get; private set; } = new();

    /// <summary>
    /// Загружает Config/appsettings.cfg. Если файл не найден или повреждён —
    /// остаются значения по умолчанию.
    /// </summary>
    public static void Load()
    {
        try
        {
            string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "appsettings.cfg");
            if (!File.Exists(cfgPath))
            {
                cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Config", "appsettings.cfg");
            }

            if (File.Exists(cfgPath))
            {
                string json = File.ReadAllText(cfgPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loaded = JsonSerializer.Deserialize<AppConfig>(json, options);
                if (loaded != null)
                {
                    Instance = loaded;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppConfigService] Ошибка загрузки конфига: {ex.Message}");
        }
    }
}
