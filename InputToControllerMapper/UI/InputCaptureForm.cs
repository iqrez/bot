#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Core;

namespace InputToControllerMapper
{
    /// <summary>
    /// Diagnostic window for capturing raw input and displaying controller state.
    /// </summary>
    public class InputCaptureForm : Form
    {
        private RawInputHandler rawInput;
        private WootingAnalogHandler wootingHandler;
        private ViGEmClient client;
        private IXbox360Controller controller;
        private TextBox logBox;
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

            logBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(360, 240),
                Location = new Point(10, 40)
            };
            Controls.Add(logBox);

            try
            {
                client = new ViGEmClient();
                controller = client.CreateXbox360Controller();
                controller.Connect();
                vigemPanel.BackColor = Color.Green;
                Log("ViGEm connected");
            }
            catch (Exception ex)
            {
                vigemPanel.BackColor = Color.Red;
                Log("ViGEm init failed: " + ex.Message);
            }

            try
            {
                rawInput = RawInputHandler.Instance;
                rawInput.RegisterDevices(this.Handle);
                rawInput.KeyDown += OnKey;
                rawInput.MouseMove += OnMouseMove;
                rawInput.MouseButtonDown += OnMouseButton;
                rawPanel.BackColor = Color.Green;
                Log("Raw input ready");
            }
            catch (Exception ex)
            {
                rawPanel.BackColor = Color.Red;
                Log("Raw input init failed: " + ex.Message);
            }

            try
            {
                wootingHandler = new WootingAnalogHandler();
                wootingHandler.AnalogValueUpdated += (_, e) =>
                {
                    byte val = (byte)(Math.Clamp(e.Value, 0f, 1f) * 255);
                    controller.SetSliderValue(Xbox360Slider.LeftTrigger, val);
                    controller.Submit();
                };
                wootingPanel.BackColor = Color.Green;
                Log("Wooting ready");
            }
            catch (Exception ex)
            {
                wootingPanel.BackColor = Color.Red;
                Log("Wooting init failed: " + ex.Message);
            }

            WindowState = FormWindowState.Minimized;
        }

        private void Log(string msg)
        {
            logBox.AppendText(msg + Environment.NewLine);
        }

        private void OnKey(object sender, RawKeyEventArgs e)
        {
            bool down = e.IsKeyDown;
            switch (e.VirtualKey)
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
            controller.Submit();
            Log("Key " + e.VirtualKey + (down ? " down" : " up"));
        }

        private void OnMouseMove(object sender, RawMouseMoveEventArgs e)
        {
            controller.SetAxisValue(Xbox360Axis.RightThumbX, (short)e.DeltaX);
            controller.SetAxisValue(Xbox360Axis.RightThumbY, (short)e.DeltaY);
            controller.Submit();
        }

        private void OnMouseButton(object sender, RawMouseButtonEventArgs e)
        {
            if (e.Button == RawMouseButton.Left)
                controller.SetSliderValue(Xbox360Slider.RightTrigger, (byte)(e.IsButtonDown ? 255 : 0));
            if (e.Button == RawMouseButton.Right)
                controller.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)(e.IsButtonDown ? 255 : 0));
            controller.Submit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            wootingHandler?.Dispose();
            rawInput?.Dispose();
            controller?.Disconnect();
            controller?.Dispose();
            client?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
