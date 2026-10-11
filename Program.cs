using pxlhunt.FORMS;

namespace pxlhunt
{
    
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Загружаем централизованный конфиг приложения до старта UI
            PixelMacroEngine.Core.Services.AppConfigService.Load();

            //Application.Run(new Form1());
            Application.Run(new pxlHunt());
        }
    }
}
