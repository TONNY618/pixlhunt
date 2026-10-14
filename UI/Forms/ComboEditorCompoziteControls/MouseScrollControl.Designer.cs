namespace pxlhunt.UI.Forms.ComboEditorCompoziteControls
{
    partial class MouseScrollControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            groupBoxMouseScroll = new GroupBox();
            labelMouseScrollStep = new Label();
            textBoxStepWhell = new TextBox();
            radioButtonWhellUp = new RadioButton();
            radioButtonWhellDown = new RadioButton();
            groupBoxMouseScroll.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxMouseScroll
            // 
            groupBoxMouseScroll.BackColor = Color.Cyan;
            groupBoxMouseScroll.Controls.Add(labelMouseScrollStep);
            groupBoxMouseScroll.Controls.Add(textBoxStepWhell);
            groupBoxMouseScroll.Controls.Add(radioButtonWhellUp);
            groupBoxMouseScroll.Controls.Add(radioButtonWhellDown);
            groupBoxMouseScroll.ForeColor = SystemColors.HotTrack;
            groupBoxMouseScroll.Location = new Point(0, 0);
            groupBoxMouseScroll.Name = "groupBoxMouseScroll";
            groupBoxMouseScroll.Size = new Size(240, 28);
            groupBoxMouseScroll.TabIndex = 22;
            groupBoxMouseScroll.TabStop = false;
            groupBoxMouseScroll.Text = "Mouse Scroll";
            // 
            // labelMouseScrollStep
            // 
            labelMouseScrollStep.Location = new Point(168, 6);
            labelMouseScrollStep.Name = "labelMouseScrollStep";
            labelMouseScrollStep.Size = new Size(33, 18);
            labelMouseScrollStep.TabIndex = 0;
            labelMouseScrollStep.Text = "step";
            // 
            // textBoxStepWhell
            // 
            textBoxStepWhell.Location = new Point(204, 3);
            textBoxStepWhell.MaxLength = 2;
            textBoxStepWhell.Name = "textBoxStepWhell";
            textBoxStepWhell.Size = new Size(28, 23);
            textBoxStepWhell.TabIndex = 1;
            textBoxStepWhell.Text = "3";
            // 
            // radioButtonWhellUp
            // 
            radioButtonWhellUp.AutoSize = true;
            radioButtonWhellUp.Location = new Point(127, 4);
            radioButtonWhellUp.Name = "radioButtonWhellUp";
            radioButtonWhellUp.Size = new Size(35, 19);
            radioButtonWhellUp.TabIndex = 2;
            radioButtonWhellUp.Text = "/\\";
            // 
            // radioButtonWhellDown
            // 
            radioButtonWhellDown.AutoSize = true;
            radioButtonWhellDown.Checked = true;
            radioButtonWhellDown.Location = new Point(93, 4);
            radioButtonWhellDown.Name = "radioButtonWhellDown";
            radioButtonWhellDown.Size = new Size(35, 19);
            radioButtonWhellDown.TabIndex = 3;
            radioButtonWhellDown.TabStop = true;
            radioButtonWhellDown.Text = "\\/";
            // 
            // MouseScrollControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(groupBoxMouseScroll);
            Name = "MouseScrollControl";
            Size = new Size(240, 28);
            groupBoxMouseScroll.ResumeLayout(false);
            groupBoxMouseScroll.PerformLayout();
            ResumeLayout(false);
        }

        public System.Windows.Forms.GroupBox groupBoxMouseScroll;
        public System.Windows.Forms.Label labelMouseScrollStep;
        public System.Windows.Forms.TextBox textBoxStepWhell;
        public System.Windows.Forms.RadioButton radioButtonWhellUp;
        public System.Windows.Forms.RadioButton radioButtonWhellDown;
    }
}
