using System;
using System.Drawing;
using System.Windows.Forms;

namespace InputToControllerMapper
{
    public class TrayIcon : IDisposable
    {
        private readonly NotifyIcon notifyIcon;
        private readonly Form mainForm;

        public TrayIcon(Form form)
        {
            mainForm = form;
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Input To Controller Mapper",
                Visible = true
            };
            notifyIcon.DoubleClick += (s, e) => ShowMainForm();

            var menu = new ContextMenuStrip();
            menu.Items.Add("Show", null, (s, e) => ShowMainForm());
            menu.Items.Add("Exit", null, (s, e) => Application.Exit());
            notifyIcon.ContextMenuStrip = menu;
        }

        private void ShowMainForm()
        {
            if (mainForm.Visible)
            {
                mainForm.WindowState = FormWindowState.Normal;
                mainForm.Activate();
            }
            else
            {
                mainForm.Show();
            }
        }

        public void ShowHideNotification()
        {
            notifyIcon.BalloonTipTitle = "Input To Controller Mapper";
            notifyIcon.BalloonTipText = "Application is still running";
            notifyIcon.ShowBalloonTip(1000);
        }

        public void Dispose()
        {
            notifyIcon.Dispose();
        }
    }
}
