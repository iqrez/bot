using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core
{
    /// <summary>
    /// Manages loading and saving of <see cref="Profile"/> instances and keeps
    /// track of the currently active profile.
    /// </summary>
    public class ProfileManager
    {
        private readonly string profilesPath;
        private readonly Dictionary<string, Profile> profiles = new();

        /// <summary>Raised when <see cref="CurrentProfile"/> changes.</summary>
        public event EventHandler? ProfileChanged;

        /// <summary>
        /// Create a manager that stores profiles under the given application
        /// subfolder inside the user's roaming AppData.
        /// </summary>
        public ProfileManager(string appName)
        {
            profilesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appName);
            Directory.CreateDirectory(profilesPath);
            LoadProfiles();
        }

        /// <summary>All known profiles.</summary>
        public IEnumerable<Profile> Profiles => profiles.Values;

        /// <summary>The currently active profile.</summary>
        public Profile CurrentProfile { get; private set; } = new();

        private void LoadProfiles()
        {
            profiles.Clear();
            foreach (var file in Directory.GetFiles(profilesPath, "*.json"))
            {
                try
                {
                    var profile = Profile.Load(file);
                    profile.Name = Path.GetFileNameWithoutExtension(file);
                    profiles[profile.Name] = profile;
                }
                catch
                {
                    // ignore malformed profile
                }
            }

            if (!profiles.TryGetValue("Default", out var current))
            {
                current = new Profile { Name = "Default" };
                SaveProfile(current);
                profiles[current.Name] = current;
            }

            CurrentProfile = current;
        }

        /// <summary>Persist a profile to disk.</summary>
        public void SaveProfile(Profile profile)
        {
            string path = Path.Combine(profilesPath, profile.Name + ".json");
            profile.Save(path);
        }

        /// <summary>Switch to the profile with the given name.</summary>
        public bool SetCurrentProfile(string name)
        {
            if (profiles.TryGetValue(name, out var p))
            {
                CurrentProfile = p;
                ProfileChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>Return the profile with the specified name if it exists.</summary>
        public Profile? GetProfile(string name) =>
            profiles.TryGetValue(name, out var p) ? p : null;

        /// <summary>Add a new profile to the manager.</summary>
        public bool AddProfile(Profile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.Name) || profiles.ContainsKey(profile.Name))
                return false;
            profiles[profile.Name] = profile;
            SaveProfile(profile);
            return true;
        }

        /// <summary>Delete a profile by name.</summary>
        public bool DeleteProfile(string name)
        {
            if (!profiles.Remove(name))
                return false;
            string path = Path.Combine(profilesPath, name + ".json");
            if (File.Exists(path))
                File.Delete(path);
            if (CurrentProfile.Name == name)
            {
                CurrentProfile = profiles.Values.FirstOrDefault() ?? new Profile { Name = "Default" };
                ProfileChanged?.Invoke(this, EventArgs.Empty);
            }
            return true;
        }

        /// <summary>Create a duplicate of an existing profile.</summary>
        public bool CloneProfile(string source, string dest)
        {
            if (!profiles.TryGetValue(source, out var p) || profiles.ContainsKey(dest))
                return false;
            var clone = new Profile
            {
                Name = dest,
                Mappings = p.Mappings
                    .Select(m => new InputMapping
                    {
                        Type = m.Type,
                        Code = m.Code,
                        Actions = m.Actions
                            .Select(a => new ControllerAction
                            {
                                Element = a.Element,
                                Target = a.Target,
                                AnalogOptions = a.AnalogOptions == null ? null : new AnalogOptions
                                {
                                    Deadzone = a.AnalogOptions.Deadzone,
                                    Sensitivity = a.AnalogOptions.Sensitivity,
                                    Curve = a.AnalogOptions.Curve
                                }
                            }).ToList()
                    }).ToList()
            };
            profiles[dest] = clone;
            SaveProfile(clone);
            return true;
        }

        /// <summary>Check if a profile with the given name exists.</summary>
        public bool HasProfile(string name) => profiles.ContainsKey(name);
    }
}
