using System;
using System.Collections.Generic;
using System.Linq;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System.Windows.Forms;
using Core;

namespace InputToControllerMapper
{
    public class MappingEngine : IDisposable
    {
        private readonly ViGEmClient client;
        private readonly IXbox360Controller controller;
        private MappingProfile profile = new MappingProfile();

        public MappingEngine()
        {
            client = new ViGEmClient();
            controller = client.CreateXbox360Controller();
            controller.Connect();
        }

        public void LoadProfile(MappingProfile p)
        {
            profile = p;
        }

        public void LoadProfile(string path)
        {
            profile = MappingProfile.Load(path);
        }

        public void ProcessKeyEvent(Keys key, bool isDown)
        {
            float val = isDown ? 1f : 0f;
            foreach (var map in profile.Mappings.Where(m => m.Type == InputType.Key && string.Equals(m.Code, key.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                ApplyActions(map.Actions, val);
            }
        }

        public void ProcessMouseButtonEvent(MouseButton button, bool isDown)
        {
            float val = isDown ? 1f : 0f;
            foreach (var map in profile.Mappings.Where(m => m.Type == InputType.MouseButton && string.Equals(m.Code, button.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                ApplyActions(map.Actions, val);
            }
        }

        public void ProcessAnalogEvent(string source, float value)
        {
            foreach (var map in profile.Mappings.Where(m => m.Type == InputType.Analog && string.Equals(m.Code, source, StringComparison.OrdinalIgnoreCase)))
            {
                ApplyActions(map.Actions, value);
            }
        }

        private void ApplyActions(IEnumerable<ControllerAction> actions, float input)
        {
            foreach (var action in actions)
            {
                float val = input;
                if (action.AnalogOptions != null)
                {
                    val = ApplyAnalogOptions(val, action.AnalogOptions);
                }
                SetControllerState(action, val);
            }
            controller.SubmitReport();
        }

        private static float ApplyAnalogOptions(float value, AnalogOptions opts)
        {
            if (Math.Abs(value) < opts.Deadzone)
                return 0f;
            value *= opts.Sensitivity;
            value = Math.Clamp(value, -1f, 1f);
            return opts.Curve switch
            {
                global::Core.CurveType.Squared => MathF.Sign(value) * value * value,
                _ => value
            };
        }

        private void SetControllerState(ControllerAction action, float value)
        {
            switch (action.Element)
            {
                case ControllerElement.Button:
                    controller.SetButtonState(Enum.Parse<Xbox360Button>(action.Target, true), value > 0.5f);
                    break;
                case ControllerElement.Axis:
                    short axisVal = (short)(value * short.MaxValue);
                    controller.SetAxisValue(Enum.Parse<Xbox360Axis>(action.Target, true), axisVal);
                    break;
                case ControllerElement.Trigger:
                    byte trigVal = (byte)(Math.Clamp(value, 0f, 1f) * byte.MaxValue);
                    controller.SetSliderValue(Enum.Parse<Xbox360Slider>(action.Target, true), trigVal);
                    break;
            }
        }

        public void Dispose()
        {
            controller.Disconnect();
            controller.Dispose();
            client.Dispose();
        }
    }
}
