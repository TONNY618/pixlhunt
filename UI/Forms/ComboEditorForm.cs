using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelMacroEngine.Core.Services;

namespace pxlhunt.FORMS
{
    public partial class ComboEditorForm : Form
    {
        // Прямоугольник зоны захвата (Заголовок / Левый верхний угол)
        private static readonly Rectangle DragHandleZone = new Rectangle(0, 0, 80, 20);

        // Флаг состояния работы стандартной пипетки
        private bool isPipetteActive = false;

        // Win32 API для глобальной проверки состояния клавиш мыши (режим выбора квадрата)
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        private const int VK_LBUTTON = 0x01;

        public ComboEditorForm()
        {
            InitializeComponent();

            // 1. Разрешаем Drag-and-Drop на обеих панелях-"стаканах"
            flowLayoutPanelActions.AllowDrop = true;
            flowLayoutPanelTrigger.AllowDrop = true;

            // 2. Подписываем события приема и перемещения для обеих панелей
            flowLayoutPanelActions.DragEnter += FlowLayoutPanel_DragEnter;
            flowLayoutPanelActions.DragDrop += FlowLayoutPanel_DragDrop;

            flowLayoutPanelTrigger.DragEnter += FlowLayoutPanel_DragEnter;
            flowLayoutPanelTrigger.DragDrop += FlowLayoutPanel_DragDrop;

            // 3. Подписываем ВСЕ пресеты из палитр GroupActionPreset и GroupTriggerPreset
            SubscribePresetsToDragDrop(GroupActionPreset);
            SubscribePresetsToDragDrop(GroupTriggerPreset);

            // 4. Подписываем все пипетки
            SubscribePipettes(this);

            // 5. Подписываем кнопки просмотра координат ("П")
            SubscribeViewCoordButtons(this);

            // 6. Подписываем радиокнопки режимов (Точка/Прямоугольник) и кнопки захвата прямоугольника
            SubscribeSquareModes(this);
        }

        /// <summary>
        /// Рекурсивная подписка переключателей режимов (Точка/Прямоугольник) для ВСЕХ групп
        /// </summary>
        private void SubscribeSquareModes(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is RadioButton rb && rb.Name.StartsWith("radioButtonSquare"))
                {
                    rb.CheckedChanged -= RadioButtonSquare_CheckedChanged;
                    rb.CheckedChanged += RadioButtonSquare_CheckedChanged;

                    // Устанавливаем корректное начальное состояние, если выбран режим квадрата (_2)
                    if (rb.Name.EndsWith("_2") && rb.Checked)
                    {
                        GroupBox? parentGroup = GetParentGroupBox(rb);
                        if (parentGroup != null)
                        {
                            string index = Regex.Match(rb.Name, @"\d+").Value;
                            ApplySquareMode(parentGroup, true, index);
                        }
                    }
                }

                if (ctrl is Button btn && btn.Name.StartsWith("buttonSquare"))
                {
                    btn.Click -= ButtonSquare_Click;
                    btn.Click += ButtonSquare_Click;
                }

