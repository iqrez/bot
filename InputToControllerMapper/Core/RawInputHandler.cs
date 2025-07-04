using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Core;

namespace InputToControllerMapper
{
    /// <summary>
    /// Provides global keyboard and mouse input via the Windows Raw Input API.
    /// </summary>
    public sealed class RawInputHandler : IDisposable
    {
        // Constants
        internal const int WM_INPUT = 0x00FF;
        private const uint RID_INPUT = 0x10000003;
        private const int RIDEV_INPUTSINK = 0x100;
        private const uint RIM_TYPEMOUSE = 0;
        private const uint RIM_TYPEKEYBOARD = 1;
        private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        private const ushort HID_USAGE_GENERIC_MOUSE = 0x02;
        private const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;

        // Mouse button flags
        private const ushort RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
        private const ushort RI_MOUSE_LEFT_BUTTON_UP = 0x0002;
        private const ushort RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0004;
        private const ushort RI_MOUSE_RIGHT_BUTTON_UP = 0x0008;
        private const ushort RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010;
        private const ushort RI_MOUSE_MIDDLE_BUTTON_UP = 0x0020;
        private const ushort RI_MOUSE_BUTTON_4_DOWN = 0x0040;
        private const ushort RI_MOUSE_BUTTON_4_UP = 0x0080;
        private const ushort RI_MOUSE_BUTTON_5_DOWN = 0x0100;
        private const ushort RI_MOUSE_BUTTON_5_UP = 0x0200;
        private const ushort RI_MOUSE_WHEEL = 0x0400;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        // P/Invoke definitions
        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICE
        {
            public ushort UsagePage;
            public ushort Usage;
            public int Flags;
            public IntPtr Target;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWMOUSE
        {
            public ushort usFlags;
            public uint ulButtons;
            public ushort usButtonFlags;
            public ushort usButtonData;
            public uint ulRawButtons;
            public int lLastX;
            public int lLastY;
            public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct RAWINPUT
        {
            [FieldOffset(0)] public RAWINPUTHEADER header;
            [FieldOffset(16)] public RAWMOUSE mouse;
            [FieldOffset(16)] public RAWKEYBOARD keyboard;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        private IntPtr targetWindow = IntPtr.Zero;

        private static readonly Lazy<RawInputHandler> _instance = new(() => new RawInputHandler(), true);
        public static RawInputHandler Instance => _instance.Value;

        private RawInputHandler() { }

        public event EventHandler<RawKeyEventArgs>? KeyDown;
        public event EventHandler<RawKeyEventArgs>? KeyUp;
        public event EventHandler<RawMouseMoveEventArgs>? MouseMove;
        public event EventHandler<RawMouseButtonEventArgs>? MouseButtonDown;
        public event EventHandler<RawMouseButtonEventArgs>? MouseButtonUp;
        public event EventHandler<RawMouseWheelEventArgs>? MouseWheel;

        /// <summary>
        /// Registers keyboard and mouse devices to receive WM_INPUT messages.
        /// </summary>
        public void RegisterDevices(IntPtr hwnd)
        {
            lock (_instance)
            {
                targetWindow = hwnd;
                RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[2];

                rid[0].UsagePage = HID_USAGE_PAGE_GENERIC;
                rid[0].Usage = HID_USAGE_GENERIC_MOUSE;
                rid[0].Flags = RIDEV_INPUTSINK;
                rid[0].Target = hwnd;

                rid[1].UsagePage = HID_USAGE_PAGE_GENERIC;
                rid[1].Usage = HID_USAGE_GENERIC_KEYBOARD;
                rid[1].Flags = RIDEV_INPUTSINK;
                rid[1].Target = hwnd;

                if (!RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
                {
                    throw new InvalidOperationException("Failed to register raw input devices.");
                }
            }
        }

        /// <summary>
        /// Processes a WM_INPUT message.
        /// </summary>
        public void ProcessInputMessage(IntPtr lParam)
        {
            uint dwSize = 0;
            uint headerSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();

            if (GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref dwSize, headerSize) != 0 || dwSize == 0)
                return;

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
            try
            {
                if (GetRawInputData(lParam, RID_INPUT, buffer, ref dwSize, headerSize) != dwSize)
                    return;

                RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);
                if (raw.header.dwType == RIM_TYPEKEYBOARD)
                {
                    HandleKeyboard(raw.keyboard);
                }
                else if (raw.header.dwType == RIM_TYPEMOUSE)
                {
                    HandleMouse(raw.mouse);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private void HandleKeyboard(RAWKEYBOARD data)
        {
            bool isDown = data.Message == WM_KEYDOWN || data.Message == WM_SYSKEYDOWN;
            bool isUp = data.Message == WM_KEYUP || data.Message == WM_SYSKEYUP;

            if (!isDown && !isUp)
                return;

            var args = new RawKeyEventArgs((Keys)data.VKey, data.MakeCode, isDown);
            if (isDown)
                KeyDown?.Invoke(this, args);
            else
                KeyUp?.Invoke(this, args);
        }

        private void HandleMouse(RAWMOUSE data)
        {
            if (data.lLastX != 0 || data.lLastY != 0)
            {
                MouseMove?.Invoke(this, new RawMouseMoveEventArgs(data.lLastX, data.lLastY));
            }

            ushort flags = data.usButtonFlags;

            if ((flags & RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
                MouseButtonDown?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.Left, true));
            if ((flags & RI_MOUSE_LEFT_BUTTON_UP) != 0)
                MouseButtonUp?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.Left, false));

            if ((flags & RI_MOUSE_RIGHT_BUTTON_DOWN) != 0)
                MouseButtonDown?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.Right, true));
            if ((flags & RI_MOUSE_RIGHT_BUTTON_UP) != 0)
                MouseButtonUp?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.Right, false));

