using System;
using System.Collections.Generic;
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

    // --- Preemptive Scheduling (Extra-комбо) ---
    // Поля текущего фонового выполнения: шаги комбо крутятся в Task,
    // а очистка состояния происходит из потока пула — доступ синхронизирован _comboLock.
    private readonly object _comboLock = new object();
    private Task? _runningComboTask;
    private CancellationTokenSource? _comboCts;
    private ActiveCombo? _runningCombo;

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

    /// <summary>
    /// Принудительно и немедленно прерывает фоновое комбо (Hard Abort при выключении мастера).
    /// </summary>
    public void AbortRunningCombo()
    {
        lock (_comboLock)
        {
            try
            {
                _comboCts?.Cancel();
            }
            catch (ObjectDisposedException) { /* комбо уже завершилось и освободило CTS */ }
        }

        // Аппаратно отпускаем всё, что могло остаться физически зажатым на плате Arduino.
        PixelMacroEngine.Core.Services.ArduinoHidService.EmergencyReset();
    }

    /// <summary>Сбрасывает состояние кулдауна у всех комбо.</summary>
    public void ResetAllStates()
    {
        // Сброс состояния всегда должен убивать активное выполнение.
        AbortRunningCombo();

        // Не забываем сбрасывать общий ГКД при выключении мастера.
        _globalLastExecutedTimestamp = 0;

        foreach (var combo in Combos)
        {
            combo.LastExecutedTimestamp = 0;
            foreach (var trigger in combo.Triggers)
            {
                if (trigger is PixelMacroEngine.Triggers.TimerTriggerEvaluator timer)
                {
                    timer.ResetToInitial();
                }
            }
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

        // Флаг физического движения игрока нужен и для PauseIfWasd, и для маппинга состояния.
        bool wasdHeld = IsWasdPressed();

        // Снимок текущего фонового выполнения. Читаем под _comboLock,
        // т.к. очистка полей идёт из потока Task (ExecuteComboAsync / finally).
        ActiveCombo? running;
        Task? runningTask;
        lock (_comboLock)
        {
            running = _runningCombo;
            runningTask = _runningComboTask;
        }
        bool isRunning = running != null && runningTask != null && !runningTask.IsCompleted;

        bool startedNewCombo = false;

        // 1. Extra-комбо обрабатываются первыми и имеют безусловный приоритет.
        var extraCombos = Combos.Where(c => c.IsExtra)
                                .OrderByDescending(c => c.Priority)
                                .ToList();
        ActiveCombo? triggeredExtra = FindFirstTriggered(extraCombos, frame, wasdHeld);

        if (triggeredExtra != null)
        {
            bool sameCombo = ReferenceEquals(running, triggeredExtra);
            bool shouldPreempt = !sameCombo &&
                (running == null || !running.IsExtra || running.Priority < triggeredExtra.Priority);

            if (shouldPreempt)
            {
                if (running != null)
                {
                    await CancelRunningComboAsync().ConfigureAwait(false);

                    // Перед запуском Extra-комбо аппаратно отпускаем всё, что могло
                    // остаться физически зажатым на плате Arduino.
                    ArduinoHidService.EmergencyReset();
                }

                StartCombo(triggeredExtra, frame, ct);
                startedNewCombo = true;
            }
            // Иначе: это то же самое комбо, либо уже выполняется Extra
            // с равным/большим приоритетом — не трогаем его.
        }
        else if (!isRunning)
        {
            // 2. Normal-комбо стартуют только когда в фоне ничего не выполняется.
            var normalCombos = Combos.Where(c => !c.IsExtra)
                                     .OrderByDescending(c => c.Priority)
                                     .ToList();
            ActiveCombo? triggeredNormal = FindFirstTriggered(normalCombos, frame, wasdHeld);
            if (triggeredNormal != null)
            {
                StartCombo(triggeredNormal, frame, ct);
                startedNewCombo = true;
            }
        }

        // Флаг нагрузки на кадр для маппинга физиологического состояния.
        bool anyComboRan = isRunning || startedNewCombo;

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

    /// <summary>
    /// Возвращает первое комбо из списка (уже отсортированного по убыванию приоритета),
    /// прошедшее проверку кулдауна, ГКД, PauseIfWasd и триггеров.
    /// </summary>
    private ActiveCombo? FindFirstTriggered(List<ActiveCombo> candidates, FrameBuffer frame, bool wasdHeld)
    {
        foreach (var combo in candidates)
        {
            if (!combo.IsEnabled || !combo.CanExecute())
                continue;

            // Если включён PauseIfWasd и игрок двигается — пропускаем комбо.
            if (combo.PauseIfWasd && wasdHeld)
                continue;

            // Если комбо делит ГКД и не является фоновым сервисным действием
            if (combo.IsGlobalCooldown && !combo.IsNonBlocking && combo.CooldownMs > 0)
            {
                if (_globalLastExecutedTimestamp != 0 &&
                    TimeService.GetElapsedMilliseconds(_globalLastExecutedTimestamp) < combo.CooldownMs)
                    continue;
            }

            if (TriggersPassed(combo, frame))
                return combo;
        }

        return null;
    }

    /// <summary>Проверяет триггеры комбо с учётом логики И/ИЛИ.</summary>
    private static bool TriggersPassed(ActiveCombo combo, FrameBuffer frame)
    {
        if (combo.Triggers.Count == 0)
        {
            // Комбо без триггеров не должно спамить на каждом кадре
            return false;
        }

        return combo.IsOrTriggerLogic
            ? combo.Triggers.Any(t => t.Evaluate(frame))
            : combo.Triggers.All(t => t.Evaluate(frame));
    }

    /// <summary>
    /// Запускает комбо в фоне, не блокируя игровой цикл.
    /// Таймстампы кулдауна (в т.ч. общий ГКД) фиксируются ДО фактического старта,
    /// чтобы уже следующий кадр видел актуальное время.
    /// </summary>
    private void StartCombo(ActiveCombo combo, FrameBuffer frame, CancellationToken externalCt)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);

        long execTs = TimeService.GetTimestamp();
        combo.LastExecutedTimestamp = execTs;
        if (combo.IsGlobalCooldown && !combo.IsNonBlocking && combo.CooldownMs > 0)
        {
            _globalLastExecutedTimestamp = execTs;
        }

        // Сбрасываем триггеры комбо (таймер начинает отсчёт следующего интервала)
        combo.OnExecuted();

        combo.IsRunning = true;

        lock (_comboLock)
        {
            _comboCts = cts;
            _runningCombo = combo;
            _runningComboTask = Task.Run(() => ExecuteComboAsync(combo, frame, cts.Token));
        }
    }

    /// <summary>
    /// Отменяет текущее фоновое комбо и дожидается его завершения.
    /// Безопасно вызывается, даже если комбо успело завершиться само.
    /// </summary>
    private async Task CancelRunningComboAsync()
    {
        Task? task;
        CancellationTokenSource? cts;
        lock (_comboLock)
        {
            task = _runningComboTask;
            cts = _comboCts;
        }

        if (cts != null)
        {
            try { cts.Cancel(); }
            catch (ObjectDisposedException) { /* комбо уже завершилось и освободило CTS */ }
        }

        if (task != null)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Ожидаемая отмена (preemption / shutdown)
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Combo await error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Тело фонового выполнения шагов комбо. При отмене немедленно прекращает работу,
    /// а в finally гарантированно очищает поля текущего выполнения (только если они
    /// всё ещё принадлежат этому комбо — Extra-комбо могло уже стартовать новое).
    /// </summary>
    private async Task ExecuteComboAsync(ActiveCombo combo, FrameBuffer frame, CancellationToken ct)
    {
        try
        {
            var context = new TriggerExecutionContext
            {
                Frame = frame,
                ComboName = combo.Name
            };

            foreach (var step in combo.Steps)
            {
                ct.ThrowIfCancellationRequested();

                if (step is INonBlockingStep)
                {
                    // Фоновый шаг (звук/уведомление): запускаем без ожидания
                    // и мгновенно переходим к следующему шагу!
                    _ = step.ExecuteAsync(context, ct);
                    continue;
                }

                // Мягкая пауза: главное окно активно (потеря фокуса игры), но мастер-флаг включён.
                // Комбо замирает между шагами и продолжит с текущего шага, когда игра вернёт фокус.
                while (BotStateController.IsMasterEnabled && !BotStateController.CanExecuteCombos)
                {
                    ct.ThrowIfCancellationRequested();

                    // Сбрасываем возможные залипания, чтобы не слать ввод в наши окна.
                    PixelMacroEngine.Core.Services.ArduinoHidService.EmergencyReset();

                    await Task.Delay(50, ct).ConfigureAwait(false);
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
            // Прервано: preemption, таймаут или WASD
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Combo '{combo.Name}' failed: {ex.Message}");
        }
        finally
        {
            combo.IsRunning = false;
            lock (_comboLock)
            {
                if (ReferenceEquals(_runningCombo, combo))
                {
                    _runningCombo = null;
                    _runningComboTask = null;
                    _comboCts?.Dispose();
                    _comboCts = null;
                }
            }
        }
    }
}
