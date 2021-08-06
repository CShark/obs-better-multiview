using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDeck.Data;

namespace StreamDeck.Services {
    public class SceneService {
        private readonly ObsWatchService _obs;
        private readonly ProfileWatcher _profile;

        private UserProfile.DSlot _previewScene;
        private UserProfile.DSlot _liveScene;

        public event Action<UserProfile.DSlot> PreviewChanged;
        public event Action<UserProfile.DSlot> LiveChanged;

        public UserProfile.DSlot ActivePreviewSlot => _previewScene;
        public UserProfile.DSlot ActiveLiveSlot => _liveScene;

        public SceneService(ObsWatchService obs, ProfileWatcher profile) {
            _obs = obs;
            _profile = profile;

            _profile.ActiveProfileChanged += obsProfile => { OnPreviewChanged(null); };
        }

        public void SwitchLive() {
            var temp = _liveScene;

            UnapplyScene(_liveScene, _previewScene);
            OnLiveChanged(_previewScene);
            OnPreviewChanged(temp);
            ApplyScene(_liveScene);
        }

        public void ActivatePreview(UserProfile.DSlot slot) {
            OnPreviewChanged(slot);
        }

        protected virtual void OnPreviewChanged(UserProfile.DSlot obj) {
            _previewScene = obj;
            PreviewChanged?.Invoke(obj);
        }

        protected virtual void OnLiveChanged(UserProfile.DSlot obj) {
            _liveScene = obj;
            LiveChanged?.Invoke(obj);
        }

        private void UnapplyScene(UserProfile.DSlot slot, UserProfile.DSlot next) {
        }

        private void ApplyScene(UserProfile.DSlot slot) {
            if (!string.IsNullOrEmpty(slot?.Obs.Scene)) {
                _obs.WebSocket.SetCurrentScene(slot.Obs.Scene);
            }
        }
    }
}