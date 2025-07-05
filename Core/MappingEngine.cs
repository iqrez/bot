using System;
using System.Collections.Generic;
using System.Linq;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;

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
            {
                if (Enum.TryParse<Xbox360Button>(kv.Key, true, out var xb))
                    ctrl.SetButton(xb, kv.Value);
                else if (Enum.TryParse<DualShock4Button>(kv.Key, true, out var db))
                    ctrl.SetButton(db, kv.Value);
            }

            foreach (var kv in axisStates)
            {
                if (Enum.TryParse<Xbox360Axis>(kv.Key, true, out var xa))
                    ctrl.SetAxis(xa, (short)(kv.Value * short.MaxValue));
                else if (Enum.TryParse<DualShock4Axis>(kv.Key, true, out var da))
                    ctrl.SetAxis(da, (byte)(kv.Value * byte.MaxValue));
            }

            foreach (var kv in triggerStates)
            {
                if (Enum.TryParse<Xbox360Slider>(kv.Key, true, out var xs))
                    ctrl.SetTrigger(xs, (byte)(kv.Value * byte.MaxValue));
                else if (Enum.TryParse<DualShock4Slider>(kv.Key, true, out var ds))
                    ctrl.SetTrigger(ds, (byte)(kv.Value * byte.MaxValue));
            }

            foreach (var kv in dpadStates)
            {
                if (Enum.TryParse<Xbox360Button>(kv.Key, true, out var xbDir))
                    ctrl.SetDPad(xbDir, kv.Value);
                else if (Enum.TryParse<DualShock4DPadDirection>(kv.Key, true, out var dd))
                    ctrl.SetDPad(dd, kv.Value);
            }

            ctrl.Submit();
        }
    }
}
