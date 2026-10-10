using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers.Steps;

/// <summary>
/// [Подсистема движка: Уровень 3 - Шаг]
/// Шаг-условие (ЕСЛИ): выполняет вложенные шаги TrueSteps, если цветовой триггер истинен.
/// Связан с: IComboStep, ColorTriggerEvaluator, ComboFactory.
/// </summary>
public class ConditionStep : IComboStep
{
    private readonly ColorTriggerEvaluator? _evaluator;

    /// <summary>Шаги, выполняемые при истинности условия.</summary>
    public List<IComboStep> TrueSteps { get; } = new List<IComboStep>();

    public ConditionStep(ColorTriggerEvaluator? evaluator)
    {
        _evaluator = evaluator;
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken cancellationToken)
    {
        if (_evaluator != null && _evaluator.Evaluate(context.Frame))
        {
            foreach (var step in TrueSteps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await step.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
