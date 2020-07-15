using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Newtonsoft.Json;
using OBS.WebSocket.NET;
using StreamDeck.Services.Obs;

namespace StreamDeck.Services {
    public class ObsService {


        private Settings Settings { get; }
        private readonly ObsWebSocket _obs;
        public ObsProfile Profile { get; private set; }
        private string _obsSettingsRoot;

        public ObsService(Settings settings) {
            Settings = settings;

            _obsSettingsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "obs-studio\\basic\\scenes");

            _obs = new ObsWebSocket();
            _obs.Connected += ObsOnConnected;
            _obs.ProfileChanged += ObsOnProfileChanged;

            _obs.Connect($"ws://{settings.ObsServer}:{settings.ObsPort}", Settings.ObsPassword);
        }

        private void ObsOnProfileChanged(object sender, EventArgs e) {
            var profile = Api.GetCurrentProfile();

            var file = Path.Combine(_obsSettingsRoot, $"{profile}.json");

            if (File.Exists(file)) {
                var content = File.ReadAllText(file);
                Profile = JsonConvert.DeserializeObject<ObsProfile>(content);
            }
        }

        private void ObsOnConnected(object sender, EventArgs e) {
            ObsOnProfileChanged(sender, e);
        }

        public ObsWebSocketApi Api => _obs.Api;

        public ObsWebSocket WebSocket => _obs;
    }
}