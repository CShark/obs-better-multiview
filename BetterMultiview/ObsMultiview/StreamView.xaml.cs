using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using Autofac;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ObsMultiview.Data;
using ObsMultiview.Dialogs;
using ObsMultiview.Services;
using OBSWebsocketDotNet.Types;
using SceneSlot = ObsMultiview.Controls.SceneSlot;

namespace ObsMultiview {
    /// <summary>
    /// Interaktionslogik für StreamView.xaml
    /// </summary>
    public partial class StreamView : Window {
        private readonly Settings _settings;
        private readonly ProfileWatcher _watcher;
        private readonly ObsWatchService _obs;
        private readonly Win32Interop _win32;
        private readonly SceneService _scenes;
        private readonly ILogger _logger;

        private readonly Dictionary<UserProfile.DSlot, SceneSlot> _sceneSlots = new();

        public bool IsClosed { get; private set; }

        public static readonly DependencyProperty PresetNameProperty = DependencyProperty.Register(
            nameof(PresetName), typeof(string), typeof(StreamView), new PropertyMetadata(default(string)));

        public string PresetName {
            get { return (string)GetValue(PresetNameProperty); }
            set { SetValue(PresetNameProperty, value); }
        }

        public StreamView() {
            InitializeComponent();
            Closed += (sender, args) => IsClosed = true;

            _settings = App.Container.Resolve<Settings>();
            _watcher = App.Container.Resolve<ProfileWatcher>();
            _obs = App.Container.Resolve<ObsWatchService>();
            _win32 = App.Container.Resolve<Win32Interop>();
            _scenes = App.Container.Resolve<SceneService>();
            _logger = App.Container.Resolve<ILogger<StreamView>>();

            _watcher.ActiveProfileChanged += SceneCollectionChanged;

            _scenes.PreviewChanged += ObsOnPreviewSceneChanged;

            Unloaded += (sender, args) => {
                _watcher.ActiveProfileChanged -= SceneCollectionChanged;
                _scenes.PreviewChanged -= ObsOnPreviewSceneChanged;
            };

            Loaded += (sender, args) => {
                if (!DesignerProperties.GetIsInDesignMode(this)) {
                    foreach (var window in _win32.GetObsWindows("- multiview")) {
                        if (window.handle != IntPtr.Zero) {
                            _win32.CloseWindow(window.handle);
                        }
                    }

                    SceneCollectionChanged(_watcher.ActiveProfile);
                }
            };

            Activated += (sender, args) => WindowActivated();
            Closed += (sender, args) => Closing();

            KeyDown += (sender, args) => {
                if (args.Key == Key.Escape)
                    Close();
            };

            var monitor = _win32.GetMonitors().ToArray()[_settings.Screen];
            Left = monitor.Offset.X;
            Top = monitor.Offset.Y;
            Width = monitor.Size.X;
            Height = monitor.Size.Y;

            if (LivePreview.Faulted) {
                LivePreview.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Called when this window is activated
        /// </summary>
        private void WindowActivated() {
            // also activate the OBS custom multiview behind this window
            _logger.LogDebug("Activating OBS scene projector");
            var window = _win32.GetObsWindows("- multiview").FirstOrDefault();
            if (window.handle == IntPtr.Zero) {
                _obs.WebSocket.OpenProjector("scene", 0, null, "multiview");
                window = _win32.GetObsWindows("- multiview").FirstOrDefault();
                _win32.HideAltTab(window.handle);

                var monitor = _win32.GetMonitors().ToArray()[_settings.Screen];
                _win32.PlaceWindowOnScreen(window.handle, monitor);
            }

            _win32.ShowWindowBehind(window.handle, this);
        }

        private void Closing() {
            // close the obs multiview as well as this window
            _logger.LogDebug("Closing OBS scene projector");
            var window = _win32.GetObsWindows("- multiview").FirstOrDefault();
            if (window.handle != IntPtr.Zero) {
                _win32.CloseWindow(window.handle);
            }
        }

        private void ObsOnPreviewSceneChanged(Guid dSlot) {
            PrepareObsPreview();
        }

        private void SceneCollectionChanged(UserProfile.DObsProfile profile) {
            // Reconfigure view
            Dispatcher.Invoke(() => PresetName = profile.Id);
            var collection = _obs.WebSocket.GetCurrentSceneCollection();

            Dispatcher.InvokeAsync(() => {
                _logger.LogInformation("Recalculating slot layout");
                SlotGrid.Children.Clear();
                _sceneSlots.Clear();

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

                        _sceneSlots.Add(slot, new SceneSlot(slot, this));
                        SlotGrid.Children.Add(_sceneSlots[slot]);
                    }
                }

                var invalid = profile.SceneView.Slots.Where(x => !_sceneSlots.Keys.Contains(x)).ToList();

                foreach (var inv in invalid) {
                    profile.SceneView.Slots.Remove(inv);
                }

                if (_scenes.ActivePreviewSlot != null) {
                    if (!_sceneSlots.ContainsKey(_scenes.ActivePreviewSlot)) {
                        _scenes.ClearPreview();
                    }
                }

                if (_scenes.ActiveLiveSlot != null) {
                    if (!_sceneSlots.ContainsKey(_scenes.ActiveLiveSlot)) {
                        _scenes.ClearLive();
                    }
                }
            }).Wait();

            PrepareObsPreview();
            PrepareObsMultiview();
        }

