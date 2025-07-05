using System;
using System.Collections.Generic;
using System.Linq;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Core
{
    /// <summary>
    /// Central class that translates input events into controller state.
    /// </summary>
    public class MappingEngine
    {
        private readonly MouseToStickMapper mouseMapper = new();
        private Profile profile = new Profile();

        private readonly Dictionary<string, bool> buttonStates = new();
        private readonly Dictionary<string, float> axisStates = new();
        private readonly Dictionary<string, float> triggerStates = new();
        private readonly Dictionary<string, bool> dpadStates = new();

        private static readonly Dictionary<string, Xbox360Button> XboxButtons = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Up"] = Xbox360Button.Up,
            ["Down"] = Xbox360Button.Down,
            ["Left"] = Xbox360Button.Left,
            ["Right"] = Xbox360Button.Right,
            ["Start"] = Xbox360Button.Start,
            ["Back"] = Xbox360Button.Back,
            ["LeftThumb"] = Xbox360Button.LeftThumb,
            ["RightThumb"] = Xbox360Button.RightThumb,
            ["LeftShoulder"] = Xbox360Button.LeftShoulder,
            ["RightShoulder"] = Xbox360Button.RightShoulder,
            ["Guide"] = Xbox360Button.Guide,
            ["A"] = Xbox360Button.A,
            ["B"] = Xbox360Button.B,
            ["X"] = Xbox360Button.X,
            ["Y"] = Xbox360Button.Y
        };

        private static readonly Dictionary<string, Xbox360Axis> XboxAxes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LeftThumbX"] = Xbox360Axis.LeftThumbX,
            ["LeftThumbY"] = Xbox360Axis.LeftThumbY,
            ["RightThumbX"] = Xbox360Axis.RightThumbX,
            ["RightThumbY"] = Xbox360Axis.RightThumbY
        };

        private static readonly Dictionary<string, Xbox360Slider> XboxTriggers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LeftTrigger"] = Xbox360Slider.LeftTrigger,
            ["RightTrigger"] = Xbox360Slider.RightTrigger
        };

        private int mouseDeltaX;
        private int mouseDeltaY;

        /// <summary>Apply a new profile.</summary>
        public void ApplyProfile(Profile profile) => this.profile = profile;

        /// <summary>Handle an incoming input event.</summary>
        public void ProcessInputEvent(InputEvent e)
        {
            if (e.Type == InputType.MouseMoveX)
                mouseDeltaX += (int)e.Value;
            else if (e.Type == InputType.MouseMoveY)
                mouseDeltaY += (int)e.Value;
            else
                ProcessDirectEvent(e.Type, e.Code, e.Value);
        }

        private void ProcessDirectEvent(InputType type, string code, float value)
        {
            foreach (var map in profile.Mappings.Where(m => m.Type == type &&
                        string.Equals(m.Code, code, StringComparison.OrdinalIgnoreCase)))
            {
                ApplyActions(map.Actions, value);
            }
        }

        private void ApplyMouseMappings()
        {
            if (mouseDeltaX == 0 && mouseDeltaY == 0)
                return;

            (short x, short y) = mouseMapper.Map(mouseDeltaX, mouseDeltaY);
            float fx = x / 32767f;
            float fy = y / 32767f;

            foreach (var map in profile.Mappings.Where(m => m.Type == InputType.MouseMoveX))
                ApplyActions(map.Actions, fx);
            foreach (var map in profile.Mappings.Where(m => m.Type == InputType.MouseMoveY))
                ApplyActions(map.Actions, fy);

            mouseDeltaX = 0;
            mouseDeltaY = 0;
        }

        private void ApplyActions(IEnumerable<ControllerAction> actions, float input)
        {
            foreach (var action in actions)
            {
                float val = input;
                if (action.AnalogOptions != null)
                    val = ApplyAnalogOptions(val, action.AnalogOptions);

                switch (action.Element)
                {
                    case ControllerElement.Button:
                        buttonStates[action.Target] = val > 0.5f;
                        break;
                    case ControllerElement.Axis:
                        axisStates[action.Target] = Math.Clamp(val, -1f, 1f);
                        break;
                    case ControllerElement.Trigger:
                        triggerStates[action.Target] = Math.Clamp(val, 0f, 1f);
                        break;
                    case ControllerElement.DPad:
                        dpadStates[action.Target] = val > 0.5f;
                        break;
                }
            }
        }

        private static float ApplyAnalogOptions(float value, AnalogOptions opts)
        {
            if (Math.Abs(value) < opts.Deadzone)
                return 0f;
            value *= opts.Sensitivity;
            value = Math.Clamp(value, -1f, 1f);
            return opts.Curve switch
            {
                CurveType.Squared => MathF.Sign(value) * value * value,
                _ => value
            };
        }

        /// <summary>
        /// Update the provided virtual controller with the current computed state.
        /// </summary>
        public void UpdateControllerState(IVirtualController ctrl)
        {
            ApplyMouseMappings();

            foreach (var kv in buttonStates)
                if (XboxButtons.TryGetValue(kv.Key, out var btn))
                    ctrl.SetButton(btn, kv.Value);

            foreach (var kv in axisStates)
                if (XboxAxes.TryGetValue(kv.Key, out var axis))
                    ctrl.SetAxis(axis, (short)(kv.Value * short.MaxValue));

            foreach (var kv in triggerStates)
                if (XboxTriggers.TryGetValue(kv.Key, out var trig))
                    ctrl.SetTrigger(trig, (byte)(kv.Value * byte.MaxValue));

            foreach (var kv in dpadStates)
                if (XboxButtons.TryGetValue(kv.Key, out var dir))
                    ctrl.SetDPad(dir, kv.Value);

            ctrl.Submit();
        }
    }
}
