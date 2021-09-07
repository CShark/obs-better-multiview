using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StreamDeck.Annotations;
using StreamDeck.Data;
using StreamDeck.Plugins;

namespace StreamDeck.Services {
    public class PluginInfo : INotifyPropertyChanged {
        private bool _active;
        public PluginBase Plugin { get; }

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

    public class PluginService {
        private List<PluginInfo> _availablePlugins = new();
        private readonly Settings _settings;

        public IReadOnlyCollection<PluginInfo> Plugins => _availablePlugins.AsReadOnly();

        public PluginService(Settings settings) {
            _settings = settings;
        }

        public void Scan() {
            var assembly = Assembly.GetAssembly(typeof(PluginBase));
            ScanAssembly(assembly);
        }

        private void ScanAssembly(Assembly assembly) {
            foreach (var type in assembly.GetExportedTypes()) {
                if (typeof(PluginBase).IsAssignableFrom(type) && !type.IsAbstract) {
                    try {
                        var plugin = Activator.CreateInstance(type) as PluginBase;
                        var management = new PluginManagement(s => {
                            s = string.IsNullOrWhiteSpace(s) ? "" : "." + s;
                            var key = plugin.Name + s;
                            if (_settings.PluginSettings.ContainsKey(key)) {
                                return _settings.PluginSettings[key];
                            } else {
                                return null;
                            }
                        }, (s, o) => {
                            s = string.IsNullOrWhiteSpace(s) ? "" : "." + s;
                            var key = plugin.Name + s;
                            _settings.PluginSettings[key] = o;
                        });

                        plugin.SetPluginManagement(management);

                        if (plugin != null) {
                            var info = new PluginInfo(plugin);
                            
                            info.PropertyChanged += (sender, args) => {
                                var obj = sender as PluginInfo;
                                if (obj.Active) {
                                    _settings.ActivePlugins.Add(obj.Plugin.Name);
                                    obj.Plugin.OnEnabled();
                                } else {
                                    _settings.ActivePlugins.Remove(obj.Plugin.Name);
                                    obj.Plugin.OnDisabled();
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
    }
}