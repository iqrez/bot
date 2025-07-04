using System;
using System.Reflection;
using System.Windows.Forms;
using InputToControllerMapper;
using Xunit;

namespace InputMapper.Tests;

public class RawInputHandlerTests
{
    private static T CreateStruct<T>() where T : struct => (T)Activator.CreateInstance(typeof(T))!;

    [Fact]
    public void KeyboardEventsAreRaised()
    {
        var type = typeof(RawInputHandler);
        var handler = (RawInputHandler)Activator.CreateInstance(type, true)!;
        var handleKeyboard = type.GetMethod("HandleKeyboard", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var kbType = type.GetNestedType("RAWKEYBOARD", BindingFlags.NonPublic)!;
        object kb = Activator.CreateInstance(kbType)!;
        kbType.GetField("VKey")!.SetValue(kb, (ushort)Keys.A);
        kbType.GetField("MakeCode")!.SetValue(kb, (ushort)0);
        kbType.GetField("Flags")!.SetValue(kb, (ushort)0);

        bool down = false, up = false;
        handler.KeyDown += (_, e) => { down = e.IsKeyDown && e.VirtualKey == Keys.A; };
        handler.KeyUp += (_, e) => { up = !e.IsKeyDown && e.VirtualKey == Keys.A; };

        kbType.GetField("Message")!.SetValue(kb, (uint)0x0100); // WM_KEYDOWN
        handleKeyboard.Invoke(handler, new object[] { kb });
        kbType.GetField("Message")!.SetValue(kb, (uint)0x0101); // WM_KEYUP
        handleKeyboard.Invoke(handler, new object[] { kb });

        Assert.True(down);
        Assert.True(up);
    }

    [Fact]
    public void MouseEventsAreRaised()
    {
        var type = typeof(RawInputHandler);
        var handler = (RawInputHandler)Activator.CreateInstance(type, true)!;
        var handleMouse = type.GetMethod("HandleMouse", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var mouseType = type.GetNestedType("RAWMOUSE", BindingFlags.NonPublic)!;
        object m = Activator.CreateInstance(mouseType)!;

        bool move = false, down = false, up = false, wheel = false;
        handler.MouseMove += (_, _) => move = true;
        handler.MouseButtonDown += (_, e) => { if (e.Button == RawMouseButton.Left) down = true; };
        handler.MouseButtonUp += (_, e) => { if (e.Button == RawMouseButton.Left) up = true; };
        handler.MouseWheel += (_, e) => { if (e.Delta == 120) wheel = true; };

        mouseType.GetField("lLastX")!.SetValue(m, 1);
        mouseType.GetField("lLastY")!.SetValue(m, 2);
        mouseType.GetField("usButtonFlags")!.SetValue(m, (ushort)0x0001); // left down
        handleMouse.Invoke(handler, new object[] { m });
        mouseType.GetField("usButtonFlags")!.SetValue(m, (ushort)0x0002); // left up
        handleMouse.Invoke(handler, new object[] { m });
        mouseType.GetField("usButtonFlags")!.SetValue(m, (ushort)0x0400);
        mouseType.GetField("usButtonData")!.SetValue(m, (ushort)120); // wheel
        handleMouse.Invoke(handler, new object[] { m });

        Assert.True(move);
        Assert.True(down);
        Assert.True(up);
        Assert.True(wheel);
    }
}
