using System;
using System.Drawing;
using System.Windows.Forms;
using Core;

namespace InputToControllerMapper
{
    /// <summary>
    /// System tray integration for the application.
    /// </summary>
    public class TrayIcon : IDisposable
    {
        private readonly NotifyIcon notifyIcon;
        private readonly Form mainForm;
        private readonly ProfileManager manager;
        private bool enabled = true;
        private bool disposed;

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
            var menu = new ContextMenuStrip
            {
                AccessibleName = "Tray menu",
                AccessibleDescription = "Application options"
            };

            var showItem = new ToolStripMenuItem("&Show")
            {
                AccessibleName = "Show main window",
                AccessibleDescription = "Bring the main window to the front"
            };
            showItem.Click += (s, e) => ShowMainForm();
            menu.Items.Add(showItem);

            var enable = new ToolStripMenuItem("&Enabled")
            {
                Checked = enabled,
                CheckOnClick = true,
                AccessibleName = "Enable mappings",
                AccessibleDescription = "Toggle input to controller mapping"
            };
            enable.CheckedChanged += (s, e) => enabled = enable.Checked;
            menu.Items.Add(enable);

            // Profile menu (using manager.Profiles and manager.ActiveProfile)
            var profiles = new ToolStripMenuItem("&Profiles")
            {
                AccessibleName = "Profiles",
                AccessibleDescription = "Choose active profile"
            };
            foreach (var p in manager.Profiles)
            {
                var item = new ToolStripMenuItem(p.Name)
                {
                    Checked = manager.ActiveProfile != null && p.Name == manager.ActiveProfile.Name,
                    AccessibleName = p.Name,
                    AccessibleDescription = $"Activate profile {p.Name}"
                };
                item.Click += (s, e) =>
                {
                    manager.SetActiveProfile(p.Name);
                    BuildMenu();
                };
                profiles.DropDownItems.Add(item);
            }
            menu.Items.Add(profiles);

            var exitItem = new ToolStripMenuItem("E&xit")
            {
                AccessibleName = "Exit",
                AccessibleDescription = "Close the application"
            };
            exitItem.Click += (s, e) => Application.Exit();
            menu.Items.Add(exitItem);

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
            if (disposed)
                return;
            disposed = true;
            notifyIcon.Dispose();
        }
    }
}
