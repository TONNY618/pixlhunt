namespace pxlhunt.FORMS
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            checkBoxStatistyc = new CheckBox();
            groupBoxKeyStartMain = new GroupBox();
            comboBoxKeyStartMain = new ComboBox();
            groupBoxTelegram = new GroupBox();
            comboBox1 = new ComboBox();
            textBox2 = new TextBox();
            groupBoxKeyStartMain.SuspendLayout();
            groupBoxTelegram.SuspendLayout();
            SuspendLayout();
            // 
            // checkBoxStatistyc
            // 
            checkBoxStatistyc.AutoSize = true;
            checkBoxStatistyc.Location = new Point(21, 26);
            checkBoxStatistyc.Name = "checkBoxStatistyc";
            checkBoxStatistyc.Size = new Size(118, 19);
            checkBoxStatistyc.TabIndex = 15;
            checkBoxStatistyc.Text = "Сбор статистики";
            checkBoxStatistyc.UseVisualStyleBackColor = true;
            // 
            // groupBoxKeyStartMain
            // 
            groupBoxKeyStartMain.BackColor = Color.Cyan;
            groupBoxKeyStartMain.Controls.Add(comboBoxKeyStartMain);
            groupBoxKeyStartMain.ForeColor = Color.DarkOliveGreen;
            groupBoxKeyStartMain.Location = new Point(21, 74);
            groupBoxKeyStartMain.Name = "groupBoxKeyStartMain";
            groupBoxKeyStartMain.Size = new Size(318, 105);
            groupBoxKeyStartMain.TabIndex = 16;
            groupBoxKeyStartMain.TabStop = false;
            groupBoxKeyStartMain.Text = "Клавиша ВКЛ/ВЫКЛ";
            // 
            // comboBoxKeyStartMain
            // 
            comboBoxKeyStartMain.FormattingEnabled = true;
            comboBoxKeyStartMain.Items.AddRange(new object[] { "NumPad Enter", "Caps Lock", "NumLock", "Left Shift", "Left Ctrl", "Left Alt", "Right Shift", "Right Ctrl", "Right Alt", "Space", "Enter", "Tab", "Escape", "Backspace", "Left Win", "Right Win", "Insert", "Delete", "Home", "End", "Page Up", "Page Down", "Print Screen", "Scroll Lock", "Pause", "` (Тильда)", "- (Минус)", "= (Равно)", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14", "F15", "F16", "F17", "F18", "F19", "F20", "F21", "F22", "F23", "F24", "Up Arrow", "Down Arrow", "Left Arrow", "Right Arrow", "[", "]", "\\", ";", "'", ",", ".", "/", "NumPad 0", "NumPad 1", "NumPad 2", "NumPad 3", "NumPad 4", "NumPad 5", "NumPad 6", "NumPad 7", "NumPad 8", "NumPad 9", "NumPad +", "NumPad -", "NumPad *", "NumPad /", "NumPad ." });
            comboBoxKeyStartMain.Location = new Point(49, 22);
            comboBoxKeyStartMain.Name = "comboBoxKeyStartMain";
            comboBoxKeyStartMain.Size = new Size(162, 23);
            comboBoxKeyStartMain.TabIndex = 16;
            comboBoxKeyStartMain.Text = "NumPad Enter";
            // 
            // groupBoxTelegram
            // 
            groupBoxTelegram.BackColor = Color.Gainsboro;
            groupBoxTelegram.Controls.Add(comboBox1);
            groupBoxTelegram.Controls.Add(textBox2);
            groupBoxTelegram.ForeColor = Color.DarkOliveGreen;
            groupBoxTelegram.Location = new Point(241, 244);
            groupBoxTelegram.Name = "groupBoxTelegram";
            groupBoxTelegram.Size = new Size(318, 68);
            groupBoxTelegram.TabIndex = 65;
            groupBoxTelegram.TabStop = false;
            groupBoxTelegram.Text = "Telegram";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(68, 6);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(242, 23);
            comboBox1.TabIndex = 3;
            // 
            // textBox2
            // 
            textBox2.AcceptsReturn = true;
            textBox2.Location = new Point(10, 33);
            textBox2.MaxLength = 500;
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(300, 28);
            textBox2.TabIndex = 2;
            textBox2.Text = "Текст (Enter работает)";
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 556);
            Controls.Add(groupBoxTelegram);
            Controls.Add(groupBoxKeyStartMain);
            Controls.Add(checkBoxStatistyc);
            Name = "Settings";
            Text = "Settings";
            groupBoxKeyStartMain.ResumeLayout(false);
            groupBoxTelegram.ResumeLayout(false);
            groupBoxTelegram.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox checkBoxStatistyc;
        private GroupBox groupBoxKeyStartMain;
        private ComboBox comboBoxKeyStartMain;
        private GroupBox groupBoxTelegram;
        private ComboBox comboBox1;
        private TextBox textBox2;
    }
}