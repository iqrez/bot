using System;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Core;

namespace InputToControllerMapper
{
    /// <summary>
    /// Simple wrapper that manages either a virtual Xbox 360 or DualShock 4 controller.
    /// </summary>
    public class VirtualControllerManager : IDisposable, IVirtualController
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

        public void SetButton(Xbox360Button xbButton, bool pressed)
        {
            xbox?.SetButtonState(xbButton, pressed);
        }
        public void SetButton(DualShock4Button dsButton, bool pressed)
        {
            ds4?.SetButtonState(dsButton, pressed);
        }

        public void SetAxis(Xbox360Axis xbAxis, short value)
        {
            xbox?.SetAxisValue(xbAxis, value);
        }
        public void SetAxis(DualShock4Axis dsAxis, byte value)
        {
            ds4?.SetAxisValue(dsAxis, value);
        }

        public void SetTrigger(Xbox360Slider xbSlider, byte value)
        {
            xbox?.SetSliderValue(xbSlider, value);
        }
        public void SetTrigger(DualShock4Slider dsSlider, byte value)
        {
            ds4?.SetSliderValue(dsSlider, value);
        }

        public void SetDPad(Xbox360Button xbButton, bool pressed)
        {
            xbox?.SetButtonState(xbButton, pressed);
        }
        public void SetDPad(DualShock4DPadDirection dsDPad, bool pressed)
        {
            if (ds4 == null)
                return;
            ds4.SetDPadDirection(pressed ? dsDPad : DualShock4DPadDirection.None);
        }

        /// <summary>
        /// Sends the prepared report to the driver.
        /// </summary>
        public void Submit()
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
