using System.Collections.Generic;
using System.Reflection;
using InputToControllerMapper;
using Xunit;

namespace InputMapper.Tests;

public class DummyController : Nefarius.ViGEm.Client.Targets.Xbox360.IXbox360Controller
{
    public readonly Dictionary<string, object> States = new();
    public void Connect() { }
    public void Disconnect() { }
    public void Dispose() { }
    public void SubmitReport() { }
    public void SetAxisValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Axis axis, short value) => States[axis.ToString()] = value;
    public void SetButtonState(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Button button, bool pressed) => States[button.ToString()] = pressed;
    public void SetSliderValue(Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Slider slider, byte value) => States[slider.ToString()] = value;
}

public class MappingEngineTests
{
    [Fact]
    public void ApplyActionsSetsControllerState()
    {
        var engine = (MappingEngine)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(MappingEngine));
        var controllerField = typeof(MappingEngine).GetField("controller", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var clientField = typeof(MappingEngine).GetField("client", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var dummy = new DummyController();
        controllerField.SetValue(engine, dummy);
        clientField.SetValue(engine, null);
        var profile = new MappingProfile();
        profile.Mappings.Add(new InputMapping
        {
            Type = InputType.Key,
            Code = "A",
            Actions = new List<ControllerAction>{ new(){Element=ControllerElement.Button, Target="A"} }
        });
        engine.LoadProfile(profile);
        engine.ProcessKeyEvent(System.Windows.Forms.Keys.A, true);
        Assert.Equal(true, dummy.States["A"]);
    }
}
