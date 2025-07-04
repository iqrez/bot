using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InputToControllerMapper
{
    public enum ControllerType
    {
        Xbox360,
        DS4
    }

    public class AnalogConfig
    {
        public float Deadzone { get; set; }
        public float Sensitivity { get; set; } = 1f;
        public CurveType Curve { get; set; } = CurveType.Linear;
    }

    public class Profile
    {
        public const int CurrentVersion = 2;

        public int Version { get; set; } = CurrentVersion;
        public string Name { get; set; } = "Default";
        public ControllerType ControllerType { get; set; } = ControllerType.Xbox360;
        [JsonPropertyName("KeyBindings")]
        public Dictionary<string, string> Bindings { get; set; } = new();
        public Dictionary<string, AnalogConfig> Analog { get; set; } = new();
        public Dictionary<string, Macro> Macros { get; set; } = new();

        public static Profile Load(string path)
        {
            string json = File.ReadAllText(path);
            return FromJson(json);
        }

        public static Profile FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            return JsonSerializer.Deserialize<Profile>(json, options) ?? new Profile();
        }

        public void Save(string path)
        {
            File.WriteAllText(path, ToJson());
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }
    }
}
