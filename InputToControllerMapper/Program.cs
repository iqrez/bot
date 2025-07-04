using System;
using System.Windows.Forms;

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

            string profilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Profiles");
            var manager = new ProfileManager(profilePath);
            using var mainForm = new UI.MainWindow(manager);
            Application.Run(mainForm);
        }
    }
}
