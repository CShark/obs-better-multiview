﻿using System;
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
        private readonly UserProfile.DSlot _slot;
        private readonly SceneService _scenes;
        private readonly StreamView _owner;
        private readonly PluginService _plugins;

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

        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            nameof(Name), typeof(string), typeof(SceneSlot), new PropertyMetadata(default(string)));

        public string Name {
            get { return (string) GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public SceneSlot(UserProfile.DSlot slot, StreamView owner) {
            _slot = slot;
            _owner = owner;
            _scenes = App.Container.Resolve<SceneService>();
            _plugins = App.Container.Resolve<PluginService>();
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
        }

        private void SceneSlot_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var config = new SlotConfig(_slot);
            config.Owner = Window.GetWindow(this);

            _plugins.PausePlugins(null, true);

            if (config.ShowDialog() == true) {
                LoadSlot();
                _owner.PrepareObsMultiview();
            }

            _plugins.PausePlugins(null, false);
        }

        private void SceneSlot_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (!Unconfigured)
                _scenes.ActivatePreview(_slot.Id);
        }

        private void SceneSlot_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (!Unconfigured) {
                _scenes.SwitchLive();
                e.Handled = true;
            }
        }
    }
}