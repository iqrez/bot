using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InputToControllerMapper
{
    public class RawKeyEventArgs : EventArgs
    {
        public Keys KeyCode { get; }
        public bool IsKeyDown { get; }
        public RawKeyEventArgs(Keys k, bool d) { KeyCode = k; IsKeyDown = d; }
    }

    public class RawMouseEventArgs : EventArgs
    {
        public int DeltaX { get; }
        public int DeltaY { get; }
        public RawMouseEventArgs(int x, int y) { DeltaX = x; DeltaY = y; }
    }

    public class RawMouseButtonEventArgs : EventArgs
    {
        public bool IsLeftButton { get; }
        public bool IsRightButton { get; }
        public bool IsButtonDown { get; }
        public RawMouseButtonEventArgs(bool l, bool r, bool d) { IsLeftButton = l; IsRightButton = r; IsButtonDown = d; }
    }

    public class RawInputHandler : IDisposable
    {
        public const int WM_INPUT = 0x00FF;
        private const int RID_INPUT = 0x10000003;
        private const ushort PAGE_GENERIC = 0x01;
        private const ushort USAGE_MOUSE = 0x02;
        private const ushort USAGE_KEYBOARD = 0x06;
        private const uint FLAG_INPUTSINK = 0x00000100;

        [StructLayout(LayoutKind.Sequential)]
        struct RAWINPUTDEVICE { public ushort UsagePage; public ushort Usage; public uint Flags; public IntPtr Target; }
        [DllImport("user32.dll", SetLastError=true)]
        static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] d, uint n, uint cb);
        [DllImport("user32.dll", SetLastError=true)]
        static extern uint GetRawInputData(IntPtr h, uint cmd, IntPtr data, ref uint size, uint cb);

        [StructLayout(LayoutKind.Sequential)] struct RAWINPUTHEADER { public uint dwType; public uint dwSize; public IntPtr hDevice; public IntPtr wParam; }
        [StructLayout(LayoutKind.Sequential)] struct RAWMOUSE { public ushort usFlags; public uint ulButtons; public ushort usButtonFlags; public ushort usButtonData; public int lLastX; public int lLastY; public uint ulExtraInformation; }
        [StructLayout(LayoutKind.Sequential)] struct RAWKEYBOARD { public ushort MakeCode; public ushort Flags; public ushort Reserved; public ushort VKey; public uint Message; public uint ExtraInformation; }
        [StructLayout(LayoutKind.Explicit)] struct RAWINPUT { [FieldOffset(0)] public RAWINPUTHEADER header; [FieldOffset(16)] public RAWMOUSE mouse; [FieldOffset(16)] public RAWKEYBOARD keyboard; }

        public event EventHandler<RawKeyEventArgs> KeyPressed;
        public event EventHandler<RawMouseEventArgs> MouseMoved;
        public event EventHandler<RawMouseButtonEventArgs> MouseButtonPressed;

        public void RegisterDevice(IntPtr hwnd)
        {
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[2];
            rid[0].UsagePage = PAGE_GENERIC; rid[0].Usage = USAGE_KEYBOARD; rid[0].Flags = FLAG_INPUTSINK; rid[0].Target = hwnd;
            rid[1].UsagePage = PAGE_GENERIC; rid[1].Usage = USAGE_MOUSE; rid[1].Flags = FLAG_INPUTSINK; rid[1].Target = hwnd;
            if (!RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
                throw new ApplicationException("raw input failed");
        }

        public void ProcessMessage(IntPtr lParam)
        {
            uint size = 0;
            GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref size, (uint)Marshal.SizeOf<RAWINPUTHEADER>());
            IntPtr buffer = Marshal.AllocHGlobal((int)size);
            try
            {
                if (GetRawInputData(lParam, RID_INPUT, buffer, ref size, (uint)Marshal.SizeOf<RAWINPUTHEADER>()) != size) return;
                RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);
                if (raw.header.dwType == 0)
                {
                    MouseMoved?.Invoke(this, new RawMouseEventArgs(raw.mouse.lLastX, raw.mouse.lLastY));
                    ushort f = raw.mouse.usButtonFlags;
                    if ((f & 0x0001) != 0) MouseButtonPressed?.Invoke(this, new RawMouseButtonEventArgs(true, false, true));
                    if ((f & 0x0002) != 0) MouseButtonPressed?.Invoke(this, new RawMouseButtonEventArgs(true, false, false));
                    if ((f & 0x0004) != 0) MouseButtonPressed?.Invoke(this, new RawMouseButtonEventArgs(false, true, true));
                    if ((f & 0x0008) != 0) MouseButtonPressed?.Invoke(this, new RawMouseButtonEventArgs(false, true, false));
                }
                else if (raw.header.dwType == 1)
                {
                    bool down = raw.keyboard.Flags == 0;
                    Keys k = (Keys)raw.keyboard.VKey;
                    KeyPressed?.Invoke(this, new RawKeyEventArgs(k, down));
                }
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        public void Dispose() { }
    }
}
