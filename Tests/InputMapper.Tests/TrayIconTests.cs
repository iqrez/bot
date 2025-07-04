using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using InputToControllerMapper;
using InputToControllerMapper.UI;
using Xunit;

namespace InputMapper.Tests;

public class TrayIconTests
{
    [Fact]
    public void ContextMenuShowMakesFormVisible()
    {
        using var form = new Form();
        form.Hide();
        var manager = new ProfileManager(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        using var tray = new TrayIcon(form, manager);

        var notifyField = typeof(TrayIcon).GetField("notifyIcon", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var notify = (NotifyIcon)notifyField.GetValue(tray)!;
        var showItem = notify.ContextMenuStrip!.Items
            .OfType<ToolStripMenuItem>()
            .First(i => i.Text == "Show");
        showItem.PerformClick();

        Assert.True(form.Visible);
    }

    [Fact]
    public void ContextMenuEnabledTogglesState()
    {
        using var form = new Form();
        var manager = new ProfileManager(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        using var tray = new TrayIcon(form, manager);

        var notifyField = typeof(TrayIcon).GetField("notifyIcon", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var notify = (NotifyIcon)notifyField.GetValue(tray)!;
        var enableItem = notify.ContextMenuStrip!.Items
            .OfType<ToolStripMenuItem>()
            .First(i => i.Text == "Enabled");
        enableItem.PerformClick();

        Assert.False(tray.Enabled);
    }

    [Fact]
    public void ContextMenuProfileSwitchChangesProfile()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        var manager = new ProfileManager(path);
        manager.CreateProfile("A");
        manager.CreateProfile("B");
        using var form = new MainWindow(manager);
        using var tray = new TrayIcon(form, manager);

        var notifyField = typeof(TrayIcon).GetField("notifyIcon", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var notify = (NotifyIcon)notifyField.GetValue(tray)!;
        var profilesMenu = notify.ContextMenuStrip!.Items
            .OfType<ToolStripMenuItem>()
            .First(i => i.Text == "Profiles");
        var itemB = profilesMenu.DropDownItems
            .OfType<ToolStripMenuItem>()
            .First(i => i.Text == "B");
        itemB.PerformClick();

        Assert.Equal("B", manager.CurrentProfile.Name);
        var listField = typeof(MainWindow).GetField("profileList", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var list = (ListBox)listField.GetValue(form)!;
        Assert.Equal("B", list.SelectedItem);
    }
}
