using System;
using System.Collections.Generic;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Triggers;

public class DynamicTrigger : ITriggerEvaluator
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Новый триггер";
    public bool IsEnabled { get; set; } = true;

    public int BaseWeight { get; set; } = 10;
    public int MaxWeight { get; set; } = 10;
    public int CooldownMs { get; set; } = 500;
    public int StepTimeoutMs { get; set; } = 2500;

    public List<IComboStep> Steps { get; set; } = new();

    private int _currentStepIndex = 0;
    private DateTime _lastStepTime = DateTime.MinValue;
    private DateTime _cooldownUntil = DateTime.MinValue;

    public bool IsExecuting => _currentStepIndex > 0 && _currentStepIndex < Steps.Count;

    // Растущий вес: чем ближе к финалу цепочки, тем выше вес
    public int CurrentWeight
    {
        get
        {
            if (Steps.Count <= 1 || _currentStepIndex == 0)
                return BaseWeight;

            float progress = (float)_currentStepIndex / (Steps.Count - 1);
            return (int)(BaseWeight + (MaxWeight - BaseWeight) * progress);
        }
    }

    public EvaluationResult Evaluate(TriggerExecutionContext context)
    {
        if (!IsEnabled || Steps.Count == 0 || DateTime.Now < _cooldownUntil)
            return EvaluationResult.None;

        // Защита от зависания посередине комбо
        if (IsExecuting && (DateTime.Now - _lastStepTime).TotalMilliseconds > StepTimeoutMs)
        {
            Reset();
            return EvaluationResult.None;
        }

        var currentStep = Steps[_currentStepIndex];

        if (currentStep.IsConditionMet(context.Frame, _lastStepTime))
        {
            var batch = currentStep.GenerateBatch();
            _lastStepTime = DateTime.Now;
            _currentStepIndex++;

            if (_currentStepIndex >= Steps.Count)
            {
                _cooldownUntil = DateTime.Now.AddMilliseconds(CooldownMs);
                _currentStepIndex = 0;
            }

            return new EvaluationResult
            {
                WantsToExecute = true,
                DynamicWeight = this.CurrentWeight,
                BatchToExecute = batch
            };
        }

        return EvaluationResult.None;
    }

    public void InterruptAndReset() => Reset();

    public void Reset()
    {
        _currentStepIndex = 0;
        _lastStepTime = DateTime.MinValue;
    }
}