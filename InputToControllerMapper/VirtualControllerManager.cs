#nullable enable
using System;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Core;

namespace InputToControllerMapper
{
    /// <summary>
    /// Manages a virtual Xbox 360 controller via ViGEm.
    /// </summary>
    public class VirtualControllerManager : IDisposable, IVirtualController
    {
        private readonly ViGEmClient client;
        private readonly IXbox360Controller controller;

        public VirtualControllerManager()
        {
            client = new ViGEmClient();
            controller = client.CreateXbox360Controller();
            controller.FeedbackReceived += OnFeedback;
            controller.Connect();
        }

        public void SetButton(Xbox360Button button, bool pressed) => controller.SetButtonState(button, pressed);

        public void SetAxis(Xbox360Axis axis, short value) => controller.SetAxisValue(axis, value);

        public void SetTrigger(Xbox360Slider trigger, byte value) => controller.SetSliderValue(trigger, value);

        public void SetDPad(Xbox360Button direction, bool pressed) => controller.SetButtonState(direction, pressed);

        public void Submit() => controller.SubmitReport();

        private void OnFeedback(object? sender, Xbox360FeedbackReceivedEventArgs e)
        {
            // Optional: handle rumble or LED feedback here.
        }

        public void Dispose()
        {
            controller.FeedbackReceived -= OnFeedback;
            controller.Disconnect();
            controller.Dispose();
            client.Dispose();
        }
    }
}
