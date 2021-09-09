using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using StreamDeck.Data;

namespace StreamDeck.Services {
    /// <summary>
    /// Manage the currently loaded profile file
    /// </summary>
    public class ProfileManager {
        private ObservableCollection<string> _profiles;
        private ReadOnlyObservableCollection<string> _profilesRO;
        private readonly Settings _settings;

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

        public ProfileManager(Settings settings) {
            _settings = settings;
            _profiles = new ObservableCollection<string>();
            _profilesRO = new ReadOnlyObservableCollection<string>(_profiles);

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
            _profiles.Clear();
            var profiles = Directory.EnumerateFiles("Profiles", "*.json", SearchOption.TopDirectoryOnly);
            foreach (var profile in profiles) {
                _profiles.Add(Path.GetFileNameWithoutExtension(profile));
            }
        }

        /// <summary>
        /// Load a specific profile
        /// </summary>
        /// <param name="name">Name of the profile</param>
        public void LoadProfile(string name) {
            SaveProfile();

            if (_profiles.Contains(name)) {
                _settings.LastProfile = name;
                var profile = File.ReadAllText(Path.Combine("Profiles", name + ".json"));
                ActiveProfile = JsonConvert.DeserializeObject<UserProfile>(profile);
                ActiveProfile.Name = name;
                OnProfileChanged();
            }
        }

        /// <summary>
        /// Save changes to the active profile back into the file
        /// </summary>
        public void SaveProfile() {
            if (ActiveProfile != null) {
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
            if (!File.Exists(Path.Combine("Profiles", name + ".json"))) {
                File.Create(Path.Combine("Profiles", name + ".json"));
                SaveProfile();
                ActiveProfile = new UserProfile {Name = name};
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to rename the currently active profile
        /// </summary>
        /// <param name="name">The new name of the profile</param>
        /// <returns></returns>
        public bool RenameActiveProfile(string name) {
            if (ActiveProfile != null) {
                if (!File.Exists(Path.Combine("Profiles", name + ".json"))) {
                    File.Move(Path.Combine("Profiles", ActiveProfile.Name + ".json"),
                        Path.Combine("Profiles", name + ".json"));
                    _profiles.Remove(ActiveProfile.Name);
                    _profiles.Add(name);
                    ActiveProfile.Name = name;
                    _settings.LastProfile = name;
                    return true;
                }
            }

            return false;
        }
    }
}