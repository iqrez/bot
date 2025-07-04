using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InputToControllerMapper;
using Xunit;

namespace InputMapper.Tests;

public class TestSink : IButtonSink
{
    public readonly List<(string, bool)> States = new();
    public void SetButtonState(string button, bool pressed) => States.Add((button, pressed));
}

public class MacroEngineTests
{
    [Fact]
    public async Task MacroExecutesActions()
    {
        var sink = new TestSink();
        var engine = new MacroEngine(sink);
        var macro = new Macro
        {
            Name = "Test",
            Actions = new List<MacroAction>
            {
                new PressAction("A"),
                new DelayAction(10),
                new ReleaseAction("A")
            }
        };
        engine.AddMacro(macro);
        var token = new CancellationTokenSource();
        await Task.Run(() => engine.GetType().GetMethod("RunMacroAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(engine, new object[]{"Test", CancellationToken.None, 0}));
        Assert.Equal(new[]{("A",true),("A",false)}, sink.States.ToArray());
    }
}