                if (ctrl.HasChildren)
                {
                    SubscribeSquareModes(ctrl);
                }
            }
        }

        private void RadioButtonSquare_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is RadioButton rb && rb.Checked)
            {
                GroupBox? parentGroup = GetParentGroupBox(rb);
                if (parentGroup == null) return;

                // Извлекаем цифру (1, 2 или 3) из имени RadioButton (например, из "radioButtonSquare2_1")
                string index = Regex.Match(rb.Name, @"\d+").Value;
                bool isSquareMode = rb.Name.EndsWith("_2"); // _2 это режим квадрата

                ApplySquareMode(parentGroup, isSquareMode, index);
            }
        }

        /// <summary>
        /// Универсальное включение/отключение элементов по индексу
        /// </summary>
        private void ApplySquareMode(GroupBox parentGroup, bool isSquareMode, string index)
        {
            Button? btnSeePos = FindControlByName<Button>(parentGroup, $"buttonSeePos{index}");
            PictureBox? pboxPXL = FindControlByName<PictureBox>(parentGroup, $"pictureBoxCreatePXL{index}");
            Button? btnSquare = FindControlByName<Button>(parentGroup, $"buttonSquare{index}");
            TextBox? txtXY2 = FindControlByName<TextBox>(parentGroup, $"textBoxXY{index}_2");
            TextBox? txtDeviant = FindControlByName<TextBox>(parentGroup, $"textBoxDeviant{index}");

            // В режиме прямоугольника блокируются просмотр точки ("П") и пипетка PXL
            if (btnSeePos != null) btnSeePos.Enabled = !isSquareMode;
            if (pboxPXL != null) pboxPXL.Enabled = !isSquareMode;

            // В режиме прямоугольника становятся активны элементы работы с областью
            if (btnSquare != null) btnSquare.Enabled = isSquareMode;
            if (txtXY2 != null) txtXY2.Enabled = isSquareMode;
            if (txtDeviant != null) txtDeviant.Enabled = isSquareMode;
        }

        /// <summary>
        /// Двухэтапный захват координат области (универсальный для всех buttonSquareX)
        /// </summary>
        private async void ButtonSquare_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btnSquare) return;

            GroupBox? parentGroup = GetParentGroupBox(btnSquare);
            if (parentGroup == null) return;

            string index = Regex.Match(btnSquare.Name, @"\d+").Value;

            TextBox? txtXY1 = FindControlByName<TextBox>(parentGroup, $"textBoxXY{index}_1");
            TextBox? txtXY2 = FindControlByName<TextBox>(parentGroup, $"textBoxXY{index}_2");

            btnSquare.Enabled = false;
            string originalText = btnSquare.Text;
            btnSquare.Text = "Клик: Л.Верх";

            // Ждем отпускания кнопки мыши после клика по кнопке
            while ((GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0)
            {
                await Task.Delay(20);
            }

            // --- ШАГ 1: Ожидание первого клика (Левый верхний угол) ---
            Point topLeft = await WaitForMouseClickAsync();
            if (txtXY1 != null)
            {
                txtXY1.Text = $"{topLeft.X}, {topLeft.Y}";
            }

            btnSquare.Text = "Клик: Пр.Низ";

            // Ждем отпускания клавиши после первого клика
            while ((GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0)
            {
                await Task.Delay(20);
            }

            // --- ШАГ 2: Ожидание второго клика (Правый нижний угол) ---
            Point bottomRight = await WaitForMouseClickAsync();
            if (txtXY2 != null)
            {
                txtXY2.Text = $"{bottomRight.X}, {bottomRight.Y}";
            }

            btnSquare.Text = originalText;
            btnSquare.Enabled = true;
        }

        private async Task<Point> WaitForMouseClickAsync()
        {
            while (true)
            {
                if ((GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0)
                {
                    return Cursor.Position;
                }
                await Task.Delay(10);
            }
        }

        private void SubscribePresetsToDragDrop(Control parentContainer)
        {
            foreach (Control ctrl in parentContainer.Controls)
            {
                if (ctrl is GroupBox presetGroup)
                {
                    presetGroup.MouseDown += ActionPreset_MouseDown;
                }
            }
        }

        private void SubscribePipettes(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is PictureBox pbox && pbox.Name.StartsWith("pictureBoxCreatePXL", StringComparison.OrdinalIgnoreCase))
                {
                    pbox.MouseDown -= Pipette_MouseDown;
                    pbox.MouseDown += Pipette_MouseDown;

                    pbox.MouseMove -= Pipette_MouseMove;
                    pbox.MouseMove += Pipette_MouseMove;

                    pbox.MouseUp -= Pipette_MouseUp;
                    pbox.MouseUp += Pipette_MouseUp;
                }

                if (ctrl.HasChildren)
                {
                    SubscribePipettes(ctrl);
                }
            }
        }

        private void SubscribeViewCoordButtons(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button btn && btn.Name.StartsWith("buttonSeePos", StringComparison.OrdinalIgnoreCase))
                {
                    btn.Click -= ViewCoordButton_Click;
                    btn.Click += ViewCoordButton_Click;
                }

                if (ctrl.HasChildren)
                {
                    SubscribeViewCoordButtons(ctrl);
                }
            }
        }

        // Затычки для старых событий дизайнера
        private void ComboEditorForm_Load(object? sender, EventArgs e) { }

        // -----------------------------------------------------------------------------
        // 1. ИНИЦИАЦИЯ ПЕРЕТАСКИВАНИЯ ИЗ ПАЛИТРЫ ИЛИ ВНУТРИ СНИППЕТА
        // -----------------------------------------------------------------------------

        private void ActionPreset_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && sender is GroupBox sourcePreset)
            {
                if (DragHandleZone.Contains(e.Location))
                {
                    DoDragDrop(sourcePreset.Name, DragDropEffects.Copy);
                }
            }
        }

        private void ClonedItem_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && sender is GroupBox clonedGroup)
            {
                if (DragHandleZone.Contains(e.Location))
                {
                    FlowLayoutPanel? parentPanel = clonedGroup.Parent as FlowLayoutPanel;

                    DragDropEffects effect = clonedGroup.DoDragDrop(clonedGroup, DragDropEffects.Move);

                    Point mousePosInForm = this.PointToClient(Cursor.Position);

                    if (!this.ClientRectangle.Contains(mousePosInForm))
                    {
                        if (parentPanel != null)
                        {
                            parentPanel.Controls.Remove(clonedGroup);
                        }
                        else
                        {
                            clonedGroup.Parent?.Controls.Remove(clonedGroup);
                        }
                        clonedGroup.Dispose();
                    }
                }
            }
        }

        // -----------------------------------------------------------------------------
        // 2. ОБРАБОТКА ДВИЖЕНИЯ И БРОСКА ВНУТРИ FLOWLAYOUTPANEL
        // -----------------------------------------------------------------------------

        private void FlowLayoutPanel_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(GroupBox)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FlowLayoutPanel_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null || sender is not FlowLayoutPanel targetPanel) return;

            Point clientPoint = targetPanel.PointToClient(new Point(e.X, e.Y));
            int marginLeft = clientPoint.X > 30 ? 25 : 3;

            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string? presetName = e.Data.GetData(DataFormats.StringFormat) as string;

                if (!string.IsNullOrEmpty(presetName))
                {
                    Control[] found = GroupActionPreset.Controls.Find(presetName, true);
                    if (found.Length == 0)
                    {
                        found = GroupTriggerPreset.Controls.Find(presetName, true);
                    }

                    if (found.Length > 0 && found[0] is GroupBox sourcePreset)
                    {
                        GroupBox newGroup = CloneControlHierarchy(sourcePreset) as GroupBox ?? new GroupBox();
                        newGroup.Margin = new Padding(marginLeft, 3, 3, 3);
                        newGroup.MouseDown += ClonedItem_MouseDown;

                        // Подписываем клона
                        SubscribePipettes(newGroup);
                        SubscribeViewCoordButtons(newGroup);
                        SubscribeSquareModes(newGroup);

                        int targetIndex = GetTargetIndexAtPoint(targetPanel, clientPoint);

                        targetPanel.Controls.Add(newGroup);

                        if (targetIndex >= 0 && targetIndex < targetPanel.Controls.Count)
                        {
                            targetPanel.Controls.SetChildIndex(newGroup, targetIndex);
                        }
                    }
                }
            }
            else if (e.Data.GetDataPresent(typeof(GroupBox)))
            {
                GroupBox? movedGroup = e.Data.GetData(typeof(GroupBox)) as GroupBox;

                if (movedGroup != null && targetPanel.Controls.Contains(movedGroup))
                {
                    movedGroup.Margin = new Padding(marginLeft, 3, 3, 3);

                    int targetIndex = GetTargetIndexAtPoint(targetPanel, clientPoint);

                    if (targetIndex >= 0)
                    {
                        targetPanel.Controls.SetChildIndex(movedGroup, targetIndex);
                    }
                    else
                    {
                        targetPanel.Controls.SetChildIndex(movedGroup, targetPanel.Controls.Count - 1);
                    }
                }
            }
        }

        // -----------------------------------------------------------------------------
        // 3. ЛОГИКА ПРЫЖКА МЫШИ НА КООРДИНАТЫ И ЕЕ ВОЗВРАТ
        // -----------------------------------------------------------------------------

        private async void ViewCoordButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            GroupBox? parentGroup = GetParentGroupBox(btn);
            if (parentGroup == null) return;

            // Динамически получаем индекс кнопки ("buttonSeePos3" -> "3")
            string index = Regex.Match(btn.Name, @"\d+").Value;
            string targetBoxName = $"textBoxXY{index}_1";

            TextBox? txtXY = FindControlByName<TextBox>(parentGroup, targetBoxName);
            if (txtXY == null || string.IsNullOrWhiteSpace(txtXY.Text)) return;

            string[] parts = txtXY.Text.Split(',');
            if (parts.Length == 2 &&
                int.TryParse(parts[0].Trim(), out int targetX) &&
                int.TryParse(parts[1].Trim(), out int targetY))
            {
                Point originalPosition = Cursor.Position;
                btn.Enabled = false;

                try
                {
                    Cursor.Position = new Point(targetX, targetY);
                    await Task.Delay(1000);
                }
                finally
                {
                    // Возвращаем мышь на место
                    Cursor.Position = originalPosition;
                    btn.Enabled = true;
                }
            }
        }

        // -----------------------------------------------------------------------------
        // 4. ЕДИНАЯ ЛОГИКА ПИПЕТОК И ПРИЦЕЛОВ
        // -----------------------------------------------------------------------------

        private void Pipette_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && sender is PictureBox pbox && pbox.Enabled)
            {
                isPipetteActive = true;
                pbox.Capture = true;

                UpdatePipetteData(pbox, Cursor.Position);
            }
        }

        private void Pipette_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isPipetteActive && sender is PictureBox pbox && pbox.Enabled)
            {
                UpdatePipetteData(pbox, Cursor.Position);
            }
        }

        private void Pipette_MouseUp(object? sender, MouseEventArgs e)
        {
            if (isPipetteActive && sender is PictureBox pbox)
            {
                isPipetteActive = false;
                pbox.Capture = false;

                if (pbox.Enabled)
                {
                    UpdatePipetteData(pbox, Cursor.Position);
                }
            }
        }

        private void UpdatePipetteData(PictureBox currentPbox, Point screenPoint)
        {
            GroupBox? parentGroup = GetParentGroupBox(currentPbox);
            if (parentGroup == null) return;

            Color pixelColor = GetPixelColorFromBufferOrScreen(screenPoint);

            string targetTextBoxXY = string.Empty;
            string targetTextBoxColor = string.Empty;

            switch (currentPbox.Name)
            {
                case "pictureBoxCreatePXL1":
                    targetTextBoxXY = "textBoxXY1_1";
                    targetTextBoxColor = "textBoxColor1";
                    break;

                case "pictureBoxCreatePXL2":
                    targetTextBoxXY = "textBoxXY2_1";
                    targetTextBoxColor = "textBoxColor2";
                    break;

                case "pictureBoxCreatePXL3":
                    targetTextBoxXY = "textBoxXY3_1";
                    targetTextBoxColor = "textBoxColor3";
                    break;

                case "pictureBoxCreatePXL4":
                    targetTextBoxXY = "textBoxXY4_1";
                    targetTextBoxColor = string.Empty;
                    break;

                default:
                    return;
            }

            if (!string.IsNullOrEmpty(targetTextBoxXY))
            {
                TextBox? txtXY = FindControlByName<TextBox>(parentGroup, targetTextBoxXY);
                if (txtXY != null)
                {
                    txtXY.Text = $"{screenPoint.X}, {screenPoint.Y}";
                }
            }

            if (!string.IsNullOrEmpty(targetTextBoxColor))
            {
                TextBox? txtColor = FindControlByName<TextBox>(parentGroup, targetTextBoxColor);
                if (txtColor != null)
                {
                    txtColor.Text = $"#{pixelColor.R:X2}{pixelColor.G:X2}{pixelColor.B:X2}";
                }
            }

            currentPbox.BackColor = pixelColor;
        }

        private Color GetPixelColorFromBufferOrScreen(Point point)
        {
            return ScreenCaptureService.GetPixel(point.X, point.Y);
        }

        private GroupBox? GetParentGroupBox(Control? control)
        {
            while (control != null)
            {
                if (control is GroupBox gb) return gb;
                control = control.Parent;
            }
            return null;
        }

        private T? FindControlByName<T>(Control parent, string exactName) where T : Control
        {
            if (string.IsNullOrEmpty(exactName)) return null;

            foreach (Control child in parent.Controls)
            {
                if (child is T typedControl && string.Equals(child.Name, exactName, StringComparison.Ordinal))
                {
                    return typedControl;
                }
                if (child.HasChildren)
                {
                    var result = FindControlByName<T>(child, exactName);
                    if (result != null) return result;
                }
            }
            return null;
        }

        // -----------------------------------------------------------------------------
        // 5. ВСПОМОГАТЕЛЬНЫЕ РАСЧЕТЫ И КЛОНИРОВАНИЕ
        // -----------------------------------------------------------------------------

        private int GetTargetIndexAtPoint(FlowLayoutPanel panel, Point point)
        {
            for (int i = 0; i < panel.Controls.Count; i++)
            {
                Control ctrl = panel.Controls[i];
                if (point.Y < ctrl.Bounds.Y + (ctrl.Height / 2))
                {
                    return i;
                }
            }
            return -1;
        }

        private Control CloneControlHierarchy(Control src)
        {
            Control clone = (Control)Activator.CreateInstance(src.GetType())!;

            clone.Name = src.Name;
            clone.Text = src.Text;
            clone.Size = src.Size;
            clone.BackColor = src.BackColor;
            clone.ForeColor = src.ForeColor;
            clone.Font = src.Font;
            clone.Enabled = src.Enabled;
            clone.Visible = src.Visible;
            clone.Location = src.Location;

            if (src is RadioButton srcRb && clone is RadioButton cloneRb)
                cloneRb.Checked = srcRb.Checked;
            if (src is CheckBox srcCb && clone is CheckBox cloneCb)
                cloneCb.Checked = srcCb.Checked;
            if (src is TextBox srcTb && clone is TextBox cloneTb)
            {
                cloneTb.Text = srcTb.Text;
                cloneTb.MaxLength = srcTb.MaxLength;
                cloneTb.Multiline = srcTb.Multiline;
            }
            if (src is PictureBox srcPb && clone is PictureBox clonePb)
            {
                clonePb.BackColor = srcPb.BackColor;
                clonePb.Image = srcPb.Image;
                clonePb.SizeMode = srcPb.SizeMode;
            }

            foreach (Control child in src.Controls)
            {
                clone.Controls.Add(CloneControlHierarchy(child));
            }

            return clone;
        }

        // -----------------------------------------------------------------------------
        // 6. УНИВЕРСАЛЬНОЕ СОХРАНЕНИЕ И ЗАГРУЗКА БЛОКОВ
        // -----------------------------------------------------------------------------

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string comboName = textBox5.Text.Trim();
            if (string.IsNullOrEmpty(comboName))
            {
                MessageBox.Show("Введите название триггера перед сохранением.");
                return;
            }

            var profile = new ComboProfile
            {
                ComboName = comboName,
                CooldownTime = textBoxCDglobal.Text,
                IsCooldownGlobal = radioButtonSkillCooldownGLOBAL.Checked,
                Priority = textBoxPriorityTrigger.Text,
                PauseIfWASD = checkBoxWASDpauseTrigger.Checked,
                IsOnOff = checkBoxOnOff.Checked
            };

            profile.Triggers = ParsePanelElements(flowLayoutPanelTrigger);
            profile.Actions = ParsePanelElements(flowLayoutPanelActions);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(profile, options);

            // Указываем путь к папке Config внутри рабочей директории приложения
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");

            // Создаем директорию, если её нет
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, $"{comboName}.json");
            File.WriteAllText(filePath, jsonString);

            MessageBox.Show($"Сохранено в файл:\n{filePath}");
        }

        private List<ComboElement> ParsePanelElements(FlowLayoutPanel panel)
        {
            var list = new List<ComboElement>();

            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl is GroupBox cloneGroup)
                {
                    var element = new ComboElement
                    {
                        ElementType = cloneGroup.Name,
                        IndentLevel = cloneGroup.Margin.Left > 10 ? 1 : 0
                    };

                    // Динамически собираем состояния абсолютно всех важных контролов внутри GroupBox
                    SaveControlStates(cloneGroup, element.Parameters);
                    list.Add(element);
                }
            }
            return list;
        }

        private void SaveControlStates(Control parent, Dictionary<string, string> parameters)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is TextBox tb) parameters[tb.Name] = tb.Text;
                else if (ctrl is ComboBox cb) parameters[cb.Name] = cb.Text;
                else if (ctrl is CheckBox chk) parameters[chk.Name] = chk.Checked.ToString();
                else if (ctrl is RadioButton rb) parameters[rb.Name] = rb.Checked.ToString();
                else if (ctrl is PictureBox pb && pb.Name.StartsWith("pictureBoxCreatePXL", StringComparison.OrdinalIgnoreCase))
                {
                    parameters[pb.Name] = pb.BackColor.ToArgb().ToString(); // Сохраняем цвет для индикации
                }

                if (ctrl.HasChildren)
                {
                    SaveControlStates(ctrl, parameters);
                }
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            string comboName = textBox5.Text.Trim();
            if (string.IsNullOrEmpty(comboName))
            {
                MessageBox.Show("Введите название триггера для загрузки.");
                return;
            }

            // Ищем файл внутри папки Config
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", $"{comboName}.json");
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Файл не найден по пути:\n{filePath}");
                return;
            }

            string jsonString = File.ReadAllText(filePath);
            var profile = JsonSerializer.Deserialize<ComboProfile>(jsonString);

            if (profile != null)
            {
                textBoxCDglobal.Text = profile.CooldownTime ?? "1500";
                radioButtonSkillCooldownGLOBAL.Checked = profile.IsCooldownGlobal;
                radioButtonSkillCooldownSingleHP.Checked = !profile.IsCooldownGlobal;
                textBoxPriorityTrigger.Text = profile.Priority ?? "50";
                checkBoxWASDpauseTrigger.Checked = profile.PauseIfWASD;
                checkBoxOnOff.Checked = profile.IsOnOff;

                flowLayoutPanelTrigger.Controls.Clear();
                flowLayoutPanelActions.Controls.Clear();

                RestorePanelElements(profile.Triggers, flowLayoutPanelTrigger, GroupTriggerPreset);
                RestorePanelElements(profile.Actions, flowLayoutPanelActions, GroupActionPreset);

                MessageBox.Show("Триггер успешно загружен.");
            }
        }

        private void RestorePanelElements(List<ComboElement> elements, FlowLayoutPanel targetPanel, GroupBox sourcePallete)
        {
            foreach (var elem in elements)
            {
                Control[] found = sourcePallete.Controls.Find(elem.ElementType, true);
                if (found.Length == 0 || found[0] is not GroupBox sourcePreset) continue;

                GroupBox newGroup = CloneControlHierarchy(sourcePreset) as GroupBox;
                if (newGroup == null) continue;

                int marginLeft = elem.IndentLevel > 0 ? 25 : 3;
                newGroup.Margin = new Padding(marginLeft, 3, 3, 3);
                newGroup.MouseDown += ClonedItem_MouseDown;

                // ВАЖНО: Заново подключаем логику пипеток, координат и смены режимов для клона
                SubscribePipettes(newGroup);
                SubscribeViewCoordButtons(newGroup);
                SubscribeSquareModes(newGroup);

                // Динамически возвращаем состояния всем контролам
                RestoreControlStates(newGroup, elem.Parameters);

                targetPanel.Controls.Add(newGroup);
            }
        }

        private void RestoreControlStates(Control parent, Dictionary<string, string> parameters)
        {
            foreach (var kvp in parameters)
            {
                Control? foundCtrl = FindControlByName<Control>(parent, kvp.Key);
                if (foundCtrl != null)
                {
                    if (foundCtrl is TextBox tb) tb.Text = kvp.Value;
                    else if (foundCtrl is ComboBox cb) cb.Text = kvp.Value;
                    else if (foundCtrl is CheckBox chk && bool.TryParse(kvp.Value, out bool bChk)) chk.Checked = bChk;
                    else if (foundCtrl is RadioButton rb && bool.TryParse(kvp.Value, out bool bRb))
                    {
                        rb.Checked = bRb;
                        // Принудительно дергаем эвент, чтобы переключить доступность UI элементов (прямоугольник/точка)
                        if (bRb) RadioButtonSquare_CheckedChanged(rb, EventArgs.Empty);
                    }
                    else if (foundCtrl is PictureBox pb && int.TryParse(kvp.Value, out int argb))
                    {
                        pb.BackColor = Color.FromArgb(argb);
                    }
                }
            }
        }
    }

    public class ComboProfile
    {
        public string ComboName { get; set; } = string.Empty;

        // Новые свойства для хранения статических настроек триггера
        public bool IsOnOff { get; set; } = true;
        public string CooldownTime { get; set; } = "1500";
        public bool IsCooldownGlobal { get; set; } = true;
        public string Priority { get; set; } = "50";
        public bool PauseIfWASD { get; set; } = true;

        public List<ComboElement> Triggers { get; set; } = new List<ComboElement>();
        public List<ComboElement> Actions { get; set; } = new List<ComboElement>();
    }

    public class ComboElement
    {
        public string ElementType { get; set; } = string.Empty;
        public int IndentLevel { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
