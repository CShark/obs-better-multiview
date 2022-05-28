using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ObsMultiview.Data;
using ObsMultiview.Plugins;

namespace ObsMultiview.Services {
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
        private readonly ILogger _logger;

        public IReadOnlyCollection<PluginInfo> Plugins => _availablePlugins.AsReadOnly();

        public PluginService(Settings settings, ProfileWatcher profile, ObsWatchService obs,
            ILogger<PluginService> logger) {
            _settings = settings;
            _profile = profile;
            _obs = obs;
            _logger = logger;

            // plugins only get enabled when OBS is connected & finished initializing
            _obs.ObsInitialized += () => {
                _logger.LogInformation("Enabling active plugins");
                App.Current.Dispatcher.Invoke(() => {
                    foreach (var plugin in Plugins.Where(x => x.Active)) {
                        try {
                            plugin.Plugin.OnEnabled();
                        } catch (Exception ex) {
                            _logger.LogError(ex, "Failed to activate plugin " + plugin.Plugin.Name);
                        }
                    }
                });
            };

            _obs.ObsDisconnected += () => {
                _logger.LogInformation("Disabling running plugins");
                App.Current.Dispatcher.Invoke(() => {
                    foreach (var plugin in Plugins.Where(x => x.Active)) {
                        try {
                            plugin.Plugin.OnDisabled();
                        } catch (Exception ex) {
                            _logger.LogWarning(ex, $"Deactivating plugin {plugin.Plugin.Name} threw an exception");
                        }
                    }
                });
            };
        }

        public void PausePlugins(PluginInfo plugin, bool pause) {
            var plugins = _availablePlugins
                .Where(x => x.Active && x.Plugin.State != PluginState.Disabled)
                .Where(x => plugin == null || x == plugin);

            foreach (var p in plugins) {
                try {
                    p.Plugin.PausePlugin(pause);
                } catch (Exception ex) {
                    _logger.LogWarning(ex,$"Failed to set pause state of {p.Plugin.Name} to {pause}");
                }
            }
        }

        /// <summary>
        /// Scan for Plugin implementations
        /// </summary>
        public void Scan() {
            _logger.LogInformation("Scanning for plugins");
            var assembly = Assembly.GetAssembly(typeof(PluginBase));
            ScanAssembly(assembly);
        }

