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
            var profileManager = new ProfileManager(Path.Combine(appPath, "profiles"));

            using MainWindow mainForm = new MainWindow(profileManager);
            Application.Run(mainForm);
        }
    }
}
