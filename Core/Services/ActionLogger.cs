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

    public static void LogKey(string keyName, byte byteCode, bool isDown, string? comboName = null)
    {
        string hex = byteCode != 0 ? $" [0x{byteCode:X2}]" : "";

        string prefix = "";
        if (!string.IsNullOrEmpty(comboName) && !ModifierKeys.Contains(keyName))
        {
            prefix = $"[{comboName}] ";
        }

        Log($"{prefix}Key {(isDown ? "DOWN" : "UP")}: {keyName}{hex}");
    }
}
