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
