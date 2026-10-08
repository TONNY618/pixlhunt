using System.Collections.Generic;

namespace PixelMacroEngine.Core.Models;

/// <summary>
/// Непрерываемый пакет микро-действий ввода. Диспетчер выполняет его монолитно от начала до конца.
/// </summary>
public class InputBatchCmd
{
    public string Description { get; set; } = string.Empty;
    public List<InputEvent> Events { get; set; } = new();

    public void AddKeyDown(ushort vk) => Events.Add(InputEvent.Down(vk));
    public void AddKeyUp(ushort vk) => Events.Add(InputEvent.Up(vk));
    public void AddDelay(int ms) => Events.Add(InputEvent.Wait(ms));
}