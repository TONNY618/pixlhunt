using System;

namespace PixelMacroEngine.Core.Input;

public class PacketBatch
{
    // Буфер 65 байт (0-й байт всегда Report ID 0x00 для Windows, 1..64 - полезная нагрузка)
    private readonly byte[] _buffer = new byte[65];
    private int _offset = 1;

    public bool TryAdd(byte cmdType, byte p1 = 0, byte p2 = 0, byte p3 = 0)
    {
        if (_offset + 4 > 65) return false;

        _buffer[_offset++] = cmdType;
        _buffer[_offset++] = p1;
        _buffer[_offset++] = p2;
        _buffer[_offset++] = p3;
        return true;
    }

    public bool AddKey(byte keyCode, bool isDown) => TryAdd(0x03, keyCode, (byte)(isDown ? 1 : 0));
    public bool AddMouseButton(byte button, bool isDown) => TryAdd(0x02, button, (byte)(isDown ? 1 : 0));
    public bool AddMouseMove(sbyte dx, sbyte dy, sbyte wheel = 0) => TryAdd(0x01, (byte)dx, (byte)dy, (byte)wheel);
    public bool AddEmergencyReset() => TryAdd(0x04);
    public bool AddBootloaderJump() => TryAdd(0xFF);

    public byte[] Build() => _buffer;
}
