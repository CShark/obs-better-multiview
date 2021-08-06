using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autofac;
using StreamDeck.Data;
using StreamDeck.Services;

namespace StreamDeck.Controls {
    /// <summary>
    /// Interaktionslogik für SceneSlot.xaml
    /// </summary>
    public partial class SceneSlot : UserControl {
        private readonly UserProfile.DSlot _slot;
        private readonly SceneService _scenes;

        public static readonly DependencyProperty SceneNameProperty = DependencyProperty.Register(
            nameof(SceneName), typeof(string), typeof(SceneSlot),
            new PropertyMetadata(default(string), (o, args) => ((SceneSlot) o).SceneNameChanged()));

        public string SceneName {
            get { return (string) GetValue(SceneNameProperty); }
            set { SetValue(SceneNameProperty, value); }
        }

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

        public SceneSlot(UserProfile.DSlot slot) {
            _slot = slot;
            SceneName = _slot.Obs.Scene;
            _scenes = App.Container.Resolve<SceneService>();

            InitializeComponent();
            Unloaded += (sender, args) => {
                _scenes.PreviewChanged -= ActiveScenesChanged;
                _scenes.LiveChanged -= ActiveScenesChanged;
            };

            _scenes.PreviewChanged += ActiveScenesChanged;
            _scenes.LiveChanged += ActiveScenesChanged;
        }

        private void ActiveScenesChanged(UserProfile.DSlot slot) {
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

        private void SceneNameChanged() {
            Unconfigured = string.IsNullOrEmpty(SceneName);
        }

        private void SceneSlot_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var config = new SlotConfig();
            config.SetSlot(_slot);
            config.Owner = Window.GetWindow(this);
            config.ShowDialog();
        }

        private void SceneSlot_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (!Unconfigured)
                _scenes.ActivatePreview(_slot);
        }
    }
}