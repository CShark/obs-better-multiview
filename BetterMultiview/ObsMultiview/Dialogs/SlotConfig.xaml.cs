using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ObsMultiview.Data;
using ObsMultiview.Plugins;
using ObsMultiview.Services;

namespace ObsMultiview.Dialogs {
    /// <summary>
    /// Config Dialog for a scene slot
    /// </summary>
    public partial class SlotConfig : Window {
        private readonly ILogger _logger;

        public static readonly DependencyProperty SlotProperty = DependencyProperty.Register(
            nameof(Slot), typeof(UserProfile.DSlot), typeof(SlotConfig),
            new PropertyMetadata(default(UserProfile.DSlot)));

        public UserProfile.DSlot Slot {
            get { return (UserProfile.DSlot) GetValue(SlotProperty); }
            set { SetValue(SlotProperty, value); }
        }

        public static readonly DependencyProperty AvailableScenesProperty = DependencyProperty.Register(
            nameof(AvailableScenes), typeof(ObservableCollection<string>), typeof(SlotConfig),
            new PropertyMetadata(default(ObservableCollection<string>)));

        public ObservableCollection<string> AvailableScenes {
            get { return (ObservableCollection<string>) GetValue(AvailableScenesProperty); }
            set { SetValue(AvailableScenesProperty, value); }
        }

        private string _originalConfig;
        private readonly ObsWatchService _obs;
        private readonly PluginService _plugins;
        private readonly List<(PluginBase plugin, SettingsControl settings)> _pluginSettings = new();

        public SlotConfig(UserProfile.DSlot slot) {
            _originalConfig = JsonConvert.SerializeObject(slot);
            Slot = slot;

            InitializeComponent();

            _obs = App.Container.Resolve<ObsWatchService>();
            _plugins = App.Container.Resolve<PluginService>();
            _logger = App.Container.Resolve<ILogger<SlotConfig>>();

            var scenes = _obs.WebSocket.GetSceneList().Scenes.Select(x => x.Name)
                .Where(x => x != "multiview" && x != "preview");
            AvailableScenes = new ObservableCollection<string>(scenes);

            if (Slot.PluginConfigs == null)
                Slot.PluginConfigs = new Dictionary<string, JObject>();

            // load config controls for all active plugins
            foreach (var plugin in _plugins.Plugins.Where(x => x.Active && x.Plugin.HasSlotSettings)) {
                var expander = new Expander();

                var title = new TextBlock();
                title.Text = plugin.Plugin.Name;
                title.Style = TryFindResource("Title") as Style;
                title.HorizontalAlignment = HorizontalAlignment.Stretch;
                expander.Header = title;

                var slotSettings = plugin.Plugin.GetSlotSettings(slot.Id);
                if (slotSettings == null) return;

                slotSettings.FetchSettings();
                slotSettings.Margin = new Thickness(0, 0, 0, 10);

                _pluginSettings.Add((plugin.Plugin, slotSettings));
                expander.Content = slotSettings;

                ConfigPanel.Children.Add(expander);
            }

            input.Focus();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;

            foreach (var item in _pluginSettings) {
                try {
                    item.settings.WriteSettings();
                } catch (Exception ex) {
                    _logger.LogError(ex, "Failed to write slot settings for " + item.plugin.Name);
                }
            }

            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            JsonConvert.PopulateObject(_originalConfig, Slot);
            Close();
        }

        private void SlotConfig_OnClosing(object sender, CancelEventArgs e) {
            if (DialogResult == null) {
                DialogResult = false;
                JsonConvert.PopulateObject(_originalConfig, Slot);
            }
        }

        private void Unlink_OnClick(object sender, RoutedEventArgs e) {
            // Reset this slot & delete all configs
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(new UserProfile.DSlot()), Slot);
            DialogResult = true;
            Close();
        }
    }
}