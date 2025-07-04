using System;
using System.Drawing;
using System.Windows.Forms;

namespace InputToControllerMapper
{
    // UI dialog for modifying persistent user preferences.
    // Merged after resolving conflicts with main (@iqrez)
    public class SettingsForm : Form
    {
        private readonly SettingsManager settingsManager;
        private readonly ProfileManager? profileManager;

        private CheckBox startCheck;
        private ComboBox profileCombo;
        private ComboBox themeCombo;
        private CheckBox diagnosticsCheck;
        private CheckBox updateCheck;
        private Button saveButton;

        public SettingsForm(SettingsManager manager, ProfileManager? profiles = null)
        {
            settingsManager = manager;
            profileManager = profiles;

            Text = "Settings";
            Size = new Size(400, 230);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            startCheck = new CheckBox { Text = "Start with Windows", Location = new Point(10, 10), AutoSize = true };

            profileCombo = new ComboBox { Location = new Point(10, 40), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            themeCombo = new ComboBox { Location = new Point(10, 70), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            diagnosticsCheck = new CheckBox { Text = "Enable diagnostics", Location = new Point(10, 100), AutoSize = true };
            updateCheck = new CheckBox { Text = "Check for updates", Location = new Point(10, 130), AutoSize = true };

            saveButton = new Button { Text = "Save", Location = new Point(10, 160), Width = 80 };
            saveButton.Click += (s, e) => Save();

            Controls.Add(startCheck);
            Controls.Add(new Label { Text = "Default profile", Location = new Point(220, 43), AutoSize = true });
            Controls.Add(profileCombo);
            Controls.Add(new Label { Text = "Theme", Location = new Point(220, 73), AutoSize = true });
            Controls.Add(themeCombo);
            Controls.Add(diagnosticsCheck);
            Controls.Add(updateCheck);
            Controls.Add(saveButton);

            themeCombo.Items.AddRange(new object[] { "Light", "Dark" });

            if (profileManager != null)
            {
                foreach (var p in profileManager.All)
                {
                    profileCombo.Items.Add(p.Name);
                }
            }
            if (profileCombo.Items.Count == 0)
                profileCombo.Items.Add(settingsManager.Current.DefaultProfile);

            LoadFromSettings();
        }

        private void LoadFromSettings()
        {
            startCheck.Checked = settingsManager.Current.StartWithWindows;
            diagnosticsCheck.Checked = settingsManager.Current.EnableDiagnostics;
            updateCheck.Checked = settingsManager.Current.CheckForUpdates;
            themeCombo.SelectedItem = settingsManager.Current.Theme;
            profileCombo.SelectedItem = settingsManager.Current.DefaultProfile;
        }

        private void Save()
        {
            settingsManager.Current.StartWithWindows = startCheck.Checked;
            settingsManager.Current.DefaultProfile = profileCombo.SelectedItem?.ToString() ?? settingsManager.Current.DefaultProfile;
            settingsManager.Current.Theme = themeCombo.SelectedItem?.ToString() ?? settingsManager.Current.Theme;
            settingsManager.Current.EnableDiagnostics = diagnosticsCheck.Checked;
            settingsManager.Current.CheckForUpdates = updateCheck.Checked;
            settingsManager.Save();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
