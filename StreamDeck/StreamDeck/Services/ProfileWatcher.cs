using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDeck.Data;

namespace StreamDeck.Services
{
    public class ProfileWatcher {
        private readonly ObsWatchService _obs;
        private readonly ProfileManager _profile;

        public event Action<UserProfile.DObsProfile> ActiveProfileChanged;

        public UserProfile.DObsProfile ActiveProfile { get; private set; }

        public ProfileWatcher(ObsWatchService obs, ProfileManager profile) {
            _obs = obs;
            _profile = profile;

            _obs.WebSocket.SceneCollectionChanged += (sender, args) => {
                ResolveConfig();
            };

            _profile.ProfileChanged += () => {
                ResolveConfig();
            };
        }

        private void ResolveConfig() {
            if (_profile.ActiveProfile != null) {
                var collection = _obs.WebSocket.GetCurrentSceneCollection();

                var profile = _profile.ActiveProfile.Profiles.FirstOrDefault(x => x.Id.ToLower() == collection.ToLower());

                if (profile == null) {
                    profile = new UserProfile.DObsProfile(collection);
                    _profile.ActiveProfile.Profiles.Add(profile);
                }

                OnActiveProfileChanged(profile);
            } else {
                OnActiveProfileChanged(null);
            }
        }

        protected virtual void OnActiveProfileChanged(UserProfile.DObsProfile obj) {
            ActiveProfile = obj;
            ActiveProfileChanged?.Invoke(obj);
        }
    }
}
