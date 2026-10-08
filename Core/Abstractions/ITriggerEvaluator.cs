using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Abstractions;

public interface ITriggerEvaluator
{
    string Id { get; }
    string Name { get; set; }
    bool IsEnabled { get; set; }
    bool IsExecuting { get; }
    int CurrentWeight { get; }

    // Было: EvaluationResult Evaluate(ExecutionContext context);
    EvaluationResult Evaluate(TriggerExecutionContext context);

    void InterruptAndReset();
    void Reset();
}