using System;
using System.IO;
using Core;
using Xunit;

namespace InputMapper.Tests;

public class ProfileManagerTests
{
    [Fact]
    public void CanCreateCloneDeleteAndSwitch()
    {
        string appName = "TestPM" + Guid.NewGuid().ToString("N");
        var manager = new ProfileManager(appName);

        // ensure default profile exists
        Assert.NotNull(manager.CurrentProfile);
        Assert.Equal("Default", manager.CurrentProfile.Name);

        // create new profile
        var p = new Profile { Name = "Test" };
        Assert.True(manager.AddProfile(p));
        Assert.True(manager.HasProfile("Test"));

        // switch to new profile
        Assert.True(manager.SetCurrentProfile("Test"));
        Assert.Equal("Test", manager.CurrentProfile.Name);

        // clone profile
        Assert.True(manager.CloneProfile("Test", "Copy"));
        Assert.True(manager.HasProfile("Copy"));

        // delete original
        Assert.True(manager.DeleteProfile("Test"));
        Assert.False(manager.HasProfile("Test"));

        // switch to clone
        Assert.True(manager.SetCurrentProfile("Copy"));
        Assert.Equal("Copy", manager.CurrentProfile.Name);

        // cleanup
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
    }

    [Fact]
    public void SwitchingProfileRaisesEvent()
    {
        string app = "Evt" + Guid.NewGuid().ToString("N");
        var mgr = new ProfileManager(app);
        mgr.AddProfile(new Profile { Name = "Game" });
        bool raised = false;
        mgr.ProfileChanged += (_, __) => raised = true;
        mgr.SetCurrentProfile("Game");
        Assert.True(raised);
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), app);
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
    }
}
