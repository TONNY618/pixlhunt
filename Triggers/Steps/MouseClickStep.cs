using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;

namespace PixelMacroEngine.Triggers.Steps;

/// <summary>
/// Шаг комбо: клик, зажатие или отпускание кнопки мыши (ЛКМ, ПКМ, СКМ)
/// либо расширенной клавиши F13-F24 (отправляется через Arduino RawHID).
/// Наклоны колесика (WL/WR) в шагах игнорируются — они доступны только как триггеры.
/// </summary>
public class MouseClickStep : IComboStep
{
    public string ButtonName { get; }
    public bool PressDown { get; }
    public bool PressUp { get; }

    public MouseClickStep(pxlhunt.UI.Forms.ComboEditorCompoziteControls.MouseKeyDto dto)
    {
        ButtonName = dto.Button ?? "L key";
        PressDown = dto.PressDown;
        PressUp = dto.PressUp;
    }

    public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken ct)
    {
        if (!PressDown && !PressUp) return;

        // Расширенные клавиши F13-F24: отправляем через Arduino RawHID.
        if (ButtonName.StartsWith("F") && ButtonName.Length > 1)
        {
            byte hidByte = ArduinoKeyMap.GetByte(ButtonName);
            if (hidByte == 0) return;

            var batch = new PacketBatch();
            if (PressDown) batch.AddKey(hidByte, true);

            if (PressDown && PressUp)
            {
                ArduinoHidService.Send(batch);
                batch = new PacketBatch();
                await Task.Delay(HumanizerEngine.GetKeyPressDuration(), ct).ConfigureAwait(false);
            }

            if (PressUp) batch.AddKey(hidByte, false);
            ArduinoHidService.Send(batch);
            return;
        }

        // Наклоны колесика (WL/WR) в шагах комбо не эмулируются — только как триггеры.
        if (ButtonName == "WL" || ButtonName == "WR")
            return;

        byte btnCode = (byte)(ButtonName == "R key" ? 2 : (ButtonName == "M key" ? 3 : 1));
        await MouseService.ClickButtonAsync(btnCode, PressDown, PressUp, context.ComboName ?? "Unknown", ct)
                          .ConfigureAwait(false);
    }
}
