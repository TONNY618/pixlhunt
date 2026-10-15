using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Engine;

public class Orchestrator
{
    /// <summary>Коллекция активных комбо, управляемых оркестратором.</summary>
    public List<ActiveCombo> Combos { get; } = new();

    // Аппаратный таймстамп последнего обработанного кадра (для расчёта DeltaTime).
    // Через TimeService интервал не зависит от NTP-коррекции системных часов.
    private long _lastFrameTimestamp = 0;

    // Аппаратный таймстамп последнего выполнения боевого комбо, делящего общий кулдаун (ГКД).
    private long _globalLastExecutedTimestamp = 0;

    /// <summary>Регистрирует комбо в оркестраторе.</summary>
    public void RegisterCombo(ActiveCombo combo) => Combos.Add(combo);

    /// <summary>Проверяет, нажата ли хотя бы одна из клавиш движения W, A, S, D.</summary>
    private static bool IsWasdPressed()
    {
        return PixelMacroEngine.Core.Services.RawInputService.IsKeyDown(0x57) // W
            || PixelMacroEngine.Core.Services.RawInputService.IsKeyDown(0x41) // A
            || PixelMacroEngine.Core.Services.RawInputService.IsKeyDown(0x53) // S
            || PixelMacroEngine.Core.Services.RawInputService.IsKeyDown(0x44); // D
    }

    /// <summary>Сбрасывает состояние кулдауна у всех комбо.</summary>
    public void ResetAllStates()
    {
        foreach (var combo in Combos)
        {
            combo.LastExecutedTimestamp = 0;
        }
    }

    /// <summary>
    /// Обрабатывает один кадр: сортирует комбо по приоритету, проверяет кулдаун и триггеры,
    /// и при срабатывании последовательно выполняет шаги комбо.
    /// </summary>
    public async Task ProcessFrameAsync(FrameBuffer frame, CancellationToken ct = default)
    {
        // === Расчёт DeltaTime между кадрами (реальное время, не тики) ===
        long nowTs = TimeService.GetTimestamp();
        double deltaSeconds = _lastFrameTimestamp == 0
            ? 0.0
            : TimeService.GetElapsedMilliseconds(_lastFrameTimestamp, nowTs) / 1000.0;
        _lastFrameTimestamp = nowTs;

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

            // Если комбо делит ГКД и не является фоновым сервисным действием
            if (combo.IsGlobalCooldown && !combo.IsNonBlocking && combo.CooldownMs > 0)
            {
                if (_globalLastExecutedTimestamp != 0 &&
                    TimeService.GetElapsedMilliseconds(_globalLastExecutedTimestamp) < combo.CooldownMs)
                    continue;
            }

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
                // Комбо без триггеров не должно спамить на каждом кадре
                triggersPassed = false;
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
                long execTs = TimeService.GetTimestamp();
                combo.LastExecutedTimestamp = execTs;
                if (combo.IsGlobalCooldown && !combo.IsNonBlocking && combo.CooldownMs > 0)
                {
                    _globalLastExecutedTimestamp = execTs;
                }

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
                        if (step is INonBlockingStep)
                        {
                            // Фоновый шаг (звук/уведомление): запускаем без ожидания
                            // и мгновенно переходим к следующему шагу!
                            _ = step.ExecuteAsync(context, ct);
                            continue;
                        }

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
