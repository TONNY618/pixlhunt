using System.Text.Json;
using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    public partial class TextToControl : UserControl, IComboComponentControl
    {
        public TextToControl()
        {
            InitializeComponent();
        }

        public string ComponentType => "TextToControl";

        public object GetDto()
        {
            return new TextToDto
            {
                IsContext = radioButtonTextToContext.Checked,
                TelegramUser = textBoxTextToTeleNameUser.Text.Trim(),
                Text = textBoxTextToEdit.Text
            };
        }

        public void SetDtoFromElement(JsonElement element)
        {
            var dto = element.Deserialize<TextToDto>();
            if (dto == null) return;

            radioButtonTextToContext.Checked = dto.IsContext;
            radioButtonTextToTelegtamm.Checked = !dto.IsContext;
            textBoxTextToTeleNameUser.Text = dto.TelegramUser;
            textBoxTextToEdit.Text = dto.Text;
        }
    }

    public class TextToDto
    {
        public bool IsContext { get; set; } = true;
        public string TelegramUser { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
