using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    /// <summary>
    /// DTO-модель для контрола клавиши клавиатуры.
    /// </summary>
    public class BoxKeyDto
    {
        public string KeyName { get; set; } = "E";
        public bool PressDown { get; set; } = true;
        public bool PressUp { get; set; } = true;
    }

    public partial class BoxKeyControl : UserControl, IComboComponentControl
    {
        public BoxKeyControl()
        {
            InitializeComponent();
        }

        public string ComponentType => "BoxKeyControl";

        object IComboComponentControl.GetDto() => GetDto();

        public void SetDtoFromElement(JsonElement element) => SetDto(element.Deserialize<BoxKeyDto>());

        /// <summary>
        /// Считывает текущее состояние контролов в DTO.
        /// </summary>
        public BoxKeyDto GetDto()
        {
            return new BoxKeyDto
            {
                KeyName = comboBoxKeyList.Text,
                PressDown = checkBoxKey1.Checked,
                PressUp = checkBoxKey2.Checked
            };
        }

        /// <summary>
        /// Восстанавливает состояние контролов из DTO.
        /// </summary>
        public void SetDto(BoxKeyDto? dto)
        {
            if (dto == null) return;

            comboBoxKeyList.Text = dto.KeyName;
            checkBoxKey1.Checked = dto.PressDown;
            checkBoxKey2.Checked = dto.PressUp;
        }
    }
}
