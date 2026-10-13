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
            groupBoxPixSqua = new GroupBox();
            groupBoxPixSquaVol = new GroupBox();
            trackBarPixSqua = new TrackBar();
            textBoxPixSquaVolNumber = new TextBox();
            checkBoxPixSquaOverlay = new CheckBox();
            labelPixSquaBool = new Label();
            textBoxPixSquaXY2 = new TextBox();
            pictureBoxPixSquaSample = new PictureBox();
            buttonPixSquaView = new Button();
            textBoxPixSquaColor = new TextBox();
            textBoxPixSquaXY1 = new TextBox();
            radioButtonPixSquaNotEqu = new RadioButton();
            radioButtonPixSquaEqu = new RadioButton();
            panel3 = new Panel();
            radioButtonPixSquaXY = new RadioButton();
            radioButtonPixSquaSqua = new RadioButton();
            groupBoxPixSqua.SuspendLayout();
            groupBoxPixSquaVol.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarPixSqua).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPixSquaSample).BeginInit();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxPixSqua
            // 
            groupBoxPixSqua.BackColor = Color.Cyan;
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
            groupBoxPixSqua.Location = new Point(0, 0);
            groupBoxPixSqua.Name = "groupBoxPixSqua";
            groupBoxPixSqua.Size = new Size(300, 107);
            groupBoxPixSqua.TabIndex = 48;
            groupBoxPixSqua.TabStop = false;
            groupBoxPixSqua.Text = "? IF Pixel OR square ?";
            // 
            // groupBoxPixSquaVol
            // 
            groupBoxPixSquaVol.BackColor = Color.FromArgb(224, 224, 224);
            groupBoxPixSquaVol.Controls.Add(trackBarPixSqua);
            groupBoxPixSquaVol.Controls.Add(textBoxPixSquaVolNumber);
            groupBoxPixSquaVol.Location = new Point(10, 72);
            groupBoxPixSquaVol.Name = "groupBoxPixSquaVol";
            groupBoxPixSquaVol.Size = new Size(281, 29);
            groupBoxPixSquaVol.TabIndex = 54;
            groupBoxPixSquaVol.TabStop = false;
            groupBoxPixSquaVol.Text = "% соответствия";
            // 
            // trackBarPixSqua
            // 
            trackBarPixSqua.AutoSize = false;
            trackBarPixSqua.Location = new Point(11, 14);
            trackBarPixSqua.Minimum = 5;
            trackBarPixSqua.Maximum = 100;
            trackBarPixSqua.Name = "trackBarPixSqua";
            trackBarPixSqua.Size = new Size(239, 18);
            trackBarPixSqua.TabIndex = 53;
            trackBarPixSqua.Value = 95;
            // 
            // textBoxPixSquaVolNumber
            // 
            textBoxPixSquaVolNumber.Enabled = false;
            textBoxPixSquaVolNumber.Location = new Point(250, 4);
            textBoxPixSquaVolNumber.MaxLength = 3;
            textBoxPixSquaVolNumber.Name = "textBoxPixSquaVolNumber";
            textBoxPixSquaVolNumber.Size = new Size(23, 23);
            textBoxPixSquaVolNumber.TabIndex = 20;
            textBoxPixSquaVolNumber.Text = "95";
            // 
            // checkBoxPixSquaOverlay
            // 
            checkBoxPixSquaOverlay.AutoSize = true;
            checkBoxPixSquaOverlay.Font = new Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            checkBoxPixSquaOverlay.Location = new Point(204, 51);
            checkBoxPixSquaOverlay.Name = "checkBoxPixSquaOverlay";
            checkBoxPixSquaOverlay.Size = new Size(65, 18);
            checkBoxPixSquaOverlay.TabIndex = 53;
            checkBoxPixSquaOverlay.Text = "overlay";
            checkBoxPixSquaOverlay.UseVisualStyleBackColor = true;
            // 
            // labelPixSquaBool
            // 
            labelPixSquaBool.AutoSize = true;
            labelPixSquaBool.Font = new Font("Calibri", 9.75F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 204);
            labelPixSquaBool.Location = new Point(255, 5);
            labelPixSquaBool.Name = "labelPixSquaBool";
            labelPixSquaBool.Size = new Size(34, 15);
            labelPixSquaBool.TabIndex = 52;
            labelPixSquaBool.Text = "False";
            labelPixSquaBool.TextAlign = ContentAlignment.TopRight;
            // 
            // textBoxPixSquaXY2
            // 
            textBoxPixSquaXY2.Location = new Point(114, 46);
            textBoxPixSquaXY2.MaxLength = 10;
            textBoxPixSquaXY2.Multiline = true;
            textBoxPixSquaXY2.Name = "textBoxPixSquaXY2";
            textBoxPixSquaXY2.Size = new Size(70, 23);
            textBoxPixSquaXY2.TabIndex = 16;
            textBoxPixSquaXY2.Text = "200, 200";
            // 
            // pictureBoxPixSquaSample
            // 
            pictureBoxPixSquaSample.BackColor = Color.White;
            pictureBoxPixSquaSample.Location = new Point(245, 22);
            pictureBoxPixSquaSample.Name = "pictureBoxPixSquaSample";
            pictureBoxPixSquaSample.Size = new Size(20, 23);
            pictureBoxPixSquaSample.TabIndex = 8;
            pictureBoxPixSquaSample.TabStop = false;
            // 
            // buttonPixSquaView
            // 
            buttonPixSquaView.Location = new Point(268, 21);
            buttonPixSquaView.Name = "buttonPixSquaView";
            buttonPixSquaView.Size = new Size(23, 24);
            buttonPixSquaView.TabIndex = 7;
            buttonPixSquaView.Text = "П";
            buttonPixSquaView.UseVisualStyleBackColor = true;
            // 
            // textBoxPixSquaColor
            // 
            textBoxPixSquaColor.Location = new Point(190, 22);
            textBoxPixSquaColor.MaxLength = 8;
            textBoxPixSquaColor.Name = "textBoxPixSquaColor";
            textBoxPixSquaColor.Size = new Size(54, 23);
            textBoxPixSquaColor.TabIndex = 5;
            textBoxPixSquaColor.Text = "#FFFFFF";
            // 
            // textBoxPixSquaXY1
            // 
            textBoxPixSquaXY1.Location = new Point(114, 22);
            textBoxPixSquaXY1.MaxLength = 10;
            textBoxPixSquaXY1.Multiline = true;
            textBoxPixSquaXY1.Name = "textBoxPixSquaXY1";
            textBoxPixSquaXY1.Size = new Size(70, 23);
            textBoxPixSquaXY1.TabIndex = 3;
            textBoxPixSquaXY1.Text = "100, 100";
            // 
            // radioButtonPixSquaNotEqu
            // 
            radioButtonPixSquaNotEqu.AutoSize = true;
            radioButtonPixSquaNotEqu.Location = new Point(209, 3);
            radioButtonPixSquaNotEqu.Name = "radioButtonPixSquaNotEqu";
            radioButtonPixSquaNotEqu.Size = new Size(36, 19);
            radioButtonPixSquaNotEqu.TabIndex = 1;
            radioButtonPixSquaNotEqu.Tag = "";
            radioButtonPixSquaNotEqu.Text = "!=";
            radioButtonPixSquaNotEqu.UseVisualStyleBackColor = true;
            // 
            // radioButtonPixSquaEqu
            // 
            radioButtonPixSquaEqu.AutoSize = true;
            radioButtonPixSquaEqu.Checked = true;
            radioButtonPixSquaEqu.Location = new Point(177, 3);
            radioButtonPixSquaEqu.Name = "radioButtonPixSquaEqu";
            radioButtonPixSquaEqu.Size = new Size(33, 19);
            radioButtonPixSquaEqu.TabIndex = 0;
            radioButtonPixSquaEqu.TabStop = true;
            radioButtonPixSquaEqu.Tag = "";
            radioButtonPixSquaEqu.Text = "=";
            radioButtonPixSquaEqu.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Transparent;
            panel3.Controls.Add(radioButtonPixSquaXY);
            panel3.Controls.Add(radioButtonPixSquaSqua);
            panel3.Location = new Point(10, 22);
            panel3.Name = "panel3";
            panel3.Size = new Size(106, 47);
            panel3.TabIndex = 14;
            // 
            // radioButtonPixSquaXY
            // 
            radioButtonPixSquaXY.AutoSize = true;
            radioButtonPixSquaXY.Checked = true;
            radioButtonPixSquaXY.Location = new Point(37, 4);
            radioButtonPixSquaXY.Name = "radioButtonPixSquaXY";
            radioButtonPixSquaXY.Size = new Size(39, 19);
            radioButtonPixSquaXY.TabIndex = 15;
            radioButtonPixSquaXY.TabStop = true;
            radioButtonPixSquaXY.Text = "XY";
            radioButtonPixSquaXY.UseVisualStyleBackColor = true;
            // 
            // radioButtonPixSquaSqua
            // 
            radioButtonPixSquaSqua.AutoSize = true;
            radioButtonPixSquaSqua.Location = new Point(37, 24);
            radioButtonPixSquaSqua.Name = "radioButtonPixSquaSqua";
            radioButtonPixSquaSqua.Size = new Size(61, 19);
            radioButtonPixSquaSqua.TabIndex = 14;
            radioButtonPixSquaSqua.Text = "Square";
            radioButtonPixSquaSqua.UseVisualStyleBackColor = true;
            // 
            // PixSquaControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(groupBoxPixSqua);
            Name = "PixSquaControl";
            Size = new Size(300, 107);
            groupBoxPixSqua.ResumeLayout(false);
            groupBoxPixSqua.PerformLayout();
            groupBoxPixSquaVol.ResumeLayout(false);
            groupBoxPixSquaVol.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarPixSqua).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPixSquaSample).EndInit();
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
