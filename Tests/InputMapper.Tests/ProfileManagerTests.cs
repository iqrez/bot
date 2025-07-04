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
}
