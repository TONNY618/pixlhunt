namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    partial class SoundControl
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
            this.groupBoxSound = new System.Windows.Forms.GroupBox();
            this.buttonSoundAddCustom = new System.Windows.Forms.Button();
            this.comboBoxSoundList = new System.Windows.Forms.ComboBox();
            this.groupBoxSound.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSound
            // 
            this.groupBoxSound.BackColor = System.Drawing.Color.Gainsboro;
            this.groupBoxSound.Controls.Add(this.buttonSoundAddCustom);
            this.groupBoxSound.Controls.Add(this.comboBoxSoundList);
            this.groupBoxSound.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSound.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.groupBoxSound.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSound.Name = "groupBoxSound";
            this.groupBoxSound.Size = new System.Drawing.Size(318, 28);
            this.groupBoxSound.TabIndex = 0;
            this.groupBoxSound.TabStop = false;
            this.groupBoxSound.Text = "Sound";
            // 
            // buttonSoundAddCustom
            // 
            this.buttonSoundAddCustom.Location = new System.Drawing.Point(243, 3);
            this.buttonSoundAddCustom.Name = "buttonSoundAddCustom";
            this.buttonSoundAddCustom.Size = new System.Drawing.Size(67, 23);
            this.buttonSoundAddCustom.TabIndex = 17;
            this.buttonSoundAddCustom.Text = "Custom";
            this.buttonSoundAddCustom.UseVisualStyleBackColor = true;
            // 
            // comboBoxSoundList
            // 
            this.comboBoxSoundList.FormattingEnabled = true;
            this.comboBoxSoundList.Location = new System.Drawing.Point(53, 3);
            this.comboBoxSoundList.Name = "comboBoxSoundList";
            this.comboBoxSoundList.Size = new System.Drawing.Size(183, 23);
            this.comboBoxSoundList.TabIndex = 16;
            // 
            // SoundControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxSound);
            this.Name = "SoundControl";
            this.Size = new System.Drawing.Size(318, 28);
            this.groupBoxSound.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSound;
        private System.Windows.Forms.Button buttonSoundAddCustom;
        private System.Windows.Forms.ComboBox comboBoxSoundList;
    }
}
