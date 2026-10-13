using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelMacroEngine.Core.Services;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    public partial class CursorMoveControl : UserControl, IComboComponentControl
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        private const int VK_CAPITAL = 0x14;

        public CursorMoveControl()
        {
            InitializeComponent();

            radioButtonCurMooveGetXY.CheckedChanged += ModeChanged;
            radioButtonCurMooveVol.CheckedChanged += ModeChanged;
            radioButtonCurMooveXY.CheckedChanged += ModeChanged;

            pictureBoxCurMooveSample.MouseDown += PictureBox_MouseDown;
            buttonCurMooveView.Click += ButtonView_Click;

            UpdateUiState();
        }

        private void ModeChanged(object? sender, EventArgs e) => UpdateUiState();

        private void UpdateUiState()
        {
            bool isGetXY = radioButtonCurMooveGetXY.Checked;
            bool isRegion = radioButtonCurMooveVol.Checked;

            // Если GetXY - прячем почти всё
            buttonCurMooveView.Visible = !isGetXY;
            pictureBoxCurMooveSample.Visible = !isGetXY;
            textBoxCurMoove1.Visible = !isGetXY;
            checkBoxCurMooveOverlay.Visible = !isGetXY;
            labelCurMoove2.Visible = !isGetXY;

            // Второе поле видно только в режиме Региона
            textBoxCurMoove2.Visible = isRegion && !isGetXY;
            labelCurMoove3.Visible = isRegion && !isGetXY;
        }

        private async void PictureBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            pictureBoxCurMooveSample.Capture = true;
            bool isRegion = radioButtonCurMooveVol.Checked;
            string origText = groupBoxCurMoove.Text;

            Point? p1 = null;
            Point? p2 = null;
            bool capsWasPressed = false;

            groupBoxCurMoove.Text = isRegion ? "Укажите 1-й угол (CapsLock)" : "Наведите и нажмите CapsLock";

            try
            {
                while (Control.MouseButtons == MouseButtons.Left)
                {
                    bool capsIsPressed = (GetAsyncKeyState(VK_CAPITAL) & 0x8000) != 0;

                    if (capsIsPressed && !capsWasPressed)
                    {
                        Point curPos = Cursor.Position;

                        if (!isRegion)
                        {
                            textBoxCurMoove1.Text = $"{curPos.X}, {curPos.Y}";
                        }
                        else
                        {
                            if (p1 == null)
                            {
                                p1 = curPos;
                                groupBoxCurMoove.Text = "Укажите 2-й угол (CapsLock)";
                            }
                            else
                            {
                                p2 = curPos;
                                int left = Math.Min(p1.Value.X, p2.Value.X);
                                int right = Math.Max(p1.Value.X, p2.Value.X);
                                int top = Math.Min(p1.Value.Y, p2.Value.Y);
                                int bottom = Math.Max(p1.Value.Y, p2.Value.Y);

                                textBoxCurMoove1.Text = $"{left}, {top}";
                                textBoxCurMoove2.Text = $"{right}, {bottom}";
                            }
                        }
                    }
                    capsWasPressed = capsIsPressed;
                    await Task.Delay(15);
                }
            }
            finally
            {
                pictureBoxCurMooveSample.Capture = false;
                groupBoxCurMoove.Text = "-> Курсор на коорд двиг ->";
            }
        }

        private async void ButtonView_Click(object? sender, EventArgs e)
        {
            Point? p1 = ParsePoint(textBoxCurMoove1.Text);
            if (!p1.HasValue) return;

            Point target;
            if (radioButtonCurMooveVol.Checked)
            {
                Point? p2 = ParsePoint(textBoxCurMoove2.Text);
                if (!p2.HasValue) return;
                target = new Point((p1.Value.X + p2.Value.X) / 2, (p1.Value.Y + p2.Value.Y) / 2);
            }
            else
            {
                target = p1.Value;
            }

            Point orig = Cursor.Position;
            buttonCurMooveView.Enabled = false;

            try
            {
                await MouseService.MoveToAsync(target.X, target.Y, 50, "Preview", System.Threading.CancellationToken.None);
                await Task.Delay(1000);
                await MouseService.MoveToAsync(orig.X, orig.Y, 50, "PreviewRet", System.Threading.CancellationToken.None);
            }
            finally
            {
                buttonCurMooveView.Enabled = true;
            }
        }

        private Point? ParsePoint(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var parts = text.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int x) && int.TryParse(parts[1].Trim(), out int y))
                return new Point(x, y);
            return null;
        }

        public string ComponentType => "CursorMoveControl";
        public Control Duplicate() => throw new NotImplementedException("Used via Reflection in main form");

        public object GetDto()
        {
            int mode = radioButtonCurMooveGetXY.Checked ? 0 : (radioButtonCurMooveVol.Checked ? 1 : 2);
            return new CursorMoveDto
            {
                Mode = mode,
                Point1 = textBoxCurMoove1.Text,
                Point2 = textBoxCurMoove2.Text,
                IsOverlayEnabled = checkBoxCurMooveOverlay.Checked
            };
        }

        public void SetDtoFromElement(System.Text.Json.JsonElement element)
        {
            var dto = element.Deserialize<CursorMoveDto>();
            if (dto == null) return;

            radioButtonCurMooveGetXY.Checked = dto.Mode == 0;
            radioButtonCurMooveVol.Checked = dto.Mode == 1;
            radioButtonCurMooveXY.Checked = dto.Mode == 2;

            textBoxCurMoove1.Text = dto.Point1;
            textBoxCurMoove2.Text = dto.Point2;
            checkBoxCurMooveOverlay.Checked = dto.IsOverlayEnabled;

            UpdateUiState();
        }
    }

    public class CursorMoveDto
    {
        public int Mode { get; set; } = 1; // 0=GetXY, 1=Region, 2=Point
        public string Point1 { get; set; } = "0, 0";
        public string Point2 { get; set; } = "0, 0";
        public bool IsOverlayEnabled { get; set; } = false;
    }
}
