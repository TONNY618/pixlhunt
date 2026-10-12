using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelMacroEngine.Core.Services;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    public partial class PixSquaControl : UserControl
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        private const int VK_CAPITAL = 0x14; // Caps Lock

        private System.Windows.Forms.Timer _evalTimer;

        public PixSquaControl()
        {
            InitializeComponent();

            // Настройка UI
            radioButtonPixSquaXY.CheckedChanged += RadioButtonMode_CheckedChanged;
            radioButtonPixSquaSqua.CheckedChanged += RadioButtonMode_CheckedChanged;

            trackBarPixSqua.ValueChanged += TrackBarPixSqua_ValueChanged;
            buttonPixSquaView.Click += ButtonPixSquaView_Click;
            pictureBoxPixSquaSample.MouseDown += PictureBoxPixSquaSample_MouseDown;

            // Инициализация фонового таймера для риал-тайм проверки (10 Гц)
            _evalTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _evalTimer.Tick += EvalTimer_Tick;
            _evalTimer.Start();

            UpdateUiMode();
        }

        private void RadioButtonMode_CheckedChanged(object? sender, EventArgs e) => UpdateUiMode();

        private void TrackBarPixSqua_ValueChanged(object? sender, EventArgs e)
        {
            // Инвертируем ползунок: 100 на UI = 0% отклонения (строго). 0 на UI = 100% отклонения
            int tolerance = 100 - trackBarPixSqua.Value;
            textBoxPixSquaVolNumber.Text = tolerance.ToString();
        }

        private void UpdateUiMode()
        {
            bool isSquare = radioButtonPixSquaSqua.Checked;
            textBoxPixSquaXY2.Visible = isSquare;
        }

        // =====================================================================
        // ЛОГИКА ЗАХВАТА КООРДИНАТ (ЛКМ + CAPS LOCK)
        // =====================================================================
        private async void PictureBoxPixSquaSample_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            bool isSquare = radioButtonPixSquaSqua.Checked;
            string originalTitle = groupBoxPixSqua.Text;

            Point? p1 = null;
            Point? p2 = null;
            bool capsWasPressed = false;

            if (isSquare) groupBoxPixSqua.Text = "Укажите первый угол (Нажмите CapsLock)";
            else groupBoxPixSqua.Text = "Наведите и нажмите CapsLock";

            // Пока зажата ЛКМ
            while (Control.MouseButtons == MouseButtons.Left)
            {
                Point currentPos = Cursor.Position;
                Color currentColor = ScreenCaptureService.GetPixel(currentPos.X, currentPos.Y);

                // Визуальное отображение под курсором в реальном времени
                pictureBoxPixSquaSample.BackColor = currentColor;

                bool capsIsPressed = (GetAsyncKeyState(VK_CAPITAL) & 0x8000) != 0;

                if (capsIsPressed && !capsWasPressed)
                {
                    // Триггер по нажатию Caps Lock
                    if (!isSquare)
                    {
                        // Режим точки: сразу сохраняем
                        textBoxPixSquaXY1.Text = $"{currentPos.X}, {currentPos.Y}";
                        textBoxPixSquaColor.Text = ColorToHex(currentColor);
                    }
                    else
                    {
                        // Режим квадрата: сохраняем 1 или 2 точку
                        if (p1 == null)
                        {
                            p1 = currentPos;
                            groupBoxPixSqua.Text = "Укажите второй угол (Нажмите CapsLock)";
                        }
                        else if (p2 == null)
                        {
                            p2 = currentPos;

                            // Нормализуем координаты левый-верх / правый-низ
                            int left = Math.Min(p1.Value.X, p2.Value.X);
                            int right = Math.Max(p1.Value.X, p2.Value.X);
                            int top = Math.Min(p1.Value.Y, p2.Value.Y);
                            int bottom = Math.Max(p1.Value.Y, p2.Value.Y);

                            textBoxPixSquaXY1.Text = $"{left}, {top}";
                            textBoxPixSquaXY2.Text = $"{right}, {bottom}";

                            // Вычисляем средний цвет региона
                            if (ScreenCaptureService.CurrentBuffer != null)
                            {
                                Color avgColor = ScreenCaptureService.CurrentBuffer.GetAverageColor(left, top, right, bottom);
                                textBoxPixSquaColor.Text = ColorToHex(avgColor);
                                pictureBoxPixSquaSample.BackColor = avgColor;
                            }
                        }
                    }
                }
                capsWasPressed = capsIsPressed;

                await Task.Delay(15);
            }

            // ЛКМ отпущена - возвращаем заголовок
            groupBoxPixSqua.Text = "? IF Pixel OR square ?";
        }

        // =====================================================================
        // ЛОГИКА ПРОСМОТРА ПОЗИЦИИ
        // =====================================================================
        private async void ButtonPixSquaView_Click(object? sender, EventArgs e)
        {
            Point? p = ParsePoint(textBoxPixSquaXY1.Text);
            if (p.HasValue)
            {
                Point orig = Cursor.Position;
                buttonPixSquaView.Enabled = false;
                Cursor.Position = p.Value;
                await Task.Delay(1000);
                Cursor.Position = orig;
                buttonPixSquaView.Enabled = true;
            }
        }

        // =====================================================================
        // ЛОГИКА РИАЛ-ТАЙМ ПРОВЕРКИ (TRUE / FALSE)
        // =====================================================================
        private void EvalTimer_Tick(object? sender, EventArgs e)
        {
            if (ScreenCaptureService.CurrentBuffer == null) return;

            Point? p1 = ParsePoint(textBoxPixSquaXY1.Text);
            Color targetColor = HexToColor(textBoxPixSquaColor.Text);
            int tolerance = 100 - trackBarPixSqua.Value; // 0..95

            bool isMatch = false;

            if (radioButtonPixSquaXY.Checked && p1.HasValue)
            {
                Color screenColor = ScreenCaptureService.CurrentBuffer.GetPixelColor(p1.Value.X, p1.Value.Y);
                isMatch = IsColorMatch(screenColor, targetColor, tolerance);
            }
            else if (radioButtonPixSquaSqua.Checked)
            {
                Point? p2 = ParsePoint(textBoxPixSquaXY2.Text);
                if (p1.HasValue && p2.HasValue)
                {
                    Color avgColor = ScreenCaptureService.CurrentBuffer.GetAverageColor(p1.Value.X, p1.Value.Y, p2.Value.X, p2.Value.Y);
                    isMatch = IsColorMatch(avgColor, targetColor, tolerance);
                }
            }

            // Учет условия Равно / Не равно
            bool finalResult = radioButtonPixSquaEqu.Checked ? isMatch : !isMatch;

            labelPixSquaBool.Text = finalResult.ToString();
            labelPixSquaBool.ForeColor = finalResult ? Color.Green : Color.Red;
        }

        private bool IsColorMatch(Color c1, Color c2, int tolerancePct)
        {
            if (tolerancePct == 0) return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;

            // Расстояние в RGB-пространстве (макс ~441)
            double distance = Math.Sqrt(Math.Pow(c1.R - c2.R, 2) + Math.Pow(c1.G - c2.G, 2) + Math.Pow(c1.B - c2.B, 2));
            double maxDistance = 441.67;
            double diffPct = (distance / maxDistance) * 100.0;

            return diffPct <= tolerancePct;
        }

        // =====================================================================
        // DTO И ВСПОМОГАТЕЛЬНЫЕ
        // =====================================================================
        public PixSquaDto GetDto()
        {
            return new PixSquaDto
            {
                IsSquareMode = radioButtonPixSquaSqua.Checked,
                Point1 = textBoxPixSquaXY1.Text,
                Point2 = textBoxPixSquaXY2.Text,
                ColorHex = textBoxPixSquaColor.Text,
                IsEqualLogic = radioButtonPixSquaEqu.Checked,
                Tolerance = 100 - trackBarPixSqua.Value,
                IsOverlayEnabled = checkBoxPixSquaOverlay.Checked
            };
        }

        public void SetDto(PixSquaDto? dto)
        {
            if (dto == null) return;
            radioButtonPixSquaSqua.Checked = dto.IsSquareMode;
            radioButtonPixSquaXY.Checked = !dto.IsSquareMode;
            textBoxPixSquaXY1.Text = dto.Point1;
            textBoxPixSquaXY2.Text = dto.Point2;
            textBoxPixSquaColor.Text = dto.ColorHex;
            radioButtonPixSquaEqu.Checked = dto.IsEqualLogic;
            radioButtonPixSquaNotEqu.Checked = !dto.IsEqualLogic;
            trackBarPixSqua.Value = 100 - dto.Tolerance;
            checkBoxPixSquaOverlay.Checked = dto.IsOverlayEnabled;

            UpdateUiMode();
        }

        private Point? ParsePoint(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var parts = text.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int x) && int.TryParse(parts[1].Trim(), out int y))
                return new Point(x, y);
            return null;
        }

        private string ColorToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        private Color HexToColor(string hex)
        {
            try { return ColorTranslator.FromHtml(hex); } catch { return Color.White; }
        }
    }

    public class PixSquaDto
    {
        public bool IsSquareMode { get; set; } = false;
        public string Point1 { get; set; } = "0, 0";
        public string Point2 { get; set; } = "0, 0";
        public string ColorHex { get; set; } = "#FFFFFF";
        public bool IsEqualLogic { get; set; } = true;
        public int Tolerance { get; set; } = 5;
        public bool IsOverlayEnabled { get; set; } = false;
    }
}
