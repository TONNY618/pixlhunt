using System.Text.Json;
using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    public partial class MouseKeyControl : UserControl, IComboComponentControl
    {
        public MouseKeyControl()
        {
            InitializeComponent();
        }

        public string ComponentType => "MouseKeyControl";

        /// <summary>
        /// Скрывает чекбоксы down/up, если контрол размещён в панели триггеров.
        /// </summary>
        private void UpdateTriggerMode()
        {
            bool isTrigger = Parent != null &&
                string.Equals(Parent.Name, "flowLayoutPanelTrigger", StringComparison.OrdinalIgnoreCase);

            checkBoxMouseKey1.Visible = !isTrigger;
            checkBoxMouseKey2.Visible = !isTrigger;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateTriggerMode();
        }

        public object GetDto()
        {
            return new MouseKeyDto
            {
                Button = comboBoxMouseKeyList.Text,
                PressDown = checkBoxMouseKey1.Checked,
                PressUp = checkBoxMouseKey2.Checked
            };
        }

        public void SetDtoFromElement(JsonElement element)
        {
            var dto = element.Deserialize<MouseKeyDto>();
            if (dto == null) return;

            comboBoxMouseKeyList.Text = dto.Button;
            checkBoxMouseKey1.Checked = dto.PressDown;
            checkBoxMouseKey2.Checked = dto.PressUp;

            UpdateTriggerMode();
        }
    }

    public class MouseKeyDto
    {
        public string Button { get; set; } = "L key";
        public bool PressDown { get; set; } = true;
        public bool PressUp { get; set; } = true;
    }
}
