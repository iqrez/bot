using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Windows.Forms;

namespace RawMouseToXbox360
{
    public partial class MainForm : Form
    {
        private ViGEmClient client;
        private IXbox360Controller controller;

        private bool leftTriggerPressed = false;
        private bool rightTriggerPressed = false;
        private bool leftBumperPressed = false;
        private bool rightBumperPressed = false;

        public MainForm()
        {
            this.Shown += (s, e) =>
            {
                try
                {
                    RawInputHandler.RegisterMouseAndKeyboard(this.Handle);
                    Console.WriteLine("Raw input registration succeeded.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Raw input registration failed: {ex.Message}");
                    throw;
                }

                InitializeViGEm();
            };
        }

        private void InitializeViGEm()
        {
            client = new ViGEmClient();
            controller = client.CreateXbox360Controller();
            controller.Connect();
            Console.WriteLine("ViGEm controller connected.");

            RawInputHandler.OnMouseDelta += (dx, dy) =>
            {
                short stickX = (short)Math.Clamp(dx * 2000, -32768, 32767);
                short stickY = (short)Math.Clamp(-dy * 2000, -32768, 32767);

                controller.SetAxisValue(Xbox360Axis.RightThumbX, stickX);
                controller.SetAxisValue(Xbox360Axis.RightThumbY, stickY);
                controller.SubmitReport();

                Console.WriteLine($"Mouse moved: X={stickX}, Y={stickY}");
            };

            RawInputHandler.OnMouseButton += (button, pressed) =>
            {
                switch (button)
                {
                    case MouseButtons.Left:
                        leftTriggerPressed = pressed;
                        controller.SetSliderValue(Xbox360Slider.LeftTrigger, pressed ? (byte)255 : (byte)0);
                        break;

                    case MouseButtons.Right:
                        rightTriggerPressed = pressed;
                        controller.SetSliderValue(Xbox360Slider.RightTrigger, pressed ? (byte)255 : (byte)0);
                        break;

                    case MouseButtons.XButton1:
                        leftBumperPressed = pressed;
                        controller.SetButtonState(Xbox360Button.LeftShoulder, pressed);
                        break;

                    case MouseButtons.XButton2:
                        rightBumperPressed = pressed;
                        controller.SetButtonState(Xbox360Button.RightShoulder, pressed);
                        break;
                }
                controller.SubmitReport();
                Console.WriteLine($"Button {button} {(pressed ? "pressed" : "released")}");
            };

            RawInputHandler.OnMouseWheel += (delta) =>
            {
                if (delta > 0)
                {
                    controller.SetButtonState(Xbox360Button.Up, true);
                    controller.SubmitReport();
                    controller.SetButtonState(Xbox360Button.Up, false);
                    controller.SubmitReport();
                    Console.WriteLine("Mouse wheel up - DPadUp triggered");
                }
                else if (delta < 0)
                {
                    controller.SetButtonState(Xbox360Button.Down, true);
                    controller.SubmitReport();
                    controller.SetButtonState(Xbox360Button.Down, false);
                    controller.SubmitReport();
                    Console.WriteLine("Mouse wheel down - DPadDown triggered");
                }
            };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            controller?.Disconnect();
            client?.Dispose();
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == RawInputHandler.WM_INPUT)
            {
                RawInputHandler.ProcessRawInput(m.LParam);
            }
            base.WndProc(ref m);
        }
    }
}