            if ((flags & RI_MOUSE_MIDDLE_BUTTON_DOWN) != 0)
                MouseButtonDown?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.Middle, true));
            if ((flags & RI_MOUSE_MIDDLE_BUTTON_UP) != 0)
                MouseButtonUp?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.Middle, false));

            if ((flags & RI_MOUSE_BUTTON_4_DOWN) != 0)
                MouseButtonDown?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.XButton1, true));
            if ((flags & RI_MOUSE_BUTTON_4_UP) != 0)
                MouseButtonUp?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.XButton1, false));

            if ((flags & RI_MOUSE_BUTTON_5_DOWN) != 0)
                MouseButtonDown?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.XButton2, true));
            if ((flags & RI_MOUSE_BUTTON_5_UP) != 0)
                MouseButtonUp?.Invoke(this, new RawMouseButtonEventArgs(RawMouseButton.XButton2, false));

            if ((flags & RI_MOUSE_WHEEL) != 0)
                MouseWheel?.Invoke(this, new RawMouseWheelEventArgs((short)data.usButtonData));
        }

        public void Dispose()
        {
            lock (_instance)
            {
                targetWindow = IntPtr.Zero;
            }
        }
    }

    public enum RawMouseButton
    {
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }

    public class RawKeyEventArgs : EventArgs
    {
        public Keys VirtualKey { get; }
        public ushort ScanCode { get; }
        public bool IsKeyDown { get; }

        public RawKeyEventArgs(Keys virtualKey, ushort scanCode, bool isDown)
        {
            VirtualKey = virtualKey;
            ScanCode = scanCode;
            IsKeyDown = isDown;
        }
    }

    public class RawMouseMoveEventArgs : EventArgs
    {
        public int DeltaX { get; }
        public int DeltaY { get; }

        public RawMouseMoveEventArgs(int deltaX, int deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }
    }

    public class RawMouseButtonEventArgs : EventArgs
    {
        public RawMouseButton Button { get; }
        public bool IsButtonDown { get; }

        public RawMouseButtonEventArgs(RawMouseButton button, bool isDown)
        {
            Button = button;
            IsButtonDown = isDown;
        }
    }

    public class RawMouseWheelEventArgs : EventArgs
    {
        public int Delta { get; }

        public RawMouseWheelEventArgs(int delta)
        {
            Delta = delta;
        }
    }
}
