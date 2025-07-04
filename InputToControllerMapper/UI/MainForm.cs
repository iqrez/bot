using System;
using System.Drawing;
using System.Windows.Forms;
using Core;

namespace InputToControllerMapper
{
    public class MainForm : Form
    {
        private readonly SettingsManager settingsManager;
        private readonly ProfileManager profileManager;

        private ListBox profileList;
        private DataGridView mappingGrid;
        private GroupBox inputGroup;
        private GroupBox outputGroup;
        private TrayIcon tray;
        private Button settingsButton;

        public MainForm(SettingsManager settings, ProfileManager profiles)
        {
            settingsManager = settings;
            profileManager = profiles;

            Text = "Input To Controller Mapper";
            Size = new Size(800, 600);

            profileList = new ListBox { Dock = DockStyle.Left, Width = 150 };
            Controls.Add(profileList);

            mappingGrid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            mappingGrid.Columns.Add("Input", "Input");
            mappingGrid.Columns.Add("Output", "Controller Output");
            Controls.Add(mappingGrid);

            inputGroup = new GroupBox { Text = "Input State", Dock = DockStyle.Bottom, Height = 80 };
            outputGroup = new GroupBox { Text = "Output State", Dock = DockStyle.Bottom, Height = 80 };
            Controls.Add(outputGroup);
            Controls.Add(inputGroup);

            settingsButton = new Button { Text = "Settings", Dock = DockStyle.Top, Height = 30 };
            settingsButton.Click += (s, e) => {
                using SettingsForm sf = new SettingsForm(settingsManager);
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    ApplyTheme();
                }
            };
            Controls.Add(settingsButton);

            FormClosing += OnFormClosing;
            tray = new TrayIcon(this, profileManager);

            ApplyTheme();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    Hide();
                    tray.ShowHideNotification();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during form closing", ex);
            }
        }

        public void UpdateInputState(string text)
        {
            inputGroup.Text = "Input State: " + text;
        }

        public void UpdateOutputState(string text)
        {
            outputGroup.Text = "Output State: " + text;
        }

        private void ApplyTheme()
        {
            if (settingsManager.Current.Theme == "Dark")
            {
                BackColor = Color.FromArgb(45, 45, 48);
                ForeColor = Color.White;
            }
            else
            {
                BackColor = SystemColors.Control;
                ForeColor = SystemColors.ControlText;
            }
        }
    }
}
