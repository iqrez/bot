using System;

namespace InputToControllerMapper
{
    /// <summary>
    /// Minimal mouse movement event data used by InputCaptureForm.
    /// </summary>
    public class RawMouseEventArgs : EventArgs
    {
        public int DeltaX { get; }
        public int DeltaY { get; }

        public RawMouseEventArgs(int deltaX, int deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }
    }
}
