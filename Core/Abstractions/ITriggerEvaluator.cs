using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Abstractions;

/// <summary>
/// Базовый контракт триггера: проверяет условие срабатывания по свежему кадру.
/// </summary>
public interface ITriggerEvaluator
{
    bool Evaluate(FrameBuffer buffer);
}
