using System;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Abstractions;

/// <summary>
/// Базовый контракт шага комбо: выполняет асинхронное действие в контексте триггера.
/// </summary>
public interface IComboStep
{
    Task ExecuteAsync(PixelMacroEngine.Core.Models.TriggerExecutionContext context, CancellationToken ct);
}

public interface IComboStep
{
    // Проверка предусловий: таймер, цвет пикселя на свежем кадре
    bool IsConditionMet(FrameBuffer frame, DateTime lastStepExecutedTime);

    // Генерирует готовый пакет микро-действий для этого шага
    InputBatchCmd GenerateBatch();
}
