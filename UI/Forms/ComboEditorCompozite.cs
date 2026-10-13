using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using PixelMacroEngine.Core.Models;
using pxlhunt.UI.Forms.ComboEditorCompoziteControls;

namespace pxlhunt.UI.Forms
{
    public partial class ComboEditorCompozite : Form
    {
        // Зона заголовка (левый верхний угол), за которую можно цеплять контрол мышкой
        private static readonly Rectangle DragHandleZone = new Rectangle(0, 0, 100, 22);

        // Контекстное меню для элементов внутри панелей (ПКМ)
        private readonly ContextMenuStrip _itemContextMenu = new ContextMenuStrip();
        private Control? _contextTargetControl;

        public ComboEditorCompozite()
        {
            InitializeComponent();

            // Окно редактора всегда поверх остальных
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;

            // Инициализация контекстного меню элементов
            SetupItemContextMenu();

            // Инициализация Drag-and-Drop механики
            SetupDragAndDrop();
        }

        /// <summary>
        /// Создает контекстное меню для элементов внутри панелей-стаканов.
        /// </summary>
        private void SetupItemContextMenu()
        {
            var duplicateItem = new ToolStripMenuItem("Дублировать");
            duplicateItem.Click += (s, e) => DuplicateItem(_contextTargetControl);
            _itemContextMenu.Items.Add(duplicateItem);
        }

        // -----------------------------------------------------------------------------
        // 1. НАСТРОЙКА DRAG-AND-DROP И ПОДПИСКА ПАЛИТРЫ
        // -----------------------------------------------------------------------------

        private void SetupDragAndDrop()
        {
            // Разрешаем прием элементов в обе панели-стаканы
            flowLayoutPanelTrigger.AllowDrop = true;
            flowLayoutPanelActions.AllowDrop = true;

            flowLayoutPanelTrigger.DragEnter += FlowLayoutPanel_DragEnter;
            flowLayoutPanelTrigger.DragDrop += FlowLayoutPanel_DragDrop;

            flowLayoutPanelActions.DragEnter += FlowLayoutPanel_DragEnter;
            flowLayoutPanelActions.DragDrop += FlowLayoutPanel_DragDrop;

            // Подписываем элементы палитры на начало перетаскивания
            SubscribePresetsToDragDrop(GroupActionPreset);
        }

        /// <summary>
        /// Подписывает все элементы в палитре (и UserControl'ы, и GroupBox'ы) на перетаскивание.
        /// </summary>
        private void SubscribePresetsToDragDrop(Control parentContainer)
        {
            foreach (Control ctrl in parentContainer.Controls)
            {
                ctrl.MouseDown += (s, e) => Preset_MouseDown(ctrl, e);

                foreach (Control child in ctrl.Controls)
                {
                    if (child is GroupBox)
                    {
                        child.MouseDown += (s, e) => Preset_MouseDown(ctrl, e);
                    }
                }
            }
        }

        private void Preset_MouseDown(Control rootPreset, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && DragHandleZone.Contains(e.Location))
            {
                DoDragDrop(rootPreset.Name, DragDropEffects.Copy);
            }
        }

        /// <summary>
        /// Подписывает уже созданный элемент на перемещение внутри панели и удаление.
        /// </summary>
        private void SubscribeClonedItemForDragging(Control clonedItem)
        {
            clonedItem.MouseDown += (s, e) => ClonedItem_MouseDown(clonedItem, e);

            foreach (Control child in clonedItem.Controls)
            {
                if (child is GroupBox)
                {
                    child.MouseDown += (s, e) => ClonedItem_MouseDown(clonedItem, e);
                }
            }
        }

