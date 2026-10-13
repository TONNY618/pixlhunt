using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Triggers;
using PixelMacroEngine.Triggers.Steps;

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
        RegisterTrigger("TriggerTimer", data =>
        {
            var dto = data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.TriggerTimerDto>();
            if (dto == null) return null;
            int min = Math.Max(1, dto.MinMs);
            int max = Math.Max(min, dto.MaxMs);
            return new TimerTriggerEvaluator(min, max);
        });

        RegisterStep("TriggerTimer", data =>
        {
            var dto = data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.TriggerTimerDto>();
            if (dto == null) return null;
            int min = Math.Max(1, dto.MinMs);
            int max = Math.Max(min, dto.MaxMs);
            return new DelayStep(min, max);
        });

        RegisterTrigger("PixSquaControl", data =>
        {
            var dto = data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.PixSquaDto>();
            return dto != null ? CreateColorEvaluator(dto) : null;
        });

        RegisterTrigger("BoxKeyControl", data =>
        {
            var dto = data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.BoxKeyDto>();
            return dto != null ? new KeyTriggerEvaluator(dto.KeyName) : null;
        });

        RegisterStep("BoxKeyControl", data =>
        {
            var dto = data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.BoxKeyDto>();
            if (dto == null) return null;
            return new PressKeyStep(new Dictionary<string, string>
            {
                ["comboBoxKeyList"] = dto.KeyName,
                ["checkBoxUpDownKey1_1"] = dto.PressDown.ToString(),
                ["checkBoxUpDownKey1_2"] = dto.PressUp.ToString()
            });
        });

        RegisterStep("PixSquaControl", data =>
        {
            var dto = data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.PixSquaDto>();
            if (dto == null) return null;

            Point p1 = ParsePoint(dto.Point1);
            Point p2 = ParsePoint(dto.Point2);
            return new ConditionStep(CreateColorEvaluator(dto), p1, p2, dto.IsSquareMode);
        });

        RegisterStep("CursorMoveControl", data =>
        {
            var dto = data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.CursorMoveDto>();
            if (dto == null) return null;
            return new MouseMoveStep(dto);
        });

        RegisterStep("MouseKeyControl", data =>
        {
            var dto = data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.MouseKeyDto>();
            if (dto == null) return null;
            return new MouseClickStep(dto);
        });

        RegisterTrigger("MouseKeyControl", data =>
        {
            var dto = data.Deserialize<pxlhunt.UI.Forms.ComboEditorCompoziteControls.MouseKeyDto>();
            return dto != null ? new MouseButtonTriggerEvaluator(dto.Button) : null;
        });
    }

    private static ColorTriggerEvaluator CreateColorEvaluator(pxlhunt.UI.Forms.ComboEditorCompoziteControls.PixSquaDto dto)
    {
        Point p1 = ParsePoint(dto.Point1);
        Point p2 = ParsePoint(dto.Point2);
        Color expected = HexToColor(dto.ColorHex);
        int deviant = Math.Max(0, 100 - dto.Accuracy);
        return new ColorTriggerEvaluator(p1, p2, expected, deviant, !dto.IsEqualLogic, dto.IsSquareMode);
    }

    private static Point ParsePoint(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Point.Empty;
        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return Point.Empty;
        int.TryParse(parts[0], out int x);
        int.TryParse(parts[1], out int y);
        return new Point(x, y);
    }

    private static Color HexToColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Color.Black;
        try { return ColorTranslator.FromHtml(hex); } catch { return Color.Black; }
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

        int cooldownMs = profile.IsCooldownGlobal
            ? PixelMacroEngine.Core.Services.AppConfigService.Instance.GlobalCooldownDefaultMs
            : PixelMacroEngine.Core.Services.AppConfigService.Instance.InstantComboDefaultCooldownMs;
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
                    if (lastCondition is ConditionStep condStep)
                    {
                        condStep.TrueSteps.Add(step);
                    }
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
