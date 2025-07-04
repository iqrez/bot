using System.Collections.Generic;
using System.Text.Json;

namespace Controller.Core
{
    public class Profile
    {
        public const int CurrentVersion = 1;

        public int Version { get; set; } = CurrentVersion;
        public string Name { get; set; } = "Default";
        public Dictionary<string, string> KeyBindings { get; set; } = new();

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

            if (element.TryGetProperty("KeyBindings", out var bindings) && bindings.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in bindings.EnumerateObject())
                    profile.KeyBindings[prop.Name] = prop.Value.GetString() ?? string.Empty;
            }

            profile.Version = CurrentVersion;
            return profile;
        }
    }
}
