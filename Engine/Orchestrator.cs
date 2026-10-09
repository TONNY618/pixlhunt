using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Engine;

public class Orchestrator
{
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
            if (!combo.IsEnabled || !combo.CanExecute())
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

            // Обновляем время последнего выполнения
            combo.LastExecuted = DateTime.UtcNow;

            // Последовательно выполняем шаги комбо
            var context = new TriggerExecutionContext
            {
                Frame = frame
            };

            foreach (var step in combo.Steps)
            {
                ct.ThrowIfCancellationRequested();
                await step.ExecuteAsync(context, ct).ConfigureAwait(false);
            }
        }
    }
}
