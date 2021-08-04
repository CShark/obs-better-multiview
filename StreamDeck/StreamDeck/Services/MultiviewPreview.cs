using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using StreamDeck.Data;

namespace StreamDeck.Services {
    public class MultiviewPreview : IDisposable {
        private readonly ProfileWatcher _watcher;
        private readonly ObsWatchService _obs;

        public MultiviewPreview(ObsWatchService obs, ProfileWatcher watcher) {
            _obs = obs;
            _watcher = watcher;
            _watcher.ActiveProfileChanged += WatcherOnActiveProfileChanged;

            InitializeMultiview();
        }

        private void WatcherOnActiveProfileChanged(UserProfile.DObsProfile obj) {
            InitializeMultiview();
        }

        private void InitializeMultiview() {
            if (_obs.IsObsConnected) {
                if (!_obs.WebSocket.GetSceneList().Scenes.Any(x => x.Name == "multiview")) {
                    _obs.WebSocket.CreateScene("multiview");
                }

                // Clear multiview
                var scenes = _obs.WebSocket.GetSceneItemList("multiview");
                foreach (var item in scenes) {
                    _obs.WebSocket.DeleteSceneItem(new SceneItemStub {ID = item.ItemId, SourceName = item.SourceName},
                        "multiview");
                }

                // Gather required scenes
                var sceneList = _watcher.ActiveProfile.SceneView.Slots.Select(x => x.Obs.Scene).Distinct()
                    .Where(x => x != null).ToList();
                var gridSize = 1;
                while (gridSize * gridSize < sceneList.Count) {
                    gridSize++;
                }

                for (var i = 0; i < sceneList.Count; i++) {
                    var scene = sceneList[i];
                    var id = _obs.WebSocket.AddSceneItem("multiview", scene);
                    var props = _obs.WebSocket.GetSceneItemProperties(scene, "multiview");
                    var row = i / gridSize;
                    var column = i % gridSize;

                    props.Position.X = column * props.SourceWidth / (double) gridSize;
                    props.Position.Y = row * props.SourceHeight / (double) gridSize;

                    props.Bounds.Type = SceneItemBoundsType.OBS_BOUNDS_STRETCH;
                    props.Bounds.Width = props.SourceWidth / (double) gridSize;
                    props.Bounds.Height = props.SourceHeight / (double) gridSize;

                    _obs.WebSocket.SetSceneItemProperties(props, "multiview");
                }
            }
        }

        public void Dispose() {
            _watcher.ActiveProfileChanged -= WatcherOnActiveProfileChanged;
        }
    }
}