        private void PrepareObsPreview() {
            _logger.LogInformation("Building OBS Preview scene");
            var slot = _scenes.ActivePreviewSlot;

            if (!_obs.WebSocket.GetSceneList().Scenes.Any(x => x.Name == "preview")) {
                _obs.WebSocket.CreateScene("preview");
            }

            var items = _obs.WebSocket.GetSceneItemList("preview").ToList();
            var preview = slot?.Obs.Scene;
            if (items.Count != 1 || items[0].SourceName != preview) {
                foreach (var item in items) {
                    _obs.WebSocket.DeleteSceneItem(new SceneItemStub { SourceName = item.SourceName, ID = item.ItemId },
                        "preview");
                }

                if (!string.IsNullOrEmpty(preview)) {
                    if (_obs.WebSocket.GetSceneList().Scenes.Any(x => x.Name == preview)) {
                        _obs.WebSocket.AddSceneItem("preview", preview);
                    } else {
                        _sceneSlots[slot].IsInvalid = true;
                    }
                }
            }
        }

        public void PrepareObsMultiview() {
            _logger.LogInformation("Building OBS multiview scene");

            if (!_obs.WebSocket.GetSceneList().Scenes.Any(x => x.Name == "multiview")) {
                _obs.WebSocket.CreateScene("multiview");
            }

            //clear items
            var items = _obs.WebSocket.GetSceneItemList("multiview");
            foreach (var item in items) {
                _obs.WebSocket.DeleteSceneItem(new SceneItemStub { SourceName = item.SourceName, ID = item.ItemId },
                    "multiview");
            }

            // some math
            var videoInfo = _obs.WebSocket.GetVideoInfo();
            var offsetTop = videoInfo.BaseHeight / 3f;
            var height = videoInfo.BaseHeight / 3f * 2f;
            var width = (float)videoInfo.BaseWidth;
            var scaleFromCanvas = new Vector2((float)(ActualWidth / (float)videoInfo.BaseWidth),
                (float)(ActualHeight / (float)videoInfo.BaseHeight));

            // Add preview
            {
                var id = _obs.WebSocket.AddSceneItem("multiview", "preview");
                var props = _obs.WebSocket.GetSceneItemProperties(id, "multiview");

                props.Bounds.Height = offsetTop - (10 / scaleFromCanvas.Y);
                props.Bounds.Width = width / 3f - (10 / scaleFromCanvas.X);
                props.Bounds.Type = SceneItemBoundsType.OBS_BOUNDS_SCALE_INNER;

                props.Position.X = (width / 6f) + (5 / scaleFromCanvas.X);
                props.Position.Y = 5 / scaleFromCanvas.Y;

                _obs.WebSocket.SetSceneItemProperties(props, id, "multiview");
            }

            // Add slots
            if (_watcher.ActiveProfile != null) {
                var scenes = _obs.WebSocket.GetSceneList().Scenes.Select(x => x.Name);
                var multiview = scenes.First(x => x == "multiview");

                var rows = _watcher.ActiveProfile.SceneView.Rows;
                var cols = _watcher.ActiveProfile.SceneView.Columns;

                for (var i = 0; i < _watcher.ActiveProfile.SceneView.Slots.Count; i++) {
                    var slot = _watcher.ActiveProfile.SceneView.Slots[i];
                    _sceneSlots[slot].IsInvalid = false;

                    if (slot.Obs.Scene != null) {
                        if (!scenes.Contains(slot.Obs.Scene)) {
                            _sceneSlots[slot].IsInvalid = true;
                            continue;
                        }

                        var id = _obs.WebSocket.AddSceneItem("multiview", slot.Obs.Scene);
                        var props = _obs.WebSocket.GetSceneItemProperties(id, "multiview");

                        props.Position.X = (i % cols) * (width / cols);
                        props.Position.Y = (i / cols) * (height / rows) + offsetTop;

                        props.Bounds.Type = SceneItemBoundsType.OBS_BOUNDS_SCALE_INNER;
                        props.Bounds.Width = width / cols;
                        props.Bounds.Height = height / rows;

                        _obs.WebSocket.SetSceneItemProperties(props, id, "multiview");
                    }
                }
            }

            // check for obs preview
            WindowActivated();
        }

        private void ProfileSettings_OnClick(object sender, RoutedEventArgs e) {
            var settings = JObject.FromObject(_watcher.ActiveProfile.SceneView);
            var id = _watcher.ActiveProfile.Id;

            var config = new ProfileConfig(ReplaceRunningConfig);
            config.Config = _watcher.ActiveProfile.SceneView;
            config.Owner = this;
            if (config.ShowDialog() == true) {
                SceneCollectionChanged(_watcher.ActiveProfile);
            } else {
                if (_watcher.ActiveProfile.Id == id) {
                    _watcher.ActiveProfile.SceneView = settings.ToObject<UserProfile.DSceneViewConfig>();
                }
            }
        }

        private void ReplaceRunningConfig(UserProfile.DSceneViewConfig config) {
            _watcher.ReplaceProfile(config);
        }

        private void ConfigSets_OnClick(object sender, RoutedEventArgs e) {
            if (_watcher?.ActiveProfile == null) return;

            var dlg = new SetEditor();
            dlg.Owner = this;
            dlg.Sets = new ObservableCollection<Set>(_watcher.ActiveProfile.SceneView.Sets ?? new List<Set>());

            if (dlg.ShowDialog() == true) {
                _watcher.ActiveProfile.SceneView.Sets = dlg.Sets?.ToList();

                foreach (var slot in _watcher.ActiveProfile.SceneView.Slots) {
                    if (slot.SetId != null && !dlg.Sets.Any(x=>x.Id == slot.SetId.Value)) {
                        slot.SetId = null;
                    }
                }

                SceneCollectionChanged(_watcher.ActiveProfile);
            }
        }

        private void AbortEnter_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            e.Handled = (e.Key == Key.Enter || e.Key == Key.Return);
        }
    }
}