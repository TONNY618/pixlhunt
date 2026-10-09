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
        private TimeSpan _activeSessionTime = TimeSpan.Zero;
        private const string SessionFileName = "session.json";

        public pxlHunt()
        {
            InitializeComponent();

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

            // Снятие выделения при клике вне списка комбо
            this.MouseDown += (s, e) => checkedListBoxCombo.ClearSelected();
            SubscribeClearSelection(this);

            UpdateActiveTimeLabel();
        }

        /// <summary>
        /// Рекурсивно подписывает MouseDown всех дочерних контролов (кроме самого списка)
        /// на сброс выделения в checkedListBoxCombo.
        /// </summary>
        private void SubscribeClearSelection(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl == checkedListBoxCombo)
                    continue;

                ctrl.MouseDown += (s, e) => checkedListBoxCombo.ClearSelected();

                if (ctrl.HasChildren)
                {
                    SubscribeClearSelection(ctrl);
                }
            }
        }

        private async Task OnFrameCapturedHandler(FrameBuffer frame)
        {
            if (checkBoxStatus.Checked)
            {
                await _orchestrator.ProcessFrameAsync(frame);
            }
        }

        private void LoadCombosFromConfig()
        {
            try
            {
                string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
                if (!Directory.Exists(configDir))
                {
                    configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Config");
                }

                if (!Directory.Exists(configDir))
                    return;

                // Очищаем оркестратор и UI перед загрузкой
                _orchestrator.Combos.Clear();
                checkedListBoxCombo.Items.Clear();

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
            catch
            {
                // Игнорируем ошибки чтения каталога
            }
        }

        private void CheckedListBoxCombo_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _orchestrator.Combos.Count)
                return;

            // ItemCheck срабатывает ДО применения нового состояния, поэтому учитываем e.NewValue
            _orchestrator.Combos[e.Index].IsEnabled = e.NewValue == CheckState.Checked;
        }

        private void CheckBoxStatus_CheckedChanged(object? sender, EventArgs e)
        {
            if (!checkBoxStatus.Checked)
            {
                // При выключении — сбрасываем кулдауны всех комбо
                _orchestrator.ResetAllStates();
            }
        }

        private void SessionTimer_Tick(object? sender, EventArgs e)
        {
            if (checkBoxStatus.Checked)
            {
                _activeSessionTime = _activeSessionTime.Add(TimeSpan.FromSeconds(1));
                UpdateActiveTimeLabel();
            }
        }

        private void UpdateActiveTimeLabel()
        {
            labelActiveTime.Text = _activeSessionTime.ToString(@"hh\:mm\:ss");
        }

        private void RestoreSessionTime()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", SessionFileName);
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

                _activeSessionTime = TimeSpan.FromSeconds(seconds);
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
                string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
                Directory.CreateDirectory(configDir);

                var data = new SessionData
                {
                    LastExitUtc = DateTime.UtcNow,
                    ActiveSeconds = _activeSessionTime.TotalSeconds
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
                ScreenCaptureService.OnFrameCaptured -= OnFrameCapturedHandler;
                _sessionTimer?.Stop();
                _sessionTimer?.Dispose();
                _sessionTimer = null;

                SaveSessionTime();
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
            settingsForm.Show();
        }

        private void buttonComboForm_Click(object sender, EventArgs e)
        {
            // Если в списке выделено комбо — открываем редактор с его загрузкой
            string? selectedCombo = checkedListBoxCombo.SelectedItem as string;

            ComboEditorForm editor = new ComboEditorForm(selectedCombo);

            // После закрытия редактора перезагружаем список комбо
            editor.FormClosed += (s, args) => LoadCombosFromConfig();

            editor.Show();
        }

        /// <summary>Данные сессии для сохранения/восстановления времени активности.</summary>
        private class SessionData
        {
            public DateTime LastExitUtc { get; set; }
            public double ActiveSeconds { get; set; }
        }
    }
}
