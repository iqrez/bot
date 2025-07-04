using System;
using System.Drawing;
using System.Windows.Forms;
using InputToControllerMapper;
using Core;

namespace InputToControllerMapper.UI
{
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

            profileList = new ListBox
            {
                Dock = DockStyle.Left,
                Width = 150,
                TabIndex = 0,
                AccessibleName = "Profile list",
                AccessibleDescription = "Select active profile"
            };
            Controls.Add(profileList);
            profileList.SelectedIndexChanged += (s, e) => LoadSelectedProfile();

            mappingGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                TabIndex = 1,
                AccessibleName = "Mapping grid",
                AccessibleDescription = "Displays input mappings"
            };
            mappingGrid.Columns.Add("Input", "Input");
            mappingGrid.Columns.Add("Output", "Controller Output");
            Controls.Add(mappingGrid);

            var rightPanel = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 120, FlowDirection = FlowDirection.TopDown };
            var manageBtn = new Button
            {
                Text = "&Profiles",
                TabIndex = 2,
                AccessibleName = "Manage profiles",
                AccessibleDescription = "Open profile management dialog"
            };
            manageBtn.Click += (s, e) => new ProfileEditor(profileManager).ShowDialog();
            var macroBtn = new Button
            {
                Text = "&Macros",
                TabIndex = 3,
                AccessibleName = "Edit macros",
                AccessibleDescription = "Open macro editor"
            };
            macroBtn.Click += (s, e) => new MacroEditor().ShowDialog();
            var curveBtn = new Button
            {
                Text = "&Curve",
                TabIndex = 4,
                AccessibleName = "Adjust curve",
                AccessibleDescription = "Open curve editor"
            };
            curveBtn.Click += (s, e) => ShowCurveEditor();
            rightPanel.Controls.Add(manageBtn);
            rightPanel.Controls.Add(macroBtn);
            rightPanel.Controls.Add(curveBtn);
            Controls.Add(rightPanel);

            inputStateBox = new TextBox
            {
                Dock = DockStyle.Bottom,
                ReadOnly = true,
                TabIndex = 5,
                AccessibleName = "Input state",
                AccessibleDescription = "Shows recent raw input"
            };
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

            if (profileManager.ActiveProfile != null)
                profileList.SelectedItem = profileManager.ActiveProfile.Name;
            LoadActiveProfile();
        }

        private void LoadSelectedProfile()
        {
            if (profileList.SelectedItem is string name)
            {
                profileManager.SetActiveProfile(name);
                LoadActiveProfile();
            }
        }

        private void LoadActiveProfile()
        {
            var p = profileManager.ActiveProfile;
            mappingGrid.Rows.Clear();
            if (p != null)
            {
                foreach (var kv in p.KeyBindings)
                    mappingGrid.Rows.Add(kv.Key, kv.Value);
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

                var dead = new TrackBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Dock = DockStyle.Top,
                    TabIndex = 0,
                    AccessibleName = "Deadzone",
                    AccessibleDescription = "Amount of stick deadzone"
                };
                var sens = new TrackBar
                {
                    Minimum = 1,
                    Maximum = 200,
                    Dock = DockStyle.Top,
                    Value = 100,
                    TabIndex = 1,
                    AccessibleName = "Sensitivity",
                    AccessibleDescription = "Mouse sensitivity percentage"
                };

                var close = new Button
                {
                    Text = "&Close",
                    Dock = DockStyle.Bottom,
                    TabIndex = 2,
                    AccessibleName = "Close",
                    AccessibleDescription = "Close this dialog",
                    DialogResult = DialogResult.OK
                };

                Controls.Add(close);
                Controls.Add(sens);
                Controls.Add(dead);

                AcceptButton = close;
            }
        }
    }
}
