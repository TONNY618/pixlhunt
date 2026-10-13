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
            groupBoxMouseKey = new System.Windows.Forms.GroupBox();
            comboBoxMouseKeyList = new System.Windows.Forms.ComboBox();
            checkBoxMouseKey2 = new System.Windows.Forms.CheckBox();
            checkBoxMouseKey1 = new System.Windows.Forms.CheckBox();
            groupBoxMouseKey.SuspendLayout();
            SuspendLayout();

            groupBoxMouseKey.BackColor = System.Drawing.Color.Gainsboro;
            groupBoxMouseKey.Controls.Add(comboBoxMouseKeyList);
            groupBoxMouseKey.Controls.Add(checkBoxMouseKey2);
            groupBoxMouseKey.Controls.Add(checkBoxMouseKey1);
            groupBoxMouseKey.ForeColor = System.Drawing.SystemColors.HotTrack;
            groupBoxMouseKey.Location = new System.Drawing.Point(0, 0);
            groupBoxMouseKey.Name = "groupBoxMouseKey";
            groupBoxMouseKey.Size = new System.Drawing.Size(240, 28);
            groupBoxMouseKey.TabIndex = 62;
            groupBoxMouseKey.TabStop = false;
            groupBoxMouseKey.Text = "Mouse";

            comboBoxMouseKeyList.FormattingEnabled = true;
            comboBoxMouseKeyList.Items.AddRange(new object[] { "L key", "R key", "M key" });
            comboBoxMouseKeyList.Location = new System.Drawing.Point(63, 3);
            comboBoxMouseKeyList.Name = "comboBoxMouseKeyList";
            comboBoxMouseKeyList.Size = new System.Drawing.Size(58, 23);
            comboBoxMouseKeyList.TabIndex = 19;
            comboBoxMouseKeyList.Text = "L key";

            checkBoxMouseKey2.AutoSize = true;
            checkBoxMouseKey2.Checked = true;
            checkBoxMouseKey2.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxMouseKey2.Location = new System.Drawing.Point(194, 5);
            checkBoxMouseKey2.Name = "checkBoxMouseKey2";
            checkBoxMouseKey2.Size = new System.Drawing.Size(40, 19);
            checkBoxMouseKey2.Text = "up";

            checkBoxMouseKey1.AutoSize = true;
            checkBoxMouseKey1.Checked = true;
            checkBoxMouseKey1.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxMouseKey1.Location = new System.Drawing.Point(133, 5);
            checkBoxMouseKey1.Name = "checkBoxMouseKey1";
            checkBoxMouseKey1.Size = new System.Drawing.Size(56, 19);
            checkBoxMouseKey1.Text = "down";

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Transparent;
            Controls.Add(groupBoxMouseKey);
            Name = "MouseKeyControl";
            Size = new System.Drawing.Size(240, 28);
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
