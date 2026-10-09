using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers.Steps;

/// <summary>
/// [Подсистема движка: Уровень 3 - Шаг]
/// Заглушка шага-условия (ЕСЛИ). Содержит вложенные шаги TrueSteps.
/// Связан с: IComboStep, ComboFactory.
/// </summary>
public class ConditionStep : IComboStep
{
    /// <summary>Параметры условия (имена контролов -> значения из UI).</summary>
    public Dictionary<string, string> Parameters { get; }

    /// <summary>Шаги, выполняемые при истинности условия.</summary>
    public List<IComboStep> TrueSteps { get; } = new List<IComboStep>();

    public ConditionStep(Dictionary<string, string> parameters)
    {
        Parameters = parameters ?? new Dictionary<string, string>();
    }

    public Task ExecuteAsync(TriggerExecutionContext context, CancellationToken cancellationToken)
    {
        // TODO: реализовать выполнение вложенных шагов TrueSteps
        return Task.CompletedTask;
    }
}
