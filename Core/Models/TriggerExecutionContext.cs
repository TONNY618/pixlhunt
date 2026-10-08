using PixelMacroEngine.Core.Abstractions;

namespace PixelMacroEngine.Core.Models;

public class TriggerExecutionContext
{
    public FrameBuffer Frame { get; set; } = new();
    public ITriggerEvaluator? CurrentlyActiveTrigger { get; set; }
    public int ActiveTriggerCurrentWeight { get; set; }
}