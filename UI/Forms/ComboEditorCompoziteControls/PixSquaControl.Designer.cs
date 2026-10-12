namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    partial class PixSquaControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (_evalTimer != null)
            {
                _evalTimer.Stop();
                _evalTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            groupBoxPixSqua = new System.Windows.Forms.GroupBox();
            groupBoxPixSquaVol = new System.Windows.Forms.GroupBox();
            trackBarPixSqua = new System.Windows.Forms.TrackBar();
            textBoxPixSquaVolNumber = new System.Windows.Forms.TextBox();
            checkBoxPixSquaOverlay = new System.Windows.Forms.CheckBox();
            labelPixSquaBool = new System.Windows.Forms.Label();
            textBoxPixSquaXY2 = new System.Windows.Forms.TextBox();
            pictureBoxPixSquaSample = new System.Windows.Forms.PictureBox();
            buttonPixSquaView = new System.Windows.Forms.Button();
            textBoxPixSquaColor = new System.Windows.Forms.TextBox();
            textBoxPixSquaXY1 = new System.Windows.Forms.TextBox();
            radioButtonPixSquaNotEqu = new System.Windows.Forms.RadioButton();
            radioButtonPixSquaEqu = new System.Windows.Forms.RadioButton();
            panel3 = new System.Windows.Forms.Panel();
            radioButtonPixSquaXY = new System.Windows.Forms.RadioButton();
            radioButtonPixSquaSqua = new System.Windows.Forms.RadioButton();
            groupBoxPixSqua.SuspendLayout();
            groupBoxPixSquaVol.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(trackBarPixSqua)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(pictureBoxPixSquaSample)).BeginInit();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxPixSqua
            // 
            groupBoxPixSqua.BackColor = System.Drawing.Color.Cyan;
            groupBoxPixSqua.Controls.Add(groupBoxPixSquaVol);
            groupBoxPixSqua.Controls.Add(checkBoxPixSquaOverlay);
            groupBoxPixSqua.Controls.Add(labelPixSquaBool);
            groupBoxPixSqua.Controls.Add(textBoxPixSquaXY2);
            groupBoxPixSqua.Controls.Add(pictureBoxPixSquaSample);
            groupBoxPixSqua.Controls.Add(buttonPixSquaView);
            groupBoxPixSqua.Controls.Add(textBoxPixSquaColor);
            groupBoxPixSqua.Controls.Add(textBoxPixSquaXY1);
            groupBoxPixSqua.Controls.Add(radioButtonPixSquaNotEqu);
            groupBoxPixSqua.Controls.Add(radioButtonPixSquaEqu);
            groupBoxPixSqua.Controls.Add(panel3);
            groupBoxPixSqua.Location = new System.Drawing.Point(0, 0);
            groupBoxPixSqua.Name = "groupBoxPixSqua";
            groupBoxPixSqua.Size = new System.Drawing.Size(300, 107);
            groupBoxPixSqua.TabIndex = 48;
            groupBoxPixSqua.TabStop = false;
            groupBoxPixSqua.Text = "? IF Pixel OR square ?";
            // 
            // groupBoxPixSquaVol
            // 
            groupBoxPixSquaVol.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            groupBoxPixSquaVol.Controls.Add(trackBarPixSqua);
            groupBoxPixSquaVol.Controls.Add(textBoxPixSquaVolNumber);
            groupBoxPixSquaVol.Location = new System.Drawing.Point(10, 72);
            groupBoxPixSquaVol.Name = "groupBoxPixSquaVol";
            groupBoxPixSquaVol.Size = new System.Drawing.Size(281, 29);
            groupBoxPixSquaVol.TabIndex = 54;
            groupBoxPixSquaVol.TabStop = false;
            groupBoxPixSquaVol.Text = "%отклонения";
            // 
            // trackBarPixSqua
            // 
            trackBarPixSqua.AutoSize = false;
            trackBarPixSqua.Location = new System.Drawing.Point(11, 14);
            trackBarPixSqua.Maximum = 100;
            trackBarPixSqua.Minimum = 5;
            trackBarPixSqua.Name = "trackBarPixSqua";
            trackBarPixSqua.Size = new System.Drawing.Size(239, 18);
            trackBarPixSqua.TabIndex = 53;
            trackBarPixSqua.Value = 95;
            // 
            // textBoxPixSquaVolNumber
            // 
            textBoxPixSquaVolNumber.Enabled = false;
            textBoxPixSquaVolNumber.Location = new System.Drawing.Point(250, 4);
            textBoxPixSquaVolNumber.MaxLength = 3;
            textBoxPixSquaVolNumber.Name = "textBoxPixSquaVolNumber";
            textBoxPixSquaVolNumber.Size = new System.Drawing.Size(23, 23);
            textBoxPixSquaVolNumber.TabIndex = 20;
            textBoxPixSquaVolNumber.Text = "5";
            // 
            // checkBoxPixSquaOverlay
            // 
            checkBoxPixSquaOverlay.AutoSize = true;
            checkBoxPixSquaOverlay.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            checkBoxPixSquaOverlay.Location = new System.Drawing.Point(204, 51);
            checkBoxPixSquaOverlay.Name = "checkBoxPixSquaOverlay";
            checkBoxPixSquaOverlay.Size = new System.Drawing.Size(65, 18);
            checkBoxPixSquaOverlay.TabIndex = 53;
            checkBoxPixSquaOverlay.Text = "overlay";
            checkBoxPixSquaOverlay.UseVisualStyleBackColor = true;
            // 
            // labelPixSquaBool
            // 
            labelPixSquaBool.AutoSize = true;
            labelPixSquaBool.Font = new System.Drawing.Font("Calibri", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            labelPixSquaBool.Location = new System.Drawing.Point(255, 5);
            labelPixSquaBool.Name = "labelPixSquaBool";
            labelPixSquaBool.Size = new System.Drawing.Size(34, 15);
            labelPixSquaBool.TabIndex = 52;
            labelPixSquaBool.Text = "False";
            labelPixSquaBool.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // textBoxPixSquaXY2
            // 
            textBoxPixSquaXY2.Location = new System.Drawing.Point(114, 46);
            textBoxPixSquaXY2.MaxLength = 10;
            textBoxPixSquaXY2.Multiline = true;
            textBoxPixSquaXY2.Name = "textBoxPixSquaXY2";
            textBoxPixSquaXY2.Size = new System.Drawing.Size(70, 23);
            textBoxPixSquaXY2.TabIndex = 16;
            textBoxPixSquaXY2.Text = "200, 200";
            // 
            // pictureBoxPixSquaSample
            // 
            pictureBoxPixSquaSample.BackColor = System.Drawing.Color.White;
            pictureBoxPixSquaSample.Location = new System.Drawing.Point(245, 22);
            pictureBoxPixSquaSample.Name = "pictureBoxPixSquaSample";
            pictureBoxPixSquaSample.Size = new System.Drawing.Size(20, 23);
            pictureBoxPixSquaSample.TabIndex = 8;
            pictureBoxPixSquaSample.TabStop = false;
            // 
            // buttonPixSquaView
            // 
            buttonPixSquaView.Location = new System.Drawing.Point(268, 21);
            buttonPixSquaView.Name = "buttonPixSquaView";
            buttonPixSquaView.Size = new System.Drawing.Size(23, 24);
            buttonPixSquaView.TabIndex = 7;
            buttonPixSquaView.Text = "П";
            buttonPixSquaView.UseVisualStyleBackColor = true;
            // 
            // textBoxPixSquaColor
            // 
            textBoxPixSquaColor.Location = new System.Drawing.Point(190, 22);
            textBoxPixSquaColor.MaxLength = 8;
            textBoxPixSquaColor.Name = "textBoxPixSquaColor";
            textBoxPixSquaColor.Size = new System.Drawing.Size(54, 23);
            textBoxPixSquaColor.TabIndex = 5;
            textBoxPixSquaColor.Text = "#FFFFFF";
            // 
            // textBoxPixSquaXY1
            // 
            textBoxPixSquaXY1.Location = new System.Drawing.Point(114, 22);
            textBoxPixSquaXY1.MaxLength = 10;
            textBoxPixSquaXY1.Multiline = true;
            textBoxPixSquaXY1.Name = "textBoxPixSquaXY1";
            textBoxPixSquaXY1.Size = new System.Drawing.Size(70, 23);
            textBoxPixSquaXY1.TabIndex = 3;
            textBoxPixSquaXY1.Text = "100, 100";
            // 
            // radioButtonPixSquaNotEqu
            // 
            radioButtonPixSquaNotEqu.AutoSize = true;
            radioButtonPixSquaNotEqu.Location = new System.Drawing.Point(209, 3);
            radioButtonPixSquaNotEqu.Name = "radioButtonPixSquaNotEqu";
            radioButtonPixSquaNotEqu.Size = new System.Drawing.Size(35, 19);
            radioButtonPixSquaNotEqu.TabIndex = 1;
            radioButtonPixSquaNotEqu.Tag = "";
            radioButtonPixSquaNotEqu.Text = "!=";
            radioButtonPixSquaNotEqu.UseVisualStyleBackColor = true;
            // 
            // radioButtonPixSquaEqu
            // 
            radioButtonPixSquaEqu.AutoSize = true;
            radioButtonPixSquaEqu.Checked = true;
            radioButtonPixSquaEqu.Location = new System.Drawing.Point(177, 3);
            radioButtonPixSquaEqu.Name = "radioButtonPixSquaEqu";
            radioButtonPixSquaEqu.Size = new System.Drawing.Size(31, 19);
            radioButtonPixSquaEqu.TabIndex = 0;
            radioButtonPixSquaEqu.TabStop = true;
            radioButtonPixSquaEqu.Tag = "";
            radioButtonPixSquaEqu.Text = "=";
            radioButtonPixSquaEqu.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            panel3.BackColor = System.Drawing.Color.Transparent;
            panel3.Controls.Add(radioButtonPixSquaXY);
            panel3.Controls.Add(radioButtonPixSquaSqua);
            panel3.Location = new System.Drawing.Point(10, 22);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(106, 47);
            panel3.TabIndex = 14;
            // 
            // radioButtonPixSquaXY
            // 
            radioButtonPixSquaXY.AutoSize = true;
            radioButtonPixSquaXY.Checked = true;
            radioButtonPixSquaXY.Location = new System.Drawing.Point(37, 4);
            radioButtonPixSquaXY.Name = "radioButtonPixSquaXY";
            radioButtonPixSquaXY.Size = new System.Drawing.Size(38, 19);
            radioButtonPixSquaXY.TabIndex = 15;
            radioButtonPixSquaXY.TabStop = true;
            radioButtonPixSquaXY.Text = "XY";
            radioButtonPixSquaXY.UseVisualStyleBackColor = true;
            // 
            // radioButtonPixSquaSqua
            // 
            radioButtonPixSquaSqua.AutoSize = true;
            radioButtonPixSquaSqua.Location = new System.Drawing.Point(37, 24);
            radioButtonPixSquaSqua.Name = "radioButtonPixSquaSqua";
            radioButtonPixSquaSqua.Size = new System.Drawing.Size(63, 19);
            radioButtonPixSquaSqua.TabIndex = 14;
            radioButtonPixSquaSqua.Text = "Square";
            radioButtonPixSquaSqua.UseVisualStyleBackColor = true;
            // 
            // PixSquaControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Transparent;
            Controls.Add(groupBoxPixSqua);
            Name = "PixSquaControl";
            Size = new System.Drawing.Size(300, 107);
            groupBoxPixSqua.ResumeLayout(false);
            groupBoxPixSqua.PerformLayout();
            groupBoxPixSquaVol.ResumeLayout(false);
            groupBoxPixSquaVol.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(trackBarPixSqua)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(pictureBoxPixSquaSample)).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public System.Windows.Forms.GroupBox groupBoxPixSqua;
        public System.Windows.Forms.GroupBox groupBoxPixSquaVol;
        public System.Windows.Forms.TrackBar trackBarPixSqua;
        public System.Windows.Forms.TextBox textBoxPixSquaVolNumber;
        public System.Windows.Forms.CheckBox checkBoxPixSquaOverlay;
        public System.Windows.Forms.Label labelPixSquaBool;
        public System.Windows.Forms.TextBox textBoxPixSquaXY2;
        public System.Windows.Forms.PictureBox pictureBoxPixSquaSample;
        public System.Windows.Forms.Button buttonPixSquaView;
        public System.Windows.Forms.TextBox textBoxPixSquaColor;
        public System.Windows.Forms.TextBox textBoxPixSquaXY1;
        public System.Windows.Forms.RadioButton radioButtonPixSquaNotEqu;
        public System.Windows.Forms.RadioButton radioButtonPixSquaEqu;
        public System.Windows.Forms.Panel panel3;
        public System.Windows.Forms.RadioButton radioButtonPixSquaXY;
        public System.Windows.Forms.RadioButton radioButtonPixSquaSqua;
    }
}
