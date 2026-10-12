using System;
using System.Windows.Forms;

namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    public partial class TriggerTimer : UserControl
    {
        public TriggerTimer()
        {
            InitializeComponent();
        }

        public TriggerTimerDto GetDto()
        {
            return new TriggerTimerDto
            {
                IntervalMs = int.TryParse(textBoxTriggerTimer.Text, out int ms) ? Math.Max(10, ms) : 1000
            };
        }

        public void SetDto(TriggerTimerDto? dto)
        {
            if (dto == null) return;
            textBoxTriggerTimer.Text = dto.IntervalMs.ToString();
        }
    }

    /// <summary>
    /// DTO триггера периодического срабатывания по таймеру.
    /// Возвращает True каждые IntervalMs миллисекунд.
    /// </summary>
    public class TriggerTimerDto
    {
        public int IntervalMs { get; set; } = 1000;
    }
}
