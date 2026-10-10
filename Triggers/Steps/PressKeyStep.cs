using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

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

        if (code == 0) return;

        bool isModifier = code >= 0x80 && code <= 0x87;

        // 1. Атомарное нажатие (DOWN) - отдельный пакет
        if (pressDown)
        {
            var batchDown = new PixelMacroEngine.Core.Input.PacketBatch();
            batchDown.AddKey(code, true);
            PixelMacroEngine.Core.Services.ArduinoHidService.Send(batchDown);
            PixelMacroEngine.Core.Services.ActionLogger.LogKey(key, code, true, context.ComboName);

            // Если это модификатор и мы его только зажимаем (без отпускания тут же)
            // Даем Windows время "осознать", что Shift нажат, перед следующей буквой
            if (isModifier && !pressUp)
            {
                await Task.Delay(PixelMacroEngine.Core.Services.HumanizerEngine.GetModifierDelay(), cancellationToken);
            }
        }

        // 2. Пауза удержания клавиши (HOLD) - если это обычный клик (Down + Up)
        if (pressDown && pressUp)
        {
            int holdTime = PixelMacroEngine.Core.Services.HumanizerEngine.GetKeyPressDuration();
            await Task.Delay(holdTime, cancellationToken);
        }

        // 3. Атомарное отпускание (UP) - отдельный пакет
        if (pressUp)
        {
            var batchUp = new PixelMacroEngine.Core.Input.PacketBatch();
            batchUp.AddKey(code, false);
            PixelMacroEngine.Core.Services.ArduinoHidService.Send(batchUp);
            PixelMacroEngine.Core.Services.ActionLogger.LogKey(key, code, false, context.ComboName);
        }

        // 4. Пост-пауза (перенос пальца на следующую кнопку)
        // Делаем паузу после полного клика или после отпускания кнопки
        if ((pressDown && pressUp) || (!pressDown && pressUp))
        {
            int postDelay = isModifier 
                ? PixelMacroEngine.Core.Services.HumanizerEngine.GetModifierDelay() 
                : PixelMacroEngine.Core.Services.HumanizerEngine.GetFlightTime();
            await Task.Delay(postDelay, cancellationToken);
        }
    }
}
