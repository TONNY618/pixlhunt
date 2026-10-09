using System;
using System.Collections.Generic;
using PixelMacroEngine.Core.Abstractions;

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

    /// <summary>Приоритет выполнения: чем выше, тем раньше проверяется комбо.</summary>
    public int Priority { get; set; }

    /// <summary>Кулдаун между срабатываниями в миллисекундах.</summary>
    public int CooldownMs { get; set; }

    /// <summary>Использовать глобальный кулдаун (общий для всех комбо) вместо локального.</summary>
    public bool IsGlobalCooldown { get; set; }

    /// <summary>Приостанавливать выполнение, если пользователь нажимает WASD.</summary>
    public bool PauseIfWasd { get; set; }

    /// <summary>Включено ли комбо (управляется галочкой в UI).</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Время последнего успешного выполнения комбо (для расчёта кулдауна).</summary>
    public DateTime LastExecuted { get; set; } = DateTime.MinValue;

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

        return (DateTime.UtcNow - LastExecuted).TotalMilliseconds >= CooldownMs;
    }
}
