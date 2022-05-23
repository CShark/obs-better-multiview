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

            LoadProfile();
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
        public void LoadProfile() {
            _logger.LogInformation($"Loading profile default");

            try {
                var profile = File.ReadAllText(Path.Combine("Profiles", "default.json"));
                ActiveProfile = JsonConvert.DeserializeObject<UserProfile>(profile);
            }catch(FileNotFoundException){}

            if (ActiveProfile == null) ActiveProfile = new UserProfile();
            ActiveProfile.Name = "default";
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
    }
}