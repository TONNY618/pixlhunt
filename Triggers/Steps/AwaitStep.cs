using System;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Triggers.Steps;

/// <summary>
/// [Подсистема движка: Уровень 3 - Шаг]
/// Шаг ожидания: ждёт появления заданного цвета (максимум 5 секунд).
/// Связан с: IComboStep, ColorTriggerEvaluator, ComboFactory.
/// </summary>
public class AwaitStep : IComboStep
{
    private readonly ColorTriggerEvaluator? _evaluator;

    public AwaitStep(ColorTriggerEvaluator? evaluator)
    {
        _evaluator = evaluator;
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken cancellationToken)
    {
        if (_evaluator == null) return;

        // Ждём максимум 5 секунд
        using var timeoutCts = new CancellationTokenSource(5000);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            while (!linkedCts.Token.IsCancellationRequested)
            {
                // Используем актуальный кадр из контекста
                if (_evaluator.Evaluate(context.Frame))
                {
                    return; // Цвет найден, идём дальше
                }
                await Task.Delay(33, linkedCts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            if (timeoutCts.Token.IsCancellationRequested)
            {
                ActionLogger.Log($"[AWAIT TIMEOUT] Цвет не найден за 5 сек. Прерывание комбо {context.ComboName}.");
                throw new OperationCanceledException("Await timeout");
            }
            throw;
        }
    }
}
