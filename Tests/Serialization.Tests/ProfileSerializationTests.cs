using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Core;

public class ProfileSerializationTests
{
    [Fact]
    public void ProfileRoundTripWithMappings()
    {
        var profile = new Profile
        {
            Name = "Test",
            Mappings = new List<InputMapping>
            {
                new InputMapping
                {
                    Type = InputType.Key,
                    Code = "A",
                    Actions = new List<ControllerAction>
                    {
                        new ControllerAction { Element = ControllerElement.Button, Target = "X" },
                        new ControllerAction { Element = ControllerElement.Axis, Target = "LX", AnalogOptions = new AnalogOptions { Deadzone = 0.1f, Sensitivity = 0.5f } }
                    }
                }
            }
        };

        string json = JsonSerializer.Serialize(profile);
        var loaded = JsonSerializer.Deserialize<Profile>(json);
        Assert.NotNull(loaded);
        Assert.Equal(profile.Name, loaded!.Name);
        Assert.Equal(profile.Mappings.Count, loaded.Mappings.Count);
        Assert.Equal(profile.Mappings[0].Actions[1].AnalogOptions!.Sensitivity,
                     loaded.Mappings[0].Actions[1].AnalogOptions!.Sensitivity);
    }
}
