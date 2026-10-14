using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using PixelMacroEngine.Core.Services;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    /// <summary>
    /// UserControl выбора звука для неблокирующего шага PlaySoundStep.
    /// </summary>
    public partial class SoundControl : UserControl, IComboComponentControl
    {
        public SoundControl()
        {
            InitializeComponent();
            comboBoxSoundList.DropDown += (s, e) => RefreshSoundList();
            buttonSoundPlay.Click += ButtonSoundPlay_Click;
            RefreshSoundList();
        }

        public string ComponentType => "SoundControl";

        public object GetDto() => new SoundDto { SoundFileName = comboBoxSoundList.Text.Trim() };

        public void SetDtoFromElement(JsonElement element)
        {
            var dto = element.Deserialize<SoundDto>();
            if (dto == null) return;
            RefreshSoundList();
            comboBoxSoundList.Text = dto.SoundFileName;
        }

        private void RefreshSoundList()
        {
            string selected = comboBoxSoundList.Text;
            comboBoxSoundList.Items.Clear();

            string dir = GetSoundsDirectory();
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir, "*.*"))
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext == ".wav" || ext == ".mp3")
                    {
                        comboBoxSoundList.Items.Add(Path.GetFileName(file));
                    }
                }
            }

            if (!string.IsNullOrEmpty(selected)) comboBoxSoundList.Text = selected;
            else if (comboBoxSoundList.Items.Count > 0) comboBoxSoundList.SelectedIndex = 0;
        }

        /// <summary>
        /// Проигрывает звук, выбранный в данный момент в comboBoxSoundList.
        /// </summary>
        private void ButtonSoundPlay_Click(object? sender, EventArgs e)
        {
            string fileName = comboBoxSoundList.Text.Trim();
            if (string.IsNullOrWhiteSpace(fileName)) return;

            SoundService.Play(fileName);
        }

        private static string GetSoundsDirectory()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Favorite");
            if (!Directory.Exists(dir))
            {
                string fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Sounds", "Favorite");
                if (Directory.Exists(fallback)) return Path.GetFullPath(fallback);
                Directory.CreateDirectory(dir);
            }
            return dir;
        }
    }

    /// <summary>DTO звукового шага.</summary>
    public class SoundDto
    {
        public string SoundFileName { get; set; } = string.Empty;
    }
}
