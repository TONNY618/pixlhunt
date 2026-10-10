using System;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers;

/// <summary>
/// [Подсистема движка: Уровень 2 - Триггер]
/// Триггер по таймеру: срабатывает не чаще, чем раз в intervalMs миллисекунд.
/// Связан с: ITriggerEvaluator, ComboFactory.
/// </summary>
public class TimerTriggerEvaluator : ITriggerEvaluator
{
    private readonly int _intervalMs;
    private DateTime _lastTriggerTime = DateTime.MinValue;

    public TimerTriggerEvaluator(int intervalMs)
    {
        _intervalMs = intervalMs;
    }

    public bool Evaluate(FrameBuffer buffer)
    {
        if ((DateTime.UtcNow - _lastTriggerTime).TotalMilliseconds >= _intervalMs)
        {
            _lastTriggerTime = DateTime.UtcNow;
            return true;
        }
        return false;
    }
}
