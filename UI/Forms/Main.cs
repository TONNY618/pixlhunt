using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelMacroEngine.Core.Input;
using PixelMacroEngine.Core.Models;
using PixelMacroEngine.Core.Services;
using PixelMacroEngine.Engine;

namespace pxlhunt.FORMS
{
    public partial class pxlHunt : Form
    {
        private readonly Orchestrator _orchestrator;
        private System.Windows.Forms.Timer? _sessionTimer;
        private const string SessionFileName = "session.json";
        private readonly HashSet<string> _changedComboNames = new(StringComparer.OrdinalIgnoreCase);
        private bool _isLoadingCombos = false;
        private bool _wasPaused = false;

        public pxlHunt()
        {
            InitializeComponent();

            // Подключаемся к плате Arduino HID и отображаем статус
            bool isArduinoConnected = ArduinoHidService.Connect();
            groupBoxStatus.Text = isArduinoConnected ? "Статус - плата OK" : "Статус - плата НЕ НАЙДЕНА";

            // Главное окно всегда поверх остальных
            this.TopMost = true;

            // Создаём оркестратор с очередью и диспетчером ввода
            var queue = new SimpleTaskQueue();
            _orchestrator = new Orchestrator(queue, new InputDispatcher(queue));

            // Стартуем глобальный захват при запуске программы
            try
            {
                ScreenCaptureService.Start(monitorIndex: 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации захвата экрана:\n{ex.Message}",
                                "Ошибка DXGI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Подписываемся на кадры
            ScreenCaptureService.OnFrameCaptured += OnFrameCapturedHandler;

            // Загружаем комбо из Config
            LoadCombosFromConfig();

            // Восстанавливаем время активности
            RestoreSessionTime();

            // Таймер активности (1 сек)
            _sessionTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _sessionTimer.Tick += SessionTimer_Tick;
            _sessionTimer.Start();

            // Привязка событий UI
            checkedListBoxCombo.ItemCheck += CheckedListBoxCombo_ItemCheck;
            checkBoxStatus.CheckedChanged += CheckBoxStatus_CheckedChanged;

            // Синхронизируем мастер-флаг с начальным состоянием галочки
            BotStateController.IsMasterEnabled = checkBoxStatus.Checked;

            // Снятие выделения при клике по пустому пространству формы
            this.MouseDown += (s, e) => checkedListBoxCombo.ClearSelected();

            // Контекстное меню (ПКМ) для списка комбо
            SetupComboContextMenu();

            // Открытие комбо по двойному клику
            checkedListBoxCombo.DoubleClick += CheckedListBoxCombo_DoubleClick;

            // Подписка на логи действий
            ActionLogger.OnLog += AppendLogMessage;

            // Настройка трекбара прозрачности (на случай, если не задано в дизайнере)
            trackBarTransparency.Minimum = 10;
            trackBarTransparency.Maximum = 100;
            trackBarTransparency.Scroll += TrackBarTransparency_Scroll;

            UpdateActiveTimeLabel();
        }

        /// <summary>
        /// Создаёт и привязывает контекстное меню к списку комбо.
        /// </summary>
        private void SetupComboContextMenu()
        {
            var menu = new ContextMenuStrip();

            var editItem = new ToolStripMenuItem("Редактировать");
            editItem.Click += (s, e) => OpenSelectedComboInEditor();
            menu.Items.Add(editItem);

            var priorityItem = new ToolStripMenuItem("Приоритет...");
            priorityItem.Click += (s, e) =>
            {
                int index = checkedListBoxCombo.SelectedIndex;
                if (index < 0 || index >= _orchestrator.Combos.Count) return;
                var combo = _orchestrator.Combos[index];

                // Создаем легкое окно для ввода приоритета
                Form inputForm = new Form
                {
                    Text = "Приоритет",
                    Size = new System.Drawing.Size(200, 120),
                    TopMost = true,
                    ShowInTaskbar = false,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.Manual,
                    MaximizeBox = false,
                    MinimizeBox = false
                };
                NumericUpDown num = new NumericUpDown
                {
                    Value = Math.Clamp(combo.Priority, 0, 99),
                    Minimum = 0,
                    Maximum = 99,
                    Location = new System.Drawing.Point(50, 20)
                };
                Button btnOk = new Button
                {
                    Text = "OK",
                    Location = new System.Drawing.Point(50, 50)
                };
                btnOk.Click += (s1, e1) =>
                {
                    combo.Priority = (int)num.Value;
                    // Пересохраняем профиль
                    UpdateComboFilePriority(combo.FilePath, combo.Priority);
                    LoadCombosFromConfig(); // Пересортировка списка
                    inputForm.Close();
                };
                inputForm.Controls.Add(num);
                inputForm.Controls.Add(btnOk);
                CenterFormOnMain(inputForm);
                inputForm.ShowDialog(this);
            };
            menu.Items.Add(priorityItem);

            menu.Opening += (s, e) =>
            {
                int index = checkedListBoxCombo.SelectedIndex;
                if (index >= 0 && index < _orchestrator.Combos.Count)
                {
                    var combo = _orchestrator.Combos[index];
                    var editItem = menu.Items.Cast<ToolStripMenuItem>()
                        .FirstOrDefault(i => i.Text.StartsWith("Приоритет"));
                    if (editItem != null)
                        editItem.Text = $"Приоритет ({combo.Priority})";
                }
            };

            menu.Items.Add(new ToolStripSeparator());

            var deleteItem = new ToolStripMenuItem("Удалить");
            deleteItem.Click += (s, e) => DeleteSelectedCombo();
            menu.Items.Add(deleteItem);

            checkedListBoxCombo.ContextMenuStrip = menu;

            checkedListBoxCombo.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    int index = checkedListBoxCombo.IndexFromPoint(e.Location);
                    if (index >= 0)
                    {
                        checkedListBoxCombo.SelectedIndex = index;
                    }
                }
            };
        }

