using System;
using System.Windows.Forms;
using System.IO;

namespace InputToControllerMapper
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (s, e) => Logger.LogError("UI thread exception", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Logger.LogError("Unhandled exception", e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()));

            string appPath = Application.UserAppDataPath;
            Directory.CreateDirectory(appPath);
            var settingsManager = new SettingsManager(Path.Combine(appPath, "settings.json"));

            MainForm mainForm = new MainForm(settingsManager);
            try
            {
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                Logger.LogError("Fatal application error", ex);
                throw;
            }
        }
    }
}
