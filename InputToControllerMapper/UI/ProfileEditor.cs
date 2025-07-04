using System;
using System.Drawing;
using System.Windows.Forms;
using InputToControllerMapper;

namespace InputToControllerMapper.UI
{
    public class ProfileEditor : Form
    {
        private readonly ProfileManager manager;
        private readonly ListBox list;
        private readonly Button loadBtn;
        private readonly Button saveBtn;
        private readonly Button cloneBtn;
        private readonly Button deleteBtn;

        public ProfileEditor(ProfileManager mgr)
        {
            manager = mgr;
            Text = "Profiles";
            Size = new Size(300, 400);

            list = new ListBox { Dock = DockStyle.Fill };
            Controls.Add(list);

            var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 30 };
            loadBtn = new Button { Text = "Load" };
            saveBtn = new Button { Text = "Save" };
            cloneBtn = new Button { Text = "Clone" };
            deleteBtn = new Button { Text = "Delete" };
            panel.Controls.Add(loadBtn);
            panel.Controls.Add(saveBtn);
            panel.Controls.Add(cloneBtn);
            panel.Controls.Add(deleteBtn);
            Controls.Add(panel);

            loadBtn.Click += (s, e) => LoadSelected();
            saveBtn.Click += (s, e) => SaveSelected();
            cloneBtn.Click += (s, e) => CloneSelected();
            deleteBtn.Click += (s, e) => DeleteSelected();

            RefreshList();
        }

        private string? SelectedName => list.SelectedItem as string;

        private void RefreshList()
        {
            list.Items.Clear();
            foreach (var p in manager.All)
                list.Items.Add(p.Name);
            list.SelectedItem = manager.ActiveProfile.Name;
        }

        private void LoadSelected()
        {
            if (SelectedName != null)
            {
                manager.SetActiveProfile(SelectedName);
                RefreshList();
            }
        }

        private void SaveSelected()
        {
            if (SelectedName != null)
            {
                var profile = manager.GetProfile(SelectedName);
                if (profile != null)
                    manager.SaveProfile(profile);
            }
        }

        private void CloneSelected()
        {
            if (SelectedName == null) return;
            string newName = Microsoft.VisualBasic.Interaction.InputBox("New profile name:", "Clone", SelectedName + "_copy");
            if (string.IsNullOrWhiteSpace(newName)) return;
            manager.CloneProfile(SelectedName, newName);
            RefreshList();
        }

        private void DeleteSelected()
        {
            if (SelectedName == null) return;
            manager.DeleteProfile(SelectedName);
            RefreshList();
        }
    }
}
