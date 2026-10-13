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
        }

        public string ComponentType => "TriggerTimer";

        object IComboComponentControl.GetDto() => GetDto();

        public void SetDtoFromElement(JsonElement element) => SetDto(element.Deserialize<TriggerTimerDto>());

        /// <summary>
        /// Создает полную копию контрола через его DTO.
        /// </summary>
        public Control Duplicate()
        {
            var copy = new TriggerTimer();
            copy.SetDto(GetDto());
            return copy;
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
