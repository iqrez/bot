using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using InputToControllerMapper;
using Xunit;

namespace InputMapper.Tests;

public class ProfileTests
{
    [Fact]
    public void CanLoadAndSaveProfile()
    {
        var profile = new Profile
        {
            Name = "Test",
            KeyBindings = new Dictionary<string, string> { ["A"] = "B" }
        };
        profile.Version = Profile.CurrentVersion;

        string path = Path.Combine(Path.GetTempPath(), "prof.json");
        File.WriteAllText(path, JsonSerializer.Serialize(profile));
        var loaded = Profile.FromJson(File.ReadAllText(path));
        Assert.Equal("Test", loaded.Name);
        Assert.Equal("B", loaded.KeyBindings["A"]);
    }
}
