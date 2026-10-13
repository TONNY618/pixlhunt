using System.Collections.Generic;
using System.Drawing;
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
    private readonly ITriggerEvaluator? _evaluator;

    /// <summary>Шаги, выполняемые при истинности условия.</summary>
    public List<IComboStep> TrueSteps { get; } = new List<IComboStep>();

    /// <summary>Координата 1 (точка или левый верхний угол региона).</summary>
    public Point? Point1 { get; }

    /// <summary>Координата 2 (правый нижний угол региона).</summary>
    public Point? Point2 { get; }

    /// <summary>Признак того, что условие задано регионом (квадратом).</summary>
    public bool IsRegion { get; }

    public ConditionStep(ITriggerEvaluator evaluator, Point? p1 = null, Point? p2 = null, bool isRegion = false)
    {
        _evaluator = evaluator;
        Point1 = p1;
        Point2 = p2;
        IsRegion = isRegion;
    }

    /// <summary>Перегрузка для обратной совместимости.</summary>
    public ConditionStep(ITriggerEvaluator evaluator) : this(evaluator, null, null, false) { }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken cancellationToken)
    {
        if (_evaluator != null && _evaluator.Evaluate(context.Frame))
        {
            if (Point1.HasValue)
            {
                RuntimeVariables.TargetPoint1 = Point1.Value;
                RuntimeVariables.TargetPoint2 = IsRegion ? Point2 : null;
                RuntimeVariables.IsRegion = IsRegion;

                context.SetVariable("Above_Point1", Point1.Value);
                if (IsRegion && Point2.HasValue)
                    context.SetVariable("Above_Point2", Point2.Value);
                context.SetVariable("Above_IsRegion", IsRegion);
            }

            foreach (var step in TrueSteps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await step.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
