namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    partial class TextToControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            groupBoxTextTo = new System.Windows.Forms.GroupBox();
            radioButtonTextToTelegtamm = new System.Windows.Forms.RadioButton();
            radioButtonTextToContext = new System.Windows.Forms.RadioButton();
            textBoxTextToTeleNameUser = new System.Windows.Forms.TextBox();
            textBoxTextToEdit = new System.Windows.Forms.TextBox();
            groupBoxTextTo.SuspendLayout();
            SuspendLayout();

            groupBoxTextTo.BackColor = System.Drawing.Color.Gainsboro;
            groupBoxTextTo.Controls.Add(radioButtonTextToTelegtamm);
            groupBoxTextTo.Controls.Add(radioButtonTextToContext);
            groupBoxTextTo.Controls.Add(textBoxTextToTeleNameUser);
            groupBoxTextTo.Controls.Add(textBoxTextToEdit);
            groupBoxTextTo.ForeColor = System.Drawing.Color.DarkOliveGreen;
            groupBoxTextTo.Location = new System.Drawing.Point(0, 0);
            groupBoxTextTo.Name = "groupBoxTextTo";
            groupBoxTextTo.Size = new System.Drawing.Size(318, 85);
            groupBoxTextTo.TabIndex = 46;
            groupBoxTextTo.TabStop = false;
            groupBoxTextTo.Text = "Фраза / Сообщение";

            radioButtonTextToTelegtamm.AutoSize = true;
            radioButtonTextToTelegtamm.Location = new System.Drawing.Point(80, 17);
            radioButtonTextToTelegtamm.Name = "radioButtonTextToTelegtamm";
            radioButtonTextToTelegtamm.Size = new System.Drawing.Size(84, 19);
            radioButtonTextToTelegtamm.Text = "Telegramm";

            radioButtonTextToContext.AutoSize = true;
            radioButtonTextToContext.Checked = true;
            radioButtonTextToContext.Location = new System.Drawing.Point(6, 17);
            radioButtonTextToContext.Name = "radioButtonTextToContext";
            radioButtonTextToContext.Size = new System.Drawing.Size(66, 19);
            radioButtonTextToContext.Text = "Context";

            textBoxTextToTeleNameUser.Location = new System.Drawing.Point(170, 16);
            textBoxTextToTeleNameUser.Name = "textBoxTextToTeleNameUser";
            textBoxTextToTeleNameUser.Size = new System.Drawing.Size(140, 23);
            textBoxTextToTeleNameUser.Text = "@Slavik_ori";

            textBoxTextToEdit.AcceptsReturn = true;
            textBoxTextToEdit.Location = new System.Drawing.Point(6, 43);
            textBoxTextToEdit.MaxLength = 500;
            textBoxTextToEdit.Multiline = true;
            textBoxTextToEdit.Name = "textBoxTextToEdit";
            textBoxTextToEdit.Size = new System.Drawing.Size(304, 36);
            textBoxTextToEdit.Text = "Текст (Enter работает)";
            textBoxTextToEdit.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Transparent;
            Controls.Add(groupBoxTextTo);
            Name = "TextToControl";
            Size = new System.Drawing.Size(318, 85);
            groupBoxTextTo.ResumeLayout(false);
            groupBoxTextTo.PerformLayout();
            ResumeLayout(false);
        }

        public System.Windows.Forms.GroupBox groupBoxTextTo;
        public System.Windows.Forms.RadioButton radioButtonTextToTelegtamm;
        public System.Windows.Forms.RadioButton radioButtonTextToContext;
        public System.Windows.Forms.TextBox textBoxTextToTeleNameUser;
        public System.Windows.Forms.TextBox textBoxTextToEdit;
    }
}
