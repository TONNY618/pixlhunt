using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers;

public class DynamicTrigger : ITriggerEvaluator
{
    public bool Evaluate(FrameBuffer buffer)
    {
        return false;
    }
}
