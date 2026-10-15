using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;
using pxlhunt.UI.Forms.ComboEditorCompoziteControls;

namespace PixelMacroEngine.Triggers.Steps
{
    /// <summary>
    /// Неблокирующий шаг: воспроизводит звук в фоне, не занимая слот умения
    /// и не расходуя общий кулдаун (ГКД).
    /// </summary>
    public class PlaySoundStep : INonBlockingStep
    {
        public SoundDto Dto { get; }

        public PlaySoundStep(SoundDto dto)
        {
            Dto = dto;
        }

        public Task ExecuteAsync(TriggerExecutionContext context, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(Dto.SoundFileName))
            {
                SoundService.Play(Dto.SoundFileName, Dto.Volume);
            }
            return Task.CompletedTask;
        }
    }
}
