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

    public bool IsConditionMet(FrameBuffer frame, DateTime lastStepExecutedTime)
    {
        // TODO: реализовать проверку предусловий (таймер, цвет пикселя)
        return true;
    }

    public InputBatchCmd GenerateBatch()
    {
        // TODO: реализовать генерацию батча ввода на основе Parameters
        return new InputBatchCmd();
    }

    public Task ExecuteAsync(TriggerExecutionContext context, CancellationToken cancellationToken)
    {
        // TODO: реализовать выполнение шага
        return Task.CompletedTask;
    }
}
