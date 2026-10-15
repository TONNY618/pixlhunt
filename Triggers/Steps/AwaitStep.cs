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

        // Ждём максимум 5 секунд. Отсчёт ведём по аппаратному таймстампу TimeService,
        // чтобы таймаут не зависел от коррекции системных часов.
        const int timeoutMs = 5000;
        long startTs = TimeService.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested)
        {
            // Используем актуальный кадр из контекста
            if (_evaluator.Evaluate(context.Frame))
            {
                return; // Цвет найден, идём дальше
            }

            if (TimeService.GetElapsedMilliseconds(startTs) >= timeoutMs)
            {
                ActionLogger.Log($"[AWAIT TIMEOUT] Цвет не найден за 5 сек. Прерывание комбо {context.ComboName}.");
                throw new OperationCanceledException("Await timeout");
            }

            await Task.Delay(33, cancellationToken).ConfigureAwait(false);
        }

        // Внешняя отмена — пробрасываем наверх.
        cancellationToken.ThrowIfCancellationRequested();
    }
}
