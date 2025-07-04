using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace InputToControllerMapper
{
    public class ProfileChangedEventArgs : EventArgs
    {
        public Profile Profile { get; }
        public ProfileChangedEventArgs(Profile profile) { Profile = profile; }
    }

    public class ProfileManager
    {
        private readonly string profilesPath;
        private readonly Dictionary<string, Profile> profiles = new();
        private Profile activeProfile;

        public event EventHandler<ProfileChangedEventArgs> ProfileChanged;

        public ProfileManager(string path)
        {
            profilesPath = path;
            Directory.CreateDirectory(profilesPath);
            LoadProfiles();
        }

        public IEnumerable<Profile> All => profiles.Values;

        public Profile ActiveProfile => activeProfile;

        private void LoadProfiles()
        {
            profiles.Clear();
            foreach (string file in Directory.GetFiles(profilesPath, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    Profile profile = Profile.FromJson(json);
                    profile.Name = Path.GetFileNameWithoutExtension(file);
                    profiles[profile.Name] = profile;
                    SaveProfile(profile); // ensure up to date
                }
                catch { }
            }

            if (!profiles.TryGetValue("Default", out activeProfile))
            {
                activeProfile = new Profile { Name = "Default" };
                SaveProfile(activeProfile);
                profiles["Default"] = activeProfile;
            }
        }

        public void SaveProfile(Profile profile)
        {
            string path = Path.Combine(profilesPath, profile.Name + ".json");
            string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public bool SetActiveProfile(string name)
        {
            if (profiles.TryGetValue(name, out var p))
            {
                activeProfile = p;
                ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(p));
                return true;
            }
            return false;
        }

        public Profile? GetProfile(string name)
        {
            profiles.TryGetValue(name, out var p);
            return p;
        }

        public void CreateProfile(string name)
        {
            if (profiles.ContainsKey(name))
                throw new InvalidOperationException("Profile already exists");
            var p = new Profile { Name = name };
            profiles[name] = p;
            SaveProfile(p);
        }

        public void DeleteProfile(string name)
        {
            if (!profiles.Remove(name))
                return;
            string path = Path.Combine(profilesPath, name + ".json");
            if (File.Exists(path))
                File.Delete(path);
            if (activeProfile.Name == name)
            {
                activeProfile = profiles.Values.FirstOrDefault() ?? new Profile { Name = "Default" };
                ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(activeProfile));
            }
        }

        public void CloneProfile(string source, string dest)
        {
            if (!profiles.TryGetValue(source, out var p))
                return;
            var clone = new Profile
            {
                Name = dest,
                KeyBindings = new Dictionary<string, string>(p.KeyBindings)
            };
            profiles[dest] = clone;
            SaveProfile(clone);
        }
    }
}
