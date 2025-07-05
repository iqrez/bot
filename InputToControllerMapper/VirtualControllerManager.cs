#nullable enable
using System;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace InputToControllerMapper
{
    public enum VirtualControllerType
    {
        Xbox360,
        DualShock4
    }

    /// <summary>
    /// Simple wrapper that manages either a virtual Xbox 360 or DualShock 4 controller.
    /// </summary>
    public class VirtualControllerManager : IDisposable
    {
        private readonly ViGEmClient client;
        private IXbox360Controller? xbox;
        private IDualShock4Controller? ds4;
        private VirtualControllerType type;

        public VirtualControllerManager(VirtualControllerType controllerType = VirtualControllerType.Xbox360)
        {
            client = new ViGEmClient();
            ChangeControllerType(controllerType);
        }

        public VirtualControllerType ControllerType => type;

        private void DisconnectCurrent()
        {
            if (xbox != null)
            {
                xbox.FeedbackReceived -= OnXboxFeedback;
                xbox.Disconnect();
                xbox = null;
            }

            if (ds4 != null)
            {
                ds4.FeedbackReceived -= OnDs4Feedback;
                ds4.Disconnect();
                ds4 = null;
            }
        }

        /// <summary>
        /// Changes the current virtual controller type.  The previous controller
        /// will be disconnected and a new one created.
        /// </summary>
        public void ChangeControllerType(VirtualControllerType newType)
        {
            if (type == newType)
                return;

            DisconnectCurrent();
            type = newType;

            switch (newType)
            {
                case VirtualControllerType.Xbox360:
                    xbox = client.CreateXbox360Controller();
                    xbox.FeedbackReceived += OnXboxFeedback;
                    xbox.Connect();
                    break;
                case VirtualControllerType.DualShock4:
                    ds4 = client.CreateDualShock4Controller();
                    ds4.FeedbackReceived += OnDs4Feedback;
                    ds4.Connect();
                    break;
            }
        }

        public void SetButton(Enum button, bool pressed)
        {
            if (xbox != null && button is Xbox360Button xb)
                xbox.SetButtonState(xb, pressed);
            else if (ds4 != null && button is DualShock4Button db)
                ds4.SetButtonState(db, pressed);
        }

        public void SetAxis(Enum axis, short value)
        {
            if (xbox != null && axis is Xbox360Axis xa)
                xbox.SetAxisValue(xa, value);
            else if (ds4 != null && axis is DualShock4Axis da)
                ds4.SetAxisValue(da, value);
        }

        public void SetTrigger(Enum trigger, byte value)
        {
            if (xbox != null && trigger is Xbox360Slider xs)
                xbox.SetSliderValue(xs, value);
            else if (ds4 != null && trigger is DualShock4Slider ds)
                ds4.SetSliderValue(ds, value);
        }

        public void SetDPad(Enum direction, bool pressed)
        {
            if (xbox != null && direction is Xbox360Button xb)
                xbox.SetButtonState(xb, pressed);
            else if (ds4 != null && direction is DualShock4DPadDirection dd)
                ds4.SetDPadDirection(dd);
        }

        /// <summary>
        /// Sends the prepared report to the driver.
        /// </summary>
        public void SubmitReport()
        {
            xbox?.SubmitReport();
            ds4?.SubmitReport();
        }

        private void OnXboxFeedback(object? sender, Xbox360FeedbackReceivedEventArgs e)
        {
            // Optional: handle rumble or LED feedback here.
        }

        private void OnDs4Feedback(object? sender, DualShock4FeedbackReceivedEventArgs e)
        {
            // Optional: handle rumble or LED feedback here.
        }

        public void Dispose()
        {
            DisconnectCurrent();
            client.Dispose();
        }
    }
}
