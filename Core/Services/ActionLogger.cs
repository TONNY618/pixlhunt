using System;

namespace PixelMacroEngine.Core.Services;

public static class ActionLogger
{
    public static event Action<string>? OnLog;

    public static void Log(string text)
    {
        string line = $"[{DateTime.Now:HH:mm:ss}] {text}";
        OnLog?.Invoke(line);
    }

    public static void LogKey(string keyName, byte byteCode, bool isDown)
    {
        string hex = byteCode != 0 ? $" [0x{byteCode:X2}]" : "";
        Log($"Key {(isDown ? "DOWN" : "UP")}: {keyName}{hex}");
    }
}
