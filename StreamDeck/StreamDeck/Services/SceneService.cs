using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDeck.Data;

namespace StreamDeck.Services {
    /// <summary>
    /// Interacts with scenes in OBS
    /// </summary>
    public class SceneService {
        private readonly ObsWatchService _obs;
        private readonly ProfileWatcher _profile;

        private UserProfile.DSlot _previewScene;
        private UserProfile.DSlot _liveScene;

        /// <summary>
        /// Fired when the active preview changes
        /// </summary>
        /// <remarks>Internal only, not synced with OBS because one scene in OBS can be associated with multiple slots</remarks>
        public event Action<Guid> PreviewChanged;

        /// <summary>
        /// Fired when the live scene changes
        /// </summary>
        public event Action<Guid> LiveChanged;

        /// <summary>
        /// Currently active preview slot
        /// </summary>
        public UserProfile.DSlot ActivePreviewSlot => _previewScene;
        /// <summary>
        /// Currently active live slot
        /// </summary>
        public UserProfile.DSlot ActiveLiveSlot => _liveScene;

        public SceneService(ObsWatchService obs, ProfileWatcher profile) {
            _obs = obs;
            _profile = profile;

            _profile.ActiveProfileChanged += obsProfile => { OnPreviewChanged(null); };
        }

        /// <summary>
        /// Switch the preview and live scenes
        /// </summary>
        public void SwitchLive() {
            var temp = _liveScene;

            UnapplyScene(_liveScene, _previewScene);
            OnLiveChanged(_previewScene);
            OnPreviewChanged(temp);
            ApplyScene(_liveScene);
        }

        /// <summary>
        /// Activate a scene for preview based on its guid
        /// </summary>
        /// <param name="id"></param>
        public void ActivatePreview(Guid id) {
            var scene = _profile.ActiveProfile.SceneView.Slots.FirstOrDefault(x => x.Id == id);

            if (scene != null) {
                OnPreviewChanged(scene);
            }
        }

        protected virtual void OnPreviewChanged(UserProfile.DSlot obj) {
            _previewScene = obj;
            PreviewChanged?.Invoke(obj?.Id ?? Guid.Empty);
        }

        protected virtual void OnLiveChanged(UserProfile.DSlot obj) {
            _liveScene = obj;
            LiveChanged?.Invoke(obj?.Id ?? Guid.Empty);
        }

        /// <summary>
        /// Called to unload & clean up in preparation for a scene change
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="next"></param>
        private void UnapplyScene(UserProfile.DSlot slot, UserProfile.DSlot next) {
        }

        /// <summary>
        /// Called to apply a scene config
        /// </summary>
        /// <param name="slot"></param>
        private void ApplyScene(UserProfile.DSlot slot) {
            if (!string.IsNullOrEmpty(slot?.Obs.Scene)) {
                _obs.WebSocket.SetCurrentScene(slot.Obs.Scene);
            }
        }
    }
}