using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Engine;

public class Orchestrator
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    /// <summary>Коллекция активных комбо, управляемых оркестратором.</summary>
    public List<ActiveCombo> Combos { get; } = new();

    private readonly SimpleTaskQueue _queue;
    private readonly InputDispatcher _dispatcher;

    // Часы для расчёта DeltaTime между кадрами (независимо от частоты вызовов).
    private readonly Stopwatch _frameClock = Stopwatch.StartNew();
    private TimeSpan _lastFrameTime = TimeSpan.Zero;

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
        // === Расчёт DeltaTime между кадрами (реальное время, не тики) ===
        var now = _frameClock.Elapsed;
        double deltaSeconds = (now - _lastFrameTime).TotalSeconds;
        _lastFrameTime = now;

        // Сортируем комбо по убыванию приоритета
        var ordered = Combos.OrderByDescending(c => c.Priority).ToList();

        // Флаги для маппинга физиологического состояния кадра.
        bool anyComboRan = false;
        bool wasdHeld = IsWasdPressed();

        foreach (var combo in ordered)
        {
            // Если комбо уже выполняет свои шаги — пропускаем его на этом кадре
            if (combo.IsRunning)
                continue;

            if (!combo.IsEnabled || !combo.CanExecute())
                continue;

            // Если включён PauseIfWasd и игрок двигается — пропускаем комбо.
            // Состояние (Navigation) будет выставлено после цикла.
            if (combo.PauseIfWasd && wasdHeld)
            {
                continue;
            }

            // Проверяем триггеры с учётом логики И/ИЛИ
            bool triggersPassed;
            if (combo.Triggers.Count == 0)
            {
                triggersPassed = true;
            }
            else if (combo.IsOrTriggerLogic)
            {
                triggersPassed = combo.Triggers.Any(t => t.Evaluate(frame));
            }
            else
            {
                triggersPassed = combo.Triggers.All(t => t.Evaluate(frame));
            }

            if (!triggersPassed)
                continue;

            combo.IsRunning = true;
            anyComboRan = true;
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

                try
                {
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
                catch (OperationCanceledException)
                {
                    // Прервано (таймаут или WASD)
                }
            }
            finally
            {
                combo.IsRunning = false;
            }
        }

        // === Маппинг действий оркестратора на физиологические состояния ===
        // Action     — выполнялось боевое комбо (максимальная нагрузка).
        // Navigation — удерживаются WASD без боя (статическое напряжение).
        // TrueIdle   — нет ни боя, ни движения (активное восстановление).
        BotStateController.PhysiologicalState state;
        if (anyComboRan)
            state = BotStateController.PhysiologicalState.Action;
        else if (wasdHeld)
            state = BotStateController.PhysiologicalState.Navigation;
        else
            state = BotStateController.PhysiologicalState.TrueIdle;

        // Передаём реальный DeltaTime — усталость не зависит от частоты кадров.
        BotStateController.Update(state, deltaSeconds);
    }
}
