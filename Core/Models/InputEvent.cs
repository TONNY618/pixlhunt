namespace PixelMacroEngine.Core.Models;

public enum InputEventType
{
    KeyDown,
    KeyUp,
    DelayMs
}

public class InputEvent
{
    public InputEventType Type { get; set; }
    public ushort VirtualKeyCode { get; set; } // Например, VK_SHIFT, VK_F
    public int DelayValueMs { get; set; }      // Если Type == DelayMs

    public static InputEvent Down(ushort vk) => new() { Type = InputEventType.KeyDown, VirtualKeyCode = vk };
    public static InputEvent Up(ushort vk) => new() { Type = InputEventType.KeyUp, VirtualKeyCode = vk };
    public static InputEvent Wait(int ms) => new() { Type = InputEventType.DelayMs, DelayValueMs = ms };
}