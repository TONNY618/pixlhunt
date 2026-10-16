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

    /// <summary>
    /// Чистая проверка: готов ли интервал.
    /// Метод НЕ сбрасывает таймер, чтобы не сжигать тик, пока остальные триггеры не совпали.
    /// </summary>
    public bool Evaluate(FrameBuffer buffer)
    {
        // Если комбо ещё ни разу не выполнялось — таймер сразу готов
        if (_lastTriggerTimestamp == 0)
            return true;

        long now = TimeService.GetTimestamp();
        return TimeService.GetElapsedMilliseconds(_lastTriggerTimestamp, now) >= _currentIntervalMs;
    }

    /// <summary>
    /// Вызывается только тогда, когда комбо реально сработало и запустилось.
    /// </summary>
    public void Reset()
    {
        _lastTriggerTimestamp = TimeService.GetTimestamp();
        _currentIntervalMs = HumanizerEngine.CalculateGaussianDelay(_minMs, _maxMs);
    }

    /// <summary>
    /// Полный сброс (например, при выключении мастера).
    /// </summary>
    public void ResetToInitial()
    {
        _lastTriggerTimestamp = 0;
        _currentIntervalMs = HumanizerEngine.CalculateGaussianDelay(_minMs, _maxMs);
    }
}
