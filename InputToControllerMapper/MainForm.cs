using System;
using System.Drawing;
using System.Windows.Forms;

namespace InputToControllerMapper
{
    public class MainForm : Form
    {
        private ListBox profileList;
        private DataGridView mappingGrid;
        private GroupBox inputGroup;
        private GroupBox outputGroup;
        private TrayIcon tray;

        public MainForm()
        {
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
    }
}
