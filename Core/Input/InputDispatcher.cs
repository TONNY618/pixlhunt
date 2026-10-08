using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Input;

public class InputDispatcher
{
    private readonly SimpleTaskQueue _queue;
    private readonly HashSet<ushort> _pressedKeys = new(); // Для гарантии отпускания
    private CancellationTokenSource _cts = new();
    private bool _isPaused = false;

    public InputDispatcher(SimpleTaskQueue queue)
    {
        _queue = queue;
        Task.Run(ProcessingLoopAsync);
    }

    public void SetPause(bool pause)
    {
        _isPaused = pause;
        if (_isPaused)
        {
            _queue.Clear();
            ReleaseAllHeldKeys();
        }
    }

    /// <summary>
    /// Экстренный сброс очереди и клавиш при прерывании другим триггером
    /// </summary>
    public void EmergencyClear()
    {
        _queue.Clear();
        ReleaseAllHeldKeys();
    }

    private async Task ProcessingLoopAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            if (_isPaused)
            {
                await Task.Delay(50);
                continue;
            }

            var batch = _queue.Dequeue();
            if (batch == null)
            {
                await Task.Delay(5); // Спим, пока очередь пуста
                continue;
            }

            // Выполняем весь батч строго последовательно и непрерывно
            foreach (var evt in batch.Events)
            {
                switch (evt.Type)
                {
                    case InputEventType.KeyDown:
                        NativeInputStub.KeyDown(evt.VirtualKeyCode);
                        _pressedKeys.Add(evt.VirtualKeyCode);
                        break;

                    case InputEventType.KeyUp:
                        NativeInputStub.KeyUp(evt.VirtualKeyCode);
                        _pressedKeys.Remove(evt.VirtualKeyCode);
                        break;

                    case InputEventType.DelayMs:
                        // Физическая пауза удержания/задержки внутри батча
                        await Task.Delay(evt.DelayValueMs);
                        break;
                }
            }
        }
    }

    private void ReleaseAllHeldKeys()
    {
        foreach (var vk in _pressedKeys)
        {
            NativeInputStub.KeyUp(vk);
        }
        _pressedKeys.Clear();
    }
}

// Временная заглушка для P/Invoke NativeInput (заменим на SendInput)
internal static class NativeInputStub
{
    public static void KeyDown(ushort vk) { /* P/Invoke SendInput KeyDown */ }
    public static void KeyUp(ushort vk) { /* P/Invoke SendInput KeyUp */ }
}