using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// [Подсистема управления состоянием: Уровень 3 - Контроллер]
/// Определяет, разрешено ли выполнение комбо в текущий момент.
/// Комбо не выполняются, если мастер-флаг выключен, если открыто любое дочернее окно
/// (например, редактор комбо или настройки) или если активно главное окно приложения.
/// Также ведёт волновую модель усталости: растёт при нагрузке, спадает при простое.
/// Связан с: Orchestrator, HumanizerEngine, Main (UI).
/// </summary>
public static class BotStateController
{
    /// <summary>Мастер-флаг: включены ли комбо пользователем (галочка в главном окне).</summary>
    public static bool IsMasterEnabled { get; set; } = false;

    /// <summary>Время активной сессии (используется движком HumanizerEngine для расчёта усталости).</summary>
    public static TimeSpan ActiveSessionTime { get; set; } = TimeSpan.Zero;

    // === Волновая модель усталости ===

    /// <summary>Минимальный уровень усталости (бодрый, восстановившийся).</summary>
    private const double FatigueFloor = 1.00;

    /// <summary>Максимальный уровень усталости (долгая непрерывная нагрузка).</summary>
    private const double FatigueCeiling = 1.40;

    /// <summary>Скорость роста усталости за секунду нагрузки (примерно +0.4 за 60 сек).</summary>
    private const double FatigueRisePerSecond = 0.0067;

    /// <summary>Скорость восстановления за секунду простоя (примерно -0.35 за 60 сек).</summary>
    private const double FatigueFallPerSecond = 0.0058;

    /// <summary>Текущий волновой уровень усталости (1.0 .. 1.4).</summary>
    public static double FatigueLevel { get; private set; } = FatigueFloor;

    /// <summary>Множитель усталости, применяемый HumanizerEngine к mu и sigma.</summary>
    public static double FatigueMultiplier => FatigueLevel;

    private static readonly Stopwatch _clock = Stopwatch.StartNew();
    private static TimeSpan _lastUpdate = TimeSpan.Zero;

    /// <summary>
    /// Сигнал от оркестратора: идёт фаза нагрузки (выполняется комбо).
    /// Усталость растёт.
    /// </summary>
    public static void NotifyLoad() => UpdateFatigue(isLoad: true);

    /// <summary>
    /// Сигнал от оркестратора: идёт фаза микро-отдыха (нет активных комбо,
    /// либо игрок двигается и комбо на паузе). Усталость спадает.
    /// </summary>
    public static void NotifyIdle() => UpdateFatigue(isLoad: false);

    /// <summary>
    /// Пересчитывает уровень усталости на основе времени, прошедшего с прошлого вызова.
    /// </summary>
    private static void UpdateFatigue(bool isLoad)
    {
        var now = _clock.Elapsed;
        double dt = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        if (dt <= 0) return;

        if (isLoad)
        {
            FatigueLevel += FatigueRisePerSecond * dt;
            if (FatigueLevel > FatigueCeiling) FatigueLevel = FatigueCeiling;
        }
        else
        {
            FatigueLevel -= FatigueFallPerSecond * dt;
            if (FatigueLevel < FatigueFloor) FatigueLevel = FatigueFloor;
        }
    }

    /// <summary>Сбрасывает усталость к базовому уровню (например, при старте новой сессии).</summary>
    public static void ResetFatigue()
    {
        FatigueLevel = FatigueFloor;
        _lastUpdate = _clock.Elapsed;
    }

    /// <summary>
    /// true, если открыто хотя бы одно видимое дочернее окно (кроме главного).
    /// </summary>
    public static bool HasOpenChildForms()
        => Application.OpenForms.OfType<Form>().Any(f => !(f is pxlhunt.FORMS.pxlHunt) && f.Visible);

    /// <summary>
    /// true, если активно (в фокусе) главное окно приложения.
    /// </summary>
    public static bool IsMainFormFocused()
        => Form.ActiveForm is pxlhunt.FORMS.pxlHunt;

    /// <summary>
    /// Итоговое разрешение на выполнение комбо.
    /// </summary>
    public static bool CanExecuteCombos
        => IsMasterEnabled && !HasOpenChildForms() && !IsMainFormFocused();
}
