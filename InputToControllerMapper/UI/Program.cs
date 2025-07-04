using System;
using System.Windows.Forms;
using System.IO;
using Core;

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

            // App data location
            string appPath = Application.UserAppDataPath;
            Directory.CreateDirectory(appPath);

            // Profiles directory inside app data
            string profilesDir = Path.Combine(appPath, "profiles");
            Directory.CreateDirectory(profilesDir);

            // ProfileManager expects a folder, not a file
            var profileManager = new ProfileManager(profilesDir);

            // Use MainWindow as your main form
            using var mainForm = new UI.MainWindow(profileManager);
            Application.Run(mainForm);
        }
    }
}
