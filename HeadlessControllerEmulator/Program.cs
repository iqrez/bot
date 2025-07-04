using System;
using System.Windows.Forms;
using Core;
using InputToControllerMapper;

namespace HeadlessControllerEmulator
{
    internal class HiddenForm : Form
    {
        private readonly ControllerMappingEngine engine;
        private readonly ProfileManager manager;

        public HiddenForm()
        {
            manager = new ProfileManager("HeadlessEmulator");
            engine = new ControllerMappingEngine();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            var input = RawInputHandler.Instance;
            input.RegisterDevices(Handle);
            input.KeyDown += (s, args) => engine.ProcessKeyEvent(args.VirtualKey, true);
            input.KeyUp += (s, args) => engine.ProcessKeyEvent(args.VirtualKey, false);
            input.MouseButtonDown += (s, args) => engine.ProcessMouseButtonEvent((MouseButton)args.Button, true);
            input.MouseButtonUp += (s, args) => engine.ProcessMouseButtonEvent((MouseButton)args.Button, false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                engine.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new HiddenForm());
        }
    }
}
