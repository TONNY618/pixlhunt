using System;
using System.Collections.Generic;

namespace PixelMacroEngine.Core.Input;

public static class ArduinoKeyMap
{
    public static readonly Dictionary<string, byte> KeyMap = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase)
    {
        // Буквы (ASCII)
        ["A"] = (byte)'a', ["B"] = (byte)'b', ["C"] = (byte)'c', ["D"] = (byte)'d',
        ["E"] = (byte)'e', ["F"] = (byte)'f', ["G"] = (byte)'g', ["H"] = (byte)'h',
        ["I"] = (byte)'i', ["J"] = (byte)'j', ["K"] = (byte)'k', ["L"] = (byte)'l',
        ["M"] = (byte)'m', ["N"] = (byte)'n', ["O"] = (byte)'o', ["P"] = (byte)'p',
        ["Q"] = (byte)'q', ["R"] = (byte)'r', ["S"] = (byte)'s', ["T"] = (byte)'t',
        ["U"] = (byte)'u', ["V"] = (byte)'v', ["W"] = (byte)'w', ["X"] = (byte)'x',
        ["Y"] = (byte)'y', ["Z"] = (byte)'z',

        // Цифры верхнего ряда
        ["0"] = (byte)'0', ["1"] = (byte)'1', ["2"] = (byte)'2', ["3"] = (byte)'3',
        ["4"] = (byte)'4', ["5"] = (byte)'5', ["6"] = (byte)'6', ["7"] = (byte)'7',
        ["8"] = (byte)'8', ["9"] = (byte)'9',

        // F-клавиши
        ["F1"] = 0xC2, ["F2"] = 0xC3, ["F3"] = 0xC4, ["F4"] = 0xC5,
        ["F5"] = 0xC6, ["F6"] = 0xC7, ["F7"] = 0xC8, ["F8"] = 0xC9,
        ["F9"] = 0xCA, ["F10"] = 0xCB, ["F11"] = 0xCC, ["F12"] = 0xCD,

        // Numpad (Цифровой блок)
        ["NumPad 0"] = 0xEA, ["NumPad 1"] = 0xE1, ["NumPad 2"] = 0xE2,
        ["NumPad 3"] = 0xE3, ["NumPad 4"] = 0xE4, ["NumPad 5"] = 0xE5,
        ["NumPad 6"] = 0xE6, ["NumPad 7"] = 0xE7, ["NumPad 8"] = 0xE8,
        ["NumPad 9"] = 0xE9,
        ["NumPad +"] = 0xDF, ["NumPad -"] = 0xDE, ["NumPad *"] = 0xDD,
        ["NumPad /"] = 0xDC, ["NumPad Enter"] = 0xE0, ["NumPad ."] = 0xEB,
        ["NumLock"]  = 0xDB,

        // Модификаторы
        ["Left Shift"]  = 0x81, ["Right Shift"] = 0x85,
        ["Left Ctrl"]   = 0x80, ["Right Ctrl"]  = 0x84,
        ["Left Alt"]    = 0x82, ["Right Alt"]   = 0x86,
        ["Left Win"]    = 0x83, ["Right Win"]   = 0x87,

        // Служебные и навигация
        ["Space"]       = 0x20,
        ["Enter"]       = 0xB0,
        ["Tab"]         = 0xB3,
        ["Escape"]      = 0xB1,
        ["Backspace"]   = 0xB2,
        ["Insert"]      = 0xD1,
        ["Delete"]      = 0xD4,
        ["Home"]        = 0xD2,
        ["End"]         = 0xD5,
        ["Page Up"]     = 0xD3,
        ["Page Down"]   = 0xD6,
        ["Up Arrow"]    = 0xDA,
        ["Down Arrow"]  = 0xD9,
        ["Left Arrow"]  = 0xD8,
        ["Right Arrow"] = 0xD7,
        ["Print Screen"]= 0xCE,
        ["Scroll Lock"] = 0xCF,
        ["Pause"]       = 0xD0,
        ["Caps Lock"]   = 0xC1,

        // Знаки препинания / спецсимволы
        ["` (Тильда)"]  = (byte)'`',
        ["- (Минус)"]   = (byte)'-',
        ["= (Равно)"]   = (byte)'=',
        ["["]           = (byte)'[',
        ["]"]           = (byte)']',
        ["\\"]          = (byte)'\\',
        [";"]           = (byte)';',
        ["'"]           = (byte)'\'',
        [","]           = (byte)',',
        ["."]           = (byte)'.',
        ["/"]           = (byte)'/'
    };

    public static byte GetByte(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName)) return 0;
        return KeyMap.TryGetValue(keyName.Trim(), out byte code) ? code : (byte)0;
    }

    /// <summary>
    /// Системные коды Windows Virtual Keys (VK).
    /// Используются для проверки состояния клавиш через GetAsyncKeyState.
    /// </summary>
    public static readonly Dictionary<string, int> WindowsVkMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Буквы A-Z (0x41..0x5A)
        ["A"] = 0x41, ["B"] = 0x42, ["C"] = 0x43, ["D"] = 0x44, ["E"] = 0x45, ["F"] = 0x46,
        ["G"] = 0x47, ["H"] = 0x48, ["I"] = 0x49, ["J"] = 0x4A, ["K"] = 0x4B, ["L"] = 0x4C,
        ["M"] = 0x4D, ["N"] = 0x4E, ["O"] = 0x4F, ["P"] = 0x50, ["Q"] = 0x51, ["R"] = 0x52,
        ["S"] = 0x53, ["T"] = 0x54, ["U"] = 0x55, ["V"] = 0x56, ["W"] = 0x57, ["X"] = 0x58,
        ["Y"] = 0x59, ["Z"] = 0x5A,

        // Цифры верхнего ряда (0x30..0x39)
        ["0"] = 0x30, ["1"] = 0x31, ["2"] = 0x32, ["3"] = 0x33, ["4"] = 0x34,
        ["5"] = 0x35, ["6"] = 0x36, ["7"] = 0x37, ["8"] = 0x38, ["9"] = 0x39,

        // F-клавиши (0x70..0x7B)
        ["F1"] = 0x70, ["F2"] = 0x71, ["F3"] = 0x72, ["F4"] = 0x73, ["F5"] = 0x74, ["F6"] = 0x75,
        ["F7"] = 0x76, ["F8"] = 0x77, ["F9"] = 0x78, ["F10"] = 0x79, ["F11"] = 0x7A, ["F12"] = 0x7B,

        // Модификаторы
        ["Left Shift"] = 0xA0, ["Right Shift"] = 0xA1,
        ["Left Ctrl"] = 0xA2, ["Right Ctrl"] = 0xA3,
        ["Left Alt"] = 0xA4, ["Right Alt"] = 0xA5,
        ["Left Win"] = 0x5B, ["Right Win"] = 0x5C,

        // Служебные
        ["Space"] = 0x20, ["Enter"] = 0x0D, ["Tab"] = 0x09, ["Escape"] = 0x1B, ["Backspace"] = 0x08,
        ["Insert"] = 0x2D, ["Delete"] = 0x2E, ["Home"] = 0x24, ["End"] = 0x23,
        ["Page Up"] = 0x21, ["Page Down"] = 0x22,
        ["Up Arrow"] = 0x26, ["Down Arrow"] = 0x28, ["Left Arrow"] = 0x25, ["Right Arrow"] = 0x27,
        ["Print Screen"] = 0x2C, ["Scroll Lock"] = 0x91, ["Pause"] = 0x13, ["Caps Lock"] = 0x14, ["NumLock"] = 0x90,

        // Знаки
        ["` (Тильда)"] = 0xC0, ["- (Минус)"] = 0xBD, ["= (Равно)"] = 0xBB,
        ["["] = 0xDB, ["]"] = 0xDD, ["\\"] = 0xDC, [";"] = 0xBA, ["'"] = 0xDE,
        [","] = 0xBC, ["."] = 0xBE, ["/"] = 0xBF,

        // Numpad (0x60..0x69)
        ["NumPad 0"] = 0x60, ["NumPad 1"] = 0x61, ["NumPad 2"] = 0x62, ["NumPad 3"] = 0x63,
        ["NumPad 4"] = 0x64, ["NumPad 5"] = 0x65, ["NumPad 6"] = 0x66, ["NumPad 7"] = 0x67,
        ["NumPad 8"] = 0x68, ["NumPad 9"] = 0x69,
        ["NumPad +"] = 0x6B, ["NumPad -"] = 0x6D, ["NumPad *"] = 0x6A, ["NumPad /"] = 0x6F,
        ["NumPad Enter"] = 0x0D, ["NumPad ."] = 0x6E
    };

    /// <summary>
    /// Возвращает Windows Virtual Key (VK) по имени клавиши.
    /// Возвращает 0, если клавиша не найдена.
    /// </summary>
    public static int GetWindowsVk(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName)) return 0;
        return WindowsVkMap.TryGetValue(keyName.Trim(), out int vk) ? vk : 0;
    }
}
