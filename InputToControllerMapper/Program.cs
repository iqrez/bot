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

            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "application.log");
            Logger.Initialize(logPath);

            MainForm mainForm = new MainForm();
            Application.Run(mainForm);

            Logger.Shutdown();
        }
    }
}
