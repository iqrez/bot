using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RawMouseToXbox360
{
    public static class RawInputHandler
    {
        public const int WM_INPUT = 0x00FF;

        public delegate void MouseDeltaEventHandler(int dx, int dy);
        public static event MouseDeltaEventHandler OnMouseDelta;

        public delegate void MouseButtonEventHandler(MouseButtons button, bool pressed);
        public static event MouseButtonEventHandler OnMouseButton;

        public delegate void MouseWheelEventHandler(int delta);
        public static event MouseWheelEventHandler OnMouseWheel;

        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("User32.dll")]
        private static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        private const uint RID_INPUT = 0x10000003;
        private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        private const ushort HID_USAGE_GENERIC_MOUSE = 0x02;
        private const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;
        private const uint RIDEV_INPUTSINK = 0x00000100;

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
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
            public int lLastX;
            public int lLastY;
            public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWMOUSE mouse;
        }

        public static void RegisterMouseAndKeyboard(IntPtr hwnd)
        {
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[2];

            rid[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
            rid[0].usUsage = HID_USAGE_GENERIC_MOUSE;
            rid[0].dwFlags = RIDEV_INPUTSINK;
            rid[0].hwndTarget = hwnd;

            rid[1].usUsagePage = HID_USAGE_PAGE_GENERIC;
            rid[1].usUsage = HID_USAGE_GENERIC_KEYBOARD;
            rid[1].dwFlags = 0;
            rid[1].hwndTarget = hwnd;

            if (!RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine($"Raw Input registration failed with error code: {error}");
                throw new ApplicationException("Failed to register raw input devices.");
            }
            else
            {
                Console.WriteLine("Raw Input devices registered successfully.");
            }
        }

        public static void ProcessRawInput(IntPtr lParam)
        {
            uint dwSize = 0;
            GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
            try
            {
                if (GetRawInputData(lParam, RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) != dwSize)
                {
                    Console.WriteLine("GetRawInputData returned unexpected size.");
                    return;
                }

                RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);

                if (raw.header.dwType == 0) // Mouse input
                {
                    const ushort MOUSE_MOVE_RELATIVE = 0x0000;

                    if ((raw.mouse.usFlags & 0x0001) == MOUSE_MOVE_RELATIVE)
                    {
                        int deltaX = raw.mouse.lLastX;
                        int deltaY = raw.mouse.lLastY;
                        Console.WriteLine($"Mouse delta detected: dx={deltaX}, dy={deltaY}");
                        OnMouseDelta?.Invoke(deltaX, deltaY);
                    }
                    else
                    {
                        Console.WriteLine("Absolute mouse movement detected - ignoring.");
                    }

                    ushort buttonFlags = raw.mouse.usButtonFlags;

                    Console.WriteLine($"Button flags: 0x{buttonFlags:X4}, Button data: {raw.mouse.usButtonData}");

                    // Mouse button down/up events
                    if ((buttonFlags & 0x0001) != 0) { Console.WriteLine("Left Button Down"); OnMouseButton?.Invoke(MouseButtons.Left, true); }
                    if ((buttonFlags & 0x0002) != 0) { Console.WriteLine("Left Button Up"); OnMouseButton?.Invoke(MouseButtons.Left, false); }
                    if ((buttonFlags & 0x0004) != 0) { Console.WriteLine("Right Button Down"); OnMouseButton?.Invoke(MouseButtons.Right, true); }
                    if ((buttonFlags & 0x0008) != 0) { Console.WriteLine("Right Button Up"); OnMouseButton?.Invoke(MouseButtons.Right, false); }
                    if ((buttonFlags & 0x0010) != 0) { Console.WriteLine("Middle Button Down"); OnMouseButton?.Invoke(MouseButtons.Middle, true); }
                    if ((buttonFlags & 0x0020) != 0) { Console.WriteLine("Middle Button Up"); OnMouseButton?.Invoke(MouseButtons.Middle, false); }
                    if ((buttonFlags & 0x0040) != 0) { Console.WriteLine("XButton1 Down"); OnMouseButton?.Invoke(MouseButtons.XButton1, true); }
                    if ((buttonFlags & 0x0080) != 0) { Console.WriteLine("XButton1 Up"); OnMouseButton?.Invoke(MouseButtons.XButton1, false); }
                    if ((buttonFlags & 0x0100) != 0) { Console.WriteLine("XButton2 Down"); OnMouseButton?.Invoke(MouseButtons.XButton2, true); }
                    if ((buttonFlags & 0x0200) != 0) { Console.WriteLine("XButton2 Up"); OnMouseButton?.Invoke(MouseButtons.XButton2, false); }

                    // Mouse wheel
                    if ((buttonFlags & 0x0400) != 0)
                    {
                        short wheelDelta = (short)raw.mouse.usButtonData;
                        Console.WriteLine($"Mouse wheel moved: delta={wheelDelta}");
                        OnMouseWheel?.Invoke(wheelDelta);
                    }
                }
                else
                {
                    Console.WriteLine($"Input type {raw.header.dwType} ignored.");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
