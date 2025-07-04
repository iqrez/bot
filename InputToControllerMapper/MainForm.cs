using System;
using System.Drawing;
using System.Windows.Forms;

namespace InputToControllerMapper
{
    public class MainForm : Form
    {
        private readonly ListBox profileList;
        private readonly DataGridView mappingGrid;
        private readonly GroupBox inputGroup;
        private readonly GroupBox outputGroup;
        private readonly DiagnosticsPanel diagnosticsPanel;
        private readonly TabControl tabs;
        private readonly TrayIcon tray;

        public MainForm()
        {
            Text = "Input To Controller Mapper";
            Size = new Size(800, 600);

            tabs = new TabControl { Dock = DockStyle.Fill };
            var mainTab = new TabPage("Mapping");
            var diagTab = new TabPage("Diagnostics");
            tabs.TabPages.Add(mainTab);
            tabs.TabPages.Add(diagTab);
            Controls.Add(tabs);

            profileList = new ListBox { Dock = DockStyle.Left, Width = 150 };
            mainTab.Controls.Add(profileList);

            mappingGrid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            mappingGrid.Columns.Add("Input", "Input");
            mappingGrid.Columns.Add("Output", "Controller Output");
            mainTab.Controls.Add(mappingGrid);

            inputGroup = new GroupBox { Text = "Input State", Dock = DockStyle.Bottom, Height = 80 };
            outputGroup = new GroupBox { Text = "Output State", Dock = DockStyle.Bottom, Height = 80 };
            mainTab.Controls.Add(outputGroup);
            mainTab.Controls.Add(inputGroup);

            diagnosticsPanel = new DiagnosticsPanel { Dock = DockStyle.Fill };
            diagTab.Controls.Add(diagnosticsPanel);

            FormClosing += OnFormClosing;
            tray = new TrayIcon(this);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                tray.ShowHideNotification();
                Logger.LogInfo("Main window hidden to tray");
            }
            else
            {
                Logger.LogInfo("Main window closing");
            }
        }

        public void UpdateInputState(string text)
        {
            inputGroup.Text = "Input State: " + text;
            Logger.LogInfo($"Input state: {text}");
        }

        public void UpdateOutputState(string text)
        {
            outputGroup.Text = "Output State: " + text;
            Logger.LogInfo($"Output state: {text}");
        }
    }
}
