using System;
using System.Linq;
using HidSharp;
using PixelMacroEngine.Core.Input;

namespace PixelMacroEngine.Core.Services;

public static class ArduinoHidService
{
    private static int VendorId => AppConfigService.Instance.Usb_hwKeyClicker.GetVendorId();
    private static int ProductId => AppConfigService.Instance.Usb_hwKeyClicker.GetProductId();

    private static HidDevice? _device;
    private static HidStream? _stream;
    private static readonly object _lock = new();

    public static bool IsConnected => _stream != null;

    /// <summary>
    /// Быстрая проверка физического присутствия устройства на шине USB.
    /// Не открывает поток — только перечисляет HID-устройства.
    /// </summary>
    public static bool CheckDevicePresence()
    {
        try
        {
            return DeviceList.Local.GetHidDevices(VendorId, ProductId)
                .Any(d => d.MaxOutputReportLength >= 64);
        }
        catch
        {
            return false;
        }
    }

    public static bool Connect()
    {
        lock (_lock)
        {
            if (IsConnected) return true;

            try
            {
                var devices = DeviceList.Local.GetHidDevices(VendorId, ProductId).ToList();
                foreach (var dev in devices)
                {
                    // Ищем строго RawHID интерфейс с буфером >= 64 байт
                    if (dev.MaxOutputReportLength >= 64)
                    {
                        if (dev.TryOpen(out _stream))
                        {
                            _device = dev;
                            return true;
                        }
                    }
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

            byte[] data = batch.Build();
            try
            {
                _stream!.Write(data);
                return true;
            }
            catch
            {
                // Защита от transient-ошибок (буфер HID-драйвера Windows переполнен
                // из-за слишком быстрой отправки back-to-back пакетов).
                // Ждем 2мс (2 цикла USB-опроса) и пробуем еще раз.
                System.Threading.Thread.Sleep(2);
                try
                {
                    _stream!.Write(data);
                    return true;
                }
                catch
                {
                    Disconnect(); // Реально потеряли физическую связь с платой
                    return false;
                }
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
