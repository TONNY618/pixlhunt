namespace PixelMacroEngine.Core.Models;

public class EvaluationResult
{
    public static readonly EvaluationResult None = new() { WantsToExecute = false };

    public bool WantsToExecute { get; set; }
    public int DynamicWeight { get; set; }
    public InputBatchCmd? BatchToExecute { get; set; }
}