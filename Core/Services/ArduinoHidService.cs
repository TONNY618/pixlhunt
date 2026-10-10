using System;
using System.Linq;
using HidSharp;
using PixelMacroEngine.Core.Input;

namespace PixelMacroEngine.Core.Services;

public static class ArduinoHidService
{
    private const int VendorId = 0x04F2;
    private const int ProductId = 0x0833;

    private static HidDevice? _device;
    private static HidStream? _stream;
    private static readonly object _lock = new();

    public static bool IsConnected => _stream != null;

    public static bool Connect()
    {
        lock (_lock)
        {
            if (IsConnected) return true;

            try
            {
                _device = DeviceList.Local.GetHidDevices(VendorId, ProductId).FirstOrDefault();
                if (_device != null && _device.TryOpen(out _stream))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HID Connect error: {ex.Message}");
            }
            return false;
        }
    }

    public static void Disconnect()
    {
        lock (_lock)
        {
            _stream?.Dispose();
            _stream = null;
            _device = null;
        }
    }

    public static bool Send(PacketBatch batch)
    {
        lock (_lock)
        {
            if (!IsConnected) return false;

            try
            {
                _stream!.Write(batch.Build());
                return true;
            }
            catch
            {
                Disconnect(); // Потеряли связь с платой
                return false;
            }
        }
    }

    public static void EmergencyReset()
    {
        if (!IsConnected) return;
        var batch = new PacketBatch();
        batch.AddEmergencyReset();
        Send(batch);
    }
}
