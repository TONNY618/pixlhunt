namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    partial class TriggerTimer
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
            groupBoxTriggerTimer = new GroupBox();
            textBoxTriggerTimer = new TextBox();
            labelTriggerTimer = new Label();
            groupBoxTriggerTimer.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxTriggerTimer
            // 
            groupBoxTriggerTimer.BackColor = SystemColors.Info;
            groupBoxTriggerTimer.Controls.Add(textBoxTriggerTimer);
            groupBoxTriggerTimer.Controls.Add(labelTriggerTimer);
            groupBoxTriggerTimer.Location = new Point(0, 0);
            groupBoxTriggerTimer.Name = "groupBoxTriggerTimer";
            groupBoxTriggerTimer.Size = new Size(155, 44);
            groupBoxTriggerTimer.TabIndex = 0;
            groupBoxTriggerTimer.TabStop = false;
            groupBoxTriggerTimer.Text = "Таймер";
            // 
            // textBoxTriggerTimer
            // 
            textBoxTriggerTimer.Location = new Point(97, 11);
            textBoxTriggerTimer.MaxLength = 6;
            textBoxTriggerTimer.Name = "textBoxTriggerTimer";
            textBoxTriggerTimer.Size = new Size(50, 23);
            textBoxTriggerTimer.TabIndex = 1;
            textBoxTriggerTimer.Text = "1000";
            // 
            // labelTriggerTimer
            // 
            labelTriggerTimer.AutoSize = true;
            labelTriggerTimer.Location = new Point(6, 14);
            labelTriggerTimer.Name = "labelTriggerTimer";
            labelTriggerTimer.Size = new Size(89, 15);
            labelTriggerTimer.TabIndex = 0;
            labelTriggerTimer.Text = "Каждые ~X, мс";
            // 
            // TriggerTimer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(groupBoxTriggerTimer);
            Name = "TriggerTimer";
            Size = new Size(155, 44);
            groupBoxTriggerTimer.ResumeLayout(false);
            groupBoxTriggerTimer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public GroupBox groupBoxTriggerTimer;
        public TextBox textBoxTriggerTimer;
        public Label labelTriggerTimer;
    }
}
