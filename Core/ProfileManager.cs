using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Controller.Core
{
    public class ProfileChangedEventArgs : EventArgs
    {
        public Profile Profile { get; }
        public ProfileChangedEventArgs(Profile profile) => Profile = profile;
    }

    public class ProfileManager
    {
        private readonly string configDir;
        private readonly string profilesDir;
        private readonly string settingsPath;
        private readonly Dictionary<string, Profile> profiles = new();
        private Profile? current;
        private ProfileSettings settings = new();

        public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;
        public event EventHandler? ProfileListChanged;

        public ProfileManager(string appName)
        {
            configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
            profilesDir = Path.Combine(configDir, "profiles");
            settingsPath = Path.Combine(configDir, "profiles.json");
            Directory.CreateDirectory(profilesDir);
            Load();
        }

        public IEnumerable<Profile> Profiles => profiles.Values;
        public IEnumerable<string> ProfileNames => profiles.Keys;
        public bool HasProfile(string name) => profiles.ContainsKey(name);
        public Profile? GetProfile(string name) => profiles.TryGetValue(name, out var p) ? p : null;
        public Profile CurrentProfile => current ?? throw new InvalidOperationException("No profile loaded");

        private void Load()
        {
            if (File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                settings = JsonSerializer.Deserialize<ProfileSettings>(json) ?? new ProfileSettings();
            }
            else
            {
                settings = new ProfileSettings();
                SaveSettings();
            }

            profiles.Clear();
            foreach (var file in Directory.GetFiles(profilesDir, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var profile = Profile.FromJson(json);
                    profile.Name = Path.GetFileNameWithoutExtension(file);
                    profiles[profile.Name] = profile;
                }
                catch { }
            }

            if (!profiles.TryGetValue(settings.CurrentProfile, out current))
            {
                if (profiles.Count > 0)
                {
                    current = profiles.Values.First();
                    settings.CurrentProfile = current.Name;
                }
                else
                {
                    current = new Profile { Name = settings.CurrentProfile };
                    profiles[current.Name] = current;
                    SaveProfile(current);
                }
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            Directory.CreateDirectory(configDir);
            File.WriteAllText(settingsPath, json);
        }

        public void SaveProfile(Profile profile)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(profile, options);
            File.WriteAllText(Path.Combine(profilesDir, profile.Name + ".json"), json);
        }

        public bool AddProfile(Profile profile)
        {
            if (profiles.ContainsKey(profile.Name))
                return false;
            profiles[profile.Name] = profile;
            SaveProfile(profile);
            ProfileListChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool DeleteProfile(string name)
        {
            if (!profiles.Remove(name))
                return false;
            var file = Path.Combine(profilesDir, name + ".json");
            if (File.Exists(file))
                File.Delete(file);
            if (settings.CurrentProfile == name)
            {
                settings.CurrentProfile = profiles.Keys.FirstOrDefault() ?? "Default";
                SetCurrentProfile(settings.CurrentProfile);
            }
            SaveSettings();
            ProfileListChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool CloneProfile(string sourceName, string newName)
        {
            if (!profiles.TryGetValue(sourceName, out var src) || profiles.ContainsKey(newName))
                return false;
            var clone = JsonSerializer.Deserialize<Profile>(JsonSerializer.Serialize(src)) ?? new Profile();
            clone.Name = newName;
            return AddProfile(clone);
        }

        public bool SetCurrentProfile(string name)
        {
            if (!profiles.TryGetValue(name, out var p))
                return false;
            current = p;
            settings.CurrentProfile = name;
            SaveSettings();
            ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(p));
            return true;
        }

        public void SaveCurrentProfile() => SaveProfile(CurrentProfile);

        public string ResolveProfile(string process, string windowTitle)
        {
            foreach (var rule in settings.AppProfiles)
            {
                if (!string.IsNullOrEmpty(rule.ProcessName) && process.Equals(rule.ProcessName, StringComparison.OrdinalIgnoreCase))
                    return rule.ProfileName;
                if (!string.IsNullOrEmpty(rule.WindowTitleContains) && !string.IsNullOrEmpty(windowTitle) && windowTitle.Contains(rule.WindowTitleContains, StringComparison.OrdinalIgnoreCase))
                    return rule.ProfileName;
            }
            return settings.CurrentProfile;
        }

        public void AutoSwitch(string process, string windowTitle)
        {
            var profile = ResolveProfile(process, windowTitle);
            if (profile != CurrentProfile.Name)
                SetCurrentProfile(profile);
        }

        public void SetProcessProfile(string process, string profileName)
        {
            var rule = settings.AppProfiles.FirstOrDefault(r => r.ProcessName?.Equals(process, StringComparison.OrdinalIgnoreCase) == true);
            if (rule == null)
            {
                rule = new AppProfileRule { ProcessName = process, ProfileName = profileName };
                settings.AppProfiles.Add(rule);
            }
            else
            {
                rule.ProfileName = profileName;
            }
            SaveSettings();
        }

        public void SetWindowProfile(string title, string profileName)
        {
            var rule = settings.AppProfiles.FirstOrDefault(r => r.WindowTitleContains?.Equals(title, StringComparison.OrdinalIgnoreCase) == true);
            if (rule == null)
            {
                rule = new AppProfileRule { WindowTitleContains = title, ProfileName = profileName };
                settings.AppProfiles.Add(rule);
            }
            else
            {
                rule.ProfileName = profileName;
            }
            SaveSettings();
        }
    }

    public class ProfileSettings
    {
        public string CurrentProfile { get; set; } = "Default";
        public List<AppProfileRule> AppProfiles { get; set; } = new();
    }

    public class AppProfileRule
    {
        public string? ProcessName { get; set; }
        public string? WindowTitleContains { get; set; }
        public string ProfileName { get; set; } = "Default";
    }
}
