using System.Text.Json;
using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    /// <summary>
    /// Универсальный контракт для компонентов редактора комбо.
    /// Позволяет создавать полную копию контрола без знания его конкретного типа,
    /// а также сериализовать/десериализовать его состояние в JSON.
    /// </summary>
    public interface IComboComponentControl
    {
        /// <summary>
        /// Строковый идентификатор типа компонента (используется при сохранении в JSON).
        /// </summary>
        string ComponentType { get; }

        /// <summary>
        /// Возвращает DTO-модель состояния контрола (boxed).
        /// </summary>
        object GetDto();

        /// <summary>
        /// Восстанавливает состояние контрола из JSON-элемента.
        /// </summary>
        void SetDtoFromElement(JsonElement element);
    }
}
