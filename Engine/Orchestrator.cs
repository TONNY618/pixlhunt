using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Engine;

public class Orchestrator
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    /// <summary>Коллекция активных комбо, управляемых оркестратором.</summary>
    public List<ActiveCombo> Combos { get; } = new();

    private readonly SimpleTaskQueue _queue;
    private readonly InputDispatcher _dispatcher;

    public Orchestrator(SimpleTaskQueue queue, InputDispatcher dispatcher)
    {
        _queue = queue;
        _dispatcher = dispatcher;
    }

    /// <summary>Регистрирует комбо в оркестраторе.</summary>
    public void RegisterCombo(ActiveCombo combo) => Combos.Add(combo);

    /// <summary>Проверяет, нажата ли хотя бы одна из клавиш движения W, A, S, D.</summary>
    private static bool IsWasdPressed()
    {
        const int VK_W = 0x57;
        const int VK_A = 0x41;
        const int VK_S = 0x53;
        const int VK_D = 0x44;

        return (GetAsyncKeyState(VK_W) & 0x8000) != 0
            || (GetAsyncKeyState(VK_A) & 0x8000) != 0
            || (GetAsyncKeyState(VK_S) & 0x8000) != 0
            || (GetAsyncKeyState(VK_D) & 0x8000) != 0;
    }

    /// <summary>Сбрасывает состояние кулдауна у всех комбо.</summary>
    public void ResetAllStates()
    {
        foreach (var combo in Combos)
        {
            combo.LastExecuted = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Обрабатывает один кадр: сортирует комбо по приоритету, проверяет кулдаун и триггеры,
    /// и при срабатывании последовательно выполняет шаги комбо.
    /// </summary>
    public async Task ProcessFrameAsync(FrameBuffer frame, CancellationToken ct = default)
    {
        // Сортируем комбо по убыванию приоритета
        var ordered = Combos.OrderByDescending(c => c.Priority).ToList();

        foreach (var combo in ordered)
        {
            // Если комбо уже выполняет свои шаги — пропускаем его на этом кадре
            if (combo.IsRunning)
                continue;

            if (!combo.IsEnabled || !combo.CanExecute())
                continue;

            // Если включён PauseIfWasd и игрок двигается — пропускаем комбо
            if (combo.PauseIfWasd && IsWasdPressed())
                continue;

            // Проверяем все триггеры: должны вернуть true
            bool allTriggersPassed = true;
            foreach (var trigger in combo.Triggers)
            {
                if (!trigger.Evaluate(frame))
                {
                    allTriggersPassed = false;
                    break;
                }
            }

            if (!allTriggersPassed)
                continue;

            combo.IsRunning = true;
            try
            {
                // Обновляем время последнего выполнения
                combo.LastExecuted = DateTime.UtcNow;

                // Последовательно выполняем шаги комбо
                var context = new TriggerExecutionContext
                {
                    Frame = frame,
                    ComboName = combo.Name
                };

                foreach (var step in combo.Steps)
                {
                    // Ожидаем, пока игрок отпустит клавиши движения (если включён PauseIfWasd)
                    while (combo.PauseIfWasd && IsWasdPressed())
                    {
                        ct.ThrowIfCancellationRequested();
                        await Task.Delay(25, ct).ConfigureAwait(false);
                    }

                    ct.ThrowIfCancellationRequested();
                    await step.ExecuteAsync(context, ct).ConfigureAwait(false);
                }
            }
            finally
            {
                combo.IsRunning = false;
            }
        }
    }
}
