using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using ObsMultiview.Data;

namespace ObsMultiview.Services {
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
                _profile.SaveProfile();

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

        /// <summary>
        /// Swap to slot configs
        /// </summary>
        /// <param name="localSlotId"></param>
        /// <param name="remSlotId"></param>
        public void SwapSlots(Guid slotA, Guid slotB) {
            if (ActiveProfile?.SceneView != null) {
                var idxA = ActiveProfile.SceneView.Slots.FindIndex(x => x.Id == slotA);
                var idxB = ActiveProfile.SceneView.Slots.FindIndex(x => x.Id == slotB);

                if (idxA >= 0 && idxB >= 0) {
                    var sA = ActiveProfile.SceneView.Slots[idxA];
                    var sB = ActiveProfile.SceneView.Slots[idxB];

                    ActiveProfile.SceneView.Slots[idxB] = sA;
                    ActiveProfile.SceneView.Slots[idxA] = sB;
                    OnActiveProfileChanged(ActiveProfile);
                }
            }
        }

        protected virtual void OnActiveProfileChanged(UserProfile.DObsProfile obj) {
            App.Current.Dispatcher.Invoke(() => {
                ActiveProfile = obj;
                ActiveProfileChanged?.Invoke(obj);
            });
        }

        public void ReplaceProfile(UserProfile.DSceneViewConfig config) {
            var newProfile = new UserProfile.DObsProfile(ActiveProfile.Id);
            newProfile.SceneView = config;
            _profile.ActiveProfile.Profiles.Remove(ActiveProfile);
            _profile.ActiveProfile.Profiles.Add(newProfile);
            _profile.SaveProfile();

            OnActiveProfileChanged(newProfile);
        }
    }
}