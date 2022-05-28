using System;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autofac;
using ObsMultiview.Data;
using ObsMultiview.Dialogs;
using ObsMultiview.Services;

namespace ObsMultiview.Controls {
    /// <summary>
    /// Control for a single slot in the stream view
    /// </summary>
    public partial class SceneSlot : UserControl {
        private UserProfile.DSlot _slot;
        private readonly SceneService _scenes;
        private readonly StreamView _owner;
        private readonly PluginService _plugins;
        private readonly ProfileWatcher _profile;

        public static readonly DependencyProperty UnconfiguredProperty = DependencyProperty.Register(
            nameof(Unconfigured), typeof(bool), typeof(SceneSlot), new PropertyMetadata(true));

        public bool Unconfigured {
            get { return (bool) GetValue(UnconfiguredProperty); }
            set { SetValue(UnconfiguredProperty, value); }
        }

        public static readonly DependencyProperty ActivePreviewProperty = DependencyProperty.Register(
            nameof(ActivePreview), typeof(bool), typeof(SceneSlot), new PropertyMetadata(default(bool)));

        public bool ActivePreview {
            get { return (bool) GetValue(ActivePreviewProperty); }
            set { SetValue(ActivePreviewProperty, value); }
        }

        public static readonly DependencyProperty ActiveLiveProperty = DependencyProperty.Register(
            nameof(ActiveLive), typeof(bool), typeof(SceneSlot), new PropertyMetadata(default(bool)));

        public bool ActiveLive {
            get { return (bool) GetValue(ActiveLiveProperty); }
            set { SetValue(ActiveLiveProperty, value); }
        }

        public static readonly DependencyProperty IsInvalidProperty = DependencyProperty.Register(
            nameof(IsInvalid), typeof(bool), typeof(SceneSlot), new PropertyMetadata(default(bool)));

        public bool IsInvalid {
            get { return (bool)GetValue(IsInvalidProperty); }
            set { SetValue(IsInvalidProperty, value); }
        }

        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            nameof(Name), typeof(string), typeof(SceneSlot), new PropertyMetadata(default(string)));

        public string Name {
            get { return (string) GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty SlotConfiguringProperty = DependencyProperty.Register(
            nameof(SlotConfiguring), typeof(bool), typeof(SceneSlot), new PropertyMetadata(default(bool)));

        public bool SlotConfiguring {
            get { return (bool) GetValue(SlotConfiguringProperty); }
            set { SetValue(SlotConfiguringProperty, value); }
        }

        public static readonly DependencyProperty SetProperty = DependencyProperty.Register(
            nameof(Set), typeof(Set), typeof(SceneSlot), new PropertyMetadata(default(Set)));

        public Set Set {
            get { return (Set)GetValue(SetProperty); }
            set { SetValue(SetProperty, value); }
        }

        public SceneSlot(UserProfile.DSlot slot, StreamView owner) {
            _slot = slot;
            _owner = owner;
            _scenes = App.Container.Resolve<SceneService>();
            _plugins = App.Container.Resolve<PluginService>();
            _profile = App.Container.Resolve<ProfileWatcher>();
            LoadSlot();

            InitializeComponent();
            Unloaded += (sender, args) => {
                _scenes.PreviewChanged -= ActiveScenesChanged;
                _scenes.LiveChanged -= ActiveScenesChanged;
            };

            _scenes.PreviewChanged += ActiveScenesChanged;
            _scenes.LiveChanged += ActiveScenesChanged;

            ActiveScenesChanged(_slot.Id);
        }

        /// <summary>
        /// Update highlights when active & live changes
        /// </summary>
        /// <param name="slot"></param>
        private void ActiveScenesChanged(Guid slot) {
            Dispatcher.Invoke(() => {
                if (_scenes.ActivePreviewSlot == _slot) {
                    ActivePreview = true;
                } else {
                    ActivePreview = false;
                }

                if (_scenes.ActiveLiveSlot == _slot) {
                    ActiveLive = true;
                } else {
                    ActiveLive = false;
                }
            });
        }

        /// <summary>
        /// Populate settings based on slot config
        /// </summary>
        private void LoadSlot() {
            Unconfigured = string.IsNullOrEmpty(_slot.Obs.Scene);
            Name = _slot.Name;
            Set = _profile.ActiveProfile?.SceneView?.Sets.FirstOrDefault(x => x.Id == _slot.SetId);
        }

        private void SceneSlot_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            if (_profile.ActiveProfile == null) return;

            var config = new SlotConfig(_slot, _profile.ActiveProfile.SceneView);
            config.Owner = Window.GetWindow(this);

            _plugins.PausePlugins(null, true);

            SlotConfiguring = true;
            if (config.ShowDialog() == true) {
                LoadSlot();
                _owner.PrepareObsMultiview();
            }

            SlotConfiguring = false;
            _plugins.PausePlugins(null, false);
        }

        private void SceneSlot_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (!Unconfigured) {
                _scenes.ActivatePreview(_slot.Id);

                var data = new DataObject();
                data.SetData(typeof(SceneSlot), this);
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            }
        }

        private void SceneSlot_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (!Unconfigured) {
                _scenes.SwitchLive();
                e.Handled = true;
            }
        }

        private void SceneSlot_OnDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(SceneSlot))) {
                var data = e.Data.GetData(typeof(SceneSlot));

                if (data is SceneSlot slot && slot != this) {
                    var localSlot = _slot;
                    var remSlot = slot._slot;
                    
                    _profile.SwapSlots(localSlot.Id, remSlot.Id);
                }
            }
        }

        private void SceneSlot_OnDragOver(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(SceneSlot))) {
                var data = e.Data.GetData(typeof(SceneSlot));

                if (data == this) {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
            } else {
                e.Effects = DragDropEffects.None;
                e.Handled = false;
            }
        }
    }
}