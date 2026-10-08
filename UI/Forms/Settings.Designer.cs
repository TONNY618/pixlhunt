namespace pxlhunt.FORMS
{
    partial class Settings
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
            groupBoxCombos = new GroupBox();
            listBox1 = new ListBox();
            checkedListBox1 = new CheckedListBox();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            groupBoxCombos.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxCombos
            // 
            groupBoxCombos.Controls.Add(button3);
            groupBoxCombos.Controls.Add(button2);
            groupBoxCombos.Controls.Add(button1);
            groupBoxCombos.Controls.Add(checkedListBox1);
            groupBoxCombos.Location = new Point(12, 12);
            groupBoxCombos.Name = "groupBoxCombos";
            groupBoxCombos.Size = new Size(519, 497);
            groupBoxCombos.TabIndex = 0;
            groupBoxCombos.TabStop = false;
            groupBoxCombos.Text = "List Combos";
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(600, 180);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(120, 94);
            listBox1.TabIndex = 0;
            // 
            // checkedListBox1
            // 
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Items.AddRange(new object[] { "groupBoxEventTMPName TMPName TMPName", "2", "3", "4", "5", "6", "7", "8", "9", "10" });
            checkedListBox1.Location = new Point(6, 22);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new Size(303, 364);
            checkedListBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new Point(6, 392);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 4;
            button1.Text = "Add";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(87, 392);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 5;
            button2.Text = "Delete";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(193, 396);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 6;
            button3.Text = "Edit";
            button3.UseVisualStyleBackColor = true;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 556);
            Controls.Add(groupBoxCombos);
            Controls.Add(listBox1);
            Name = "Settings";
            Text = "Settings";
            groupBoxCombos.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBoxCombos;
        private Button button2;
        private Button button1;
        private CheckedListBox checkedListBox1;
        private ListBox listBox1;
        private Button button3;
    }
}