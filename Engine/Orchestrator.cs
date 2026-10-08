using System.Collections.Generic;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Engine;

public class Orchestrator
{
    private readonly List<ITriggerEvaluator> _triggers = new();
    private readonly SimpleTaskQueue _queue;
    private readonly InputDispatcher _dispatcher;
    private ITriggerEvaluator? _activeExclusiveTrigger = null;

    public Orchestrator(SimpleTaskQueue queue, InputDispatcher dispatcher)
    {
        _queue = queue;
        _dispatcher = dispatcher;
    }

    public void RegisterTrigger(ITriggerEvaluator trigger) => _triggers.Add(trigger);

    public void ProcessFrame(FrameBuffer frame)
    {
        var context = new TriggerExecutionContext
        {
            Frame = frame,
            CurrentlyActiveTrigger = _activeExclusiveTrigger,
            ActiveTriggerCurrentWeight = _activeExclusiveTrigger?.CurrentWeight ?? 0
        };

        ITriggerEvaluator? bestCandidate = null;
        EvaluationResult? bestResult = null;

        // Опрашиваем триггеры и ищем победителя аукциона
        foreach (var trigger in _triggers)
        {
            if (!trigger.IsEnabled) continue;

            var result = trigger.Evaluate(context);
            if (result.WantsToExecute)
            {
                if (bestResult == null || result.DynamicWeight > bestResult.DynamicWeight)
                {
                    bestCandidate = trigger;
                    bestResult = result;
                }
            }
        }

        if (bestCandidate == null || bestResult == null || bestResult.BatchToExecute == null)
            return;

        // Если победитель пытается перебить уже выполняющийся триггер
        if (_activeExclusiveTrigger != null && _activeExclusiveTrigger != bestCandidate)
        {
            // Строгое правило: перебить можно, только если новый вес строго больше
            if (bestResult.DynamicWeight > _activeExclusiveTrigger.CurrentWeight)
            {
                _activeExclusiveTrigger.InterruptAndReset();
                _dispatcher.EmergencyClear(); // Сброс очереди и клавиш
                _activeExclusiveTrigger = bestCandidate;
            }
            else
            {
                // Недостаточно веса — уступаем текущему исполнителю
                return;
            }
        }
        else
        {
            _activeExclusiveTrigger = bestCandidate;
        }

        // Закидываем непрерываемый батч в очередь диспетчера
        _queue.Enqueue(bestResult.BatchToExecute);

        // Если триггер закончил комбо — отпускаем монополию
        if (!_activeExclusiveTrigger.IsExecuting)
        {
            _activeExclusiveTrigger = null;
        }
    }
}