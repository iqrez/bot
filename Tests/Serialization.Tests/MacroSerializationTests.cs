using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using InputToControllerMapper;

public class MacroSerializationTests
{
    [Fact]
    public void MacroActionsSerializeAndDeserialize()
    {
        var engine = new MacroEngine(new DummySink());
        var macro = new Macro
        {
            Name = "Test",
            Repeat = 2,
            Actions = new List<MacroAction>
            {
                new PressAction("A"),
                new DelayAction(50),
                new ReleaseAction("A")
            }
        };
        engine.AddMacro(macro);

        string json = engine.SerializeMacros();
        Assert.Contains("\"type\": \"press\"", json);

        var engine2 = new MacroEngine(new DummySink());
        engine2.LoadMacros(json);
        string json2 = engine2.SerializeMacros();
        Assert.Equal(json, json2);

        var list = JsonSerializer.Deserialize<List<Macro>>(json2)!;
        Assert.IsType<PressAction>(list[0].Actions[0]);
        Assert.IsType<DelayAction>(list[0].Actions[1]);
        Assert.IsType<ReleaseAction>(list[0].Actions[2]);
    }

    private class DummySink : IButtonSink
    {
        public void SetButtonState(string button, bool pressed) { }
    }
}
