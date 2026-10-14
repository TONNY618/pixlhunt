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
            groupBoxMouseScroll = new System.Windows.Forms.GroupBox();
            labelMouseScrollStep = new System.Windows.Forms.Label();
            textBoxStepWhell = new System.Windows.Forms.TextBox();
            radioButtonWhellUp = new System.Windows.Forms.RadioButton();
            radioButtonWhellDown = new System.Windows.Forms.RadioButton();
            groupBoxMouseScroll.SuspendLayout();
            SuspendLayout();

            groupBoxMouseScroll.BackColor = System.Drawing.Color.Gainsboro;
            groupBoxMouseScroll.Controls.Add(labelMouseScrollStep);
            groupBoxMouseScroll.Controls.Add(textBoxStepWhell);
            groupBoxMouseScroll.Controls.Add(radioButtonWhellUp);
            groupBoxMouseScroll.Controls.Add(radioButtonWhellDown);
            groupBoxMouseScroll.ForeColor = System.Drawing.SystemColors.HotTrack;
            groupBoxMouseScroll.Location = new System.Drawing.Point(0, 0);
            groupBoxMouseScroll.Name = "groupBoxMouseScroll";
            groupBoxMouseScroll.Size = new System.Drawing.Size(240, 28);
            groupBoxMouseScroll.TabIndex = 22;
            groupBoxMouseScroll.TabStop = false;
            groupBoxMouseScroll.Text = "Mouse Scroll";

            labelMouseScrollStep.Location = new System.Drawing.Point(168, 6);
            labelMouseScrollStep.Name = "labelMouseScrollStep";
            labelMouseScrollStep.Size = new System.Drawing.Size(33, 18);
            labelMouseScrollStep.Text = "step";

            textBoxStepWhell.Location = new System.Drawing.Point(204, 3);
            textBoxStepWhell.MaxLength = 2;
            textBoxStepWhell.Name = "textBoxStepWhell";
            textBoxStepWhell.Size = new System.Drawing.Size(28, 23);
            textBoxStepWhell.Text = "3";

            radioButtonWhellUp.AutoSize = true;
            radioButtonWhellUp.Location = new System.Drawing.Point(127, 4);
            radioButtonWhellUp.Name = "radioButtonWhellUp";
            radioButtonWhellUp.Size = new System.Drawing.Size(35, 19);
            radioButtonWhellUp.Text = "/\\";

            radioButtonWhellDown.AutoSize = true;
            radioButtonWhellDown.Checked = true;
            radioButtonWhellDown.Location = new System.Drawing.Point(93, 4);
            radioButtonWhellDown.Name = "radioButtonWhellDown";
            radioButtonWhellDown.Size = new System.Drawing.Size(35, 19);
            radioButtonWhellDown.Text = "\\/";

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Transparent;
            Controls.Add(groupBoxMouseScroll);
            Name = "MouseScrollControl";
            Size = new System.Drawing.Size(240, 28);
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
