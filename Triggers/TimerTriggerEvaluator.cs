using System;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers;

/// <summary>
/// [Подсистема движка: Уровень 2 - Триггер]
/// Триггер по таймеру: срабатывает через случайный интервал в диапазоне [minMs, maxMs].
/// Распределение интервалов — нормальное (Гаусс), что даёт плавную концентрацию
/// вокруг середины диапазона и снижает предсказуемость срабатываний (антидетект).
/// Связан с: ITriggerEvaluator, ComboFactory.
/// </summary>
public class TimerTriggerEvaluator : ITriggerEvaluator
{
    private readonly int _minMs;
    private readonly int _maxMs;
    private readonly Random _rnd = new Random();

    private int _currentIntervalMs;
    private DateTime _lastTriggerTime = DateTime.MinValue;

    /// <summary>
    /// Конструктор с диапазоном интервалов.
    /// </summary>
    public TimerTriggerEvaluator(int minMs, int maxMs)
    {
        _minMs = Math.Max(1, minMs);
        _maxMs = Math.Max(_minMs, maxMs);
        _currentIntervalMs = CalculateGaussianDelay(_minMs, _maxMs);
    }

    /// <summary>
    /// Fallback-конструктор с одним параметром: minMs = maxMs (фиксированный интервал).
    /// </summary>
    public TimerTriggerEvaluator(int intervalMs) : this(intervalMs, intervalMs)
    {
    }

    public bool Evaluate(FrameBuffer buffer)
    {
        if ((DateTime.UtcNow - _lastTriggerTime).TotalMilliseconds >= _currentIntervalMs)
        {
            _lastTriggerTime = DateTime.UtcNow;
            _currentIntervalMs = CalculateGaussianDelay(_minMs, _maxMs);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Расчёт задержки по нормальному распределению (Box-Muller).
    /// Среднее — середина диапазона, stdDev = (max - min) / 6,
    /// что укладывает ~99.7% значений в [min, max] до клампа.
    /// </summary>
    private int CalculateGaussianDelay(int min, int max)
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
