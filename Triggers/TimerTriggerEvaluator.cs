using System;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers;

/// <summary>
/// [Подсистема движка: Уровень 2 - Триггер]
/// Триггер по таймеру: срабатывает через случайный интервал в диапазоне [minMs, maxMs].
/// Распределение интервалов — вероятностный колокол (Irwin-Hall, n=3),
/// что даёт плавную концентрацию вокруг середины диапазона и снижает
/// предсказуемость срабатываний (антидетект).
/// Связан с: ITriggerEvaluator, ComboFactory.
/// </summary>
public class TimerTriggerEvaluator : ITriggerEvaluator
{
    private readonly int _minMs;
    private readonly int _maxMs;
    private readonly Random _random = new Random();

    private int _currentIntervalMs;
    private DateTime _lastTriggerTime = DateTime.MinValue;

    /// <summary>
    /// Конструктор с диапазоном интервалов.
    /// </summary>
    public TimerTriggerEvaluator(int minMs, int maxMs)
    {
        if (minMs < 1) minMs = 1;
        if (maxMs < minMs) maxMs = minMs;

        _minMs = minMs;
        _maxMs = maxMs;
        _currentIntervalMs = _minMs;
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
            RecalculateNextInterval();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Пересчёт следующего интервала по вероятностному колоколу (Irwin-Hall, n=3).
    /// Сумма трёх равномерных [0..1] даёт колоколообразное распределение,
    /// нормированное делением на 3 в диапазон [0..1].
    /// </summary>
    private void RecalculateNextInterval()
    {
        if (_maxMs <= _minMs)
        {
            _currentIntervalMs = _minMs;
            return;
        }

        double bellFactor = (_random.NextDouble() + _random.NextDouble() + _random.NextDouble()) / 3.0; // [0..1] колокол
        _currentIntervalMs = _minMs + (int)(bellFactor * (_maxMs - _minMs));
    }
}
