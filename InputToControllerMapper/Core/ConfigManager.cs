using System;
using System.IO;
using System.Text.Json;

namespace InputToControllerMapper.Core
{
    /// <summary>
    /// Helper utilities for loading and saving JSON configuration files.
    /// </summary>
    public static class ConfigManager
    {
        public static T? Load<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default;
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }

        public static void Save<T>(string path, T data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