        private void ScanAssembly(Assembly assembly) {
            var logFactory = App.Container.Resolve<ILoggerFactory>();

            foreach (var type in assembly.GetExportedTypes()) {
                if (typeof(PluginBase).IsAssignableFrom(type) && !type.IsAbstract && type.IsPublic) {
                    try {
                        var plugin = Activator.CreateInstance(type) as PluginBase;

                        if (plugin != null) {
                            _logger.LogDebug("Found Plugin " + plugin.Name);

                            if (!_settings.ExperimentalPlugins && plugin.IsExperimental) {
                                _logger.LogInformation("Plugin is in experimental and therefore disabled");
                                continue;
                            }

                            if (_settings.HiddenPlugins.Contains(plugin.Name)) {
                                _logger.LogDebug("Plugin is hidden");
                                continue;
                            }

                            var logger = logFactory.CreateLogger(type);
                            var management = new CommandFacadeBound(plugin, logger);
                            plugin.SetCommandFacade(management, logger);

                            var info = new PluginInfo(plugin);

                            info.PropertyChanged += (sender, args) => {
                                if (info.Active) {
                                    _logger.LogInformation("Activating plugin " + plugin.Name);
                                    _settings.ActivePlugins.Add(plugin.Name);

                                    if (_obs.IsInitialized) {
                                        try {
                                            plugin.OnEnabled();
                                        } catch (Exception ex) {
                                            _logger.LogError(ex, "Failed to activate plugin " + plugin.Name);
                                        }
                                    }
                                } else {
                                    _logger.LogInformation("Deactivating plugin " + plugin.Name);
                                    _settings.ActivePlugins.Remove(plugin.Name);

                                    if (_obs.IsInitialized) {
                                        try {
                                            plugin.OnDisabled();
                                        } catch (Exception ex) {
                                            _logger.LogWarning(ex,
                                                $"Deactivating plugin {plugin.Name} threw an exception");
                                        }
                                    }
                                }
                            };

                            info.Active = _settings.ActivePlugins.Contains(plugin.Name);
                            _availablePlugins.Add(info);
                        } else {
                            _logger.LogWarning("Failed to activate plugin " + type.FullName);
                        }
                    } catch (Exception ex) {
                        _logger.LogWarning(ex, "Failed to activate plugin " + type.FullName);
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
            private readonly ILogger _logger;

            public CommandFacadeBound(PluginBase plugin, ILogger logger) {
                _plugin = plugin;
                _settings = App.Container.Resolve<Settings>();
                _profile = App.Container.Resolve<ProfileWatcher>();
                _scenes = App.Container.Resolve<SceneService>();
                var factory = App.Container.Resolve<ILoggerFactory>();
                _logger = factory.CreateLogger(plugin.Name + "(CF)");
                Logger = logger;
            }

            public override void ActivateScene(Guid scene) {
                _logger.LogDebug($"Activating scene {scene}");
                _scenes.ActivatePreview(scene);
            }

            public override void SwitchLive() {
                _logger.LogDebug($"{_plugin.Name}: Switching to Live");
                _scenes.SwitchLive();
            }

            protected override JObject RequestSettings(string subtype = null) {
                _logger.LogDebug($"Reading settings for subtype {subtype ?? "(default)"}");
                subtype = string.IsNullOrWhiteSpace(subtype) ? "" : "." + subtype;
                var key = _plugin.Name + subtype;
                if (_settings.PluginSettings.ContainsKey(key)) {
                    return _settings.PluginSettings[key];
                } else {
                    return null;
                }
            }

            protected override void WriteSettings(JObject settings, string subtype = null) {
                _logger.LogDebug($"Writing settings for subtype {subtype ?? "(default)"}");
                subtype = string.IsNullOrWhiteSpace(subtype) ? "" : "." + subtype;
                var key = _plugin.Name + subtype;
                _settings.PluginSettings[key] = settings;
            }

            protected override JObject RequestSlotSetting(Guid guid) {
                _logger.LogDebug($"Requesting slot config for slot {guid}");
                var slot = _profile.ActiveProfile.SceneView.Slots.FirstOrDefault(x => x.Id == guid);

                if (slot != null && slot.PluginConfigs.ContainsKey(_plugin.Name)) {
                    return slot.PluginConfigs[_plugin.Name];
                } else {
                    var set = _profile.ActiveProfile.SceneView.Sets.FirstOrDefault(x => x.Id == guid);

                    if (set != null && set.PluginConfigs.ContainsKey(_plugin.Name)) {
                        return set.PluginConfigs[_plugin.Name];
                    }
                }

                return null;
            }

            protected override IEnumerable<(Guid id, JObject config)> RequestSlotSettings() {
                _logger.LogDebug($"Requesting all slot configs");
                return _profile.ActiveProfile.SceneView.Slots.Select(x => (x.Id,
                        x.PluginConfigs?.FirstOrDefault(y => y.Key == _plugin.Name).Value))
                    .Where(x => x.Value != null);
            }

            protected override void WriteSlotSettings(Guid guid, JObject config) {
                _logger.LogDebug($"Writing slot config for slot {guid}");
                var slot = _profile.ActiveProfile.SceneView.Slots.FirstOrDefault(x => x.Id == guid);

                if (slot != null) {
                    slot.PluginConfigs[_plugin.Name] = config;
                } else {
                    var set = _profile.ActiveProfile.SceneView.Sets.FirstOrDefault(x => x.Id == guid);

                    if (set != null) {
                        set.PluginConfigs[_plugin.Name] = config;
                    }
                }
            }
        }
    }
}