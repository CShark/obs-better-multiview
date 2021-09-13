using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ObsMultiview.Data;

namespace ObsMultiview.Services {
    /// <summary>
    /// Manage the currently loaded profile file
    /// </summary>
    public class ProfileManager {
        private ObservableCollection<string> _profiles;
        private ReadOnlyObservableCollection<string> _profilesRO;
        private readonly Settings _settings;
        private readonly ILogger _logger;

        /// <summary>
        /// List of all available profiles
        /// </summary>
        public ReadOnlyObservableCollection<string> Profiles => _profilesRO;

        /// <summary>
        /// The currently loaded profile file
        /// </summary>
        public UserProfile? ActiveProfile { get; private set; }

        /// <summary>
        /// Fired when the active profile changes
        /// </summary>
        public event Action ProfileChanged;

        public ProfileManager(Settings settings, ILogger<ProfileManager> logger) {
            _settings = settings;
            _profiles = new ObservableCollection<string>();
            _profilesRO = new ReadOnlyObservableCollection<string>(_profiles);
            _logger = logger;

            if (!Directory.Exists("Profiles")) {
                Directory.CreateDirectory("Profiles");
            }

            Refresh();

            LoadProfile(_settings.LastProfile);
        }

        /// <summary>
        /// Refresh the list of available profiles
        /// </summary>
        public void Refresh() {
            _logger.LogInformation("Updating list of available profiles");
            _profiles.Clear();
            var profiles = Directory.EnumerateFiles("Profiles", "*.json", SearchOption.TopDirectoryOnly);
            foreach (var profile in profiles) {
                _profiles.Add(Path.GetFileNameWithoutExtension(profile));
            }

            _logger.LogDebug("Available profiles: " + string.Join(',', _profiles));
        }

        /// <summary>
        /// Load a specific profile
        /// </summary>
        /// <param name="name">Name of the profile</param>
        public void LoadProfile(string name) {
            _logger.LogInformation($"Loading profile {name}");
            SaveProfile();

            if (_profiles.Contains(name)) {
                _settings.LastProfile = name;
                var profile = File.ReadAllText(Path.Combine("Profiles", name + ".json"));
                ActiveProfile = JsonConvert.DeserializeObject<UserProfile>(profile);
                if (ActiveProfile == null) ActiveProfile = new UserProfile();

                ActiveProfile.Name = name;
                OnProfileChanged();
            }
        }

        /// <summary>
        /// Save changes to the active profile back into the file
        /// </summary>
        public void SaveProfile() {
            if (ActiveProfile != null) {
                _logger.LogInformation("Saving active profile");
                var json = JsonConvert.SerializeObject(ActiveProfile, Formatting.Indented);
                File.WriteAllText(Path.Combine("Profiles", ActiveProfile.Name + ".json"), json);
            }
        }

        protected virtual void OnProfileChanged() {
            ProfileChanged?.Invoke();
        }

        /// <summary>
        /// Deletes the active profile file and unloads the profile
        /// </summary>
        public void DeleteActiveProfile() {
            if (ActiveProfile != null) {
                _logger.LogInformation("Deleting active profile");
                File.Delete(Path.Combine("Profiles", ActiveProfile.Name + ".json"));
                ActiveProfile = null;
                OnProfileChanged();
            }
        }

        /// <summary>
        /// Create a new empty profile
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <returns></returns>
        public bool CreateProfile(string name) {
            _logger.LogInformation($"Creating new profile {name}");
            if (!File.Exists(Path.Combine("Profiles", name + ".json"))) {
                using (var file = new FileStream(Path.Combine("Profiles", name + ".json"), FileMode.Create)) {
                }
                SaveProfile();
                Refresh();
                ActiveProfile = new UserProfile {Name = name};
                return true;
            }

            _logger.LogError($"Profile could not be created");

            return false;
        }

        /// <summary>
        /// Try to rename the currently active profile
        /// </summary>
        /// <param name="name">The new name of the profile</param>
        /// <returns></returns>
        public bool RenameActiveProfile(string name) {
            if (ActiveProfile != null) {
                _logger.LogInformation($"Renaming active profile to {name}");
                if (!File.Exists(Path.Combine("Profiles", name + ".json"))) {
                    File.Move(Path.Combine("Profiles", ActiveProfile.Name + ".json"),
                        Path.Combine("Profiles", name + ".json"));
                    _profiles.Remove(ActiveProfile.Name);
                    _profiles.Add(name);
                    ActiveProfile.Name = name;
                    _settings.LastProfile = name;
                    return true;
                }
                _logger.LogError($"Renaming profile failed");
            }

            return false;
        }
    }
}