using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Abstractions;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;
using pxlhunt.UI.Forms.ComboEditorCompoziteControls;

namespace PixelMacroEngine.Triggers.Steps
{
    public class TypeTextStep : IComboStep
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        private const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;
        private const int KLF_ACTIVATE = 1;

        // Идентификаторы раскладок Windows. 0x0409 — Английский, 0x0419 — Русский.
        private const string KLID_EN = "00000409";
        private const string KLID_RU = "00000419";

        // Максимальная длина строки HID-ключей посимвольного маппинга.
        private readonly TextToDto _dto;

        public TypeTextStep(TextToDto dto)
        {
            _dto = dto;
        }

        public async Task ExecuteAsync(TriggerExecutionContext context, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_dto.Text)) return;

            string text = _dto.Text.Replace("\r\n", "\n").Replace("\r", "\n");

            IntPtr layoutEn = LoadKeyboardLayout(KLID_EN, KLF_ACTIVATE);
            IntPtr layoutRu = LoadKeyboardLayout(KLID_RU, KLF_ACTIVATE);
            IntPtr currentLayoutId = IntPtr.Zero;

            foreach (char c in text)
            {
                ct.ThrowIfCancellationRequested();

                if (c == '\n')
                {
                    await SendKeyAsync("Enter", false, ct).ConfigureAwait(false);
                    continue;
                }

                if (c == ' ')
                {
                    await SendKeyAsync("Space", false, ct).ConfigureAwait(false);
                    continue;
                }

                var (hidKeyName, needShift, langId) = TranslateChar(c);
                if (string.IsNullOrEmpty(hidKeyName)) continue;

                // Переключаем язык активного окна на лету (только если язык изменился)
                IntPtr targetLayout = langId == "RU" ? layoutRu : layoutEn;
                if (targetLayout != currentLayoutId && targetLayout != IntPtr.Zero)
                {
                    IntPtr hwnd = GetForegroundWindow();
                    if (hwnd != IntPtr.Zero)
                    {
                        PostMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, targetLayout);
                        currentLayoutId = targetLayout;
                        // Даём ОС время применить раскладку до ввода символа.
                        await Task.Delay(HumanizerEngine.GetLayoutSwitchDelay(), ct).ConfigureAwait(false);
                    }
                }

                await SendKeyAsync(hidKeyName, needShift, ct).ConfigureAwait(false);
            }
        }

        private async Task SendKeyAsync(string hidKeyName, bool needShift, CancellationToken ct)
        {
            byte hidByte = ArduinoKeyMap.GetByte(hidKeyName);
            if (hidByte == 0) return;

            byte shiftByte = ArduinoKeyMap.GetByte("Left Shift");

            var batchDown = new PacketBatch();
            if (needShift) batchDown.AddKey(shiftByte, true);
            batchDown.AddKey(hidByte, true);
            ArduinoHidService.Send(batchDown);

            await Task.Delay(HumanizerEngine.GetKeyPressDuration(), ct).ConfigureAwait(false);

            var batchUp = new PacketBatch();
            batchUp.AddKey(hidByte, false);
            if (needShift) batchUp.AddKey(shiftByte, false);
            ArduinoHidService.Send(batchUp);

            await Task.Delay(HumanizerEngine.GetFlightTime(), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Переводит символ текста в физическую HID-клавишу, признак необходимости Shift
        /// и требуемую раскладку активного окна.
        /// Работает через физическую клавишу + раскладку ОС, а не через Unicode-коды.
        /// </summary>
        private static (string hidKeyName, bool shift, string langId) TranslateChar(char c)
        {
            bool isUpper = char.IsUpper(c);
            char lowerC = char.ToLowerInvariant(c);

            // Физические клавиши верхнего ряда (английская раскладка).
            // При активной русской раскладке ОС сама превратит их в русские буквы.
            const string enRow =
                "qwertyuiop[]" +   // 0..11
                "asdfghjkl;'" +    // 12..22
                "zxcvbnm,./`";     // 23..33

            // Соответствие русских букв индексам в enRow (позиция физической клавиши).
            // ЙЦУКЕН является перестановкой QWERTY один-в-один.
            // Индекс в enRow: q=0, w=1, ..., ъ=11, ф=12, ..., э=22, я=23, ..., б=30, ю=31, ё=33.
            const string ruAlphabet =
                "йцукенгшщзхъ" +   // 0..11  (й..ъ)
                "фывапролджэ"  +   // 12..22 (ф..э)
                "ячсмитьбю"    +   // 23..31 (я..ю)
                "ё";               // 33 -> физическая клавиша ` (тильда)

            // Русская буква — ищем и берём физическую клавишу по общему индексу.
            int ruIdx = ruAlphabet.IndexOf(lowerC);
            if (ruIdx >= 0)
            {
                // Индекс 32 зарезервирован: в QWERTY на нём находится точка? Нет.
                // Явно сопоставляем "ё" с клавишей "` (Тильда)".
                if (lowerC == 'ё')
                    return ("` (Тильда)", isUpper, "RU");

                // ruAlphabet индексы 0..11, 12..22, 23..31 точно совпадают с enRow 0..31.
                char physical = enRow[ruIdx];
                return (GetHidName(physical), isUpper, "RU");
            }

            // Английская буква.
            int enIdx = enRow.IndexOf(lowerC);
            if (enIdx >= 0)
            {
                return (GetHidName(lowerC), isUpper, "EN");
            }

            // Цифры и спецсимволы клавиатуры обрабатываем в английской раскладке.
            string spec = lowerC switch
            {
                '0' => "0", '1' => "1", '2' => "2", '3' => "3", '4' => "4",
                '5' => "5", '6' => "6", '7' => "7", '8' => "8", '9' => "9",
                '-' => "- (Минус)", '=' => "= (Равно)", '\\' => "\\",
                '!' => "1", '@' => "2", '#' => "3", '$' => "4", '%' => "5",
                '^' => "6", '&' => "7", '*' => "8", '(' => "9", ')' => "0",
                '_' => "- (Минус)", '+' => "= (Равно)", '|' => "\\",
                ':' => ";", '"' => "'", '<' => ",", '>' => ".", '?' => "/", '~' => "` (Тильда)",
                _ => ""
            };

            bool shiftSpec = "!@#$%^&*()_+|:\"<>?~".Contains(c);

            if (!string.IsNullOrEmpty(spec)) return (spec, shiftSpec, "EN");

            return ("", false, "EN");
        }

        private static string GetHidName(char c) => char.ToUpperInvariant(c).ToString();
    }
}
