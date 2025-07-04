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

            // If you have a profile system with a new main window:
            try
            {
                string profilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Profiles");
                var manager = new ProfileManager(profilePath);
                Application.Run(new UI.MainWindow(manager));
            }
            catch
            {
                // Fallback to classic/test main form
                Application.Run(new MainForm());
                // Or, if your main form is called InputCaptureForm, use:
                // Application.Run(new InputCaptureForm());
            }
        }
    }
}
