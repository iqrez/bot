using System.Collections.Generic;
using System.Text.Json;
using Core;

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
