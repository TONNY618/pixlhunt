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
            SendKeybCMD = new Button();
            textBox1 = new TextBox();
            groupBox2 = new GroupBox();
            mooveCursor = new GroupBox();
            label10 = new Label();
            Speed = new TextBox();
            mooveCursorTextBox = new TextBox();
            mooveCursorlabel = new Label();
            SendMouseCMD = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            mooveCursor.SuspendLayout();
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
            // SendKeybCMD
            // 
            SendKeybCMD.Location = new Point(11, 63);
            SendKeybCMD.Name = "SendKeybCMD";
            SendKeybCMD.Size = new Size(215, 44);
            SendKeybCMD.TabIndex = 1;
            SendKeybCMD.Text = "Отправить команду";
            SendKeybCMD.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(11, 22);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 4;
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
            // SendMouseCMD
            // 
            SendMouseCMD.Location = new Point(16, 92);
            SendMouseCMD.Name = "SendMouseCMD";
            SendMouseCMD.Size = new Size(215, 44);
            SendMouseCMD.TabIndex = 13;
            SendMouseCMD.Text = "Отправить команду";
            SendMouseCMD.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(625, 570);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(button1);
            Controls.Add(checkBox1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            mooveCursor.ResumeLayout(false);
            mooveCursor.PerformLayout();
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
    }
}
