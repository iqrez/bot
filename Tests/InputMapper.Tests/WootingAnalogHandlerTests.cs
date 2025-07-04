using System;
using System.Threading;
using InputToControllerMapper;
using Xunit;

namespace InputMapper.Tests;

public class WootingAnalogHandlerTests
{
    [Fact]
    public void GetAnalogValueReturnsSdkValue()
    {
        using var handler = new WootingAnalogHandler();
        Thread.Sleep(50); // allow poll thread
        float val = handler.GetAnalogValue(10);
        Assert.InRange(val, 0f, 1f);
    }

    [Fact]
    public void KeyPressedAndReleasedEventsFire()
    {
        using var handler = new WootingAnalogHandler();
        handler.PressThreshold = 0.2f;
        handler.ReleaseThreshold = 0.1f;
        ushort sc = 5;
        bool pressed = false, released = false;
        handler.KeyPressed += (s, e) => { if (e.ScanCode == sc) pressed = true; };
        handler.KeyReleased += (s, e) => { if (e.ScanCode == sc) released = true; };
        // wait for events to occur (values increment in stub)
        for (int i = 0; i < 20 && !(pressed && released); i++)
        {
            Thread.Sleep(20);
        }
        Assert.True(pressed);
        Assert.True(released);
    }
}
