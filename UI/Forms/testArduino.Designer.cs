namespace pxlhunt
{
    partial class testArduino
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            checkBox1 = new CheckBox();
            button1 = new Button();
            groupBox1 = new GroupBox();
            textBox1 = new TextBox();
            SendKeybCMD = new Button();
            groupBox2 = new GroupBox();
            SendMouseCMD = new Button();
            mooveCursor = new GroupBox();
            label10 = new Label();
            Speed = new TextBox();
            mooveCursorTextBox = new TextBox();
            mooveCursorlabel = new Label();
            groupBoxIMAGE = new GroupBox();
            checkBoxTakeXY = new CheckBox();
            labelIMAGE2 = new Label();
            labelIMAGE1 = new Label();
            checkBoxIMAGEOverlay = new CheckBox();
            buttonIMAGEsettings = new Button();
            textBoxIMAGEXY2 = new TextBox();
            textBoxIMAGEXY1 = new TextBox();
            pictureBoxIMAGESample = new PictureBox();
            buttonIMAGEView = new Button();
            labelIMAGEBool = new Label();
            groupBoxIMAGEVol = new GroupBox();
            trackBarIMAGE = new TrackBar();
            textBoxIMAGEVolNumber = new TextBox();
            pictureBoxIMAGEExample = new PictureBox();
            radioButtonIMAGENotEqu = new RadioButton();
            radioButtonIMAGEEqu = new RadioButton();
            checkBox2 = new CheckBox();
            radioButton1 = new RadioButton();
            radioButton2 = new RadioButton();
            groupBox3 = new GroupBox();
            trackBar1 = new TrackBar();
            textBox2 = new TextBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            mooveCursor.SuspendLayout();
            groupBoxIMAGE.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxIMAGESample).BeginInit();
            groupBoxIMAGEVol.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarIMAGE).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxIMAGEExample).BeginInit();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            SuspendLayout();
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(44, 45);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(83, 19);
            checkBox1.TabIndex = 0;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(44, 81);
            button1.Name = "button1";
            button1.Size = new Size(192, 23);
            button1.TabIndex = 1;
            button1.Text = "Команда на перепрошивку";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(SendKeybCMD);
            groupBox1.Location = new Point(30, 159);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(255, 170);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Клавиатура";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(11, 22);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 4;
            // 
            // SendKeybCMD
            // 
            SendKeybCMD.Location = new Point(11, 63);
            SendKeybCMD.Name = "SendKeybCMD";
            SendKeybCMD.Size = new Size(215, 44);
            SendKeybCMD.TabIndex = 1;
            SendKeybCMD.Text = "Отправить команду";
            SendKeybCMD.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(SendMouseCMD);
            groupBox2.Controls.Add(mooveCursor);
            groupBox2.Location = new Point(321, 159);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(267, 170);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Мышка";
            // 
            // SendMouseCMD
            // 
            SendMouseCMD.Location = new Point(16, 92);
            SendMouseCMD.Name = "SendMouseCMD";
            SendMouseCMD.Size = new Size(215, 44);
            SendMouseCMD.TabIndex = 13;
            SendMouseCMD.Text = "Отправить команду";
            SendMouseCMD.UseVisualStyleBackColor = true;
            // 
            // mooveCursor
            // 
            mooveCursor.BackColor = Color.Gainsboro;
            mooveCursor.Controls.Add(label10);
            mooveCursor.Controls.Add(Speed);
            mooveCursor.Controls.Add(mooveCursorTextBox);
            mooveCursor.Controls.Add(mooveCursorlabel);
            mooveCursor.Location = new Point(6, 22);
            mooveCursor.Name = "mooveCursor";
            mooveCursor.Size = new Size(238, 43);
            mooveCursor.TabIndex = 12;
            mooveCursor.TabStop = false;
            mooveCursor.Text = "Курсор на коорд двиг";
            // 
            // label10
            // 
            label10.Location = new Point(161, 18);
            label10.Name = "label10";
            label10.Size = new Size(42, 18);
            label10.TabIndex = 9;
            label10.Text = "speed";
            // 
            // Speed
            // 
            Speed.Location = new Point(204, 16);
            Speed.MaxLength = 3;
            Speed.Name = "Speed";
            Speed.Size = new Size(28, 23);
            Speed.TabIndex = 8;
            Speed.Text = "50";
            // 
            // mooveCursorTextBox
            // 
            mooveCursorTextBox.Location = new Point(38, 15);
            mooveCursorTextBox.MaxLength = 10;
            mooveCursorTextBox.Multiline = true;
            mooveCursorTextBox.Name = "mooveCursorTextBox";
            mooveCursorTextBox.Size = new Size(70, 23);
            mooveCursorTextBox.TabIndex = 3;
            mooveCursorTextBox.Text = "1234, 1234";
            // 
            // mooveCursorlabel
            // 
            mooveCursorlabel.Location = new Point(10, 18);
            mooveCursorlabel.Name = "mooveCursorlabel";
            mooveCursorlabel.Size = new Size(25, 18);
            mooveCursorlabel.TabIndex = 2;
            mooveCursorlabel.Text = "x, y";
            // 
            // groupBoxIMAGE
            // 
            groupBoxIMAGE.BackColor = Color.Cyan;
            groupBoxIMAGE.Controls.Add(labelIMAGE2);
            groupBoxIMAGE.Controls.Add(labelIMAGE1);
            groupBoxIMAGE.Controls.Add(textBoxIMAGEXY2);
            groupBoxIMAGE.Controls.Add(textBoxIMAGEXY1);
            groupBoxIMAGE.Controls.Add(labelIMAGEBool);
            groupBoxIMAGE.Controls.Add(pictureBoxIMAGEExample);
            groupBoxIMAGE.Controls.Add(radioButtonIMAGENotEqu);
            groupBoxIMAGE.Controls.Add(radioButtonIMAGEEqu);
            groupBoxIMAGE.Location = new Point(30, 352);
            groupBoxIMAGE.Name = "groupBoxIMAGE";
            groupBoxIMAGE.Size = new Size(300, 122);
            groupBoxIMAGE.TabIndex = 50;
            groupBoxIMAGE.TabStop = false;
            groupBoxIMAGE.Text = "IF IMAGE";
            // 
            // checkBoxTakeXY
            // 
            checkBoxTakeXY.AutoSize = true;
            checkBoxTakeXY.Location = new Point(126, 533);
            checkBoxTakeXY.Name = "checkBoxTakeXY";
            checkBoxTakeXY.Size = new Size(15, 14);
            checkBoxTakeXY.TabIndex = 66;
            checkBoxTakeXY.UseVisualStyleBackColor = true;
            // 
            // labelIMAGE2
            // 
            labelIMAGE2.AutoSize = true;
            labelIMAGE2.Location = new Point(165, 17);
            labelIMAGE2.Name = "labelIMAGE2";
            labelIMAGE2.Size = new Size(64, 15);
            labelIMAGE2.TabIndex = 65;
            labelIMAGE2.Text = "Где искать";
            // 
            // labelIMAGE1
            // 
            labelIMAGE1.AutoSize = true;
            labelIMAGE1.Location = new Point(11, 17);
            labelIMAGE1.Name = "labelIMAGE1";
            labelIMAGE1.Size = new Size(66, 15);
            labelIMAGE1.TabIndex = 64;
            labelIMAGE1.Text = "Что искать";
            // 
            // checkBoxIMAGEOverlay
            // 
            checkBoxIMAGEOverlay.AutoSize = true;
            checkBoxIMAGEOverlay.Font = new Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            checkBoxIMAGEOverlay.Location = new Point(312, 540);
            checkBoxIMAGEOverlay.Name = "checkBoxIMAGEOverlay";
            checkBoxIMAGEOverlay.Size = new Size(65, 18);
            checkBoxIMAGEOverlay.TabIndex = 63;
            checkBoxIMAGEOverlay.Text = "overlay";
            checkBoxIMAGEOverlay.UseVisualStyleBackColor = true;
            // 
            // buttonIMAGEsettings
            // 
            buttonIMAGEsettings.Location = new Point(41, 500);
            buttonIMAGEsettings.Name = "buttonIMAGEsettings";
            buttonIMAGEsettings.Size = new Size(76, 47);
            buttonIMAGEsettings.TabIndex = 62;
            buttonIMAGEsettings.Text = "Тонкие настройки";
            buttonIMAGEsettings.UseVisualStyleBackColor = true;
            // 
            // textBoxIMAGEXY2
            // 
            textBoxIMAGEXY2.Enabled = false;
            textBoxIMAGEXY2.Location = new Point(165, 59);
            textBoxIMAGEXY2.MaxLength = 10;
            textBoxIMAGEXY2.Multiline = true;
            textBoxIMAGEXY2.Name = "textBoxIMAGEXY2";
            textBoxIMAGEXY2.Size = new Size(70, 23);
            textBoxIMAGEXY2.TabIndex = 60;
            textBoxIMAGEXY2.Text = "200, 200";
            // 
            // textBoxIMAGEXY1
            // 
            textBoxIMAGEXY1.Location = new Point(165, 35);
            textBoxIMAGEXY1.MaxLength = 10;
            textBoxIMAGEXY1.Multiline = true;
            textBoxIMAGEXY1.Name = "textBoxIMAGEXY1";
            textBoxIMAGEXY1.Size = new Size(70, 23);
            textBoxIMAGEXY1.TabIndex = 59;
            textBoxIMAGEXY1.Text = "100, 100";
            // 
            // pictureBoxIMAGESample
            // 
            pictureBoxIMAGESample.BackColor = Color.White;
            pictureBoxIMAGESample.Location = new Point(195, 524);
            pictureBoxIMAGESample.Name = "pictureBoxIMAGESample";
            pictureBoxIMAGESample.Size = new Size(20, 23);
            pictureBoxIMAGESample.TabIndex = 58;
            pictureBoxIMAGESample.TabStop = false;
            // 
            // buttonIMAGEView
            // 
            buttonIMAGEView.Location = new Point(258, 515);
            buttonIMAGEView.Name = "buttonIMAGEView";
            buttonIMAGEView.Size = new Size(23, 48);
            buttonIMAGEView.TabIndex = 57;
            buttonIMAGEView.Text = "Показать";
            buttonIMAGEView.UseVisualStyleBackColor = true;
            // 
            // labelIMAGEBool
            // 
            labelIMAGEBool.AutoSize = true;
            labelIMAGEBool.Font = new Font("Calibri", 9.75F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 204);
            labelIMAGEBool.Location = new Point(256, 3);
            labelIMAGEBool.Name = "labelIMAGEBool";
            labelIMAGEBool.Size = new Size(34, 15);
            labelIMAGEBool.TabIndex = 56;
            labelIMAGEBool.Text = "False";
            labelIMAGEBool.TextAlign = ContentAlignment.TopRight;
            // 
            // groupBoxIMAGEVol
            // 
            groupBoxIMAGEVol.BackColor = Color.FromArgb(224, 224, 224);
            groupBoxIMAGEVol.Controls.Add(trackBarIMAGE);
            groupBoxIMAGEVol.Controls.Add(textBoxIMAGEVolNumber);
            groupBoxIMAGEVol.Location = new Point(30, 272);
            groupBoxIMAGEVol.Name = "groupBoxIMAGEVol";
            groupBoxIMAGEVol.Size = new Size(281, 29);
            groupBoxIMAGEVol.TabIndex = 55;
            groupBoxIMAGEVol.TabStop = false;
            groupBoxIMAGEVol.Text = "%отклонения";
            // 
            // trackBarIMAGE
            // 
            trackBarIMAGE.AutoSize = false;
            trackBarIMAGE.Location = new Point(11, 14);
            trackBarIMAGE.Maximum = 100;
            trackBarIMAGE.Minimum = 5;
            trackBarIMAGE.Name = "trackBarIMAGE";
            trackBarIMAGE.Size = new Size(239, 18);
            trackBarIMAGE.TabIndex = 53;
            trackBarIMAGE.Value = 95;
            // 
            // textBoxIMAGEVolNumber
            // 
            textBoxIMAGEVolNumber.Enabled = false;
            textBoxIMAGEVolNumber.Location = new Point(250, 4);
            textBoxIMAGEVolNumber.MaxLength = 3;
            textBoxIMAGEVolNumber.Name = "textBoxIMAGEVolNumber";
            textBoxIMAGEVolNumber.Size = new Size(23, 23);
            textBoxIMAGEVolNumber.TabIndex = 20;
            textBoxIMAGEVolNumber.Text = "0";
            // 
            // pictureBoxIMAGEExample
            // 
            pictureBoxIMAGEExample.BackColor = Color.White;
            pictureBoxIMAGEExample.Location = new Point(10, 35);
            pictureBoxIMAGEExample.Name = "pictureBoxIMAGEExample";
            pictureBoxIMAGEExample.Size = new Size(67, 47);
            pictureBoxIMAGEExample.TabIndex = 8;
            pictureBoxIMAGEExample.TabStop = false;
            // 
            // radioButtonIMAGENotEqu
            // 
            radioButtonIMAGENotEqu.AutoSize = true;
            radioButtonIMAGENotEqu.Location = new Point(215, 1);
            radioButtonIMAGENotEqu.Name = "radioButtonIMAGENotEqu";
            radioButtonIMAGENotEqu.Size = new Size(36, 19);
            radioButtonIMAGENotEqu.TabIndex = 1;
            radioButtonIMAGENotEqu.Tag = "";
            radioButtonIMAGENotEqu.Text = "!=";
            radioButtonIMAGENotEqu.UseVisualStyleBackColor = true;
            // 
            // radioButtonIMAGEEqu
            // 
            radioButtonIMAGEEqu.AutoSize = true;
            radioButtonIMAGEEqu.Checked = true;
            radioButtonIMAGEEqu.Location = new Point(178, 1);
            radioButtonIMAGEEqu.Name = "radioButtonIMAGEEqu";
            radioButtonIMAGEEqu.Size = new Size(33, 19);
            radioButtonIMAGEEqu.TabIndex = 0;
            radioButtonIMAGEEqu.TabStop = true;
            radioButtonIMAGEEqu.Tag = "";
            radioButtonIMAGEEqu.Text = "=";
            radioButtonIMAGEEqu.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Font = new Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            checkBox2.Location = new Point(351, 403);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(107, 18);
            checkBox2.TabIndex = 64;
            checkBox2.Text = "Ближе/Дальше";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(351, 379);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(98, 19);
            radioButton1.TabIndex = 66;
            radioButton1.Tag = "";
            radioButton1.Text = "Блики/Туман";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Checked = true;
            radioButton2.Location = new Point(351, 353);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(59, 19);
            radioButton2.TabIndex = 65;
            radioButton2.TabStop = true;
            radioButton2.Tag = "";
            radioButton2.Text = "Точно";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.BackColor = Color.FromArgb(224, 224, 224);
            groupBox3.Controls.Add(trackBar1);
            groupBox3.Controls.Add(textBox2);
            groupBox3.Location = new Point(352, 427);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(181, 51);
            groupBox3.TabIndex = 67;
            groupBox3.TabStop = false;
            groupBox3.Text = "%отклонения";
            // 
            // trackBar1
            // 
            trackBar1.AutoSize = false;
            trackBar1.Location = new Point(6, 22);
            trackBar1.Maximum = 100;
            trackBar1.Minimum = 5;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(137, 18);
            trackBar1.TabIndex = 53;
            trackBar1.Value = 95;
            // 
            // textBox2
            // 
            textBox2.Enabled = false;
            textBox2.Location = new Point(146, 22);
            textBox2.MaxLength = 3;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(23, 23);
            textBox2.TabIndex = 20;
            textBox2.Text = "0";
            // 
            // testArduino
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(625, 570);
            Controls.Add(checkBoxTakeXY);
            Controls.Add(groupBox3);
            Controls.Add(checkBoxIMAGEOverlay);
            Controls.Add(radioButton1);
            Controls.Add(radioButton2);
            Controls.Add(groupBoxIMAGEVol);
            Controls.Add(checkBox2);
            Controls.Add(buttonIMAGEView);
            Controls.Add(pictureBoxIMAGESample);
            Controls.Add(buttonIMAGEsettings);
            Controls.Add(groupBoxIMAGE);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(button1);
            Controls.Add(checkBox1);
            Name = "testArduino";
            Text = "Form1";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            mooveCursor.ResumeLayout(false);
            mooveCursor.PerformLayout();
            groupBoxIMAGE.ResumeLayout(false);
            groupBoxIMAGE.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxIMAGESample).EndInit();
            groupBoxIMAGEVol.ResumeLayout(false);
            groupBoxIMAGEVol.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarIMAGE).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxIMAGEExample).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox checkBox1;
        private Button button1;
        private GroupBox groupBox1;
        private TextBox textBox1;
        private Button SendKeybCMD;
        private GroupBox groupBox2;
        private Button SendMouseCMD;
        private GroupBox mooveCursor;
        private Label label10;
        private TextBox Speed;
        private TextBox mooveCursorTextBox;
        private Label mooveCursorlabel;
        private GroupBox groupBoxIMAGE;
        private CheckBox checkBoxTakeXY;
        private Label labelIMAGE2;
        private Label labelIMAGE1;
        private CheckBox checkBoxIMAGEOverlay;
        private Button buttonIMAGEsettings;
        private TextBox textBoxIMAGEXY2;
        private TextBox textBoxIMAGEXY1;
        private PictureBox pictureBoxIMAGESample;
        private Button buttonIMAGEView;
        private Label labelIMAGEBool;
        private GroupBox groupBoxIMAGEVol;
        private TrackBar trackBarIMAGE;
        private TextBox textBoxIMAGEVolNumber;
        private PictureBox pictureBoxIMAGEExample;
        private RadioButton radioButtonIMAGENotEqu;
        private RadioButton radioButtonIMAGEEqu;
        private CheckBox checkBox2;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private GroupBox groupBox3;
        private TrackBar trackBar1;
        private TextBox textBox2;
    }
}
