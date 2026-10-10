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
        byte code = ArduinoKeyMap.GetByte(key);

        bool pressDown = Parameters.GetValueOrDefault("checkBoxUpDownKey1_1") == "True";
        bool pressUp = Parameters.GetValueOrDefault("checkBoxUpDownKey1_2") == "True";

        // Клавиша не найдена в мапе — просто логируем текстовое действие.
        if (code == 0)
        {
            ActionLogger.Log($"[PressKeyStep] Клавиша '{key}' не найдена в ArduinoKeyMap (down={pressDown}, up={pressUp}).");
            return;
        }

        // Модификаторы (Shift, Ctrl, Alt, Win) занимают диапазон 0x80 - 0x87.
        bool isModifier = code >= 0x80 && code <= 0x87;

        var batch = new PacketBatch();

        if (pressDown)
        {
            batch.AddKey(code, isDown: true);
            ActionLogger.LogKey(key, code, isDown: true, context.ComboName);

            // Для модификатора сразу отправляем нажатие и даём паузу на его "подготовку",
            // чтобы последующая клавиша успела застать модификатор активным.
            if (isModifier)
            {
                ArduinoHidService.Send(batch);
                await Task.Delay(PixelMacroEngine.Core.Services.HumanizerEngine.GetModifierDelay(), cancellationToken);
                batch = new PacketBatch();
            }
        }

        if (pressDown && pressUp)
        {
            // Отправляем нажатие, держим клавишу, затем готовим пачку на отпускание.
            ArduinoHidService.Send(batch);
            int holdTime = HumanizerEngine.GetKeyPressDuration();
            await Task.Delay(holdTime, cancellationToken);
            batch = new PacketBatch();
        }

        if (pressUp)
        {
            batch.AddKey(code, isDown: false);
            ActionLogger.LogKey(key, code, isDown: false, context.ComboName);
        }

        ArduinoHidService.Send(batch);

        // Пост-пауза (Flight Time): имитация переноса пальца к следующей клавише.
        // Для модификаторов (Shift, Ctrl, Alt, Win = 0x80 - 0x87) используется укороченная задержка.
        int postDelay = isModifier
            ? HumanizerEngine.GetModifierDelay()
            : HumanizerEngine.GetFlightTime();

        await Task.Delay(postDelay, cancellationToken);
    }
}
