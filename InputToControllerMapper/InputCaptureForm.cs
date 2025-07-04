using System;
using System.Drawing;
using System.Windows.Forms;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace InputToControllerMapper
{
    public class InputCaptureForm : Form
    {
        private RawInputHandler rawInput;
        private WootingAnalogHandler wootingHandler;
        private ViGEmClient client;
        private IXbox360Controller controller;
        private Panel wootingPanel;
        private Panel vigemPanel;
        private Panel rawPanel;

        public InputCaptureForm()
        {
            wootingPanel = new Panel { Size = new Size(20, 20), Location = new Point(10, 10) };
            vigemPanel = new Panel { Size = new Size(20, 20), Location = new Point(40, 10) };
            rawPanel = new Panel { Size = new Size(20, 20), Location = new Point(70, 10) };
            Controls.Add(wootingPanel);
            Controls.Add(vigemPanel);
            Controls.Add(rawPanel);


            try
            {
                client = new ViGEmClient();
                controller = client.CreateXbox360Controller();
                controller.Connect();
                vigemPanel.BackColor = Color.Green;
                Logger.LogInfo("ViGEm connected");
            }
            catch (Exception ex)
            {
                vigemPanel.BackColor = Color.Red;
                Logger.LogError("ViGEm init failed: " + ex.Message);
            }

            try
            {
                rawInput = new RawInputHandler();
                rawInput.RegisterDevice(this.Handle);
                rawInput.KeyPressed += OnKey;
                rawInput.MouseMoved += OnMouse;
                rawInput.MouseButtonPressed += OnButton;
                rawPanel.BackColor = Color.Green;
                Logger.LogInfo("Raw input ready");
            }
            catch (Exception ex)
            {
                rawPanel.BackColor = Color.Red;
                Logger.LogError("Raw input init failed: " + ex.Message);
            }

            try
            {
                wootingHandler = new WootingAnalogHandler(v =>
                {
                    byte val = (byte)(v * 255);
                    controller.SetSliderValue(Xbox360Slider.LeftTrigger, val);
                    controller.SubmitReport();
                });
                wootingPanel.BackColor = Color.Green;
                Logger.LogInfo("Wooting ready");
            }
            catch (Exception ex)
            {
                wootingPanel.BackColor = Color.Red;
                Logger.LogError("Wooting init failed: " + ex.Message);
            }

            WindowState = FormWindowState.Minimized;
        }


        private void OnKey(object sender, RawKeyEventArgs e)
        {
            bool down = e.IsKeyDown;
            switch (e.KeyCode)
            {
                case Keys.W:
                    controller.SetAxisValue(Xbox360Axis.LeftThumbY, (short)(down ? short.MaxValue : 0));
                    break;
                case Keys.S:
                    controller.SetAxisValue(Xbox360Axis.LeftThumbY, (short)(down ? short.MinValue : 0));
                    break;
                case Keys.A:
                    controller.SetAxisValue(Xbox360Axis.LeftThumbX, (short)(down ? short.MinValue : 0));
                    break;
                case Keys.D:
                    controller.SetAxisValue(Xbox360Axis.LeftThumbX, (short)(down ? short.MaxValue : 0));
                    break;
                case Keys.Space:
                    controller.SetButtonState(Xbox360Button.A, down);
                    break;
            }
            controller.SubmitReport();
            Logger.LogInfo("Key " + e.KeyCode + (down ? " down" : " up"));
        }

        private void OnMouse(object sender, RawMouseEventArgs e)
        {
            controller.SetAxisValue(Xbox360Axis.RightThumbX, (short)e.DeltaX);
            controller.SetAxisValue(Xbox360Axis.RightThumbY, (short)e.DeltaY);
            controller.SubmitReport();
        }

        private void OnButton(object sender, RawMouseButtonEventArgs e)
        {
            if (e.IsLeftButton)
                controller.SetSliderValue(Xbox360Slider.RightTrigger, (byte)(e.IsButtonDown ? 255 : 0));
            if (e.IsRightButton)
                controller.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)(e.IsButtonDown ? 255 : 0));
            controller.SubmitReport();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Logger.LogInfo("Input capture form closing");
            wootingHandler?.Dispose();
            rawInput?.Dispose();
            controller?.Disconnect();
            controller?.Dispose();
            client?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
