using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers.Steps;

public class DelayStep : IComboStep
{
    private readonly int _minMs;
    private readonly int _maxMs;
    private static readonly Random _rnd = new Random();

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
        int delay = CalculateGaussianDelay(_minMs, _maxMs);
        if (delay > 0)
        {
            await Task.Delay(delay, cancellationToken);
        }
    }

    /// <summary>
    /// Расчёт задержки по нормальному распределению (Box-Muller).
    /// Среднее — середина диапазона, stdDev = (max - min) / 6.
    /// </summary>
    private static int CalculateGaussianDelay(int min, int max)
    {
        if (min >= max) return min;

        double u1 = 1.0 - _rnd.NextDouble();
        double u2 = 1.0 - _rnd.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        double mean = (min + max) / 2.0;
        double stdDev = (max - min) / 6.0;
        double val = mean + randStdNormal * stdDev;

        return (int)Math.Clamp(Math.Round(val), min, max);
    }
}
