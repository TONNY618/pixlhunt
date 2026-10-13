using System;
using System.Collections.Generic;
using System.Text.Json;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Engine;

/// <summary>
/// [Подсистема движка: Уровень 3 - Фабрика]
/// Универсальная фабрика на паттерне Registry: сборщики шагов и триггеров
/// регистрируются в словарях делегатов, что устраняет жёсткую привязку к конструкторам.
/// </summary>
public static class ComboFactory
{
    // УНИВЕРСАЛЬНЫЕ РЕЕСТРЫ СБОРЩИКОВ
    // Ключ - имя контрола (ComponentType), Значение - функция сборки шага/триггера
    private static readonly Dictionary<string, Func<JsonElement, ITriggerEvaluator?>> TriggerBuilders = new();
    private static readonly Dictionary<string, Func<JsonElement, IComboStep?>> StepBuilders = new();

    static ComboFactory()
    {
        // Здесь в будущем будут регистрироваться сборщики (когда файлы шагов будут адаптированы)
        // Пример (пока закомментирован, чтобы не было ошибок компиляции):
        // RegisterTrigger("TriggerTimer", data => new TimerTriggerEvaluator(data.Deserialize<TriggerTimerDto>().IntervalMs));
    }

    public static void RegisterTrigger(string componentType, Func<JsonElement, ITriggerEvaluator?> builder)
    {
        TriggerBuilders[componentType] = builder;
    }

    public static void RegisterStep(string componentType, Func<JsonElement, IComboStep?> builder)
    {
        StepBuilders[componentType] = builder;
    }

    public static ActiveCombo Create(ComboProfileDto profile)
    {
        if (profile == null) throw new ArgumentNullException(nameof(profile));

        int cooldownMs = profile.IsCooldownGlobal ? 1100 : 1500; // Fallbacks
        int priority = int.TryParse(profile.Priority, out int p) ? p : 50;

        var combo = new ActiveCombo
        {
            Name = profile.ComboName ?? string.Empty,
            Priority = priority,
            CooldownMs = cooldownMs,
            IsGlobalCooldown = profile.IsCooldownGlobal,
            PauseIfWasd = profile.PauseIfWASD,
            IsEnabled = profile.IsOnOff
        };

        BuildTriggers(profile.Triggers, combo);
        BuildSteps(profile.Actions, combo.Steps);

        return combo;
    }

    private static void BuildTriggers(List<ComboElementDto> elements, ActiveCombo combo)
    {
        if (elements == null) return;
        foreach (var elem in elements)
        {
            if (elem.Type == "TriggerOrAnd")
            {
                // Логика И/ИЛИ зашита в сам комбо, а не является триггером
                var orAndDto = elem.Data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.TriggerOrAndDto>();
                if (orAndDto != null) combo.IsOrTriggerLogic = orAndDto.IsOrLogic;
                continue;
            }

            if (TriggerBuilders.TryGetValue(elem.Type, out var builder))
            {
                var trigger = builder(elem.Data);
                if (trigger != null) combo.Triggers.Add(trigger);
            }
        }
    }

    private static void BuildSteps(List<ComboElementDto> elements, List<IComboStep> rootSteps)
    {
        if (elements == null) return;

        IComboStep? lastCondition = null; // Позже заменим на ConditionStep

        foreach (var elem in elements)
        {
            if (StepBuilders.TryGetValue(elem.Type, out var builder))
            {
                IComboStep? step = builder(elem.Data);
                if (step == null) continue;

                if (elem.Indent > 0 && lastCondition != null)
                {
                    // Вложенность (в будущем: lastCondition.TrueSteps.Add(step))
                }
                else
                {
                    rootSteps.Add(step);
                    lastCondition = step;
                }
            }
        }
    }
}
