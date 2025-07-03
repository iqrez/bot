using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;

namespace RawMouseToXbox360
{
    public class ControllerManager : IDisposable
    {
        private readonly ViGEmClient client;
        private readonly IXbox360Controller controller;

        public ControllerManager()
        {
            client = new ViGEmClient();
            controller = client.CreateXbox360Controller();
            controller.Connect();
        }

        public void SetRightStick(int dx, int dy, float sensitivity = 100f, float deadzone = 1.0f)
        {
            short stickX = Math.Abs(dx) > deadzone ? (short)Math.Clamp(dx * sensitivity, -32768, 32767) : (short)0;
            short stickY = Math.Abs(dy) > deadzone ? (short)Math.Clamp(-dy * sensitivity, -32768, 32767) : (short)0;
            controller.SetAxisValue(Xbox360Axis.RightThumbX, stickX);
            controller.SetAxisValue(Xbox360Axis.RightThumbY, stickY);
            controller.SubmitReport();
        }

        public void SetLeftStick(short lx, short ly)
        {
            controller.SetAxisValue(Xbox360Axis.LeftThumbX, lx);
            controller.SetAxisValue(Xbox360Axis.LeftThumbY, ly);
            controller.SubmitReport();
        }

        public void SetTrigger(bool left, bool pressed)
        {
            if (left)
                controller.SetSliderValue(Xbox360Slider.LeftTrigger, pressed ? (byte)255 : (byte)0);
            else
                controller.SetSliderValue(Xbox360Slider.RightTrigger, pressed ? (byte)255 : (byte)0);
            controller.SubmitReport();
        }

        public void SetShoulder(bool left, bool pressed)
        {
            if (left)
                controller.SetButtonState(Xbox360Button.LeftShoulder, pressed);
            else
                controller.SetButtonState(Xbox360Button.RightShoulder, pressed);
            controller.SubmitReport();
        }

        public void TapDpad(bool up)
        {
            var button = up ? Xbox360Button.Up : Xbox360Button.Down;
            controller.SetButtonState(button, true);
            controller.SubmitReport();
            controller.SetButtonState(button, false);
            controller.SubmitReport();
        }

        public void Dispose()
        {
            controller?.Disconnect();
            client?.Dispose();
        }
    }
}
