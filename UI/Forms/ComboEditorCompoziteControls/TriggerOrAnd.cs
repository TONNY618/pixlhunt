using System;
using System.Text.Json;
using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    public partial class TriggerOrAnd : UserControl, IComboComponentControl
    {
        public TriggerOrAnd()
        {
            InitializeComponent();
        }

        public string ComponentType => "TriggerOrAnd";

        object IComboComponentControl.GetDto() => GetDto();

        public void SetDtoFromElement(JsonElement element) => SetDto(element.Deserialize<TriggerOrAndDto>());

        /// <summary>
        /// Автономная сборка состояния контрола в чистую DTO-модель.
        /// </summary>
        public TriggerOrAndDto GetDto()
        {
            return new TriggerOrAndDto
            {
                IsOrLogic = radioButtonTriggerOrAnd1.Checked
            };
        }

        /// <summary>
        /// Автономное восстановление состояния контрола из DTO (при загрузке JSON).
        /// </summary>
        public void SetDto(TriggerOrAndDto? dto)
        {
            if (dto == null) return;
            radioButtonTriggerOrAnd1.Checked = dto.IsOrLogic;
            radioButtonTriggerOrAnd2.Checked = !dto.IsOrLogic;
        }
    }

    /// <summary>
    /// Чистый DTO для сохранения логики И / ИЛИ в JSON.
    /// </summary>
    public class TriggerOrAndDto
    {
        public bool IsOrLogic { get; set; } = true;
    }
}
