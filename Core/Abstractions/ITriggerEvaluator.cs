using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Abstractions;

/// <summary>
/// Базовый контракт триггера: проверяет условие срабатывания по свежему кадру.
/// </summary>
public interface ITriggerEvaluator
{
    bool Evaluate(PixelMacroEngine.Core.Models.FrameBuffer buffer);
}

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
