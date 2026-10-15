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
            groupBoxSound = new GroupBox();
            trackBarMusicVol = new TrackBar();
            comboBoxSoundList = new ComboBox();
            groupBoxSound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarMusicVol).BeginInit();
            SuspendLayout();
            // 
            // groupBoxSound
            // 
            groupBoxSound.BackColor = Color.Gainsboro;
            groupBoxSound.Controls.Add(trackBarMusicVol);
            groupBoxSound.Controls.Add(comboBoxSoundList);
            groupBoxSound.Dock = DockStyle.Fill;
            groupBoxSound.ForeColor = Color.DarkOliveGreen;
            groupBoxSound.Location = new Point(0, 0);
            groupBoxSound.Name = "groupBoxSound";
            groupBoxSound.Size = new Size(318, 28);
            groupBoxSound.TabIndex = 0;
            groupBoxSound.TabStop = false;
            groupBoxSound.Text = "Sound";
            // 
            // trackBarMusicVol
            // 
            trackBarMusicVol.Location = new Point(226, 2);
            trackBarMusicVol.Maximum = 100;
            trackBarMusicVol.Minimum = 0;
            trackBarMusicVol.Name = "trackBarMusicVol";
            trackBarMusicVol.Size = new Size(86, 24);
            trackBarMusicVol.TabIndex = 18;
            trackBarMusicVol.TickStyle = TickStyle.None;
            trackBarMusicVol.Value = 100;
            // 
            // comboBoxSoundList
            // 
            comboBoxSoundList.FormattingEnabled = true;
            comboBoxSoundList.Location = new Point(53, 3);
            comboBoxSoundList.Name = "comboBoxSoundList";
            comboBoxSoundList.Size = new Size(168, 23);
            comboBoxSoundList.TabIndex = 16;
            // 
            // SoundControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBoxSound);
            Name = "SoundControl";
            Size = new Size(318, 28);
            groupBoxSound.ResumeLayout(false);
            groupBoxSound.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarMusicVol).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSound;
        private System.Windows.Forms.ComboBox comboBoxSoundList;
        private TrackBar trackBarMusicVol;
    }
}
