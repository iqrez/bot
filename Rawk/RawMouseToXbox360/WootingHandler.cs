using System;
using System.Windows.Forms;
using WootingAnalogSDKNET;

namespace RawMouseToXbox360
{
    public class WootingHandler : IDisposable
    {
        public event Action<short, short>? OnAnalogChanged;

        private readonly Timer wootingTimer;

        public WootingHandler()
        {
            try
            {
                var (count, result) = WootingAnalogSDK.Initialise();
                if (result != WootingAnalogResult.Ok)
                {
                    Console.WriteLine($"Wooting SDK init failed: {result}");
                }
                else
                {
                    Console.WriteLine($"Wooting SDK initialised with {count} devices");
                    WootingAnalogSDK.SetKeycodeMode(KeycodeType.VirtualKey);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialise Wooting SDK: {ex.Message}");
            }

            wootingTimer = new Timer { Interval = 8 };
            wootingTimer.Tick += (s, e) => Poll();
            wootingTimer.Start();
        }

        private void Poll()
        {
            float w = WootingAnalogSDK.ReadAnalog((ushort)Keys.W).Item1;
            float sVal = WootingAnalogSDK.ReadAnalog((ushort)Keys.S).Item1;
            float a = WootingAnalogSDK.ReadAnalog((ushort)Keys.A).Item1;
            float d = WootingAnalogSDK.ReadAnalog((ushort)Keys.D).Item1;

            short lx = (short)Math.Clamp((d - a) * 32767f, -32768, 32767);
            short ly = (short)Math.Clamp((w - sVal) * 32767f, -32768, 32767);

            OnAnalogChanged?.Invoke(lx, ly);
        }

        public void Dispose()
        {
            wootingTimer?.Stop();
            wootingTimer?.Dispose();
            try
            {
                WootingAnalogSDK.UnInitialise();
            }
            catch
            {
                // ignored
            }
        }
    }
}

