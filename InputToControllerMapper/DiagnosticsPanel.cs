using System.Windows.Forms;

namespace InputToControllerMapper
{
    public class DiagnosticsPanel : UserControl
    {
        private readonly TextBox logBox;

        public DiagnosticsPanel()
        {
            logBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill
            };
            Controls.Add(logBox);

            Logger.LogMessage += OnLogMessage;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger.LogMessage -= OnLogMessage;
            }
            base.Dispose(disposing);
        }

        private void OnLogMessage(Logger.LogLevel level, string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => Append(message)));
            }
            else
            {
                Append(message);
            }
        }

        private void Append(string msg)
        {
            logBox.AppendText(msg + System.Environment.NewLine);
        }
    }
}
