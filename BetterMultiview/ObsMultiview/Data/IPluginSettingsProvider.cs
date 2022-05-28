using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace ObsMultiview.Data {
    public interface IPluginSettingsProvider {
        JObject GetPluginSettings(string pluginId);

        void SetPluginSettings(string pluginId, JObject pluginSettings);

        Guid Id { get; }

        string Name { get; set; }
    }
}
