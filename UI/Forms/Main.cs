using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace pxlhunt.FORMS
{
    public partial class pxlHunt : Form
    {
        public pxlHunt()
        {
            InitializeComponent();
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            testArduino settingsForm = new testArduino();
            settingsForm.Show();
        }

        private void buttonComboForm_Click(object sender, EventArgs e)
        {
            ComboEditorForm settingsForm = new ComboEditorForm();
            settingsForm.Show();
        }
    }
}
