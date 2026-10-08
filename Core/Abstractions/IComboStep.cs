using System;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Abstractions;

public interface IComboStep
{
    // Проверка предусловий: таймер, цвет пикселя на свежем кадре
    bool IsConditionMet(FrameBuffer frame, DateTime lastStepExecutedTime);

    // Генерирует готовый пакет микро-действий для этого шага
    InputBatchCmd GenerateBatch();
}