        /// <summary>
        /// Универсальное дублирование элемента внутри панели-стакана.
        /// Работает только с контролами, реализующими IComboComponentControl.
        /// </summary>
        private void DuplicateItem(Control? source)
        {
            if (source is IComboComponentControl comp && source.Parent is FlowLayoutPanel parentPanel)
            {
                int idx = parentPanel.Controls.GetChildIndex(source);

                // УНИВЕРСАЛЬНОЕ ДУБЛИРОВАНИЕ ЧЕРЕЗ REFLECTION И JSON
                Control clone = (Control)Activator.CreateInstance(source.GetType())!;

                // Передаем данные из оригинала в клон
                var dtoJson = System.Text.Json.JsonSerializer.SerializeToElement(comp.GetDto());
                ((IComboComponentControl)clone).SetDtoFromElement(dtoJson);

                clone.Margin = source.Margin;
                SubscribeClonedItemForDragging(clone);

                parentPanel.Controls.Add(clone);
                parentPanel.Controls.SetChildIndex(clone, idx + 1);
            }
        }

        private void ClonedItem_MouseDown(Control clonedControl, MouseEventArgs e)
        {
            // ПКМ в зоне заголовка — открываем контекстное меню элемента
            if (e.Button == MouseButtons.Right && DragHandleZone.Contains(e.Location))
            {
                _contextTargetControl = clonedControl;
                _itemContextMenu.Show(Cursor.Position);
                return;
            }

            if (e.Button == MouseButtons.Left && DragHandleZone.Contains(e.Location))
            {
                FlowLayoutPanel? parentPanel = clonedControl.Parent as FlowLayoutPanel;

                DragDropEffects effect = clonedControl.DoDragDrop(clonedControl, DragDropEffects.Move);

                Point mousePosInForm = this.PointToClient(Cursor.Position);

                // Если вытащили за пределы окна — удаляем элемент из панели
                if (!this.ClientRectangle.Contains(mousePosInForm))
                {
                    if (parentPanel != null)
                    {
                        parentPanel.Controls.Remove(clonedControl);
                    }
                    else
                    {
                        clonedControl.Parent?.Controls.Remove(clonedControl);
                    }
                    clonedControl.Dispose();
                }
            }
        }

        // -----------------------------------------------------------------------------
        // 2. ОБРАБОТКА ПЕРЕМЕЩЕНИЯ И СБРОСА (DRAG & DROP)
        // -----------------------------------------------------------------------------

