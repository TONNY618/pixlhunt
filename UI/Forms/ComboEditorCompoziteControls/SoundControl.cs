using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

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
            buttonSoundPlay.Click += ButtonSoundAddCustom_Click;
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

        private void ButtonSoundAddCustom_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Выберите аудиофайл",
                Filter = "Аудио (*.wav;*.mp3)|*.wav;*.mp3|Все файлы (*.*)|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string targetDir = GetSoundsDirectory();
                string destPath = Path.Combine(targetDir, Path.GetFileName(ofd.FileName));
                try
                {
                    File.Copy(ofd.FileName, destPath, overwrite: true);
                    RefreshSoundList();
                    comboBoxSoundList.Text = Path.GetFileName(ofd.FileName);
                }
                catch { }
            }
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
