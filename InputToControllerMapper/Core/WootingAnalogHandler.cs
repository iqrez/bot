using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

namespace InputToControllerMapper
{
    /// <summary>
    /// Handles polling of analog values from a Wooting keyboard using the
    /// official Analog SDK.  Values are read in the background and can be
    /// queried at any time.  Optional events are raised when the analog value
    /// crosses configured thresholds to facilitate Dual Keystroke or Rapid
    /// Trigger behaviour.
    /// </summary>
    public class WootingAnalogHandler : IDisposable
    {
        // Import of the required functions from the SDK wrapper.  They are
        // placed here so the file is self contained.
        private static class Native
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

        /// <summary>Number of scan codes that are polled.</summary>
        private const int KeyCount = 256;
        /// <summary>Polling interval in milliseconds.</summary>
        private const int PollIntervalMs = 5;

        private readonly float[] values = new float[KeyCount];
        private readonly float[] previousValues = new float[KeyCount];
        private readonly object valueLock = new();
        private readonly Thread pollThread;
        private bool running;

        // Thresholds for raising digital style events.  These can be tuned for
        // different behaviour.  The defaults work reasonably well for most
        // scenarios.
        public float PressThreshold { get; set; } = 0.8f;
        public float ReleaseThreshold { get; set; } = 0.2f;

        /// <summary>Raised when a key's analog value exceeds <see cref="PressThreshold"/>.</summary>
        public event EventHandler<AnalogKeyEventArgs> KeyPressed;
        /// <summary>Raised when a key's analog value goes below <see cref="ReleaseThreshold"/>.</summary>
        public event EventHandler<AnalogKeyEventArgs> KeyReleased;
        /// <summary>Raised every poll with the current value of a key.</summary>
        public event EventHandler<AnalogKeyEventArgs> AnalogValueUpdated;

        /// <summary>
        /// Initialises the SDK and starts polling in the background.
        /// </summary>
        public WootingAnalogHandler()
        {
            int res = Native.Initialise();
            if (res != 0)
                throw new InvalidOperationException($"Wooting Analog initialisation failed ({res})");

            // Use ScanCode1 so the application can reference keys by scan code.
            Native.SetKeyCodeMode(KeyCodeMode.ScanCode1);

            running = true;
            pollThread = new Thread(PollLoop) { IsBackground = true };
            pollThread.Start();
        }

        /// <summary>
        /// Gets the latest analog value for the specified scan code.
        /// </summary>
        public float GetAnalogValue(ushort scanCode)
        {
            lock (valueLock)
            {
                if (scanCode < values.Length)
                    return values[scanCode];
            }
            return 0f;
        }

        private void PollLoop()
        {
            while (running)
            {
                try
                {
                    for (ushort sc = 0; sc < KeyCount; sc++)
                    {
                        float val;
                        int ret = Native.ReadAnalog(0, sc, out val);
                        if (ret != 0)
                            val = 0f;

                    float prev;
                    lock (valueLock)
                    {
                        prev = values[sc];
                        previousValues[sc] = prev;
                        values[sc] = val;
                    }

                    AnalogValueUpdated?.Invoke(this, new AnalogKeyEventArgs(sc, val));

                    if (val >= PressThreshold && prev < PressThreshold)
                        KeyPressed?.Invoke(this, new AnalogKeyEventArgs(sc, val));
                        if (val <= ReleaseThreshold && prev > ReleaseThreshold)
                            KeyReleased?.Invoke(this, new AnalogKeyEventArgs(sc, val));
                    }

                    Thread.Sleep(PollIntervalMs);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in Wooting poll loop", ex);
                    Thread.Sleep(PollIntervalMs);
                }
            }
        }

        /// <summary>
        /// Stops polling and uninitialises the SDK.
        /// </summary>
        public void Dispose()
        {
            running = false;
            if (pollThread != null && pollThread.IsAlive)
                pollThread.Join();

            Native.Uninitialize();
        }
    }

    /// <summary>Event data for analog key events.</summary>
    public class AnalogKeyEventArgs : EventArgs
    {
        public ushort ScanCode { get; }
        public float Value { get; }
        public AnalogKeyEventArgs(ushort scanCode, float value)
        {
            ScanCode = scanCode;
            Value = value;
        }
    }

    /// <summary>Possible key code modes for the SDK.</summary>
    public enum KeyCodeMode { HID = 0, ScanCode1 = 1, VirtualKey = 2 }
}
