using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Capture;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// [Подсистема видеозахвата: Уровень 3 - Фасад / Сервис]
/// Потокобезопасная публичная точка входа. Управляет фоновым циклом опроса экрана на заданном FPS.
/// Связан с: Объединяет DxgiScreenCapture и FrameBuffer. Предоставляет GetPixel(...) для форм и триггеров.
/// </summary>
/// 
public static class ScreenCaptureService
{
    private static readonly DxgiScreenCapture _capturer = new();
    private static readonly FrameBuffer _buffer = new();
    private static CancellationTokenSource? _cts;
    private static Task? _loopTask;
    private static readonly object _syncLock = new();

    public static bool IsRunning { get; private set; }
    public static int TargetFps { get; set; } = 30; // Настраиваемая частота

    /// <summary>Событие, вызываемое после обновления буфера кадра.</summary>
    public static event Func<FrameBuffer, Task>? OnFrameCaptured;

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
                // Захват кадра и обновление буфера держим под одним локом,
                // чтобы Stop() не вызвал Dispose прямо во время работы с _capturer.
                lock (_syncLock)
                {
                    if (_capturer.TryCaptureFrame(timeoutMs: 5))
                    {
                        _buffer.Update(_capturer.Buffer, _capturer.Width, _capturer.Height, _capturer.Stride);
                    }
                }

                // Уведомляем подписчиков о новом кадре (вне лока, чтобы не блокировать Stop()).
                var handler = OnFrameCaptured;
                if (handler != null)
                {
                    await handler.Invoke(_buffer).ConfigureAwait(false);
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
            // Освобождаем _capturer под локом, чтобы не пересечься с фоновым
            // потоком, который может находиться внутри TryCaptureFrame.
            lock (_syncLock)
            {
                try
                {
                    _capturer.Dispose();
                }
                catch
                {
                    // Игнорируем ошибки освобождения COM-ресурсов.
                }
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
