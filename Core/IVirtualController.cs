using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace Core
{
    public enum VirtualControllerType
    {
        Xbox360,
        DualShock4
    }

    public interface IVirtualController
    {
        VirtualControllerType ControllerType { get; }
        void SetButton(Xbox360Button button, bool pressed);
        void SetButton(DualShock4Button button, bool pressed);
        void SetAxis(Xbox360Axis axis, short value);
        void SetAxis(DualShock4Axis axis, byte value);
        void SetTrigger(Xbox360Slider trigger, byte value);
        void SetTrigger(DualShock4Slider trigger, byte value);
        void SetDPad(Xbox360Button direction, bool pressed);
        void SetDPad(DualShock4DPadDirection direction, bool pressed);
        void Submit();
    }
}
