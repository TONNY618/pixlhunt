using PixelMacroEngine.Core.Services;



namespace pxlhunt.FORMS
{
    

    public partial class pxlHunt : Form
    {
        //public bool getVideoBuffer = false;

        public pxlHunt()
        {
            InitializeComponent();

            // Стартуем глобальный захват при запуске программы
            try
            {
                // Стартуем глобальный захват при запуске программы
                ScreenCaptureService.Start(monitorIndex: 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации захвата экрана:\n{ex.Message}",
                                "Ошибка DXGI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // При закрытии главного окна — корректно глушим видеокарту
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            ScreenCaptureService.Stop();
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            testArduino settingsForm = new testArduino();
            settingsForm.Show();
        }

        private void buttonComboForm_Click(object sender, EventArgs e)
        {
            // Просто открываем форму — ей ничего передавать не нужно,
            // буфер уже сам работает в фоне!
            ComboEditorForm editor = new ComboEditorForm();
            editor.Show();
        }
    }
    
}
