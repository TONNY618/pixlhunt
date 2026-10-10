using System.Linq;
using System.Windows.Forms;

namespace PixelMacroEngine.Core.Services;

/// <summary>
/// [Подсистема управления состоянием: Уровень 3 - Контроллер]
/// Определяет, разрешено ли выполнение комбо в текущий момент.
/// Комбо не выполняются, если мастер-флаг выключен, если открыто любое дочернее окно
/// (например, редактор комбо или настройки) или если активно главное окно приложения.
/// Связан с: Orchestrator, Main (UI).
/// </summary>
public static class BotStateController
{
    /// <summary>Мастер-флаг: включены ли комбо пользователем (галочка в главном окне).</summary>
    public static bool IsMasterEnabled { get; set; } = false;

    /// <summary>Время активной сессии (используется движком HumanizerEngine для расчёта усталости).</summary>
    public static TimeSpan ActiveSessionTime { get; set; } = TimeSpan.Zero;

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