        /// <summary>
        /// Возвращает путь к каталогу Config, создавая его при необходимости.
        /// </summary>
        private string GetConfigDir()
        {
            string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            if (!Directory.Exists(configDir))
            {
                string alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Config");
                if (Directory.Exists(alt))
                    configDir = alt;
            }

            Directory.CreateDirectory(configDir);
            return configDir;
        }

        /// <summary>
        /// Перемещает файл в Config/RecycleBin. При конфликте имён старый файл в корзине удаляется.
        /// </summary>
        private void MoveToRecycleBin(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            try
            {
                string binDir = Path.Combine(GetConfigDir(), "RecycleBin");
                Directory.CreateDirectory(binDir);

                string dest = Path.Combine(binDir, Path.GetFileName(filePath));

                if (File.Exists(dest))
                    File.Delete(dest);

                File.Move(filePath, dest);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MoveToRecycleBin] Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Открывает диалог изменения приоритета выделенного комбо.
        /// </summary>
        private void ChangeSelectedComboPriority()
        {
            int index = checkedListBoxCombo.SelectedIndex;
            if (index < 0 || index >= _orchestrator.Combos.Count)
                return;

            var combo = _orchestrator.Combos[index];

            using var dialog = new Form
            {
                Text = "Приоритет",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new System.Drawing.Size(240, 110),
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false
            };

            var label = new Label
            {
                Text = "Приоритет (0-99):",
                Location = new System.Drawing.Point(12, 15),
                AutoSize = true
            };

            var numeric = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 99,
                Value = Math.Clamp(combo.Priority, 0, 99),
                Location = new System.Drawing.Point(12, 40),
                Width = 100
            };

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(60, 75),
                Width = 75
            };

            var cancelButton = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(145, 75),
                Width = 75
            };

            dialog.Controls.Add(label);
            dialog.Controls.Add(numeric);
            dialog.Controls.Add(okButton);
            dialog.Controls.Add(cancelButton);
            dialog.AcceptButton = okButton;
            dialog.CancelButton = cancelButton;

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            int newPriority = (int)numeric.Value;
            if (newPriority == combo.Priority)
                return;

            combo.Priority = newPriority;

            // Перезаписываем JSON комбо
            UpdateComboFileState(combo.FilePath, combo.IsEnabled, newPriority);

