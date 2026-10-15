using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PixelMacroEngine.Core.Services;

namespace pxlhunt.FORMS
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();

            // Окно настроек всегда поверх остальных
            this.TopMost = true;

            // Загружаем текущую горячую клавишу в комбобокс
            comboBoxKeyStartMain.Text = AppConfigService.Instance.hotKeyONOFF;
            comboBoxKeyStartMain.SelectedIndexChanged += ComboBoxKeyStartMain_SelectedIndexChanged;

            // Страховка для редактируемого комбобокса (ручной ввод клавиши)
            this.FormClosing += Settings_FormClosing;
        }

        private void ComboBoxKeyStartMain_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SaveHotkeyFromCombo();
        }

        private void Settings_FormClosing(object? sender, FormClosingEventArgs e)
        {
            SaveHotkeyFromCombo();
        }

        /// <summary>
        /// Переносит выбранную клавишу из комбобокса в конфиг и сразу сохраняет его на диск.
        /// </summary>
        private void SaveHotkeyFromCombo()
        {
            string key = comboBoxKeyStartMain.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (string.Equals(AppConfigService.Instance.hotKeyONOFF, key, StringComparison.Ordinal))
                return;

            AppConfigService.Instance.hotKeyONOFF = key;
            AppConfigService.Save();
        }
    }
}
