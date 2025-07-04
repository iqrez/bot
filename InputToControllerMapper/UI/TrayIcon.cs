using System;
using System.Drawing;
using System.Windows.Forms;
using Core;

namespace InputToControllerMapper
{
    public class TrayIcon : IDisposable
    {
        private readonly NotifyIcon notifyIcon;
        private readonly Form mainForm;
        private readonly ProfileManager manager;
        private bool enabled = true;

        public bool Enabled => enabled;

        public TrayIcon(Form form, ProfileManager profileManager)
        {
            mainForm = form;
            manager = profileManager;
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Input To Controller Mapper",
                Visible = true
            };
            notifyIcon.DoubleClick += (s, e) => ShowMainForm();
            BuildMenu();
            manager.ProfileChanged += (s, e) => BuildMenu();
        }

        private void BuildMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Show", null, (s, e) => ShowMainForm());

            var enable = new ToolStripMenuItem("Enabled") { Checked = enabled, CheckOnClick = true };
            enable.CheckedChanged += (s, e) => enabled = enable.Checked;
            menu.Items.Add(enable);

            var profiles = new ToolStripMenuItem("Profiles");
            foreach (var p in manager.Profiles)
            {
                var item = new ToolStripMenuItem(p.Name) { Checked = p.Name == manager.CurrentProfile.Name };
                item.Click += (s, e) => { manager.SetCurrentProfile(p.Name); BuildMenu(); };
                profiles.DropDownItems.Add(item);
            }
            menu.Items.Add(profiles);

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
