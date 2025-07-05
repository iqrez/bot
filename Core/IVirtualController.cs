using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Core
{
    public interface IVirtualController
    {
        void SetButton(Xbox360Button button, bool pressed);
        void SetAxis(Xbox360Axis axis, short value);
        void SetTrigger(Xbox360Slider trigger, byte value);
        void SetDPad(Xbox360Button direction, bool pressed);
        void Submit();
    }
}
