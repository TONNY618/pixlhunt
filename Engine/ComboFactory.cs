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

        // Определяем кулдаун: для общих комбо (Skill) — из профиля или глобальный дефолт,
        // для мгновенных (HP) — из профиля, если задан, иначе дефолт мгновенных комбо.
        int cooldownMs;
        if (profile.IsCooldownGlobal)
        {
            cooldownMs = ParseIntOrDefault(profile.CooldownTime,
                AppConfigManager.Config.GlobalCooldownDefaultMs);
        }
        else
        {
            int parsed = ParseIntOrDefault(profile.CooldownTime, 0);
            cooldownMs = parsed > 0
                ? parsed
                : AppConfigManager.Config.InstantComboDefaultCooldownMs;
        }

        var combo = new ActiveCombo
        {
            Name = profile.ComboName ?? string.Empty,
            Priority = ParseIntOrDefault(profile.Priority, 50),
            CooldownMs = cooldownMs,
            IsGlobalCooldown = profile.IsCooldownGlobal,
            PauseIfWasd = profile.PauseIfWASD
        };

        BuildSteps(profile.Actions, combo.Steps);
        BuildTriggers(profile.Triggers, combo);

        return combo;
    }

    /// <summary>
    /// Собирает триггеры из UI-элементов профиля.
    /// Поддерживаются типы: "groupBox1" (цвет), "IFconditionsKEYgroupBox" (клавиша),
    /// "IFconditionsOrAndStart" (логика И/ИЛИ), "IFconditionsTimerStart" (таймер).
    /// </summary>
    private static void BuildTriggers(List<ComboElement> elements, ActiveCombo combo)
    {
        if (elements == null) return;

        foreach (var elem in elements)
        {
            if (elem == null) continue;

            switch (elem.ElementType)
            {
                case "groupBox1":
                    var trigger = ParseColorEvaluator(elem.Parameters, "1");
                    if (trigger != null)
                        combo.Triggers.Add(trigger);
                    break;

                case "IFconditionsKEYgroupBox":
                    var keyTrigger = CreateKeyTrigger(elem);
                    if (keyTrigger != null)
                        combo.Triggers.Add(keyTrigger);
                    break;

                case "IFconditionsOrAndStart":
                    if (elem.Parameters != null &&
                        elem.Parameters.GetValueOrDefault("radioButtonOrAnd1_1") == "True")
                    {
                        combo.IsOrTriggerLogic = true;
                    }
                    break;

                case "IFconditionsTimerStart":
                    if (elem.Parameters != null)
                    {
                        int ms = ParseIntOrDefault(elem.Parameters.GetValueOrDefault("TimerStarttextBox"), 0);
                        if (ms > 0)
                            combo.Triggers.Add(new TimerTriggerEvaluator(ms));
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Создаёт ColorTriggerEvaluator из параметров UI-элемента по заданному суффиксу.
    /// </summary>
    private static ColorTriggerEvaluator? ParseColorEvaluator(Dictionary<string, string>? p, string suffix)
    {
        if (p == null) return null;

        Point point1 = ParsePoint(p.GetValueOrDefault($"textBoxXY{suffix}_1"));
        Point point2 = ParsePoint(p.GetValueOrDefault($"textBoxXY{suffix}_2"));
        Color expected = ParseColor(p.GetValueOrDefault($"textBoxColor{suffix}"));
        int deviant = ParseIntOrDefault(p.GetValueOrDefault($"textBoxDeviant{suffix}"), 0);

        bool isNotEqual = p.GetValueOrDefault($"radioButtonEqu{suffix}_2") == "True";
        bool isRegionAverage = p.GetValueOrDefault($"radioButtonSquare{suffix}_2") == "True";

        return new ColorTriggerEvaluator(point1, point2, expected, deviant, isNotEqual, isRegionAverage);
    }

    /// <summary>
    /// Создаёт KeyTriggerEvaluator из параметров UI-элемента "IFconditionsKEYgroupBox".
    /// Имя клавиши берётся из параметра "IFconditionsKEYcomboBox".
    /// </summary>
    private static KeyTriggerEvaluator? CreateKeyTrigger(ComboElement elem)
    {
        var p = elem.Parameters;
        if (p == null) return null;

        string keyName = p.GetValueOrDefault("IFconditionsKEYcomboBox", "");
        if (string.IsNullOrWhiteSpace(keyName)) return null;

        return new KeyTriggerEvaluator(keyName);
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
                return new ConditionStep(ParseColorEvaluator(elem.Parameters, "2"));

            case "groupBoxAwait":
                return new AwaitStep(ParseColorEvaluator(elem.Parameters, "3"));

            case "delayMs":
                return new PixelMacroEngine.Triggers.Steps.DelayStep(elem.Parameters);

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
