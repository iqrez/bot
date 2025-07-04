using System;
using System.Runtime.InteropServices;

namespace InputToControllerMapper
{
    public enum KeyCodeMode { HID = 0, ScanCode1 = 1, VirtualKey = 2 }

    public static class WootingAnalog
    {
        [DllImport("wooting_analog_wrapper.dll", EntryPoint = "wooting_analog_initialise")]
        public static extern int Initialise();
        [DllImport("wooting_analog_wrapper.dll", EntryPoint = "wooting_analog_uninitialize")]
        public static extern int Uninitialize();
        [DllImport("wooting_analog_wrapper.dll", EntryPoint = "wooting_analog_set_key_mode")]
        public static extern int SetKeyCodeMode(KeyCodeMode mode);
        [DllImport("wooting_analog_wrapper.dll", EntryPoint = "wooting_analog_read")]
        public static extern int ReadAnalog(uint device, ushort scanCode, out float value);
    }
}
