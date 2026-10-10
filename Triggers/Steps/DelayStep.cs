using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers.Steps;

public class DelayStep : IComboStep
{
    public int DelayMs { get; }

    public DelayStep(Dictionary<string, string> parameters)
    {
        if (parameters != null && int.TryParse(parameters.GetValueOrDefault("delaymsTextBox"), out int delay))
        {
            DelayMs = delay;
        }
        else
        {
            DelayMs = 100; // По умолчанию
        }
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken cancellationToken)
    {
        if (DelayMs > 0)
        {
            await Task.Delay(DelayMs, cancellationToken);
        }
    }
}