        private void FlowLayoutPanel_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void FlowLayoutPanel_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null || sender is not FlowLayoutPanel targetPanel) return;

            Point clientPoint = targetPanel.PointToClient(new Point(e.X, e.Y));

            // ВАРИАНТ А: Сброс нового элемента из палитры
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string? presetName = e.Data.GetData(DataFormats.StringFormat) as string;
                if (string.IsNullOrEmpty(presetName)) return;

                Control? newControl = CreateControlFromPreset(presetName);
                if (newControl != null)
                {
                    int targetIndex = GetTargetIndexAtPoint(targetPanel, clientPoint);
                    if (targetIndex < 0) targetIndex = targetPanel.Controls.Count;

                    // Расчет левого отступа по строгим правилам иерархии
                    int marginLeft = CalculateMarginLeft(targetPanel, clientPoint, targetIndex, null);
                    newControl.Margin = new Padding(marginLeft, 3, 3, 3);

                    SubscribeClonedItemForDragging(newControl);

                    targetPanel.Controls.Add(newControl);

                    if (targetIndex >= 0 && targetIndex < targetPanel.Controls.Count)
                    {
                        targetPanel.Controls.SetChildIndex(newControl, targetIndex);
                    }
                }
            }
            // ВАРИАНТ Б: Перемещение уже добавленного элемента
            else
            {
                var formats = e.Data.GetFormats();
                object? rawData = formats.Length > 0 ? e.Data.GetData(formats[0]) : null;

                if (rawData is Control movedControl && (movedControl.Parent is FlowLayoutPanel srcPanel))
                {
                    int targetIndex = GetTargetIndexAtPoint(targetPanel, clientPoint);
                    if (targetIndex < 0) targetIndex = targetPanel.Controls.Count;

                    // Если переместили из другой панели
                    if (srcPanel != targetPanel)
                    {
                        srcPanel.Controls.Remove(movedControl);
                        targetPanel.Controls.Add(movedControl);
                    }

                    // Расчет левого отступа с учетом перемещаемого контрола
                    int marginLeft = CalculateMarginLeft(targetPanel, clientPoint, targetIndex, movedControl);
                    movedControl.Margin = new Padding(marginLeft, 3, 3, 3);

                    if (targetIndex >= 0 && targetIndex < targetPanel.Controls.Count)
                    {
                        targetPanel.Controls.SetChildIndex(movedControl, targetIndex);
                    }
                    else
                    {
                        targetPanel.Controls.SetChildIndex(movedControl, targetPanel.Controls.Count - 1);
                    }
                }
            }
        }

        // -----------------------------------------------------------------------------
        // 3. ПРАВИЛА И РАСЧЕТ ИЕРАРХИЧЕСКОГО ОТСТУПА
        // -----------------------------------------------------------------------------

        /// <summary>
        /// Вычисляет Margin.Left (3 или 25) строго по правилам:
        /// 1. В зоне триггеров отступы ЗАПРЕЩЕНЫ (всегда 3).
        /// 2. В зоне действий отступ разрешен ТОЛЬКО после блоков условий (groupBoxPixSqua или groupBoxIMAGE)
        ///    либо внутри уже открытой ветки с отступом.
        /// </summary>
        private int CalculateMarginLeft(FlowLayoutPanel targetPanel, Point clientPoint, int targetIndex, Control? movedControl)
        {
            // 1. В триггерах все строго одноранговое
            if (targetPanel == flowLayoutPanelTrigger)
                return 3;

            // 2. Если курсор не сдвинут вправо — пользователь и не просит отступ
            if (clientPoint.X <= 30)
                return 3;

            // 3. Ищем предыдущий контрол перед позицией вставки
            Control? prevControl = null;
            for (int i = (targetIndex > 0 ? targetIndex - 1 : 0); i >= 0; i--)
            {
                if (i < targetPanel.Controls.Count)
                {
                    Control candidate = targetPanel.Controls[i];
                    if (candidate != movedControl)
                    {
                        prevControl = candidate;
                        break;
                    }
                }
            }

            // Если перед нами ничего нет — отступать не от кого
            if (prevControl == null)
                return 3;

            // 4. Отступ разрешен, если:
            // - предыдущий элемент УЖЕ с отступом (продолжаем цепочку условий);
            // - предыдущий элемент является одним из двух блоков условий (Pixel/Square или IMAGE).
            if (prevControl.Margin.Left > 10 || IsConditionControl(prevControl))
            {
                return 25;
            }

            return 3;
        }

        /// <summary>
        /// Проверяет, является ли контрол блоком ветвления условий (Pixel/Square или IMAGE).
        /// </summary>
        private static bool IsConditionControl(Control ctrl)
        {
            string name = ctrl.Name;
            if (string.Equals(name, "pixSquaControl", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "groupBoxIMAGE", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Проверка на случай перевода в UserControl
            string typeName = ctrl.GetType().Name;
            if (typeName.Contains("PixSqua", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("IMAGE", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        // -----------------------------------------------------------------------------
        // 4. ФАБРИКА И ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // -----------------------------------------------------------------------------

        private Control? CreateControlFromPreset(string presetName)
        {
            // 1. Новые UserControl'ы
            if (string.Equals(presetName, "triggerOrAnd", StringComparison.OrdinalIgnoreCase))
            {
                return new TriggerOrAnd();
            }
            if (string.Equals(presetName, "triggerTimer", StringComparison.OrdinalIgnoreCase))
            {
                return new TriggerTimer();
            }
            if (string.Equals(presetName, "pixSquaControl", StringComparison.OrdinalIgnoreCase))
            {
                return new PixSquaControl();
            }
            if (string.Equals(presetName, "pixSquaControl", StringComparison.OrdinalIgnoreCase))
            {
                return new PixSquaControl();
            }
            if (string.Equals(presetName, "boxKeyControl", StringComparison.OrdinalIgnoreCase))
            {
                return new BoxKeyControl();
            }

            // 2. Временная обратная совместимость для GroupBox'ов
            Control[] found = GroupActionPreset.Controls.Find(presetName, true);
            if (found.Length > 0 && found[0] is GroupBox sourcePreset)
            {
                return CloneControlHierarchy(sourcePreset);
            }

            return null;
        }

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
        // 5. СОХРАНЕНИЕ И ЗАГРУЗКА ПРОФИЛЯ (JSON)
        // -----------------------------------------------------------------------------

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// Возвращает путь к папке Config рядом с исполняемым файлом.
        /// </summary>
        private static string GetConfigDirectory()
        {
            string baseDir = AppContext.BaseDirectory;
            string configDir = Path.Combine(baseDir, "Config");
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
            return configDir;
        }

        /// <summary>
        /// Собирает список элементов панели в DTO-список.
        /// </summary>
        private static List<ComboElementDto> CollectElements(FlowLayoutPanel panel)
        {
            var list = new List<ComboElementDto>();

            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl is IComboComponentControl comp)
                {
                    list.Add(new ComboElementDto
                    {
                        Type = comp.ComponentType,
                        Indent = ctrl.Margin.Left > 10 ? 1 : 0,
                        Data = JsonSerializer.SerializeToElement(comp.GetDto(), comp.GetDto().GetType(), JsonOptions)
                    });
                }
            }

            return list;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string comboName = textBoxNameCOMBO.Text.Trim();
            if (string.IsNullOrEmpty(comboName))
            {
                MessageBox.Show("Введите имя комбо перед сохранением.", "Сохранение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var profile = new ComboProfileDto
            {
                ComboName = comboName,
                IsOnOff = checkBoxOnOff.Checked,
                IsCooldownGlobal = radioButtonSkillCooldownGLOBAL.Checked,
                Priority = textBoxPriorityTrigger.Text,
                PauseIfWASD = checkBoxWASDpauseTrigger.Checked,
                Triggers = CollectElements(flowLayoutPanelTrigger),
                Actions = CollectElements(flowLayoutPanelActions)
            };

            try
            {
                string path = Path.Combine(GetConfigDirectory(), comboName + ".json");
                string json = JsonSerializer.Serialize(profile, JsonOptions);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Сохранение",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            string comboName = textBoxNameCOMBO.Text.Trim();
            if (string.IsNullOrEmpty(comboName))
            {
                MessageBox.Show("Введите имя комбо для загрузки.", "Загрузка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string primaryPath = Path.Combine(GetConfigDirectory(), comboName + ".json");
            string fallbackPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Config", comboName + ".json");

            string? path = null;
            if (File.Exists(primaryPath)) path = primaryPath;
            else if (File.Exists(fallbackPath)) path = Path.GetFullPath(fallbackPath);

            if (path == null)
            {
                MessageBox.Show($"Файл профиля \"{comboName}.json\" не найден.", "Загрузка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                ComboProfileDto? profile = JsonSerializer.Deserialize<ComboProfileDto>(json, JsonOptions);
                if (profile == null)
                {
                    MessageBox.Show("Не удалось прочитать профиль.", "Загрузка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Восстановление полей формы
                textBoxNameCOMBO.Text = profile.ComboName;
                checkBoxOnOff.Checked = profile.IsOnOff;
                radioButtonSkillCooldownGLOBAL.Checked = profile.IsCooldownGlobal;
                textBoxPriorityTrigger.Text = profile.Priority;
                checkBoxWASDpauseTrigger.Checked = profile.PauseIfWASD;

                // Очистка панелей
                flowLayoutPanelTrigger.Controls.Clear();
                flowLayoutPanelActions.Controls.Clear();

                RestoreElements(flowLayoutPanelTrigger, profile.Triggers);
                RestoreElements(flowLayoutPanelActions, profile.Actions);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Загрузка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Восстанавливает элементы панели из DTO-списка.
        /// </summary>
        private void RestoreElements(FlowLayoutPanel panel, List<ComboElementDto> elements)
        {
            foreach (ComboElementDto elem in elements)
            {
                Control? ctrl = CreateControlFromPreset(elem.Type);
                if (ctrl == null) continue;

                if (ctrl is IComboComponentControl comp)
                {
                    comp.SetDtoFromElement(elem.Data);
                }

                ctrl.Margin = new Padding(elem.Indent > 0 ? 25 : 3, 3, 3, 3);
                SubscribeClonedItemForDragging(ctrl);
                panel.Controls.Add(ctrl);
            }
        }
    }
}
