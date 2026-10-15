using System;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

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

    private int _currentIntervalMs;
    private long _lastTriggerTimestamp = 0;

    /// <summary>
    /// Конструктор с диапазоном интервалов.
    /// </summary>
    public TimerTriggerEvaluator(int minMs, int maxMs)
    {
        _minMs = Math.Max(1, minMs);
        _maxMs = Math.Max(_minMs, maxMs);
        _currentIntervalMs = HumanizerEngine.CalculateGaussianDelay(_minMs, _maxMs);
    }

    /// <summary>
    /// Fallback-конструктор с одним параметром: minMs = maxMs (фиксированный интервал).
    /// </summary>
    public TimerTriggerEvaluator(int intervalMs) : this(intervalMs, intervalMs)
    {
    }

    public bool Evaluate(FrameBuffer buffer)
    {
        long now = TimeService.GetTimestamp();

        // Первый вызов (нет метки) трактуем как "интервал уже прошёл".
        if (_lastTriggerTimestamp == 0 ||
            TimeService.GetElapsedMilliseconds(_lastTriggerTimestamp, now) >= _currentIntervalMs)
        {
            _lastTriggerTimestamp = now;
            _currentIntervalMs = HumanizerEngine.CalculateGaussianDelay(_minMs, _maxMs);
            return true;
        }
        return false;
    }
}
