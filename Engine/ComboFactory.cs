using System;
using System.Collections.Generic;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Triggers.Steps;
using pxlhunt.FORMS;

namespace PixelMacroEngine.Engine;

/// <summary>
/// [Подсистема движка: Уровень 3 - Фабрика]
/// Конвертирует UI-модель ComboProfile (из ComboEditorForm) в боевую модель ActiveCombo.
/// Связан с: ComboProfile, ActiveCombo, IComboStep, PressKeyStep, ConditionStep.
/// </summary>
public static class ComboFactory
{
    /// <summary>
    /// Создаёт ActiveCombo из UI-профиля.
    /// </summary>
    public static ActiveCombo Create(ComboProfile profile)
    {
        if (profile == null) throw new ArgumentNullException(nameof(profile));

        var combo = new ActiveCombo
        {
            Name = profile.ComboName ?? string.Empty,
            Priority = ParseIntOrDefault(profile.Priority, 50),
            CooldownMs = ParseIntOrDefault(profile.CooldownTime, 1500),
            IsGlobalCooldown = profile.IsCooldownGlobal,
            PauseIfWasd = profile.PauseIfWASD
        };

        BuildSteps(profile.Actions, combo.Steps);

        return combo;
    }

    /// <summary>
    /// Собирает дерево шагов с учётом IndentLevel (поддерживается 1 уровень вложенности).
    /// Элементы с IndentLevel > 0 попадают во внутренний список TrueSteps последнего ConditionStep.
    /// </summary>
    private static void BuildSteps(List<ComboElement> elements, List<IComboStep> rootSteps)
    {
        if (elements == null) return;

        ConditionStep? lastCondition = null;

        foreach (var elem in elements)
        {
            if (elem == null) continue;

            IComboStep? step = CreateStep(elem);
            if (step == null) continue;

            if (elem.IndentLevel > 0)
            {
                // Вложенный шаг — добавляем в TrueSteps последнего условия
                if (lastCondition != null)
                {
                    lastCondition.TrueSteps.Add(step);
                }
                // Если условия нет — игнорируем (некорректный профиль)
            }
            else
            {
                rootSteps.Add(step);

                // Запоминаем последнее условие корневого уровня для вложенных шагов
                lastCondition = step as ConditionStep;
            }
        }
    }

    /// <summary>
    /// Создаёт конкретный шаг по типу элемента.
    /// </summary>
    private static IComboStep? CreateStep(ComboElement elem)
    {
        switch (elem.ElementType)
        {
            case "pressKey":
                return new PressKeyStep(elem.Parameters);

            case "groupBoxCond":
                return new ConditionStep(elem.Parameters);

            default:
                return null;
        }
    }

    private static int ParseIntOrDefault(string? value, int defaultValue)
    {
        if (int.TryParse(value, out int result))
            return result;

        return defaultValue;
    }
}
