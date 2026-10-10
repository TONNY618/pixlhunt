using PixelMacroEngine.Core.Abstractions;

namespace PixelMacroEngine.Core.Models;

public class TriggerExecutionContext
{
    public FrameBuffer Frame { get; set; } = new();
    public ITriggerEvaluator? CurrentlyActiveTrigger { get; set; }
    public int ActiveTriggerCurrentWeight { get; set; }

    /// <summary>Имя комбо, к которому относится текущий контекст выполнения.</summary>
    public string ComboName { get; set; } = string.Empty;
}
