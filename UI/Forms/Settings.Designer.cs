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
            // Settings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 556);
            Controls.Add(checkBoxStatistyc);
            Name = "Settings";
            Text = "Settings";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox checkBoxStatistyc;
    }
}