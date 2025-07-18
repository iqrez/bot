#nullable enable
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InputToControllerMapper;
using Core;

namespace InputToControllerMapper.UI
{
    /// <summary>
    /// Main application window that displays the current profile and mappings.
    /// </summary>
    public class MainWindow : Form
    {
        private readonly ProfileManager profileManager;
        private readonly ListBox profileList;
        private readonly DataGridView mappingGrid;
        private readonly TextBox inputStateBox;
        private readonly TrayIcon tray;

        public MainWindow(ProfileManager manager)
        {
            profileManager = manager ?? throw new ArgumentNullException(nameof(manager));

            Text = "Input To Controller Mapper";
            Size = new Size(900, 600);
            FormClosing += OnFormClosing;

            profileList = new ListBox { Dock = DockStyle.Left, Width = 150 };
            Controls.Add(profileList);
            profileList.SelectedIndexChanged += (s, e) => LoadSelectedProfile();

            mappingGrid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            mappingGrid.Columns.Add("Input", "Input");
            mappingGrid.Columns.Add("Output", "Controller Output");
            Controls.Add(mappingGrid);

            var rightPanel = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 120, FlowDirection = FlowDirection.TopDown };
            var manageBtn = new Button { Text = "Profiles" };
            manageBtn.Click += (s, e) => new ProfileEditor(profileManager).ShowDialog();
            var macroBtn = new Button { Text = "Macros" };
            macroBtn.Click += (s, e) => new MacroEditor().ShowDialog();
            var curveBtn = new Button { Text = "Curve" };
            curveBtn.Click += (s, e) => ShowCurveEditor();
            rightPanel.Controls.Add(manageBtn);
            rightPanel.Controls.Add(macroBtn);
            rightPanel.Controls.Add(curveBtn);
            Controls.Add(rightPanel);

            inputStateBox = new TextBox { Dock = DockStyle.Bottom, ReadOnly = true };
            Controls.Add(inputStateBox);

            tray = new TrayIcon(this, profileManager);

            // Proper tray disposal on exit and close
            Application.ApplicationExit += (s, e) => tray.Dispose();
            FormClosed += (s, e) => tray.Dispose();

            // Refresh UI when profile changes
            profileManager.ProfileChanged += (s, e) =>
            {
                if (InvokeRequired)
                    BeginInvoke(new Action(RefreshProfileList));
                else
                    RefreshProfileList();
            };

            RefreshProfileList();
        }

        private void OnFormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void RefreshProfileList()
        {
            profileList.Items.Clear();
            foreach (var p in profileManager.Profiles)
                profileList.Items.Add(p.Name);

            if (profileManager.CurrentProfile != null)
                profileList.SelectedItem = profileManager.CurrentProfile.Name;
            LoadCurrentProfile();
        }

        private void LoadSelectedProfile()
        {
            if (profileList.SelectedItem is string name)
            {
                profileManager.SetCurrentProfile(name);
                LoadCurrentProfile();
            }
        }

        private void LoadCurrentProfile()
        {
            var p = profileManager.CurrentProfile;
            mappingGrid.Rows.Clear();
            if (p != null)
            {
                foreach (var m in p.Mappings)
                {
                    string input = $"{m.Type}:{m.Code}";
                    string output = string.Join(", ", m.Actions.Select(a => $"{a.Element}:{a.Target}"));
                    mappingGrid.Rows.Add(input, output);
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            var input = RawInputHandler.Instance;
            input.RegisterDevices(Handle);
            input.KeyDown += OnKey;
            input.KeyUp += OnKey;
            input.MouseMove += OnMouseMove;
            input.MouseButtonDown += OnMouseButton;
            input.MouseButtonUp += OnMouseButton;
        }

        private void OnKey(object? sender, RawKeyEventArgs e)
        {
            inputStateBox.Text = $"Key {e.VirtualKey} {(e.IsKeyDown ? "Down" : "Up")}";
        }

        private void OnMouseButton(object? sender, RawMouseButtonEventArgs e)
        {
            inputStateBox.Text = $"Button {e.Button} {(e.IsButtonDown ? "Down" : "Up")}";
        }

        private void OnMouseMove(object? sender, RawMouseMoveEventArgs e)
        {
            inputStateBox.Text = $"Move {e.DeltaX},{e.DeltaY}";
        }

        private void ShowCurveEditor()
        {
            using var dlg = new CurveEditorDialog();
            dlg.ShowDialog();
        }

        private class CurveEditorDialog : Form
        {
            public CurveEditorDialog()
            {
                Text = "Curve / Deadzone";
                Size = new Size(300, 200);
                var dead = new TrackBar { Minimum = 0, Maximum = 100, Dock = DockStyle.Top };
                var sens = new TrackBar { Minimum = 1, Maximum = 200, Dock = DockStyle.Top, Value = 100 };
                Controls.Add(sens);
                Controls.Add(dead);
            }
        }
    }
}
