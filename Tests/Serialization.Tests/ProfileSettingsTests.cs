#nullable enable
using System.Collections.Generic;
using System.Text.Json;
using Core;

public class ProfileSettings
{
    public string CurrentProfile { get; set; } = "Default";
    public List<AppProfileRule> AppProfiles { get; set; } = new();
}

public class AppProfileRule
{
    public string? ProcessName { get; set; }
    public string? WindowTitleContains { get; set; }
    public string ProfileName { get; set; } = "";
}

public class ProfileSettingsTests
{
    [Fact]
    public void ProfileSettingsRoundTrip()
    {
        var settings = new ProfileSettings
        {
            CurrentProfile = "Game",
            AppProfiles = new List<AppProfileRule>
            {
                new AppProfileRule { ProcessName = "game.exe", ProfileName = "Game" },
                new AppProfileRule { WindowTitleContains = "Editor", ProfileName = "Edit" }
            }
        };

        string json = JsonSerializer.Serialize(settings);
        var loaded = JsonSerializer.Deserialize<ProfileSettings>(json);
        Assert.NotNull(loaded);
        Assert.Equal(settings.CurrentProfile, loaded!.CurrentProfile);
        Assert.Equal(2, loaded.AppProfiles.Count);
        Assert.Equal("game.exe", loaded.AppProfiles[0].ProcessName);
    }
}
