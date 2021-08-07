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
    public class ProfileManager {
        private ObservableCollection<string> _profiles;
        private ReadOnlyObservableCollection<string> _profilesRO;
        private readonly Settings _settings;

        public ReadOnlyObservableCollection<string> Profiles => _profilesRO;

        public UserProfile? ActiveProfile { get; private set; }

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

        public void Refresh() {
            _profiles.Clear();
            var profiles = Directory.EnumerateFiles("Profiles", "*.json", SearchOption.TopDirectoryOnly);
            foreach (var profile in profiles) {
                _profiles.Add(Path.GetFileNameWithoutExtension(profile));
            }
        }

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

        public void SaveProfile() {
            if (ActiveProfile != null) {
                var json = JsonConvert.SerializeObject(ActiveProfile);
                File.WriteAllText(Path.Combine("Profiles", ActiveProfile.Name + ".json"), json);
            }
        }

        protected virtual void OnProfileChanged() {
            ProfileChanged?.Invoke();
        }

        public void DeleteActiveProfile() {
            if (ActiveProfile != null) {
                File.Delete(Path.Combine("Profiles", ActiveProfile.Name + ".json"));
                ActiveProfile = null;
                OnProfileChanged();
            }
        }

        public bool CreateProfile(string name) {
            if (!File.Exists(Path.Combine("Profiles", name + ".json"))) {
                File.Create(Path.Combine("Profiles", name + ".json"));
                SaveProfile();
                ActiveProfile = new UserProfile {Name = name};
                return true;
            }

            return false;
        }

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