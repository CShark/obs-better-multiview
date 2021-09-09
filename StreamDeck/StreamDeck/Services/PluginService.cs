using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using StreamDeck.Data;
using StreamDeck.Plugins;

namespace StreamDeck.Services {
    /// <summary>
    /// Contains information about a plugin
    /// </summary>
    public class PluginInfo : INotifyPropertyChanged {
        private bool _active;

        /// <summary>
        /// Plugin reference
        /// </summary>
        public PluginBase Plugin { get; }

        /// <summary>
        /// Whether this Plugin is activated
        /// </summary>
        public bool Active {
            get => _active;
            set {
                if (value == _active) return;
                _active = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PluginInfo(PluginBase plugin) {
            Plugin = plugin;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Manages Plugins and their Activation
    /// </summary>
    public class PluginService {
        private List<PluginInfo> _availablePlugins = new();
        private readonly Settings _settings;
        private readonly ProfileWatcher _profile;
        private readonly ObsWatchService _obs;

        public IReadOnlyCollection<PluginInfo> Plugins => _availablePlugins.AsReadOnly();

        public PluginService(Settings settings, ProfileWatcher profile, ObsWatchService obs) {
            _settings = settings;
            _profile = profile;
            _obs = obs;

            // plugins only get enabled when OBS is connected & finished initializing
            _obs.ObsInitialized += () => {
                App.Current.Dispatcher.Invoke(() => {
                    foreach (var plugin in Plugins.Where(x => x.Active))
                        plugin.Plugin.OnEnabled();
                });
            };

            _obs.ObsDisconnected += () => {
                App.Current.Dispatcher.Invoke(() => {
                    foreach (var plugin in Plugins.Where(x => x.Active))
                        plugin.Plugin.OnDisabled();
                });
            };
        }

        /// <summary>
        /// Scan for Plugin implementations
        /// </summary>
        public void Scan() {
            var assembly = Assembly.GetAssembly(typeof(PluginBase));
            ScanAssembly(assembly);
        }

        private void ScanAssembly(Assembly assembly) {
            foreach (var type in assembly.GetExportedTypes()) {
                if (typeof(PluginBase).IsAssignableFrom(type) && !type.IsAbstract) {
                    try {
                        var plugin = Activator.CreateInstance(type) as PluginBase;
                        var management = new CommandFacadeBound(plugin);

                        plugin.SetCommandFacade(management);

                        if (plugin != null) {
                            var info = new PluginInfo(plugin);

                            info.PropertyChanged += (sender, args) => {
                                var obj = sender as PluginInfo;

                                if (obj.Active) {
                                    _settings.ActivePlugins.Add(obj.Plugin.Name);

                                    if (_obs.IsInitialized) {
                                        plugin.OnEnabled();
                                    }
                                } else {
                                    _settings.ActivePlugins.Remove(obj.Plugin.Name);

                                    if (_obs.IsInitialized) {
                                        plugin.OnDisabled();
                                    }
                                }
                            };

                            info.Active = _settings.ActivePlugins.Contains(plugin.Name);
                            _availablePlugins.Add(info);
                        }
                    } catch {
                    }
                }
            }
        }

        /// <summary>
        /// A custom command handler for plugins to access functionality of the main program
        /// </summary>
        private class CommandFacadeBound : CommandFacade {
            private readonly PluginBase _plugin;
            private readonly Settings _settings;
            private readonly ProfileWatcher _profile;
            private readonly SceneService _scenes;

            public CommandFacadeBound(PluginBase plugin) {
                _plugin = plugin;
                _settings = App.Container.Resolve<Settings>();
                _profile = App.Container.Resolve<ProfileWatcher>();
                _scenes = App.Container.Resolve<SceneService>();
            }

            public override void ActivateScene(Guid scene) {
                _scenes.ActivatePreview(scene);
            }

            public override void SwitchLive() {
                _scenes.SwitchLive();
            }

            protected override JObject RequestSettings(string subtype = null) {
                subtype = string.IsNullOrWhiteSpace(subtype) ? "" : "." + subtype;
                var key = _plugin.Name + subtype;
                if (_settings.PluginSettings.ContainsKey(key)) {
                    return _settings.PluginSettings[key];
                } else {
                    return null;
                }
            }

            protected override void WriteSettings(JObject settings, string subtype = null) {
                subtype = string.IsNullOrWhiteSpace(subtype) ? "" : "." + subtype;
                var key = _plugin.Name + subtype;
                _settings.PluginSettings[key] = settings;
            }

            protected override JObject RequestSlotSetting(Guid guid) {
                var slot = _profile.ActiveProfile.SceneView.Slots.FirstOrDefault(x => x.Id == guid);

                if (slot != null && slot.PluginConfigs != null) {
                    return slot.PluginConfigs.FirstOrDefault(x => x.Key == _plugin.Name).Value;
                } else {
                    return new JObject();
                }
            }

            protected override IEnumerable<(Guid id, JObject config)> RequestSlotSettings() {
                return _profile.ActiveProfile.SceneView.Slots.Select(x => (x.Id,
                        x.PluginConfigs?.FirstOrDefault(y => y.Key == _plugin.Name).Value))
                    .Where(x => x.Value != null);
            }

            protected override void WriteSlotSettings(Guid guid, JObject config) {
                var slot = _profile.ActiveProfile.SceneView.Slots.FirstOrDefault(x => x.Id == guid);

                if (slot != null) {
                    if (slot.PluginConfigs == null)
                        slot.PluginConfigs = new Dictionary<string, JObject>();

                    slot.PluginConfigs[_plugin.Name] = config;
                }
            }
        }
    }
}