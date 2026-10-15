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
                Text = textBoxTextToEdit.Text
            };
        }

        public void SetDtoFromElement(JsonElement element)
        {
            var dto = element.Deserialize<TextToDto>();
            if (dto == null) return;

            textBoxTextToEdit.Text = dto.Text;
        }
    }

    public class TextToDto
    {
        public string Text { get; set; } = string.Empty;
    }
}
