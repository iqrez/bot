using System;
namespace InputToControllerMapper
{
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