            // Пересортировываем оркестратор и перерисовываем список
            RebuildComboList();
        }

        /// <summary>
        /// Удаляет выделенное комбо: файл — в корзину, комбо — из оркестратора и UI.
        /// </summary>
        private void DeleteSelectedCombo()
        {
            int index = checkedListBoxCombo.SelectedIndex;
            if (index < 0 || index >= _orchestrator.Combos.Count)
                return;

            var combo = _orchestrator.Combos[index];

            var result = MessageBox.Show(
                $"Удалить комбо \"{combo.Name}\"?\nФайл будет перемещён в Config/RecycleBin.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            MoveToRecycleBin(combo.FilePath);

            _orchestrator.Combos.RemoveAt(index);
            RebuildComboList();
        }

        /// <summary>
        /// Пересортировывает комбо по приоритету и перерисовывает список в UI.
        /// </summary>
        private void RebuildComboList()
        {
            var sorted = _orchestrator.Combos
                .OrderByDescending(c => c.Priority)
                .ToList();

            _orchestrator.Combos.Clear();
            _orchestrator.Combos.AddRange(sorted);

            _isLoadingCombos = true;
            try
            {
                checkedListBoxCombo.Items.Clear();
                foreach (var combo in _orchestrator.Combos)
                {
                    int i = checkedListBoxCombo.Items.Add(combo.Name);
                    checkedListBoxCombo.SetItemChecked(i, combo.IsEnabled);
                }
            }
            finally
            {
                _isLoadingCombos = false;
            }
        }

        /// <summary>
        /// Размещает дочернюю форму строго по центру главного окна.
        /// </summary>
        private void CenterFormOnMain(Form child)
        {
            if (child == null) return;

            // Если главное окно свёрнуто — используем его RestoreBounds
            Rectangle mainBounds = (this.WindowState == FormWindowState.Normal)
                ? this.Bounds
                : this.RestoreBounds;

            int centerX = mainBounds.X + (mainBounds.Width - child.Width) / 2;
            int centerY = mainBounds.Y + (mainBounds.Height - child.Height) / 2;

            child.StartPosition = FormStartPosition.Manual;
            child.Location = new Point(centerX, centerY);
        }

        /// <summary>
        /// Открывает выделенное в списке комбо в редакторе.
        /// </summary>
        private void OpenSelectedComboInEditor()
        {
            string? selectedCombo = checkedListBoxCombo.SelectedItem as string;

            ComboEditorForm editor = new ComboEditorForm(selectedCombo);
            editor.FormClosed += (s, args) => LoadCombosFromConfig();
            CenterFormOnMain(editor);
            editor.ShowDialog(this);
        }

        private void CheckedListBoxCombo_DoubleClick(object? sender, EventArgs e)
        {
            if (checkedListBoxCombo.SelectedItem != null)
            {
                OpenSelectedComboInEditor();
            }
        }

        private async Task OnFrameCapturedHandler(FrameBuffer frame)
        {
            // Тихая пауза: если окно активно или открыт редактор — просто пропускаем кадр,
            // не сбрасывая состояния комбо.
            if (BotStateController.CanExecuteCombos)
            {
                _wasPaused = false;
                await _orchestrator.ProcessFrameAsync(frame);
            }
            else
            {
                // Если только что ушли в паузу (например, открыли окно настроек) - отпускаем все залипшие кнопки
                if (!_wasPaused)
                {
                    _wasPaused = true;
                    PixelMacroEngine.Core.Services.ArduinoHidService.EmergencyReset();
                }
            }
        }

        private void LoadCombosFromConfig()
        {
            try
            {
                string configDir = GetConfigDir();

                _isLoadingCombos = true;
                try
                {
                    // Очищаем оркестратор и UI перед загрузкой
                    _orchestrator.Combos.Clear();
                    checkedListBoxCombo.Items.Clear();
                    _changedComboNames.Clear();

                    // Сначала собираем все валидные комбо
                    var loaded = new List<ActiveCombo>();

                    foreach (var file in Directory.GetFiles(configDir, "*.json"))
                    {
                        if (string.Equals(Path.GetFileName(file), SessionFileName, StringComparison.OrdinalIgnoreCase))
                            continue;

                        try
                        {
                            string json = File.ReadAllText(file);
                            var profile = JsonSerializer.Deserialize<ComboProfile>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (profile == null) continue;

                            var combo = ComboFactory.Create(profile);
                            combo.IsEnabled = profile.IsOnOff;
                            combo.FilePath = file;

                            loaded.Add(combo);
                        }
                        catch
                        {
                            // Пропускаем битые файлы
                        }
                    }

                    // Сортируем по приоритету (от большего к меньшему) и регистрируем
                    var sorted = loaded.OrderByDescending(c => c.Priority).ToList();

                    foreach (var combo in sorted)
                    {
                        _orchestrator.RegisterCombo(combo);

                        int index = checkedListBoxCombo.Items.Add(combo.Name);
                        checkedListBoxCombo.SetItemChecked(index, combo.IsEnabled);
                    }
                }
                finally
                {
                    _isLoadingCombos = false;
                }
            }
            catch
            {
                // Игнорируем ошибки чтения каталога
            }
        }

        private void CheckedListBoxCombo_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_isLoadingCombos)
                return;

            if (e.Index < 0 || e.Index >= _orchestrator.Combos.Count)
                return;

            // ItemCheck срабатывает ДО применения нового состояния, поэтому учитываем e.NewValue
            var combo = _orchestrator.Combos[e.Index];
            bool isEnabled = (e.NewValue == CheckState.Checked);
            combo.IsEnabled = isEnabled;
            _changedComboNames.Add(combo.Name);

            // Мгновенно обновляем JSON-файл комбо на диске
            UpdateComboFileState(combo.FilePath, isEnabled);
        }

        /// <summary>
        /// Записывает новое значение Priority в JSON-файл комбо.
        /// </summary>
        private void UpdateComboFilePriority(string? filePath, int priority)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;
            try
            {
                string json = File.ReadAllText(filePath);
                var node = System.Text.Json.Nodes.JsonNode.Parse(json);
                if (node != null)
                {
                    node["Priority"] = priority.ToString();
                    File.WriteAllText(filePath, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                }
            }
            catch { }
        }

        /// <summary>
        /// Мгновенно записывает состояние IsOnOff (и, опционально, Priority) в JSON-файл комбо.
        /// </summary>
        private void UpdateComboFileState(string? filePath, bool isOnOff, int? priority = null)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            try
            {
                string json = File.ReadAllText(filePath);
                var node = System.Text.Json.Nodes.JsonNode.Parse(json);
                if (node != null)
                {
                    node["IsOnOff"] = isOnOff;
                    if (priority.HasValue)
                        node["Priority"] = priority.Value;

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(filePath, node.ToJsonString(options));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateComboFileState] Ошибка записи {filePath}: {ex.Message}");
            }
        }

        private void CheckBoxStatus_CheckedChanged(object? sender, EventArgs e)
        {
            // Синхронизируем мастер-флаг с галочкой
            BotStateController.IsMasterEnabled = checkBoxStatus.Checked;

            if (!checkBoxStatus.Checked)
            {
                // При явном выключении — сбрасываем кулдауны всех комбо
                _orchestrator.ResetAllStates();

                // Стоп-кран: аварийно отпускаем все залипшие клавиши на плате
                PixelMacroEngine.Core.Services.ArduinoHidService.EmergencyReset();
            }
        }

        private void SessionTimer_Tick(object? sender, EventArgs e)
        {
            // Время активной игры накручиваем только когда комбо реально могут работать
            if (BotStateController.CanExecuteCombos)
            {
                BotStateController.ActiveSessionTime = BotStateController.ActiveSessionTime.Add(TimeSpan.FromSeconds(1));
                UpdateActiveTimeLabel();
            }
        }

        private void UpdateActiveTimeLabel()
        {
            labelActiveTime.Text = BotStateController.ActiveSessionTime.ToString(@"hh\:mm\:ss");
        }

        private void TrackBarTransparency_Scroll(object? sender, EventArgs e)
        {
            this.Opacity = trackBarTransparency.Value / 100.0;
        }

        /// <summary>
        /// Потокобезопасно добавляет сообщение в список логов listBoxActiv.
        /// </summary>
        private void AppendLogMessage(string message)
        {
            if (listBoxActiv.IsDisposed) return;

            if (listBoxActiv.InvokeRequired)
            {
                listBoxActiv.BeginInvoke(() => AppendLogMessage(message));
                return;
            }

            listBoxActiv.Items.Add(message);

            if (listBoxActiv.Items.Count > 150)
                listBoxActiv.Items.RemoveAt(0);

            listBoxActiv.TopIndex = listBoxActiv.Items.Count - 1;
        }

        private void RestoreSessionTime()
        {
            try
            {
                string path = Path.Combine(GetConfigDir(), SessionFileName);
                if (!File.Exists(path))
                    return;

                string json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<SessionData>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data == null) return;

                double minutesAway = (DateTime.UtcNow - data.LastExitUtc).TotalMinutes;
                double seconds = data.ActiveSeconds;

                if (minutesAway <= 10)
                {
                    // 100% времени
                }
                else if (minutesAway <= 120)
                {
                    // Охлаждение усталости: -50%
                    seconds *= 0.5;
                }
                else
                {
                    // Полный сброс
                    seconds = 0;
                }

                BotStateController.ActiveSessionTime = TimeSpan.FromSeconds(seconds);

                // Восстанавливаем прозрачность окна
                int transparency = data.TransparencyPercent >= 10 ? data.TransparencyPercent : 100;
                trackBarTransparency.Value = Math.Clamp(transparency, 10, 100);
                this.Opacity = trackBarTransparency.Value / 100.0;

                // Восстанавливаем позицию окна, если она попадает в видимую область хотя бы одного экрана
                if (data.WindowX.HasValue && data.WindowY.HasValue)
                {
                    Point pt = new Point(data.WindowX.Value, data.WindowY.Value);
                    if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(new Rectangle(pt, this.Size))))
                    {
                        this.StartPosition = FormStartPosition.Manual;
                        this.Location = pt;
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки восстановления
            }
        }

        private void SaveSessionTime()
        {
            try
            {
                string configDir = GetConfigDir();

                // Определяем реальные координаты окна (с защитой от свёрнутого состояния)
                Point loc = (this.WindowState == FormWindowState.Normal)
                    ? this.Location
                    : this.RestoreBounds.Location;

                var data = new SessionData
                {
                    LastExitUtc = DateTime.UtcNow,
                    ActiveSeconds = BotStateController.ActiveSessionTime.TotalSeconds,
                    TransparencyPercent = trackBarTransparency.Value,
                    WindowX = loc.X,
                    WindowY = loc.Y
                };

                string json = JsonSerializer.Serialize(data,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(Path.Combine(configDir, SessionFileName), json);
            }
            catch
            {
                // Игнорируем ошибки сохранения
            }
        }

        // При закрытии главного окна — корректно глушим видеокарту
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                ActionLogger.OnLog -= AppendLogMessage;
                ScreenCaptureService.OnFrameCaptured -= OnFrameCapturedHandler;
                _sessionTimer?.Stop();
                _sessionTimer?.Dispose();
                _sessionTimer = null;

                SaveSessionTime();

                // Аварийный сброс платы и отключение HID
                ArduinoHidService.EmergencyReset();
                ArduinoHidService.Disconnect();
            }
            catch
            {
                // Игнорируем ошибки при закрытии
            }

            ScreenCaptureService.Stop();
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            testArduino settingsForm = new testArduino();
            CenterFormOnMain(settingsForm);
            settingsForm.Show(this);
        }

        private void buttonComboForm_Click(object sender, EventArgs e)
        {
            // Если в списке выделено комбо — открываем редактор с его загрузкой
            OpenSelectedComboInEditor();
        }

        /// <summary>Данные сессии для сохранения/восстановления времени активности.</summary>
        private class SessionData
        {
            public DateTime LastExitUtc { get; set; }
            public double ActiveSeconds { get; set; }
            public int TransparencyPercent { get; set; } = 100;
            public int? WindowX { get; set; }
            public int? WindowY { get; set; }
        }
    }
}
