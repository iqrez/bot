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

            InputCaptureForm mainForm = new InputCaptureForm();
            mainForm.WindowState = FormWindowState.Minimized;
            Application.Run(mainForm);
        }
    }
}
