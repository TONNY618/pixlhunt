namespace pxlhunt.FORMS
{
    partial class pxlHunt
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
            groupBoxActiveTime = new GroupBox();
            labelActiveTime = new Label();
            groupBoxStatus = new GroupBox();
            checkBoxStatus = new CheckBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            checkBox1 = new CheckBox();
            labelEvents = new Label();
            buttonSettings = new Button();
            buttonComboForm = new Button();
            groupBoxActiveTime.SuspendLayout();
            groupBoxStatus.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxActiveTime
            // 
            groupBoxActiveTime.Controls.Add(labelActiveTime);
            groupBoxActiveTime.Location = new Point(12, 41);
            groupBoxActiveTime.Name = "groupBoxActiveTime";
            groupBoxActiveTime.Size = new Size(163, 23);
            groupBoxActiveTime.TabIndex = 2;
            groupBoxActiveTime.TabStop = false;
            groupBoxActiveTime.Text = "Время ACTIVE";
            // 
            // labelActiveTime
            // 
            labelActiveTime.AutoSize = true;
            labelActiveTime.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            labelActiveTime.Location = new Point(97, 0);
            labelActiveTime.Name = "labelActiveTime";
            labelActiveTime.Size = new Size(63, 20);
            labelActiveTime.TabIndex = 1;
            labelActiveTime.Text = "00:00:00";
            // 
            // groupBoxStatus
            // 
            groupBoxStatus.BackColor = Color.White;
            groupBoxStatus.Controls.Add(checkBoxStatus);
            groupBoxStatus.Location = new Point(12, 12);
            groupBoxStatus.Name = "groupBoxStatus";
            groupBoxStatus.Size = new Size(163, 23);
            groupBoxStatus.TabIndex = 3;
            groupBoxStatus.TabStop = false;
            groupBoxStatus.Text = "Статус - пауза";
            // 
            // checkBoxStatus
            // 
            checkBoxStatus.AutoSize = true;
            checkBoxStatus.Location = new Point(142, 3);
            checkBoxStatus.Name = "checkBoxStatus";
            checkBoxStatus.Size = new Size(15, 14);
            checkBoxStatus.TabIndex = 0;
            checkBoxStatus.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(checkBox1);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(12, 85);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(163, 353);
            flowLayoutPanel1.TabIndex = 4;
            flowLayoutPanel1.WrapContents = false;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(3, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(83, 19);
            checkBox1.TabIndex = 0;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // labelEvents
            // 
            labelEvents.AutoSize = true;
            labelEvents.Location = new Point(12, 67);
            labelEvents.Name = "labelEvents";
            labelEvents.Size = new Size(56, 15);
            labelEvents.TabIndex = 5;
            labelEvents.Text = "События";
            // 
            // buttonSettings
            // 
            buttonSettings.Location = new Point(100, 444);
            buttonSettings.Name = "buttonSettings";
            buttonSettings.Size = new Size(75, 23);
            buttonSettings.TabIndex = 7;
            buttonSettings.Text = "Настройки";
            buttonSettings.UseVisualStyleBackColor = true;
            buttonSettings.Click += buttonSettings_Click;
            // 
            // buttonComboForm
            // 
            buttonComboForm.Location = new Point(195, 444);
            buttonComboForm.Name = "buttonComboForm";
            buttonComboForm.Size = new Size(101, 23);
            buttonComboForm.TabIndex = 8;
            buttonComboForm.Text = "ComboForm";
            buttonComboForm.UseVisualStyleBackColor = true;
            buttonComboForm.Click += buttonComboForm_Click;
            // 
            // pxlHunt
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Ivory;
            ClientSize = new Size(349, 516);
            Controls.Add(buttonComboForm);
            Controls.Add(buttonSettings);
            Controls.Add(labelEvents);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(groupBoxStatus);
            Controls.Add(groupBoxActiveTime);
            Name = "pxlHunt";
            ShowIcon = false;
            Text = "pxlHunt";
            groupBoxActiveTime.ResumeLayout(false);
            groupBoxActiveTime.PerformLayout();
            groupBoxStatus.ResumeLayout(false);
            groupBoxStatus.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private GroupBox groupBoxActiveTime;
        private Label labelActiveTime;
        private GroupBox groupBoxStatus;
        private CheckBox checkBoxStatus;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label labelEvents;
        private Button buttonSettings;
        private CheckBox checkBox1;
        private Button buttonComboForm;
    }
}