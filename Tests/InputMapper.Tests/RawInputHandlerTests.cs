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
        dynamic kb = Activator.CreateInstance(kbType)!;
        kb.VKey = (ushort)Keys.A;
        kb.MakeCode = 0;
        kb.Flags = 0;

        bool down = false, up = false;
        handler.KeyDown += (_, e) => { down = e.IsKeyDown && e.VirtualKey == Keys.A; };
        handler.KeyUp += (_, e) => { up = !e.IsKeyDown && e.VirtualKey == Keys.A; };

        kb.Message = (uint)0x0100; // WM_KEYDOWN
        handleKeyboard.Invoke(handler, new object[] { kb });
        kb.Message = (uint)0x0101; // WM_KEYUP
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
        dynamic m = Activator.CreateInstance(mouseType)!;

        bool move = false, down = false, up = false, wheel = false;
        handler.MouseMove += (_, _) => move = true;
        handler.MouseButtonDown += (_, e) => { if (e.Button == RawMouseButton.Left) down = true; };
        handler.MouseButtonUp += (_, e) => { if (e.Button == RawMouseButton.Left) up = true; };
        handler.MouseWheel += (_, e) => { if (e.Delta == 120) wheel = true; };

        m.lLastX = 1; m.lLastY = 2;
        m.usButtonFlags = 0x0001; // left down
        handleMouse.Invoke(handler, new object[] { m });
        m.usButtonFlags = 0x0002; // left up
        handleMouse.Invoke(handler, new object[] { m });
        m.usButtonFlags = 0x0400; m.usButtonData = 120; // wheel
        handleMouse.Invoke(handler, new object[] { m });

        Assert.True(move);
        Assert.True(down);
        Assert.True(up);
        Assert.True(wheel);
    }
}
