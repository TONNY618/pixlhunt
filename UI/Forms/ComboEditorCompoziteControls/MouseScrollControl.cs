using System.Text.Json;
using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    public partial class MouseScrollControl : UserControl, IComboComponentControl
    {
        public MouseScrollControl()
        {
            InitializeComponent();
        }

        public string ComponentType => "MouseScrollControl";

        public object GetDto()
        {
            int steps = int.TryParse(textBoxStepWhell.Text, out int s) && s > 0 ? s : 3;
            int windowMs = int.TryParse(textBoxWindowMs.Text, out int w) && w > 0 ? w : 350;
            return new MouseScrollDto
            {
                ScrollUp = radioButtonWhellUp.Checked,
                Steps = steps,
                WindowMs = windowMs
            };
        }

        public void SetDtoFromElement(JsonElement element)
        {
            var dto = element.Deserialize<MouseScrollDto>();
            if (dto == null) return;

            radioButtonWhellUp.Checked = dto.ScrollUp;
            radioButtonWhellDown.Checked = !dto.ScrollUp;
            textBoxStepWhell.Text = (dto.Steps > 0 ? dto.Steps : 3).ToString();
            textBoxWindowMs.Text = (dto.WindowMs > 0 ? dto.WindowMs : 350).ToString();
        }
    }

    public class MouseScrollDto
    {
        public bool ScrollUp { get; set; } = false;
        public int Steps { get; set; } = 3;

        /// <summary>Окно накопления тиков колесика для триггера (мс).</summary>
        public int WindowMs { get; set; } = 350;
    }
}
