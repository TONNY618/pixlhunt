namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    partial class TriggerOrAnd
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
            groupBoxTriggerOrAnd = new GroupBox();
            radioButtonTriggerOrAnd2 = new RadioButton();
            radioButtonTriggerOrAnd1 = new RadioButton();
            groupBoxTriggerOrAnd.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxTriggerOrAnd
            // 
            groupBoxTriggerOrAnd.BackColor = SystemColors.Info;
            groupBoxTriggerOrAnd.Controls.Add(radioButtonTriggerOrAnd2);
            groupBoxTriggerOrAnd.Controls.Add(radioButtonTriggerOrAnd1);
            groupBoxTriggerOrAnd.Location = new Point(0, 0);
            groupBoxTriggerOrAnd.Name = "groupBoxTriggerOrAnd";
            groupBoxTriggerOrAnd.Size = new Size(101, 44);
            groupBoxTriggerOrAnd.TabIndex = 0;
            groupBoxTriggerOrAnd.TabStop = false;
            groupBoxTriggerOrAnd.Text = "И / ИЛИ";
            // 
            // radioButtonTriggerOrAnd2
            // 
            radioButtonTriggerOrAnd2.AutoSize = true;
            radioButtonTriggerOrAnd2.Location = new Point(49, 16);
            radioButtonTriggerOrAnd2.Name = "radioButtonTriggerOrAnd2";
            radioButtonTriggerOrAnd2.Size = new Size(46, 19);
            radioButtonTriggerOrAnd2.TabIndex = 1;
            radioButtonTriggerOrAnd2.Text = "and";
            radioButtonTriggerOrAnd2.UseVisualStyleBackColor = true;
            // 
            // radioButtonTriggerOrAnd1
            // 
            radioButtonTriggerOrAnd1.AutoSize = true;
            radioButtonTriggerOrAnd1.Checked = true;
            radioButtonTriggerOrAnd1.Location = new Point(6, 16);
            radioButtonTriggerOrAnd1.Name = "radioButtonTriggerOrAnd1";
            radioButtonTriggerOrAnd1.Size = new Size(36, 19);
            radioButtonTriggerOrAnd1.TabIndex = 0;
            radioButtonTriggerOrAnd1.TabStop = true;
            radioButtonTriggerOrAnd1.Text = "or";
            radioButtonTriggerOrAnd1.UseVisualStyleBackColor = true;
            // 
            // TriggerOrAnd
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(groupBoxTriggerOrAnd);
            Name = "TriggerOrAnd";
            Size = new Size(101, 44);
            groupBoxTriggerOrAnd.ResumeLayout(false);
            groupBoxTriggerOrAnd.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public GroupBox groupBoxTriggerOrAnd;
        public RadioButton radioButtonTriggerOrAnd2;
        public RadioButton radioButtonTriggerOrAnd1;
    }
}