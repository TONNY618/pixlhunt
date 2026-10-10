using System.Runtime.InteropServices;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers;

/// <summary>
/// [Подсистема движка: Уровень 2 - Триггер]
/// Проверяет физическое состояние клавиши через Win32 API GetAsyncKeyState.
/// Использует ArduinoKeyMap.WindowsVkMap для перевода имени клавиши в VK-код.
/// </summary>
public class KeyTriggerEvaluator : ITriggerEvaluator
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    public string KeyName { get; }
    public int VirtualKey { get; }

    public KeyTriggerEvaluator(string keyName)
    {
        KeyName = keyName ?? string.Empty;
        VirtualKey = ArduinoKeyMap.GetWindowsVk(KeyName);
    }

    public bool Evaluate(FrameBuffer buffer)
    {
        if (VirtualKey == 0) return false;
        // Проверяем старший бит Win32 API — нажата ли физическая клавиша прямо сейчас
        return (GetAsyncKeyState(VirtualKey) & 0x8000) != 0;
    }
}
