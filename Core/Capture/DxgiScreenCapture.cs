using System;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace PixelMacroEngine.Core.Capture;

/// <summary>
/// Захват кадра экрана через DXGI Desktop Duplication API.
/// Класс изолирован: вся работа с D3D11/DXGI инкапсулирована здесь.
/// Буфер кадра переиспользуется между вызовами, чтобы минимизировать нагрузку на GC.
/// </summary>
public sealed class DxgiScreenCapture : IDisposable
{
    private ID3D11Device? _device;
    private ID3D11DeviceContext? _context;
    private IDXGIOutputDuplication? _duplication;
    private ID3D11Texture2D? _stagingTexture;

    private int _width;
    private int _height;
    private int _stride;
    private int _outputIndex;

    // Переиспользуемый CPU-буфер с сырыми BGRA-данными.
    private byte[] _buffer = Array.Empty<byte>();

    private bool _disposed;

    public int Width => _width;
    public int Height => _height;
    public int Stride => _stride;

    /// <summary>Сырые BGRA-данные последнего захваченного кадра.</summary>
    public ReadOnlySpan<byte> Buffer => _buffer;

    /// <summary>
    /// Инициализирует захват для указанного монитора (0 — основной).
    /// </summary>
    public void Initialize(int outputIndex = 0)
    {
        ThrowIfDisposed();

        _outputIndex = outputIndex;

        // Освобождаем старые ресурсы, если это повторная инициализация
        _stagingTexture?.Dispose();
        _duplication?.Dispose();
        _context?.Dispose();
        _device?.Dispose();

        // Создаём D3D11 устройство.
        D3D11.D3D11CreateDevice(
            null,
            DriverType.Hardware,
            DeviceCreationFlags.BgraSupport,
            null,
            out _device,
            out _context).CheckError();

        using var dxgiDevice = _device!.QueryInterface<IDXGIDevice>();
        using var adapter = dxgiDevice.GetAdapter();
        adapter.EnumOutputs((uint)outputIndex, out var output).CheckError();
        using var _ = output;
        using var output1 = output.QueryInterface<IDXGIOutput1>();

        var desc = output.Description;
        _width = desc.DesktopCoordinates.Right - desc.DesktopCoordinates.Left;
        _height = desc.DesktopCoordinates.Bottom - desc.DesktopCoordinates.Top;

        _duplication = output1.DuplicateOutput(_device);

        // Staging-текстура для копирования GPU -> CPU.
        var texDesc = new Texture2DDescription
        {
            Width = (uint)_width,
            Height = (uint)_height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Staging,
            BindFlags = BindFlags.None,
            CPUAccessFlags = CpuAccessFlags.Read,
            MiscFlags = ResourceOptionFlags.None
        };
        _stagingTexture = _device.CreateTexture2D(texDesc);

        _stride = _width * 4;
        _buffer = new byte[_stride * _height];
    }

    /// <summary>
    /// Захватывает очередной кадр. Возвращает true, если кадр был получен.
    /// При отсутствии изменений возвращает false (буфер остаётся прежним).
    /// </summary>
    public bool TryCaptureFrame(int timeoutMs = 0)
    {
        ThrowIfDisposed();
        if (_duplication is null || _stagingTexture is null || _context is null)
            throw new InvalidOperationException("Захват не инициализирован. Вызовите Initialize().");

        var result = _duplication.AcquireNextFrame((uint)timeoutMs, out _, out var desktopResource);
        if (result == Vortice.DXGI.ResultCode.WaitTimeout)
            return false;

        // При потере доступа (например, смена разрешения, переключение пользователя,
        // переход в полноэкранный режим, блокировка рабочего стола) пересоздаём
        // все ресурсы и пропускаем кадр, не выбрасывая исключение наружу.
        bool accessLost = result == Vortice.DXGI.ResultCode.AccessLost
                          || result.Code == unchecked((int)0x80070005); // E_ACCESSDENIED

        if (accessLost)
        {
            try
            {
                Initialize(_outputIndex);
            }
            catch
            {
                // Рабочий стол может быть временно заблокирован (UAC, lock screen).
                // Игнорируем — попробуем восстановиться на следующем кадре.
            }
            return false;
        }

        result.CheckError();
    
        try
        {
            // если ресурс null, мы выйдем, но блок finally все равно вызовет ReleaseFrame()!
            if (desktopResource is null)
                return false;

            using (desktopResource)
            using (var frameTexture = desktopResource.QueryInterface<ID3D11Texture2D>())
            {
                _context.CopyResource(_stagingTexture, frameTexture);

                var mapped = _context.Map(_stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
                try
                {
                    // Копируем построчно, т.к. RowPitch может быть больше ширины*4.
                    unsafe
                    {
                        byte* src = (byte*)mapped.DataPointer;
                        fixed (byte* dst = _buffer)
                        {
                            for (int y = 0; y < _height; y++)
                            {
                                System.Buffer.MemoryCopy(
                                    src + y * mapped.RowPitch,
                                    dst + y * _stride,
                                    _stride,
                                    _stride);
                            }
                        }
                    }
                }
                finally
                {
                    _context.Unmap(_stagingTexture, 0);
                }
            }
        }
        finally
        {
            _duplication.ReleaseFrame();
        }

        return true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DxgiScreenCapture));
    }

    ~DxgiScreenCapture() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        // COM-объекты — неуправляемые ресурсы, освобождаем их в любом случае.
        // Исключения не бросаем, чтобы не сломать финализатор.
        try
        {
            _stagingTexture?.Dispose();
            _duplication?.Dispose();
            _context?.Dispose();
            _device?.Dispose();
        }
        catch
        {
            // Намеренно игнорируем ошибки освобождения COM-объектов.
        }
        finally
        {
            _stagingTexture = null;
            _duplication = null;
            _context = null;
            _device = null;
        }
    }
}
