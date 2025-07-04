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
        public const int CurrentVersion = 1;

        public int Version { get; set; } = CurrentVersion;
        public string Name { get; set; } = "Default";
        public List<InputMapping> Mappings { get; set; } = new();

        // --- Legacy support for simple key bindings ---
        [JsonIgnore]
        public Dictionary<string, string> KeyBindings
        {
            get
            {
                var dict = new Dictionary<string, string>();
                foreach (var m in Mappings)
                {
                    if (m.Actions.Count == 1)
                        dict[m.Code] = m.Actions[0].Target;
                }
                return dict;
            }
        }

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

        public static Profile FromJson(string json)
        {
            using var doc = JsonDocument.Parse(json);
            return FromJsonElement(doc.RootElement);
        }

        public static Profile FromJsonElement(JsonElement element)
        {
            var profile = new Profile
            {
                Version = element.TryGetProperty("Version", out var v) && v.ValueKind == JsonValueKind.Number
                    ? v.GetInt32() : CurrentVersion,
                Name = element.TryGetProperty("Name", out var n) ? n.GetString() ?? "Default" : "Default"
            };

            if (element.TryGetProperty("Mappings", out var mappings) && mappings.ValueKind == JsonValueKind.Array)
            {
                foreach (var mapping in mappings.EnumerateArray())
                {
                    try
                    {
                        var im = JsonSerializer.Deserialize<InputMapping>(mapping.GetRawText());
                        if (im != null)
                            profile.Mappings.Add(im);
                    }
                    catch { /* skip corrupt mappings */ }
                }
            }
            // Legacy: Try to read KeyBindings as well
            else if (element.TryGetProperty("KeyBindings", out var bindings) && bindings.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in bindings.EnumerateObject())
                {
                    var im = new InputMapping
                    {
                        Type = InputType.Key,
                        Code = prop.Name,
                        Actions = new List<ControllerAction> {
                            new ControllerAction { Element = ControllerElement.Button, Target = prop.Value.GetString() ?? "" }
                        }
                    };
                    profile.Mappings.Add(im);
                }
            }

            profile.Version = CurrentVersion;
            return profile;
        }
    }
}
