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
            checkedListBoxCombo = new CheckedListBox();
            listBoxActiv = new ListBox();
            buttonSettings = new Button();
            buttonComboForm = new Button();
            trackBarTransparency = new TrackBar();
            checkBoxTransparentClick = new CheckBox();
            label1 = new Label();
            groupBoxActiveTime.SuspendLayout();
            groupBoxStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarTransparency).BeginInit();
            SuspendLayout();
            // 
            // groupBoxActiveTime
            // 
            groupBoxActiveTime.Controls.Add(labelActiveTime);
            groupBoxActiveTime.Location = new Point(14, 41);
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
            groupBoxStatus.Location = new Point(14, 12);
            groupBoxStatus.Name = "groupBoxStatus";
            groupBoxStatus.Size = new Size(163, 23);
            groupBoxStatus.TabIndex = 3;
            groupBoxStatus.TabStop = false;
            groupBoxStatus.Text = "Статус - пауза";
            // 
            // checkBoxStatus
            // 
            checkBoxStatus.AutoSize = true;
            checkBoxStatus.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            checkBoxStatus.Location = new Point(142, 3);
            checkBoxStatus.Name = "checkBoxStatus";
            checkBoxStatus.Size = new Size(15, 14);
            checkBoxStatus.TabIndex = 0;
            checkBoxStatus.UseVisualStyleBackColor = true;
            // 
            // checkedListBoxCombo
            // 
            checkedListBoxCombo.FormattingEnabled = true;
            checkedListBoxCombo.Location = new Point(12, 142);
            checkedListBoxCombo.Name = "checkedListBoxCombo";
            checkedListBoxCombo.Size = new Size(163, 238);
            checkedListBoxCombo.TabIndex = 4;
            // 
            // listBoxActiv
            // 
            listBoxActiv.FormattingEnabled = true;
            listBoxActiv.Location = new Point(12, 386);
            listBoxActiv.Name = "listBoxActiv";
            listBoxActiv.Size = new Size(443, 154);
            listBoxActiv.TabIndex = 9;
            // 
            // buttonSettings
            // 
            buttonSettings.Location = new Point(12, 546);
            buttonSettings.Name = "buttonSettings";
            buttonSettings.Size = new Size(75, 23);
            buttonSettings.TabIndex = 7;
            buttonSettings.Text = "Настройки";
            buttonSettings.UseVisualStyleBackColor = true;
            buttonSettings.Click += buttonSettings_Click;
            // 
            // buttonComboForm
            // 
            buttonComboForm.Location = new Point(93, 546);
            buttonComboForm.Name = "buttonComboForm";
            buttonComboForm.Size = new Size(82, 23);
            buttonComboForm.TabIndex = 8;
            buttonComboForm.Text = "Combo Edit";
            buttonComboForm.UseVisualStyleBackColor = true;
            buttonComboForm.Click += buttonComboForm_Click;
            // 
            // trackBarTransparency
            // 
            trackBarTransparency.LargeChange = 10;
            trackBarTransparency.Location = new Point(12, 109);
            trackBarTransparency.Maximum = 100;
            trackBarTransparency.Minimum = 10;
            trackBarTransparency.Name = "trackBarTransparency";
            trackBarTransparency.Size = new Size(163, 45);
            trackBarTransparency.TabIndex = 10;
            trackBarTransparency.TickFrequency = 10;
            trackBarTransparency.Value = 100;
            // 
            // checkBoxTransparentClick
            // 
            checkBoxTransparentClick.AutoSize = true;
            checkBoxTransparentClick.Location = new Point(14, 72);
            checkBoxTransparentClick.Name = "checkBoxTransparentClick";
            checkBoxTransparentClick.Size = new Size(143, 19);
            checkBoxTransparentClick.TabIndex = 11;
            checkBoxTransparentClick.Text = "Мышь сквозь форму";
            checkBoxTransparentClick.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 98);
            label1.Name = "label1";
            label1.Size = new Size(84, 15);
            label1.TabIndex = 12;
            label1.Text = "прозрачность";
            // 
            // pxlHunt
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.Ivory;
            ClientSize = new Size(607, 614);
            Controls.Add(label1);
            Controls.Add(checkBoxTransparentClick);
            Controls.Add(buttonComboForm);
            Controls.Add(buttonSettings);
            Controls.Add(listBoxActiv);
            Controls.Add(checkedListBoxCombo);
            Controls.Add(groupBoxStatus);
            Controls.Add(groupBoxActiveTime);
            Controls.Add(trackBarTransparency);
            Name = "pxlHunt";
            ShowIcon = false;
            Text = "5Ng45Ng45Ng4";
            groupBoxActiveTime.ResumeLayout(false);
            groupBoxActiveTime.PerformLayout();
            groupBoxStatus.ResumeLayout(false);
            groupBoxStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarTransparency).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private GroupBox groupBoxActiveTime;
        private Label labelActiveTime;
        private GroupBox groupBoxStatus;
        private CheckBox checkBoxStatus;
        private CheckedListBox checkedListBoxCombo;
        private ListBox listBoxActiv;
        private Button buttonSettings;
        private Button buttonComboForm;
        private TrackBar trackBarTransparency;
        private CheckBox checkBoxTransparentClick;
        private Label label1;
    }
}
