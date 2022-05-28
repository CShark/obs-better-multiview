using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Autofac;
using JetBrains.Annotations;
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
    public partial class SetConfig : Window {
        private readonly ILogger _logger;

        public static readonly DependencyProperty SetProperty = DependencyProperty.Register(
            nameof(Set), typeof(Set), typeof(SetConfig), new PropertyMetadata(default(Set)));

        public Set Set {
            get { return (Set)GetValue(SetProperty); }
            set { SetValue(SetProperty, value); }
        }

        public static readonly DependencyProperty AvailableScenesProperty = DependencyProperty.Register(
            nameof(AvailableScenes), typeof(ObservableCollection<string>), typeof(SetConfig),
            new PropertyMetadata(default(ObservableCollection<string>)));

        public ObservableCollection<string> AvailableScenes {
            get { return (ObservableCollection<string>)GetValue(AvailableScenesProperty); }
            set { SetValue(AvailableScenesProperty, value); }
        }
        public static readonly DependencyProperty PluginStateProperty = DependencyProperty.Register(
            nameof(PluginState), typeof(Dictionary<string, ObservableBoolean>), typeof(SetConfig),
            new PropertyMetadata(default(Dictionary<string, ObservableBoolean>)));

        public Dictionary<string, ObservableBoolean> PluginState {
            get { return (Dictionary<string, ObservableBoolean>)GetValue(PluginStateProperty); }
            set { SetValue(PluginStateProperty, value); }
        }

        private string _originalConfig;
        private readonly ObsWatchService _obs;
        private readonly PluginService _plugins;
        private readonly List<(PluginBase plugin, SettingsControl settings)> _pluginSettings = new();

        public SetConfig(Set set) {
            _originalConfig = JsonConvert.SerializeObject(set);
            Set = set;

            InitializeComponent();

            _obs = App.Container.Resolve<ObsWatchService>();
            _plugins = App.Container.Resolve<PluginService>();
            _logger = App.Container.Resolve<ILogger<SetConfig>>();

            var scenes = _obs.WebSocket.GetSceneList().Scenes.Select(x => x.Name)
                .Where(x => x != "multiview" && x != "preview");
            AvailableScenes = new ObservableCollection<string>(scenes);
            PluginState = new Dictionary<string, ObservableBoolean>();

            if (Set.PluginConfigs == null)
                Set.PluginConfigs = new Dictionary<string, JObject>();

            // load config controls for all active plugins
            foreach (var plugin in _plugins.Plugins.Where(x => x.Active && x.Plugin.HasSlotSettings && x.Plugin.TriggerType != PluginTriggerType.Trigger)) {
                PluginState.Add(plugin.Plugin.Name, Set.PluginConfigs.ContainsKey(plugin.Plugin.Name));

                var expander = new Expander();

                var title = new CheckBox();
                title.Content = new TextBlock {
                    Text = plugin.Plugin.Name,
                    Style = TryFindResource("Title") as Style,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                title.HorizontalAlignment = HorizontalAlignment.Stretch;
                var bind = new Binding();
                bind.Path = new PropertyPath($"PluginState[{plugin.Plugin.Name}].Value");
                bind.Mode = BindingMode.TwoWay;
                bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(title, ToggleButton.IsCheckedProperty, bind);
                expander.Header = title;
                expander.IsExpanded = PluginState[plugin.Plugin.Name];

                var slotSettings = plugin.Plugin.GetSlotSettings(Set.Id);
                if (slotSettings == null) return;

                slotSettings.FetchSettings();
                slotSettings.Margin = new Thickness(0, 0, 0, 10);
                bind = new Binding();
                bind.Source = this;
                bind.Path = new PropertyPath($"PluginState[{plugin.Plugin.Name}].Value");
                bind.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(slotSettings, UserControl.IsEnabledProperty, bind);

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

            foreach (var plugin in PluginState.Where(x => !x.Value).Select(x => x.Key)) {
                Set.PluginConfigs.Remove(plugin);
            }

            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            JsonConvert.PopulateObject(_originalConfig, Set,
                new() { ObjectCreationHandling = ObjectCreationHandling.Replace });

            Close();
        }

        private void SlotConfig_OnClosing(object sender, CancelEventArgs e) {
            if (DialogResult == null) {
                DialogResult = false;
                JsonConvert.PopulateObject(_originalConfig, Set,
                    new() { ObjectCreationHandling = ObjectCreationHandling.Replace });
            }
        }

        private void Unlink_OnClick(object sender, RoutedEventArgs e) {
            // Reset this slot & delete all configs
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(new UserProfile.DSlot()), Set,
                new() { ObjectCreationHandling = ObjectCreationHandling.Replace });
            DialogResult = true;
            Close();
        }

        public class ObservableBoolean : INotifyPropertyChanged {
            private bool _value;

            public bool Value {
                get => _value;
                set {
                    if (value == _value) return;
                    _value = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public static implicit operator ObservableBoolean(bool value) {
                return new ObservableBoolean { Value = value };
            }

            public static implicit operator bool(ObservableBoolean value) {
                return value.Value;
            }
        }
    }
}