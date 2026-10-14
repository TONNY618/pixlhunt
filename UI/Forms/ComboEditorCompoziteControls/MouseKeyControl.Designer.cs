namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    partial class MouseKeyControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            groupBoxMouseKey = new GroupBox();
            comboBoxMouseKeyList = new ComboBox();
            checkBoxMouseKey2 = new CheckBox();
            checkBoxMouseKey1 = new CheckBox();
            groupBoxMouseKey.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxMouseKey
            // 
            groupBoxMouseKey.BackColor = Color.Cyan;
            groupBoxMouseKey.Controls.Add(comboBoxMouseKeyList);
            groupBoxMouseKey.Controls.Add(checkBoxMouseKey2);
            groupBoxMouseKey.Controls.Add(checkBoxMouseKey1);
            groupBoxMouseKey.ForeColor = SystemColors.HotTrack;
            groupBoxMouseKey.Location = new Point(0, 0);
            groupBoxMouseKey.Name = "groupBoxMouseKey";
            groupBoxMouseKey.Size = new Size(240, 31);
            groupBoxMouseKey.TabIndex = 62;
            groupBoxMouseKey.TabStop = false;
            groupBoxMouseKey.Text = "Key Mouse";
            // 
            // comboBoxMouseKeyList
            // 
            comboBoxMouseKeyList.FormattingEnabled = true;
            comboBoxMouseKeyList.Items.AddRange(new object[] { "L key", "R key", "M key" });
            comboBoxMouseKeyList.Location = new Point(180, 3);
            comboBoxMouseKeyList.Name = "comboBoxMouseKeyList";
            comboBoxMouseKeyList.Size = new Size(54, 23);
            comboBoxMouseKeyList.TabIndex = 19;
            comboBoxMouseKeyList.Text = "L key";
            // 
            // checkBoxMouseKey2
            // 
            checkBoxMouseKey2.AutoSize = true;
            checkBoxMouseKey2.Checked = true;
            checkBoxMouseKey2.CheckState = CheckState.Checked;
            checkBoxMouseKey2.Location = new Point(134, 5);
            checkBoxMouseKey2.Name = "checkBoxMouseKey2";
            checkBoxMouseKey2.Size = new Size(40, 19);
            checkBoxMouseKey2.TabIndex = 20;
            checkBoxMouseKey2.Text = "up";
            // 
            // checkBoxMouseKey1
            // 
            checkBoxMouseKey1.AutoSize = true;
            checkBoxMouseKey1.Checked = true;
            checkBoxMouseKey1.CheckState = CheckState.Checked;
            checkBoxMouseKey1.Location = new Point(72, 5);
            checkBoxMouseKey1.Name = "checkBoxMouseKey1";
            checkBoxMouseKey1.Size = new Size(56, 19);
            checkBoxMouseKey1.TabIndex = 21;
            checkBoxMouseKey1.Text = "down";
            // 
            // MouseKeyControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(groupBoxMouseKey);
            Name = "MouseKeyControl";
            Size = new Size(566, 252);
            groupBoxMouseKey.ResumeLayout(false);
            groupBoxMouseKey.PerformLayout();
            ResumeLayout(false);
        }

        public System.Windows.Forms.GroupBox groupBoxMouseKey;
        public System.Windows.Forms.ComboBox comboBoxMouseKeyList;
        public System.Windows.Forms.CheckBox checkBoxMouseKey2;
        public System.Windows.Forms.CheckBox checkBoxMouseKey1;
    }
}
