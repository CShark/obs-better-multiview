using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StreamDeck.Data;

namespace StreamDeck.Services {
    /// <summary>
    /// Manage the currently active obs profile
    /// </summary>
    public class ProfileWatcher {
        private readonly ObsWatchService _obs;
        private readonly ProfileManager _profile;
        private readonly ILogger _logger;

        /// <summary>
        /// Fired when the currently active obs profile changes
        /// </summary>
        public event Action<UserProfile.DObsProfile> ActiveProfileChanged;

        /// <summary>
        /// The currently active obs profile
        /// </summary>
        public UserProfile.DObsProfile ActiveProfile { get; private set; }

        public ProfileWatcher(ObsWatchService obs, ProfileManager profile, ILogger<ProfileWatcher> logger) {
            _obs = obs;
            _profile = profile;
            _logger = logger;

            _obs.WebSocket.SceneCollectionChanged += (sender, args) => { ResolveConfig(); };

            _obs.WebSocket.Connected += (sender, args) => { ResolveConfig(); };

            _profile.ProfileChanged += () => { ResolveConfig(); };

            try {
                ResolveConfig();
            } catch {
            }
        }

        /// <summary>
        /// Try to load the current obs profile based on the active scene collection or create a new obs profile
        /// </summary>
        private void ResolveConfig() {
            if (_profile.ActiveProfile != null && _obs.IsObsConnected) {
                _logger.LogInformation("Resolving active scene collection configuration");
                var collection = _obs.WebSocket.GetCurrentSceneCollection();
                _logger.LogDebug($"Currently active scene collection: {collection}");
                
                if (collection == null) {
                    OnActiveProfileChanged(null);
                    return;
                }

                var profile =
                    _profile.ActiveProfile.Profiles.FirstOrDefault(x => x.Id.ToLower() == collection.ToLower());

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
            App.Current.Dispatcher.Invoke(() => {
                ActiveProfile = obj;
                ActiveProfileChanged?.Invoke(obj);
            });
        }
    }
}