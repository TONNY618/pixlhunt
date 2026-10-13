namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    partial class CursorMoveControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            groupBoxCurMoove = new System.Windows.Forms.GroupBox();
            labelCurMoove3 = new System.Windows.Forms.Label();
            textBoxCurMoove2 = new System.Windows.Forms.TextBox();
            radioButtonCurMooveGetXY = new System.Windows.Forms.RadioButton();
            pictureBoxCurMooveSample = new System.Windows.Forms.PictureBox();
            buttonCurMooveView = new System.Windows.Forms.Button();
            textBoxCurMoove1 = new System.Windows.Forms.TextBox();
            labelCurMoove2 = new System.Windows.Forms.Label();
            radioButtonCurMooveXY = new System.Windows.Forms.RadioButton();
            radioButtonCurMooveVol = new System.Windows.Forms.RadioButton();
            checkBoxCurMooveOverlay = new System.Windows.Forms.CheckBox();
            groupBoxCurMoove.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCurMooveSample).BeginInit();
            SuspendLayout();

            groupBoxCurMoove.BackColor = System.Drawing.Color.Gainsboro;
            groupBoxCurMoove.Controls.Add(labelCurMoove3);
            groupBoxCurMoove.Controls.Add(textBoxCurMoove2);
            groupBoxCurMoove.Controls.Add(radioButtonCurMooveGetXY);
            groupBoxCurMoove.Controls.Add(pictureBoxCurMooveSample);
            groupBoxCurMoove.Controls.Add(buttonCurMooveView);
            groupBoxCurMoove.Controls.Add(textBoxCurMoove1);
            groupBoxCurMoove.Controls.Add(labelCurMoove2);
            groupBoxCurMoove.Controls.Add(radioButtonCurMooveXY);
            groupBoxCurMoove.Controls.Add(radioButtonCurMooveVol);
            groupBoxCurMoove.Controls.Add(checkBoxCurMooveOverlay);
            groupBoxCurMoove.ForeColor = System.Drawing.SystemColors.HotTrack;
            groupBoxCurMoove.Location = new System.Drawing.Point(0, 0);
            groupBoxCurMoove.Name = "groupBoxCurMoove";
            groupBoxCurMoove.Size = new System.Drawing.Size(300, 74);
            groupBoxCurMoove.TabIndex = 48;
            groupBoxCurMoove.TabStop = false;
            groupBoxCurMoove.Text = "-> Курсор на коорд двиг ->";

            labelCurMoove3.Location = new System.Drawing.Point(145, 48);
            labelCurMoove3.Name = "labelCurMoove3";
            labelCurMoove3.Size = new System.Drawing.Size(25, 18);
            labelCurMoove3.Text = "x, y";

            textBoxCurMoove2.Location = new System.Drawing.Point(69, 45);
            textBoxCurMoove2.Name = "textBoxCurMoove2";
            textBoxCurMoove2.Size = new System.Drawing.Size(70, 23);
            textBoxCurMoove2.Text = "200, 200";

            radioButtonCurMooveGetXY.AutoSize = true;
            radioButtonCurMooveGetXY.Location = new System.Drawing.Point(176, 4);
            radioButtonCurMooveGetXY.Name = "radioButtonCurMooveGetXY";
            radioButtonCurMooveGetXY.Size = new System.Drawing.Size(116, 19);
            radioButtonCurMooveGetXY.Text = "XY взять выше /\\";

            pictureBoxCurMooveSample.BackColor = System.Drawing.Color.White;
            pictureBoxCurMooveSample.Location = new System.Drawing.Point(41, 18);
            pictureBoxCurMooveSample.Name = "pictureBoxCurMooveSample";
            pictureBoxCurMooveSample.Size = new System.Drawing.Size(18, 23);

            buttonCurMooveView.Location = new System.Drawing.Point(6, 18);
            buttonCurMooveView.Name = "buttonCurMooveView";
            buttonCurMooveView.Size = new System.Drawing.Size(29, 23);
            buttonCurMooveView.Text = "П";

            textBoxCurMoove1.Location = new System.Drawing.Point(69, 19);
            textBoxCurMoove1.Name = "textBoxCurMoove1";
            textBoxCurMoove1.Size = new System.Drawing.Size(70, 23);
            textBoxCurMoove1.Text = "100, 100";

            labelCurMoove2.Location = new System.Drawing.Point(145, 22);
            labelCurMoove2.Name = "labelCurMoove2";
            labelCurMoove2.Size = new System.Drawing.Size(25, 18);
            labelCurMoove2.Text = "x, y";

            radioButtonCurMooveXY.AutoSize = true;
            radioButtonCurMooveXY.Location = new System.Drawing.Point(176, 48);
            radioButtonCurMooveXY.Name = "radioButtonCurMooveXY";
            radioButtonCurMooveXY.Size = new System.Drawing.Size(124, 19);
            radioButtonCurMooveXY.Text = "Позиция XYне рек";

            radioButtonCurMooveVol.AutoSize = true;
            radioButtonCurMooveVol.Checked = true;
            radioButtonCurMooveVol.Location = new System.Drawing.Point(176, 27);
            radioButtonCurMooveVol.Name = "radioButtonCurMooveVol";
            radioButtonCurMooveVol.Size = new System.Drawing.Size(124, 19);
            radioButtonCurMooveVol.Text = "Регион примерно";

            checkBoxCurMooveOverlay.AutoSize = true;
            checkBoxCurMooveOverlay.Location = new System.Drawing.Point(8, 48);
            checkBoxCurMooveOverlay.Name = "checkBoxCurMooveOverlay";
            checkBoxCurMooveOverlay.Size = new System.Drawing.Size(65, 18);
            checkBoxCurMooveOverlay.Text = "overlay";

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Transparent;
            Controls.Add(groupBoxCurMoove);
            Name = "CursorMoveControl";
            Size = new System.Drawing.Size(300, 74);
            groupBoxCurMoove.ResumeLayout(false);
            groupBoxCurMoove.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCurMooveSample).EndInit();
            ResumeLayout(false);
        }

        public System.Windows.Forms.GroupBox groupBoxCurMoove;
        public System.Windows.Forms.Label labelCurMoove3;
        public System.Windows.Forms.TextBox textBoxCurMoove2;
        public System.Windows.Forms.RadioButton radioButtonCurMooveGetXY;
        public System.Windows.Forms.PictureBox pictureBoxCurMooveSample;
        public System.Windows.Forms.Button buttonCurMooveView;
        public System.Windows.Forms.TextBox textBoxCurMoove1;
        public System.Windows.Forms.Label labelCurMoove2;
        public System.Windows.Forms.RadioButton radioButtonCurMooveXY;
        public System.Windows.Forms.RadioButton radioButtonCurMooveVol;
        public System.Windows.Forms.CheckBox checkBoxCurMooveOverlay;
    }
}
