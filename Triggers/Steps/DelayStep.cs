using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Triggers.Steps;

public class DelayStep : IComboStep
{
    private readonly int _minMs;
    private readonly int _maxMs;

    public int DelayMs => _minMs;

    public DelayStep(Dictionary<string, string> parameters)
    {
        if (parameters != null && int.TryParse(parameters.GetValueOrDefault("delaymsTextBox"), out int delay))
        {
            _minMs = Math.Max(1, delay);
            _maxMs = _minMs;
        }
        else
        {
            _minMs = 100; // По умолчанию
            _maxMs = 100;
        }
    }

    public DelayStep(int minMs, int maxMs)
    {
        _minMs = Math.Max(1, minMs);
        _maxMs = Math.Max(_minMs, maxMs);
    }

    public DelayStep(int delayMs) : this(delayMs, delayMs)
    {
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken cancellationToken)
    {
        int delay = HumanizerEngine.CalculateGaussianDelay(_minMs, _maxMs);
        if (delay > 0)
        {
            await Task.Delay(delay, cancellationToken);
        }
    }
}
