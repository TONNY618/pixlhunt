using System;
using System.Text.Json;
using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    public partial class TriggerTimer : UserControl, IComboComponentControl
    {
        public TriggerTimer()
        {
            InitializeComponent();
            textBoxTriggerTimerMax.Leave += TextBoxTriggerTimerMax_Leave;
        }

        public string ComponentType => "TriggerTimer";

        object IComboComponentControl.GetDto() => GetDto();

        public void SetDtoFromElement(JsonElement element) => SetDto(element.Deserialize<TriggerTimerDto>());

        public TriggerTimerDto GetDto()
        {
            int min = int.TryParse(textBoxTriggerTimer.Text, out int m) ? Math.Max(1, m) : 50;
            int max = int.TryParse(textBoxTriggerTimerMax.Text, out int mx) ? Math.Max(min, mx) : min;
            return new TriggerTimerDto { MinMs = min, MaxMs = max };
        }

        public void SetDto(TriggerTimerDto? dto)
        {
            if (dto == null) return;
            textBoxTriggerTimer.Text = dto.MinMs.ToString();
            textBoxTriggerTimerMax.Text = dto.MaxMs.ToString();
        }

        /// <summary>
        /// Валидация правого поля: если Max меньше Min, приравниваем Max = Min.
        /// </summary>
        private void TextBoxTriggerTimerMax_Leave(object? sender, EventArgs e)
        {
            int min = int.TryParse(textBoxTriggerTimer.Text, out int m) ? Math.Max(1, m) : 50;
            int max = int.TryParse(textBoxTriggerTimerMax.Text, out int mx) ? mx : min;
            if (max < min)
            {
                textBoxTriggerTimerMax.Text = min.ToString();
            }
        }

        /// <summary>
        /// Автокоррекция: если Max меньше Min, приравниваем Max = Min.
        /// </summary>
        private void AutoCorrectMax()
        {
            if (int.TryParse(textBoxTriggerTimer.Text, out int min) &&
                int.TryParse(textBoxTriggerTimerMax.Text, out int max))
            {
                if (max < min)
                {
                    textBoxTriggerTimerMax.Text = min.ToString();
                }
            }
        }

        private void textBoxTriggerTimer_Leave(object sender, EventArgs e)
        {
            AutoCorrectMax();
        }

        private void textBoxTriggerTimer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AutoCorrectMax();
                e.SuppressKeyPress = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string helpText =
                "Задержка между значениями, вероятность колокол\n" +
                "1 000 мс = 1 секунда\n" +
                "10 000 мс = 10 секунд\n" +
                "60 000 мс = 1 минута\n" +
                "300 000 мс = 5 минут\n" +
                "3 600 000 мс = 1 час";

            using (var helpForm = new Form())
            {
                helpForm.Text = "Справка: миллисекунды";
                helpForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                helpForm.StartPosition = FormStartPosition.CenterParent;
                helpForm.TopMost = true;
                helpForm.MinimizeBox = false;
                helpForm.MaximizeBox = false;
                helpForm.ShowInTaskbar = false;
                helpForm.ClientSize = new System.Drawing.Size(280, 160);

                var label = new Label
                {
                    Text = helpText,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    Padding = new Padding(12)
                };
                helpForm.Controls.Add(label);

                Form? parentForm = this.FindForm();
                if (parentForm != null)
                {
                    helpForm.ShowDialog(parentForm);
                }
                else
                {
                    helpForm.StartPosition = FormStartPosition.CenterScreen;
                    helpForm.ShowDialog();
                }
            }
        }
    }

    /// <summary>
    /// DTO триггера периодического срабатывания по таймеру.
    /// Возвращает True через случайный интервал в диапазоне [MinIntervalMs, MaxIntervalMs].
    /// </summary>
    public class TriggerTimerDto
    {
        public int MinMs { get; set; } = 50;
        public int MaxMs { get; set; } = 100;

        /// <summary>
        /// Алиас для обратной совместимости: чтение/запись MinMs.
        /// При записи автоматически подтягивает MaxMs, если он меньше.
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
