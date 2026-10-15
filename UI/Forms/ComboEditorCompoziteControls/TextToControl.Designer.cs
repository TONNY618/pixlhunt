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
            textBoxTextToEdit = new System.Windows.Forms.TextBox();
            groupBoxTextTo.SuspendLayout();
            SuspendLayout();

            groupBoxTextTo.BackColor = System.Drawing.Color.Gainsboro;
            groupBoxTextTo.Controls.Add(textBoxTextToEdit);
            groupBoxTextTo.ForeColor = System.Drawing.Color.DarkOliveGreen;
            groupBoxTextTo.Location = new System.Drawing.Point(0, 0);
            groupBoxTextTo.Name = "groupBoxTextTo";
            groupBoxTextTo.Size = new System.Drawing.Size(318, 85);
            groupBoxTextTo.TabIndex = 46;
            groupBoxTextTo.TabStop = false;
            groupBoxTextTo.Text = "Фраза / Сообщение";

            textBoxTextToEdit.AcceptsReturn = true;
            textBoxTextToEdit.Location = new System.Drawing.Point(6, 22);
            textBoxTextToEdit.MaxLength = 500;
            textBoxTextToEdit.Multiline = true;
            textBoxTextToEdit.Name = "textBoxTextToEdit";
            textBoxTextToEdit.Size = new System.Drawing.Size(304, 55);
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
        public System.Windows.Forms.TextBox textBoxTextToEdit;
    }
}
