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
            button1 = new Button();
            textBoxtextBoxTriggerTimer2 = new TextBox();
            textBoxTriggerTimer = new TextBox();
            groupBoxTriggerTimer.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxTriggerTimer
            // 
            groupBoxTriggerTimer.BackColor = SystemColors.Info;
            groupBoxTriggerTimer.Controls.Add(button1);
            groupBoxTriggerTimer.Controls.Add(textBoxtextBoxTriggerTimer2);
            groupBoxTriggerTimer.Controls.Add(textBoxTriggerTimer);
            groupBoxTriggerTimer.Location = new Point(0, 0);
            groupBoxTriggerTimer.Name = "groupBoxTriggerTimer";
            groupBoxTriggerTimer.Size = new Size(180, 43);
            groupBoxTriggerTimer.TabIndex = 0;
            groupBoxTriggerTimer.TabStop = false;
            groupBoxTriggerTimer.Text = "Таймер";
            // 
            // button1
            // 
            button1.Location = new Point(140, 14);
            button1.Margin = new Padding(0);
            button1.Name = "button1";
            button1.Size = new Size(33, 23);
            button1.TabIndex = 3;
            button1.Text = "?";
            button1.TextAlign = ContentAlignment.TopCenter;
            button1.UseVisualStyleBackColor = true;
            // 
            // textBoxtextBoxTriggerTimer2
            // 
            textBoxtextBoxTriggerTimer2.Location = new Point(73, 14);
            textBoxtextBoxTriggerTimer2.MaxLength = 8;
            textBoxtextBoxTriggerTimer2.Name = "textBoxtextBoxTriggerTimer2";
            textBoxtextBoxTriggerTimer2.Size = new Size(64, 23);
            textBoxtextBoxTriggerTimer2.TabIndex = 2;
            textBoxtextBoxTriggerTimer2.Text = "12345678";
            // 
            // textBoxTriggerTimer
            // 
            textBoxTriggerTimer.Location = new Point(6, 14);
            textBoxTriggerTimer.MaxLength = 8;
            textBoxTriggerTimer.Name = "textBoxTriggerTimer";
            textBoxTriggerTimer.Size = new Size(61, 23);
            textBoxTriggerTimer.TabIndex = 1;
            textBoxTriggerTimer.Text = "12345678";
            // 
            // TriggerTimer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(groupBoxTriggerTimer);
            Name = "TriggerTimer";
            Size = new Size(182, 46);
            groupBoxTriggerTimer.ResumeLayout(false);
            groupBoxTriggerTimer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public GroupBox groupBoxTriggerTimer;
        public TextBox textBoxTriggerTimer;
        public TextBox textBoxtextBoxTriggerTimer2;
        private Button button1;
    }
}
