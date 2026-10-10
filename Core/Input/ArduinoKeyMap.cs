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
}
