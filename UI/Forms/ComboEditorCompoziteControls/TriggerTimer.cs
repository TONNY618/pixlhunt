using System;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    public partial class TriggerTimer : UserControl, IComboComponentControl
    {
        public TriggerTimer()
        {
            InitializeComponent();

            // Подписываем правое поле Max (левое поле и кнопка ? уже подписаны в Designer.cs)
            textBoxTriggerTimerMax.Leave += (s, e) => ValidateInterval();
            textBoxTriggerTimerMax.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ValidateInterval();
                    e.SuppressKeyPress = true;
                }
            };
        }

        public string ComponentType => "TriggerTimer";

        object IComboComponentControl.GetDto() => GetDto();

        public void SetDtoFromElement(JsonElement element) => SetDto(element.Deserialize<TriggerTimerDto>());

        public TriggerTimerDto GetDto()
        {
            ValidateInterval();
            int min = int.Parse(textBoxTriggerTimer.Text);
            int max = int.Parse(textBoxTriggerTimerMax.Text);
            return new TriggerTimerDto { MinMs = min, MaxMs = max };
        }

        public void SetDto(TriggerTimerDto? dto)
        {
            if (dto == null) return;
            int min = Math.Max(1, dto.MinMs);
            int max = Math.Max(min, dto.MaxMs);

            textBoxTriggerTimer.Text = min.ToString();
            textBoxTriggerTimerMax.Text = max.ToString();
        }

        /// <summary>
        /// Единая валидация: гарантирует 1 <= Min <= Max.
        /// Если Min > Max, поле Max автоматически приравнивается к Min.
        /// </summary>
        private void ValidateInterval()
        {
            int min = int.TryParse(textBoxTriggerTimer.Text, out int m) ? Math.Max(1, m) : 50;
            int max = int.TryParse(textBoxTriggerTimerMax.Text, out int mx) ? Math.Max(1, mx) : min;

            if (max < min)
            {
                max = min;
            }

            textBoxTriggerTimer.Text = min.ToString();
            textBoxTriggerTimerMax.Text = max.ToString();
        }

        // =====================================================================
        // ОБРАБОТЧИКИ, ПРИВЯЗАННЫЕ В ДИЗАЙНЕРЕ (TriggerTimer.Designer.cs)
        // =====================================================================

        private void textBoxTriggerTimer_Leave(object? sender, EventArgs e)
        {
            ValidateInterval();
        }

        private void textBoxTriggerTimer_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ValidateInterval();
                e.SuppressKeyPress = true;
            }
        }

        private void button1_Click(object? sender, EventArgs e)
        {
            string helpText =
                "Задержка между значениями (колоколообразное распределение):\n\n" +
                "1 000 мс = 1 секунда\n" +
                "10 000 мс = 10 секунд\n" +
                "60 000 мс = 1 минута\n" +
                "300 000 мс = 5 минут\n" +
                "3 600 000 мс = 1 час";

            using var helpForm = new Form
            {
                Text = "Справка: миллисекунды",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(290, 170)
            };

            var label = new Label
            {
                Text = helpText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14)
            };
            helpForm.Controls.Add(label);

            Form? parent = FindForm();
            if (parent != null)
                helpForm.ShowDialog(parent);
            else
                helpForm.ShowDialog();
        }
    }

    /// <summary>
    /// DTO интервала времени: используется как плавающий таймер в триггерах 
    /// и как пауза Task.Delay в действиях.
    /// </summary>
    public class TriggerTimerDto
    {
        public int MinMs { get; set; } = 50;
        public int MaxMs { get; set; } = 100;

        /// <summary>
        /// Алиас для обратной совместимости.
        /// </summary>
        public int IntervalMs
        {
            get => MinMs;
            set
            {
                MinMs = value;
                if (MaxMs < value) MaxMs = value;
            }
        }
    }
}