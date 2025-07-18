#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace InputToControllerMapper
{
    /// <summary>Serializable application configuration.</summary>
    public class Settings
    {
        public string CurrentProfile { get; set; } = "Default";
        public Dictionary<string, string> ProcessProfiles { get; set; } = new Dictionary<string, string>();

        // Advanced/general app options
        public bool StartWithWindows { get; set; } = false;
        public string DefaultProfile { get; set; } = "Default";
        public string Theme { get; set; } = "Light";
        public bool EnableDiagnostics { get; set; } = false;
        public bool CheckForUpdates { get; set; } = true;
    }

    /// <summary>
    /// Helper for reading and persisting <see cref="Settings"/> to disk.
    /// </summary>
    public class SettingsManager
    {
        private readonly string filePath;
        private Settings settings;

        public event EventHandler SettingsChanged;

        public SettingsManager(string path)
        {
            filePath = path;
            Load();
        }

        public Settings Current => settings;

        public void Load()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            else
            {
                settings = new Settings();
                Save();
            }
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public string GetProfileForProcess(string processName)
        {
            return settings.ProcessProfiles.TryGetValue(processName, out var profile) ? profile : settings.CurrentProfile;
        }

        public void SetProfileForProcess(string processName, string profile)
        {
            settings.ProcessProfiles[processName] = profile;
            Save();
        }

        public void SetCurrentProfile(string name)
        {
            settings.CurrentProfile = name;
            Save();
        }

        public string GetProfileForForegroundProcess()
        {
            try
            {
                using Process p = Process.GetCurrentProcess();
                string name = p.ProcessName;
                return GetProfileForProcess(name);
            }
            catch
            {
                return settings.CurrentProfile;
            }
        }
    }
}
