using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Triggers.Steps;

/// <summary>
/// Шаг комбо: клик, зажатие или отпускание кнопки мыши (ЛКМ, ПКМ, СКМ).
/// </summary>
public class MouseClickStep : IComboStep
{
    public byte Button { get; }
    public bool PressDown { get; }
    public bool PressUp { get; }

    public MouseClickStep(byte button, Dictionary<string, string>? p, string suffix)
    {
        Button = button;

        if (p == null)
        {
            // По умолчанию полный клик (нажал + отпустил)
            PressDown = true;
            PressUp = true;
            return;
        }

        string downKey = $"checkBoxUpDownMouseKey{suffix}_1";
        string upKey = $"checkBoxUpDownMouseKey{suffix}_2";

        bool hasDown = p.ContainsKey(downKey);
        bool hasUp = p.ContainsKey(upKey);

        // Если в параметрах нет явных ключей, считаем полным кликом
        if (!hasDown && !hasUp)
        {
            PressDown = true;
            PressUp = true;
        }
        else
        {
            PressDown = p.GetValueOrDefault(downKey) == "True";
            PressUp = p.GetValueOrDefault(upKey) == "True";
        }
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken ct)
    {
        if (!PressDown && !PressUp) return;

        await MouseService.ClickButtonAsync(Button, PressDown, PressUp, context.ComboName ?? "Unknown", ct)
                          .ConfigureAwait(false);
    }
}
