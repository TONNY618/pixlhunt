using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    /// <summary>
    /// Универсальный контракт для компонентов редактора комбо.
    /// Позволяет создавать полную копию контрола без знания его конкретного типа.
    /// </summary>
    public interface IComboComponentControl
    {
        Control Duplicate();
    }
}
