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
        private System.Windows.Forms.Timer _debounceTimer;

        public SoundControl()
        {
            InitializeComponent();
            comboBoxSoundList.DropDown += (s, e) => RefreshSoundList();

            _debounceTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _debounceTimer.Tick += DebounceTimer_Tick;

            trackBarMusicVol.ValueChanged += (s, e) =>
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            };

            RefreshSoundList();
        }

        public string ComponentType => "SoundControl";

        public object GetDto() => new SoundDto
        {
            SoundFileName = comboBoxSoundList.Text.Trim(),
            Volume = trackBarMusicVol.Value
        };

        public void SetDtoFromElement(JsonElement element)
        {
            var dto = element.Deserialize<SoundDto>();
            if (dto == null) return;
            RefreshSoundList();
            comboBoxSoundList.Text = dto.SoundFileName;
            trackBarMusicVol.Value = Math.Clamp(dto.Volume, 0, 100);
        }

        private void DebounceTimer_Tick(object? sender, EventArgs e)
        {
            _debounceTimer.Stop();
            string fileName = comboBoxSoundList.Text.Trim();
            if (string.IsNullOrWhiteSpace(fileName)) return;
            SoundService.Play(fileName, trackBarMusicVol.Value, isPreview: true);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>DTO звукового шага.</summary>
    public class SoundDto
    {
        public string SoundFileName { get; set; } = string.Empty;
        public int Volume { get; set; } = 100;
    }
}
