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

    // === Трёхсостоянийная биомеханическая модель усталости ===

    /// <summary>
    /// Физиологическое состояние игрока в текущий момент.
    /// </summary>
    public enum PhysiologicalState
    {
        /// <summary>Выполнение боевого комбо — максимальная нагрузка на кисть.</summary>
        Action,

        /// <summary>Удержание WASD — статическое напряжение, усталость не спадает.</summary>
        Navigation,

        /// <summary>Полный простой — активное восстановление.</summary>
        TrueIdle
    }

    /// <summary>Минимальный уровень усталости (бодрый, восстановившийся).</summary>
    private const double FatigueFloor = 1.00;

    /// <summary>Максимальный уровень усталости (долгая непрерывная нагрузка).</summary>
    private const double FatigueCeiling = 1.40;

    /// <summary>Скорость роста усталости за секунду в состоянии Action (примерно +0.4 за 60 сек).</summary>
    private const double FatigueRiseActionPerSecond = 0.0067;

    /// <summary>
    /// Скорость роста усталости за секунду в состоянии Navigation.
    /// 10% от Action — статическое напряжение кисти почти не утомляет,
    /// но и не даёт восстановиться.
    /// </summary>
    private const double FatigueRiseNavigationPerSecond = FatigueRiseActionPerSecond * 0.10;

    /// <summary>Скорость восстановления за секунду в состоянии TrueIdle (примерно -0.35 за 60 сек).</summary>
    private const double FatigueFallIdlePerSecond = 0.0058;

    /// <summary>Текущий волновой уровень усталости (1.0 .. 1.4).</summary>
    public static double FatigueLevel { get; private set; } = FatigueFloor;

    /// <summary>Множитель усталости, применяемый HumanizerEngine к mu и sigma.</summary>
    public static double FatigueMultiplier => FatigueLevel;

    /// <summary>Текущее физиологическое состояние (для телеметрии/отладки).</summary>
    public static PhysiologicalState CurrentState { get; private set; } = PhysiologicalState.TrueIdle;

    /// <summary>
    /// Обновляет уровень усталости на основе реального прошедшего времени (DeltaTime).
    /// Полностью отвязано от частоты кадров/тиков — только секунды.
    /// </summary>
    /// <param name="state">Текущее физиологическое состояние.</param>
    /// <param name="deltaSeconds">Реальное время с прошлого вызова, в секундах.</param>
    public static void Update(PhysiologicalState state, double deltaSeconds)
    {
        CurrentState = state;

        if (deltaSeconds <= 0 || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds))
            return;

        // Защита от гигантских скачков DeltaTime (например, после паузы отладчика).
        if (deltaSeconds > 1.0)
            deltaSeconds = 1.0;

        // Точное аналитическое решение дифференциального уравнения dF/dt = -k*(F - Target).
        // В отличие от метода Эйлера, экспонента НИКОГДА не пробивает целевой уровень
        // и не даёт расходящихся колебаний даже при гигантских скачках deltaSeconds
        // (GC-паузы, подвисания потока, отладчик).
        switch (state)
        {
            case PhysiologicalState.Action:
                // Асимптотическое приближение к потолку усталости.
                FatigueLevel = FatigueCeiling
                    + (FatigueLevel - FatigueCeiling) * Math.Exp(-FatigueRiseActionPerSecond * deltaSeconds);
                break;

            case PhysiologicalState.Navigation:
                // Статическое изометрическое напряжение (WASD).
                // Предел статической усталости ниже динамической — 85% от максимума.
                // Экспонента работает в обе стороны: из Action усталость плавно спадёт
                // до 85%, из TrueIdle — плавно вырастет до 85%.
                double isometricCeiling = FatigueFloor + (FatigueCeiling - FatigueFloor) * 0.85;
                FatigueLevel = isometricCeiling
                    + (FatigueLevel - isometricCeiling) * Math.Exp(-FatigueRiseNavigationPerSecond * deltaSeconds);
                break;

            case PhysiologicalState.TrueIdle:
                // Экспоненциальное восстановление к базовому уровню.
                FatigueLevel = FatigueFloor
                    + (FatigueLevel - FatigueFloor) * Math.Exp(-FatigueFallIdlePerSecond * deltaSeconds);
                break;
        }

        if (FatigueLevel > FatigueCeiling) FatigueLevel = FatigueCeiling;
        if (FatigueLevel < FatigueFloor) FatigueLevel = FatigueFloor;
    }

    /// <summary>Сбрасывает усталость к базовому уровню (например, при старте новой сессии).</summary>
    public static void ResetFatigue()
    {
        FatigueLevel = FatigueFloor;
        CurrentState = PhysiologicalState.TrueIdle;
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
