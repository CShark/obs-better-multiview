using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace ObsMultiview.Data {
    public class Set : INotifyPropertyChanged, IPluginSettingsProvider {
        private Color _color;
        public string Name { get; set; }

        /// <summary>
        /// Plugin configs
        /// </summary>
        public Dictionary<string, JObject> PluginConfigs { get; set; } = new();

        public void SetPluginSettings(string pluginId, JObject pluginSettings) {
            if (pluginSettings == null) {
                PluginConfigs.Remove(pluginId);
            } else {
                PluginConfigs[pluginId] = pluginSettings;
            }
        }

        public Guid? Id { get; set; }

        Guid IPluginSettingsProvider.Id => Id ?? Guid.Empty;

        public Color Color {
            get => _color;
            set {
                if (value.Equals(_color)) return;
                _color = value;
                OnPropertyChanged();
            }
        }

        public Set() {
            Id = Guid.NewGuid();
            PluginConfigs = new Dictionary<string, JObject>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public JObject GetPluginSettings(string pluginId) {
            if (PluginConfigs.ContainsKey(pluginId)) {
                return PluginConfigs[pluginId];
            } else {
                return null;
            }
        }
    }
}