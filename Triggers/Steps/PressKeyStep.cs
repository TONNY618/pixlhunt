using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers.Steps;

/// <summary>
/// [Подсистема движка: Уровень 3 - Шаг]
/// Заглушка шага нажатия клавиши. Параметры берутся из UI-профиля.
/// Связан с: IComboStep, ComboFactory.
/// </summary>
public class PressKeyStep : IComboStep
{
    /// <summary>Параметры шага (имена контролов -> значения из UI).</summary>
    public Dictionary<string, string> Parameters { get; }

    public PressKeyStep(Dictionary<string, string> parameters)
    {
        Parameters = parameters ?? new Dictionary<string, string>();
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken cancellationToken)
    {
        string key = Parameters.GetValueOrDefault("comboBoxKeyList", "");
        byte code = PixelMacroEngine.Core.Input.ArduinoKeyMap.GetByte(key);

        bool pressDown = Parameters.GetValueOrDefault("checkBoxUpDownKey1_1") == "True";
        bool pressUp = Parameters.GetValueOrDefault("checkBoxUpDownKey1_2") == "True";

        if (pressDown)
        {
            PixelMacroEngine.Core.Services.ActionLogger.LogKey(key, code, isDown: true, context.ComboName);
        }

        if (pressDown && pressUp)
        {
            await Task.Delay(40, cancellationToken);
        }

        if (pressUp)
        {
            PixelMacroEngine.Core.Services.ActionLogger.LogKey(key, code, isDown: false, context.ComboName);
        }
    }
}
