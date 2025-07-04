#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Core
{
    public enum MouseButtonType
    {
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }

    /// <summary>
    /// Provides global keyboard and mouse input using Windows Raw Input.
    /// Call <see cref="ProcessMessage"/> from your window procedure whenever a WM_INPUT message is received.
    /// </summary>
    public static class RawInputHandler
    {
        internal const int WM_INPUT = 0x00FF;

        private const uint RID_INPUT = 0x10000003;
        private const int RIDEV_INPUTSINK = 0x100;
        private const uint RIM_TYPEMOUSE = 0;
        private const uint RIM_TYPEKEYBOARD = 1;
        private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        private const ushort HID_USAGE_GENERIC_MOUSE = 0x02;
        private const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;

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

        private static IntPtr _window = IntPtr.Zero;
        private static bool _registered;

        public static event Action<int, int>? KeyDown;
        public static event Action<int, int>? KeyUp;
        public static event Action<int, int>? MouseMove;
        public static event Action<MouseButtonType, bool>? MouseButton;
        public static event Action<int>? MouseWheel;

        /// <summary>
        /// Registers keyboard and mouse to receive WM_INPUT for the specified window handle.
        /// </summary>
        public static void RegisterDevices(IntPtr hwnd)
        {
            _window = hwnd;
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

            _registered = true;
        }

        /// <summary>
        /// Processes a WM_INPUT message from the application window procedure.
        /// </summary>
        public static void ProcessMessage(IntPtr lParam)
        {
            if (!_registered)
                return;

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

        private static void HandleKeyboard(RAWKEYBOARD data)
        {
            bool isDown = data.Message == WM_KEYDOWN || data.Message == WM_SYSKEYDOWN;
            bool isUp = data.Message == WM_KEYUP || data.Message == WM_SYSKEYUP;

            if (!isDown && !isUp)
                return;

            if (isDown)
                KeyDown?.Invoke(data.VKey, data.MakeCode);
            else
                KeyUp?.Invoke(data.VKey, data.MakeCode);
        }

        private static void HandleMouse(RAWMOUSE data)
        {
            if (data.lLastX != 0 || data.lLastY != 0)
            {
                MouseMove?.Invoke(data.lLastX, data.lLastY);
            }

            ushort flags = data.usButtonFlags;

            if ((flags & RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
                MouseButton?.Invoke(MouseButtonType.Left, true);
            if ((flags & RI_MOUSE_LEFT_BUTTON_UP) != 0)
                MouseButton?.Invoke(MouseButtonType.Left, false);

            if ((flags & RI_MOUSE_RIGHT_BUTTON_DOWN) != 0)
                MouseButton?.Invoke(MouseButtonType.Right, true);
            if ((flags & RI_MOUSE_RIGHT_BUTTON_UP) != 0)
                MouseButton?.Invoke(MouseButtonType.Right, false);

            if ((flags & RI_MOUSE_MIDDLE_BUTTON_DOWN) != 0)
                MouseButton?.Invoke(MouseButtonType.Middle, true);
            if ((flags & RI_MOUSE_MIDDLE_BUTTON_UP) != 0)
                MouseButton?.Invoke(MouseButtonType.Middle, false);

            if ((flags & RI_MOUSE_BUTTON_4_DOWN) != 0)
                MouseButton?.Invoke(MouseButtonType.XButton1, true);
            if ((flags & RI_MOUSE_BUTTON_4_UP) != 0)
                MouseButton?.Invoke(MouseButtonType.XButton1, false);

            if ((flags & RI_MOUSE_BUTTON_5_DOWN) != 0)
                MouseButton?.Invoke(MouseButtonType.XButton2, true);
            if ((flags & RI_MOUSE_BUTTON_5_UP) != 0)
                MouseButton?.Invoke(MouseButtonType.XButton2, false);

            if ((flags & RI_MOUSE_WHEEL) != 0)
                MouseWheel?.Invoke((short)data.usButtonData);
        }

        /// <summary>
        /// Clears registered window handle and stops raising events.
        /// </summary>
        public static void Dispose()
        {
            _registered = false;
            _window = IntPtr.Zero;
        }
    }
}
