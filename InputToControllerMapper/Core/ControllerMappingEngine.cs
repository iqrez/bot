#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Core;
using System.Windows.Forms;

namespace InputToControllerMapper
{
    public class ControllerMappingEngine : IDisposable
    {
        private readonly ViGEmClient client;
        private readonly IXbox360Controller controller;
        private Profile profile = new Profile();

        private static readonly Dictionary<string, Xbox360Button> ButtonMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["A"] = Xbox360Button.A,
            ["B"] = Xbox360Button.B,
            ["X"] = Xbox360Button.X,
            ["Y"] = Xbox360Button.Y,
            ["Start"] = Xbox360Button.Start,
            ["Back"] = Xbox360Button.Back,
            ["LeftThumb"] = Xbox360Button.LeftThumb,
            ["RightThumb"] = Xbox360Button.RightThumb,
            ["LeftShoulder"] = Xbox360Button.LeftShoulder,
            ["RightShoulder"] = Xbox360Button.RightShoulder,
            ["Up"] = Xbox360Button.Up,
            ["Down"] = Xbox360Button.Down,
            ["Left"] = Xbox360Button.Left,
            ["Right"] = Xbox360Button.Right,
            ["Guide"] = Xbox360Button.Guide
        };

        private static readonly Dictionary<string, Xbox360Axis> AxisMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LeftThumbX"] = Xbox360Axis.LeftThumbX,
            ["LeftThumbY"] = Xbox360Axis.LeftThumbY,
            ["RightThumbX"] = Xbox360Axis.RightThumbX,
            ["RightThumbY"] = Xbox360Axis.RightThumbY
        };

        private static readonly Dictionary<string, Xbox360Slider> TriggerMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LeftTrigger"] = Xbox360Slider.LeftTrigger,
            ["RightTrigger"] = Xbox360Slider.RightTrigger
        };

        public ControllerMappingEngine()
        {
            client = new ViGEmClient();
            controller = client.CreateXbox360Controller();
            controller.Connect();
        }

        public void LoadProfile(Profile p)
        {
            profile = p;
        }

        public void LoadProfile(string path)
        {
            profile = Profile.Load(path);
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
            controller.Submit();
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
                    if (ButtonMap.TryGetValue(action.Target, out var btn))
                        controller.SetButtonState(btn, value > 0.5f);
                    break;
                case ControllerElement.Axis:
                    if (AxisMap.TryGetValue(action.Target, out var axis))
                    {
                        short axisVal = (short)(value * short.MaxValue);
                        controller.SetAxisValue(axis, axisVal);
                    }
                    break;
                case ControllerElement.Trigger:
                    if (TriggerMap.TryGetValue(action.Target, out var trig))
                    {
                        byte trigVal = (byte)(Math.Clamp(value, 0f, 1f) * byte.MaxValue);
                        controller.SetSliderValue(trig, trigVal);
                    }
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
