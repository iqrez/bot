#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;
using Core;

namespace InputToControllerMapper
{
    /// <summary>
    /// Simplified main form used in unit tests to host the tray icon.
    /// </summary>
    public class MainForm : Form
    {
        private readonly SettingsManager settingsManager;
        private readonly ProfileManager profileManager;

        private readonly ListBox profileList;
        private readonly DataGridView mappingGrid;
        private readonly GroupBox inputGroup;
        private readonly GroupBox outputGroup;
        private readonly Button settingsButton;

        private readonly TrayIcon tray;

        public MainForm(SettingsManager settings, ProfileManager profiles)
        {
            settingsManager = settings ?? throw new ArgumentNullException(nameof(settings));
            profileManager = profiles ?? throw new ArgumentNullException(nameof(profiles));
            settingsManager.Load();

            // Window setup
            Text = "Input To Controller Mapper";
            Size = new Size(800, 600);
            MinimumSize = new Size(650, 400);

            // Profile List
            profileList = new ListBox
            {
                Dock = DockStyle.Left,
                Width = 160
            };
            Controls.Add(profileList);

            // Mapping grid
            mappingGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            mappingGrid.Columns.Add("Input", "Input");
            mappingGrid.Columns.Add("Output", "Controller Output");
            Controls.Add(mappingGrid);

            // Input/Output Group Boxes
            inputGroup = new GroupBox
            {
                Text = "Input State",
                Dock = DockStyle.Bottom,
                Height = 70
            };
            outputGroup = new GroupBox
            {
                Text = "Output State",
                Dock = DockStyle.Bottom,
                Height = 70
            };
            Controls.Add(outputGroup);
            Controls.Add(inputGroup);

            // Settings Button
            settingsButton = new Button
            {
                Text = "Settings",
                Dock = DockStyle.Top,
                Height = 34
            };
            settingsButton.Click += (s, e) =>
            {
                using var sf = new SettingsForm(settingsManager);
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    ApplyTheme();
                }
            };
            Controls.Add(settingsButton);

            // Tray Icon setup (with profileManager)
            tray = new TrayIcon(this, profileManager);

            // Cleanup tray on close/app exit
            Application.ApplicationExit += (s, e) =>
            {
                tray.Dispose();
                settingsManager.Save();
            };
            FormClosed += (s, e) =>
            {
                tray.Dispose();
                settingsManager.Save();
            };

            // Hide to tray on user close
            FormClosing += OnFormClosing;

            // Initial theming
            ApplyTheme();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                tray.ShowHideNotification();
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
            if (settingsManager.Current?.Theme == "Dark")
            {
                BackColor = Color.FromArgb(45, 45, 48);
                ForeColor = Color.White;
                mappingGrid.BackgroundColor = BackColor;
                mappingGrid.DefaultCellStyle.BackColor = BackColor;
                mappingGrid.DefaultCellStyle.ForeColor = ForeColor;
            }
            else
            {
                BackColor = SystemColors.Control;
                ForeColor = SystemColors.ControlText;
                mappingGrid.BackgroundColor = BackColor;
                mappingGrid.DefaultCellStyle.BackColor = BackColor;
                mappingGrid.DefaultCellStyle.ForeColor = ForeColor;
            }
        }
    }
}
