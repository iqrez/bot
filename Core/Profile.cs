using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core
{
    public enum InputType
    {
        Key,
        MouseButton,
        MouseMoveX,
        MouseMoveY,
        Analog
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        X1,
        X2
    }

    public enum ControllerElement
    {
        Button,
        Axis,
        Trigger,
        DPad
    }

    public enum CurveType
    {
        Linear,
        Squared
    }

    public class AnalogOptions
    {
        public float Deadzone { get; set; } = 0f;
        public float Sensitivity { get; set; } = 1f;
        public CurveType Curve { get; set; } = CurveType.Linear;
    }

    public class ControllerAction
    {
        public ControllerElement Element { get; set; }
        public string Target { get; set; } = string.Empty;
        public AnalogOptions? AnalogOptions { get; set; }
    }

    public class InputMapping
    {
        public InputType Type { get; set; }
        public string Code { get; set; } = string.Empty;
        public List<ControllerAction> Actions { get; set; } = new();
    }

    public class Profile
    {
        public string Name { get; set; } = "Default";
        public List<InputMapping> Mappings { get; set; } = new();

        public static Profile Load(string path)
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            return JsonSerializer.Deserialize<Profile>(json, options) ?? new Profile();
        }

        public void Save(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(path, json);
        }
    }
}
