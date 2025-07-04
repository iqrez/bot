using System.Collections.Generic;
using System.IO;
using Core;
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
            Mappings = new List<InputMapping>
            {
                new()
                {
                    Type = InputType.Key,
                    Code = "A",
                    Actions = new List<ControllerAction>
                    {
                        new() { Element = ControllerElement.Button, Target = "B" }
                    }
                }
            }
        };

        string path = Path.Combine(Path.GetTempPath(), "prof.json");
        profile.Save(path);
        var loaded = Profile.Load(path);
        Assert.Equal("Test", loaded.Name);
        Assert.Equal(profile.Mappings[0].Code, loaded.Mappings[0].Code);
    }
}
