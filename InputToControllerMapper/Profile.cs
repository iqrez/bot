using System.Collections.Generic;
using System.Text.Json;

namespace InputToControllerMapper
{
    public class Profile
    {
        public const int CurrentVersion = 2;

        public int Version { get; set; } = CurrentVersion;
        public string Name { get; set; }
        public Dictionary<string, string> KeyBindings { get; set; } = new Dictionary<string, string>();

        public static Profile FromJson(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return FromJsonElement(doc.RootElement);
        }

        public static Profile FromJsonElement(JsonElement element)
        {
            int version = element.TryGetProperty("Version", out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 1;
            string name = element.TryGetProperty("Name", out var n) ? n.GetString() : "Default";

            var profile = new Profile { Name = name };

            if (version == 1)
            {
                if (element.TryGetProperty("Bindings", out var b) && b.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in b.EnumerateObject())
                        profile.KeyBindings[prop.Name] = prop.Value.GetString();
                }
            }
            else
            {
                if (element.TryGetProperty("KeyBindings", out var b) && b.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in b.EnumerateObject())
                        profile.KeyBindings[prop.Name] = prop.Value.GetString();
                }
            }

            profile.Version = CurrentVersion;
            return profile;
        }
    }
}
