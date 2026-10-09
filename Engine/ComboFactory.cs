using System;
using System.Collections.Generic;
using System.Drawing;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Triggers;
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
        BuildTriggers(profile.Triggers, combo.Triggers);

        return combo;
    }

    /// <summary>
    /// Собирает триггеры из UI-элементов профиля.
    /// Поддерживается тип "groupBox1" (проверка цвета/региона).
    /// </summary>
    private static void BuildTriggers(List<ComboElement> elements, List<ITriggerEvaluator> targetList)
    {
        if (elements == null) return;

        foreach (var elem in elements)
        {
            if (elem == null) continue;

            switch (elem.ElementType)
            {
                case "groupBox1":
                    var trigger = CreateColorTrigger(elem);
                    if (trigger != null)
                        targetList.Add(trigger);
                    break;

                case "IFconditionsOrAndStart":
                case "IFconditionsTimerStart":
                    // Пока безопасно пропускаем
                    break;
            }
        }
    }

    /// <summary>
    /// Создаёт ColorTriggerEvaluator из параметров UI-элемента "groupBox1".
    /// </summary>
    private static ColorTriggerEvaluator? CreateColorTrigger(ComboElement elem)
    {
        var p = elem.Parameters;
        if (p == null) return null;

        Point point1 = ParsePoint(p.GetValueOrDefault("textBoxXY1_1"));
        Point point2 = ParsePoint(p.GetValueOrDefault("textBoxXY1_2"));
        Color expected = ParseColor(p.GetValueOrDefault("textBoxColor1"));
        int deviant = ParseIntOrDefault(p.GetValueOrDefault("textBoxDeviant1"), 0);

        bool isNotEqual = p.GetValueOrDefault("radioButtonEqu1_2") == "True";
        bool isRegionAverage = p.GetValueOrDefault("radioButtonSquare1_2") == "True";

        return new ColorTriggerEvaluator(point1, point2, expected, deviant, isNotEqual, isRegionAverage);
    }

    /// <summary>
    /// Парсит строку вида "119, 273" в Point.
    /// </summary>
    private static Point ParsePoint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Point.Empty;

        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
            return Point.Empty;

        int x = ParseIntOrDefault(parts[0], 0);
        int y = ParseIntOrDefault(parts[1], 0);
        return new Point(x, y);
    }

    /// <summary>
    /// Парсит hex-строку цвета (например "#A0A0A0") в Color.
    /// </summary>
    private static Color ParseColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Color.Black;

        try
        {
            return ColorTranslator.FromHtml(value);
        }
        catch
        {
            return Color.Black;
        }
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
