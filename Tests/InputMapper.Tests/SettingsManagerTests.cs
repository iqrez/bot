using System.IO;
using InputToControllerMapper;
using Xunit;

namespace InputMapper.Tests;

public class SettingsManagerTests
{
    [Fact]
    public void SavesAndLoadsSettings()
    {
        string path = Path.Combine(Path.GetTempPath(), "settings_test.json");
        if (File.Exists(path))
            File.Delete(path);

        var manager = new SettingsManager(path);
        manager.Current.Theme = "Dark";
        manager.Current.StartWithWindows = true;
        manager.Current.CheckForUpdates = false;
        manager.Current.EnableDiagnostics = true;
        manager.Save();

        var reloaded = new SettingsManager(path);
        Assert.Equal("Dark", reloaded.Current.Theme);
        Assert.True(reloaded.Current.StartWithWindows);
        Assert.False(reloaded.Current.CheckForUpdates);
        Assert.True(reloaded.Current.EnableDiagnostics);
    }
}
