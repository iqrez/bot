using System;
using System.Windows.Forms;

namespace RawMouseToXbox360
{
    public partial class MainForm : Form
    {
        private ControllerManager controllerManager;
        private WootingHandler wootingHandler;
        private bool mnkToControllerEnabled = false;

        public MainForm()
        {
            InitializeComponent();

            this.Shown += (s, e) =>
            {
                try
                {
                    RawInputHandler.RegisterMouseAndKeyboard(this.Handle);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Raw input registration failed: {ex.Message}");
                    Close();
                    return;
                }

                controllerManager = new ControllerManager();
                wootingHandler = new WootingHandler();

                RawInputHandler.OnMouseDelta += (dx, dy) =>
                {
                    if (mnkToControllerEnabled)
                    {
                        controllerManager.SetRightStick(dx, dy);
                    }
                };

                RawInputHandler.OnMouseButton += (button, pressed) =>
                {
                    if (!mnkToControllerEnabled) return;

                    switch (button)
                    {
                        case MouseButtons.Left:
                            controllerManager.SetTrigger(true, pressed);
                            break;
                        case MouseButtons.Right:
                            controllerManager.SetTrigger(false, pressed);
                            break;
                        case MouseButtons.XButton1:
                            controllerManager.SetShoulder(true, pressed);
                            break;
                        case MouseButtons.XButton2:
                            controllerManager.SetShoulder(false, pressed);
                            break;
                    }
                };

                RawInputHandler.OnMouseWheel += delta =>
                {
                    if (!mnkToControllerEnabled) return;
                    controllerManager.TapDpad(delta > 0);
                };

                wootingHandler.OnAnalogChanged += (lx, ly) =>
                {
                    if (mnkToControllerEnabled)
                    {
                        controllerManager.SetLeftStick(lx, ly);
                    }
                };
            };

            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F10)
                {
                    mnkToControllerEnabled = !mnkToControllerEnabled;
                    MessageBox.Show("Mouse & Keyboard to Controller is now " +
                        (mnkToControllerEnabled ? "ENABLED" : "DISABLED"));
                }
            };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            controllerManager?.Dispose();
            wootingHandler?.Dispose();
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
