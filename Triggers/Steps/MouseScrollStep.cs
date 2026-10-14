using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Triggers.Steps;

/// <summary>
/// Шаг комбо: прокрутка колесика мыши (вверх/вниз).
/// </summary>
public class MouseScrollStep : IComboStep
{
    public bool ScrollUp { get; }
    public int Steps { get; }

    public MouseScrollStep(pxlhunt.UI.Forms.ComboEditorCompoziteControls.MouseScrollDto dto)
    {
        ScrollUp = dto.ScrollUp;
        Steps = dto.Steps > 0 ? dto.Steps : 3;
    }

    public MouseScrollStep(Dictionary<string, string>? p)
    {
        if (p == null)
        {
            ScrollUp = true;
            Steps = 3;
            return;
        }

        // Если радиокнопка Вверх включена — true, иначе крутим вниз
        ScrollUp = p.GetValueOrDefault("radioButtonWhellUp") == "True";

        if (int.TryParse(p.GetValueOrDefault("textBoxStepWhell", "3"), out int steps))
        {
            Steps = steps > 0 ? steps : 3;
        }
        else
        {
            Steps = 3;
        }
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken ct)
    {
        await MouseService.ScrollAsync(Steps, ScrollUp, context.ComboName ?? "Unknown", ct)
                          .ConfigureAwait(false);
    }
}
