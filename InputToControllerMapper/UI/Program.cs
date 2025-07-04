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

            string appPath = Application.UserAppDataPath;
            Directory.CreateDirectory(appPath);
            var settingsManager = new SettingsManager(Path.Combine(appPath, "settings.json"));

            MainForm mainForm = new MainForm(settingsManager);
            Application.Run(mainForm);
        }
    }
}
