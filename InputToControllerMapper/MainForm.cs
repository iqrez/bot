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
        private ComboBox themeBox;
        private TrayIcon tray;

        public MainForm()
        {
            Text = "Input To Controller Mapper";
            AccessibleName = "Main Window";
            AccessibleDescription = "Primary interface for configuring input mapping";
            Size = new Size(800, 600);
            AutoScaleMode = AutoScaleMode.Dpi;
            KeyPreview = true;

            profileList = new ListBox { Dock = DockStyle.Left, Width = 150 };
            profileList.AccessibleName = "Profile List";
            profileList.AccessibleDescription = "Available mapping profiles";
            profileList.TabIndex = 0;
            Controls.Add(profileList);

            mappingGrid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            mappingGrid.AccessibleName = "Mapping Grid";
            mappingGrid.AccessibleDescription = "Configure input to controller mapping";
            mappingGrid.Columns.Add("Input", "Input");
            mappingGrid.Columns.Add("Output", "Controller Output");
            mappingGrid.TabIndex = 1;
            Controls.Add(mappingGrid);

            themeBox = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            themeBox.Items.AddRange(Enum.GetNames(typeof(Theme)));
            themeBox.AccessibleName = "Theme Selection";
            themeBox.AccessibleDescription = "Choose between light and dark mode";
            themeBox.SelectedItem = ThemeManager.CurrentTheme.ToString();
            themeBox.SelectedIndexChanged += (s, e) =>
            {
                ThemeManager.CurrentTheme = Enum.Parse<Theme>(themeBox.SelectedItem.ToString());
                ThemeManager.ApplyTheme(this);
            };
            themeBox.TabIndex = 2;
            Controls.Add(themeBox);

            Label themeLabel = new Label { Text = "Theme:", Dock = DockStyle.Top };
            themeLabel.AccessibleName = "Theme Label";
            themeLabel.AccessibleDescription = "Label for theme selection";
            Controls.Add(themeLabel);

            inputGroup = new GroupBox { Text = "Input State", Dock = DockStyle.Bottom, Height = 80 };
            inputGroup.AccessibleName = "Input State";
            inputGroup.AccessibleDescription = "Displays the last input event";
            inputGroup.TabIndex = 3;
            outputGroup = new GroupBox { Text = "Output State", Dock = DockStyle.Bottom, Height = 80 };
            outputGroup.AccessibleName = "Output State";
            outputGroup.AccessibleDescription = "Shows the most recent controller output";
            outputGroup.TabIndex = 4;
            Controls.Add(outputGroup);
            Controls.Add(inputGroup);

            FormClosing += OnFormClosing;
            tray = new TrayIcon(this);
            ThemeManager.ApplyTheme(this);
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
