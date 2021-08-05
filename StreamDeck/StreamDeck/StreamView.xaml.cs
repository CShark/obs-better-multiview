using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autofac;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using StreamDeck.Controls;
using StreamDeck.Data;
using StreamDeck.Services;
using SceneSlot = StreamDeck.Controls.SceneSlot;

namespace StreamDeck {
    /// <summary>
    /// Interaktionslogik für StreamView.xaml
    /// </summary>
    public partial class StreamView : Window {
        private readonly Settings _settings;
        private readonly ProfileWatcher _watcher;
        private readonly ObsWatchService _obs;

        public StreamView() {
            InitializeComponent();
            _settings = App.Container.Resolve<Settings>();
            _watcher = App.Container.Resolve<ProfileWatcher>();
            _obs = App.Container.Resolve<ObsWatchService>();

            _watcher.ActiveProfileChanged += ProfileChanged;
            _obs.WebSocket.PreviewSceneChanged += ObsOnPreviewSceneChanged;

            Unloaded += (sender, args) => {
                _watcher.ActiveProfileChanged -= ProfileChanged;
                _obs.WebSocket.PreviewSceneChanged -= ObsOnPreviewSceneChanged;
            };

            Loaded += (sender, args) => {
                if (!DesignerProperties.GetIsInDesignMode(this)) {
                    ProfileChanged(_watcher.ActiveProfile);
                }
            };
        }

        private void ObsOnPreviewSceneChanged(OBSWebsocket sender, string newscenename) {
            PrepareObsPreview();
        }

        private void ProfileChanged(UserProfile.DObsProfile profile) {
            var collection = _obs.WebSocket.GetCurrentSceneCollection();

            Dispatcher.InvokeAsync(() => {
                SlotGrid.Children.Clear();

                if (profile != null) {
                    SlotGrid.Rows = profile.SceneView.Rows;
                    SlotGrid.Columns = profile.SceneView.Columns;

                    for (int i = 0; i < SlotGrid.Rows * SlotGrid.Columns; i++) {
                        var slot = new UserProfile.DSlot();
                        if (profile.SceneView.Slots.Count > i) {
                            slot = profile.SceneView.Slots[i];
                        } else {
                            profile.SceneView.Slots.Add(slot);
                        }

                        SlotGrid.Children.Add(new SceneSlot(slot));
                    }
                }
            });

            PrepareObsMultiview();
        }

        private void PrepareObsPreview() {
            if (!_obs.WebSocket.GetSceneList().Scenes.Any(x => x.Name == "preview")) {
                _obs.WebSocket.CreateScene("preview");
            }

            var items = _obs.WebSocket.GetSceneItemList("preview").ToList();
            var preview = _obs.WebSocket.GetPreviewScene();
            if (items.Count != 1 || items[0].SourceName != preview.Name) {
                foreach (var item in items) {
                    _obs.WebSocket.DeleteSceneItem(new SceneItemStub {SourceName = item.SourceName, ID = item.ItemId},
                        "preview");
                }

                if (preview.Name != "preview" && preview.Name != "multiview")
                    _obs.WebSocket.AddSceneItem("preview", preview.Name);
            }
        }

        private void PrepareObsMultiview() {
            if (!_obs.WebSocket.GetSceneList().Scenes.Any(x => x.Name == "multiview")) {
                _obs.WebSocket.CreateScene("multiview");
            }

            PrepareObsPreview();

            //clear items
            var items = _obs.WebSocket.GetSceneItemList("multiview");
            foreach (var item in items) {
                _obs.WebSocket.DeleteSceneItem(new SceneItemStub {SourceName = item.SourceName, ID = item.ItemId},
                    "multiview");
            }

            // some math
            var videoInfo = _obs.WebSocket.GetVideoInfo();
            var offsetTop = videoInfo.BaseHeight / 3f;
            var height = videoInfo.BaseHeight / 3f * 2f;
            var width = (float) videoInfo.BaseWidth;
            var scaleFromCanvas = new Vector2((float) (ActualWidth / (float) videoInfo.BaseWidth),
                (float) (ActualHeight / (float) videoInfo.BaseHeight));

            // Add preview
            {
                _obs.WebSocket.AddSceneItem("multiview", "preview");
                var props = _obs.WebSocket.GetSceneItemProperties("preview", "multiview");

                props.Bounds.Height = offsetTop - (10 / scaleFromCanvas.Y);
                props.Bounds.Width = width / 3f - (10 / scaleFromCanvas.X);
                props.Bounds.Type = SceneItemBoundsType.OBS_BOUNDS_SCALE_INNER;

                props.Position.X = (width / 6f) + (5 / scaleFromCanvas.X);
                props.Position.Y = 5 / scaleFromCanvas.Y;

                _obs.WebSocket.SetSceneItemProperties(props, "multiview");
            }

            // Add slots
            if (_watcher.ActiveProfile != null) {
                var multiview = _obs.WebSocket.GetSceneList().Scenes.First(x => x.Name == "multiview");

                var rows = _watcher.ActiveProfile.SceneView.Rows;
                var cols = _watcher.ActiveProfile.SceneView.Columns;

                for (var i = 0; i < _watcher.ActiveProfile.SceneView.Slots.Count; i++) {
                    var slot = _watcher.ActiveProfile.SceneView.Slots[i];
                    if (slot.Obs.Scene != null) {
                        var id = _obs.WebSocket.AddSceneItem("multiview", slot.Obs.Scene);
                        var props = _obs.WebSocket.GetSceneItemProperties(slot.Obs.Scene, "multiview");

                        props.Position.X = (i % cols) * (width / cols) + (5 / scaleFromCanvas.X);
                        props.Position.Y = (i / rows) * (height / rows) + offsetTop + (5 / scaleFromCanvas.Y);

                        props.Bounds.Type = SceneItemBoundsType.OBS_BOUNDS_SCALE_INNER;
                        props.Bounds.Width = width / cols - (10 / scaleFromCanvas.X);
                        props.Bounds.Height = height / rows - (10 / scaleFromCanvas.Y);

                        _obs.WebSocket.SetSceneItemProperties(props, "multiview");
                    }
                }
            }
        }
    }
}