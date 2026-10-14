namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    partial class BoxKeyControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBoxKey = new GroupBox();
            checkBoxKey1 = new CheckBox();
            checkBoxKey2 = new CheckBox();
            comboBoxKeyList = new ComboBox();
            groupBoxKey.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxKey
            // 
            groupBoxKey.BackColor = Color.Cyan;
            groupBoxKey.Controls.Add(checkBoxKey1);
            groupBoxKey.Controls.Add(checkBoxKey2);
            groupBoxKey.Controls.Add(comboBoxKeyList);
            groupBoxKey.ForeColor = Color.DarkOliveGreen;
            groupBoxKey.Location = new Point(0, 0);
            groupBoxKey.Name = "groupBoxKey";
            groupBoxKey.Size = new Size(318, 28);
            groupBoxKey.TabIndex = 3;
            groupBoxKey.TabStop = false;
            groupBoxKey.Text = "Key";
            // 
            // checkBoxKey1
            // 
            checkBoxKey1.AutoSize = true;
            checkBoxKey1.Checked = true;
            checkBoxKey1.CheckState = CheckState.Checked;
            checkBoxKey1.Location = new Point(46, 4);
            checkBoxKey1.Name = "checkBoxKey1";
            checkBoxKey1.Size = new Size(56, 19);
            checkBoxKey1.TabIndex = 18;
            checkBoxKey1.Text = "down";
            checkBoxKey1.UseVisualStyleBackColor = true;
            // 
            // checkBoxKey2
            // 
            checkBoxKey2.AutoSize = true;
            checkBoxKey2.Checked = true;
            checkBoxKey2.CheckState = CheckState.Checked;
            checkBoxKey2.Location = new Point(104, 4);
            checkBoxKey2.Name = "checkBoxKey2";
            checkBoxKey2.Size = new Size(40, 19);
            checkBoxKey2.TabIndex = 17;
            checkBoxKey2.Text = "up";
            checkBoxKey2.UseVisualStyleBackColor = true;
            // 
            // comboBoxKeyList
            // 
            comboBoxKeyList.FormattingEnabled = true;
            comboBoxKeyList.Items.AddRange(new object[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Left Shift", "Left Ctrl", "Left Alt", "Right Shift", "Right Ctrl", "Right Alt", "Space", "Enter", "Tab", "Escape", "Backspace", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14", "F15", "F16", "F17", "F18", "F19", "F20", "F21", "F22", "F23", "F24", "Up Arrow", "Down Arrow", "Left Arrow", "Right Arrow", "Left Win", "Right Win", "Insert", "Delete", "Home", "End", "Page Up", "Page Down", "Print Screen", "Scroll Lock", "Pause", "Caps Lock", "` (Тильда)", "- (Минус)", "= (Равно)", "[", "]", "\\", ";", "'", ",", ".", "/", "NumPad 0", "NumPad 1", "NumPad 2", "NumPad 3", "NumPad 4", "NumPad 5", "NumPad 6", "NumPad 7", "NumPad 8", "NumPad 9", "NumPad +", "NumPad -", "NumPad *", "NumPad /", "NumPad Enter", "NumPad .", "NumLock" });
            comboBoxKeyList.Location = new Point(150, 2);
            comboBoxKeyList.Name = "comboBoxKeyList";
            comboBoxKeyList.Size = new Size(162, 23);
            comboBoxKeyList.TabIndex = 16;
            // 
            // BoxKeyControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBoxKey);
            Name = "BoxKeyControl";
            Size = new Size(318, 28);
            groupBoxKey.ResumeLayout(false);
            groupBoxKey.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBoxKey;
        private CheckBox checkBoxKey1;
        private CheckBox checkBoxKey2;
        private ComboBox comboBoxKeyList;
    }
}
