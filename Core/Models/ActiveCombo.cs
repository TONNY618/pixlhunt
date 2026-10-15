using System;
using System.Collections.Generic;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Core.Models;

/// <summary>
/// [Подсистема движка: Уровень 2 - Модель]
/// Описывает активное комбо: набор триггеров и шагов, а также параметры кулдауна и приоритета.
/// Связан с: ITriggerEvaluator, IComboStep, движком выполнения комбо.
/// </summary>
public class ActiveCombo
{
    /// <summary>Уникальное имя комбо (совпадает с именем JSON-профиля).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Полный путь к исходному JSON-файлу комбо (для сохранения состояния).</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Приоритет выполнения: чем выше, тем раньше проверяется комбо.</summary>
    public int Priority { get; set; }

    /// <summary>Кулдаун между срабатываниями в миллисекундах.</summary>
    public int CooldownMs { get; set; }

    /// <summary>Использовать глобальный кулдаун (общий для всех комбо) вместо локального.</summary>
    public bool IsGlobalCooldown { get; set; }

    /// <summary>Приостанавливать выполнение, если пользователь нажимает WASD.</summary>
    public bool PauseIfWasd { get; set; }

    /// <summary>Использовать логику ИЛИ для триггеров (по умолчанию — И).</summary>
    public bool IsOrTriggerLogic { get; set; } = false;

    /// <summary>Включено ли комбо (управляется галочкой в UI).</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Аппаратный таймстамп последнего успешного выполнения комбо (для расчёта кулдауна).
    /// Значение 0 означает «комбо ещё ни разу не выполнялось».
    /// </summary>
    public long LastExecutedTimestamp { get; set; } = 0;

    /// <summary>Признак того, что комбо в данный момент выполняет свои шаги.</summary>
    public bool IsRunning { get; set; } = false;

    /// <summary>
    /// Признак того, что комбо состоит только из неблокирующих (фоновых) шагов.
    /// Такие комбо не занимают слот умения и не расходуют общий кулдаун (ГКД).
    /// </summary>
    public bool IsNonBlocking { get; set; } = false;

    /// <summary>
    /// Признак «Extra»-комбо (Preemptive Scheduling).
    /// Такое комбо мгновенно прерывает любое текущее Normal-комбо
    /// либо Extra-комбо с меньшим приоритетом (см. Orchestrator).
    /// </summary>
    public bool IsExtra { get; set; } = false;

    /// <summary>Список триггеров, определяющих срабатывание комбо.</summary>
    public List<ITriggerEvaluator> Triggers { get; set; } = new List<ITriggerEvaluator>();

    /// <summary>Список шагов, выполняемых при срабатывании комбо.</summary>
    public List<IComboStep> Steps { get; set; } = new List<IComboStep>();

    /// <summary>
    /// Проверяет, прошёл ли кулдаун с момента последнего выполнения.
    /// </summary>
    public bool CanExecute()
    {
        if (CooldownMs <= 0)
            return true;

        if (LastExecutedTimestamp == 0)
            return true;

        return TimeService.GetElapsedMilliseconds(LastExecutedTimestamp) >= CooldownMs;
    }
}
