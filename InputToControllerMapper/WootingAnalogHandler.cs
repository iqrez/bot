using System;
using System.Threading;

namespace InputToControllerMapper
{
    public class WootingAnalogHandler : IDisposable
    {
        private Thread thread;
        private bool running;
        private Action<float> callback;

        public WootingAnalogHandler(Action<float> cb)
        {
            callback = cb;
            running = true;
            thread = new Thread(Poll) { IsBackground = true };
            thread.Start();
        }

        private void Poll()
        {
            while (running)
            {
                // Placeholder analog value
                float val = 0f;
                callback(val);
                Thread.Sleep(10);
            }
        }

        public void Dispose()
        {
            running = false;
            thread.Join();
        }
    }
}
