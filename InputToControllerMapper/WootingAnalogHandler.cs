using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace InputToControllerMapper
{
    /// <summary>
    /// Provides access to Wooting keyboards via the official analog SDK.
    /// Values are polled in the background and events are raised when the
    /// state of a key changes.
    /// </summary>
    public sealed class WootingAnalogHandler : IDisposable
    {
        private static class Native
        {
            [DllImport("wooting_analog_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int Initialise();
            [DllImport("wooting_analog_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int Uninitialise();
            [DllImport("wooting_analog_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern float ReadAnalog(int keycode);
        }

        private const int KeyCount = 256;
        private readonly float[] values = new float[KeyCount];
        private readonly DKSStage[] dksStages = new DKSStage[KeyCount];
        private readonly bool[] rapidStates = new bool[KeyCount];

        public int PollRateHz
        {
            get => pollRateHz;
            set => pollRateHz = Math.Clamp(value, 250, 1000);
        }
        private int pollRateHz = 500; // default 500Hz

        public float DksStage1Threshold { get; set; } = 0.2f;
        public float DksStage2Threshold { get; set; } = 0.8f;
        public float RapidPressThreshold { get; set; } = 0.8f;
        public float RapidReleaseThreshold { get; set; } = 0.2f;

        public event Action<int, float>? AnalogValueChanged;
        public event Action<int, DKSStage>? DKSStageChanged;
        public event Action<int, bool>? RapidTriggerFired;

        private readonly Thread pollThread;
        private volatile bool running;
        private bool disposed;

        public WootingAnalogHandler()
        {
            int res = 0;
            try
            {
                res = Native.Initialise();
            }
            catch (DllNotFoundException ex)
            {
                throw new InvalidOperationException("Wooting analog SDK not found", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialise Wooting SDK", ex);
            }
            if (res != 0)
                throw new InvalidOperationException($"Wooting initialisation failed ({res})");

            running = true;
            pollThread = new Thread(PollLoop) { IsBackground = true };
            pollThread.Start();
        }

        private void PollLoop()
        {
            while (running)
            {
                int delay = 1000 / PollRateHz;
                if (delay <= 0) delay = 1;
                for (int key = 0; key < KeyCount; key++)
                {
                    float val;
                    try
                    {
                        val = Native.ReadAnalog(key);
                    }
                    catch
                    {
                        val = 0f;
                    }

                    if (Math.Abs(val - values[key]) > float.Epsilon)
                    {
                        values[key] = val;
                        AnalogValueChanged?.Invoke(key, val);
                    }

                    DKSStage stage = GetStage(val);
                    if (stage != dksStages[key])
                    {
                        dksStages[key] = stage;
                        DKSStageChanged?.Invoke(key, stage);
                    }

                    bool pressed = rapidStates[key];
                    bool nextPressed = pressed ? val >= RapidReleaseThreshold : val >= RapidPressThreshold;
                    if (pressed != nextPressed)
                    {
                        rapidStates[key] = nextPressed;
                        RapidTriggerFired?.Invoke(key, nextPressed);
                    }
                }

                Thread.Sleep(delay);
            }
        }

        private DKSStage GetStage(float value)
        {
            if (value >= DksStage2Threshold)
                return DKSStage.Stage2;
            if (value >= DksStage1Threshold)
                return DKSStage.Stage1;
            return DKSStage.None;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;

            running = false;
            if (pollThread.IsAlive)
            {
                try { pollThread.Join(); } catch { }
            }
            try { Native.Uninitialise(); } catch { }
        }
    }

    public enum DKSStage
    {
        None,
        Stage1,
        Stage2
    }
}
