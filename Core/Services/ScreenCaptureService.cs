//Сервис берет на себя запуск фонового цикла
//захвата видео буфера с экрана и предоставляет доступ к пикселям через общий буфер
//

using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Capture;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Services;

public static class ScreenCaptureService
{
    private static readonly DxgiScreenCapture _capturer = new();
    private static readonly FrameBuffer _buffer = new();
    private static CancellationTokenSource? _cts;
    private static Task? _loopTask;
    private static readonly object _syncLock = new();

    public static bool IsRunning { get; private set; }
    public static int TargetFps { get; set; } = 30; // Настраиваемая частота

    public static void Start(int monitorIndex = 0)
    {
        if (IsRunning) return;

        try
        {
            _capturer.Initialize(monitorIndex);
            _cts = new CancellationTokenSource();
            IsRunning = true;

            // Запускаем независимый фоновый поток обновления буфера
            _loopTask = Task.Run(() => CaptureLoopAsync(_cts.Token));
        }
        catch
        {
            // Ошибка инициализации видеокарты/дублирования не должна ронять приложение.
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
            _loopTask = null;
        }
    }

    private static async Task CaptureLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Пытаемся взять кадр (таймаут 5 мс)
                if (_capturer.TryCaptureFrame(timeoutMs: 5))
                {
                    lock (_syncLock)
                    {
                        _buffer.Update(_capturer.Buffer, _capturer.Width, _capturer.Height, _capturer.Stride);
                    }
                }

                // Динамическая пауза под нужный FPS (при 30 FPS = ~33 мс)
                int delay = 1000 / Math.Max(1, TargetFps);
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Чистый выход по отмене.
                break;
            }
            catch
            {
                // Не роняем фоновый поток: короткая пауза и продолжаем.
                try
                {
                    await Task.Delay(100, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    public static void Stop()
    {
        if (!IsRunning) return;

        try
        {
            _cts?.Cancel();
            _loopTask?.Wait(500);
        }
        catch (AggregateException)
        {
            // Подавляем исключения отмены/ошибок фоновой задачи при закрытии.
        }
        catch (TaskCanceledException)
        {
            // Подавляем отмену задачи при закрытии.
        }
        catch
        {
            // Любые прочие ошибки при остановке не должны ронять приложение.
        }
        finally
        {
            try
            {
                _capturer.Dispose();
            }
            catch
            {
                // Игнорируем ошибки освобождения COM-ресурсов.
            }

            _cts?.Dispose();
            _cts = null;
            _loopTask = null;
            IsRunning = false;
        }
    }

    /// <summary>Мгновенное чтение пикселя из общего буфера для любых окон и модулей</summary>
    public static Color GetPixel(int x, int y)
    {
        lock (_syncLock)
        {
            return _buffer.GetPixelColor(x, y);
        }
    }

    public static FrameBuffer CurrentBuffer => _buffer;
}
