using System;
using System.Collections.Generic;

namespace PixelMacroEngine.Core.Services;

public static class ActionLogger
{
    public static event Action<string>? OnLog;

    /// <summary>Клавиши-модификаторы, для которых не выводим имя комбо (чтобы не шуметь в логе).</summary>
    private static readonly HashSet<string> ModifierKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "LShift", "RShift", "Shift",
        "LCtrl", "RCtrl", "Ctrl", "Control",
        "LAlt", "RAlt", "Alt",
        "LWin", "RWin", "Win"
    };

    public static void Log(string text)
    {
        string line = $"[{DateTime.Now:HH:mm:ss}] {text}";
        OnLog?.Invoke(line);
    }

    public static void LogKey(string keyName, byte byteCode, bool isDown, string? comboName = null, int? holdMs = null)
    {
        string hex = byteCode != 0 ? $" [0x{byteCode:X2}]" : "";

        string prefix = "";
        if (!string.IsNullOrEmpty(comboName) && !ModifierKeys.Contains(keyName))
        {
            prefix = $"[{comboName}] ";
        }

        // Длительность удержания показываем только для UP (завершение клика)
        string hold = (!isDown && holdMs.HasValue) ? $" ({holdMs.Value} ms)" : "";

        Log($"{prefix}Key {(isDown ? "DOWN" : "UP")}: {keyName}{hex}{hold}");
    }

    /// <summary>Логирует действие мыши (клик, скролл).</summary>
    public static void LogMouse(string text, string? comboName = null)
    {
        string prefix = string.IsNullOrEmpty(comboName) ? "" : $"[{comboName}] ";
        Log($"{prefix}Mouse: {text}");
    }

    /// <summary>Логирует перемещение курсора к целевой точке.</summary>
    public static void LogMouse(string text, int targetX, int targetY)
    {
        Log($"Mouse: {text} [{targetX}, {targetY}]");
    }
}
