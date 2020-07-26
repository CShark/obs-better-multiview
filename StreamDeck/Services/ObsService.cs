using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using NAudio.Wave;
using Newtonsoft.Json;
using OBS.WebSocket.NET;
using StreamDeck.Services.Obs;

namespace StreamDeck.Services {
    public class ObsService {
        private Settings _settings;
        private ObsWebSocket _obs;
        public ObsProfile Profile { get; private set; }
        private string _obsSettingsRoot;

        public event Action Disconnected;
        public event Action Connected;
        public event Action ConnectTimeout;

        private Timer _timer;
        private Timer _resetTimer;

        public ObsService(Settings settings) {
            _settings = settings;
            _obsSettingsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "obs-studio\\basic\\scenes");

            _timer = new Timer(state => {
                if (Process.GetProcessesByName(_settings.ObsProcess).Length > 0) {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    SetupObs();
                }
            }, null, 0, 1000);

            _resetTimer = new Timer(state => {
                if (Process.GetProcessesByName(_settings.ObsProcess).Length == 0) {
                    _timer.Change(1000, 1000);
                    _resetTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void SetupObs() {
            _obs = new ObsWebSocket();
            _obs.ProfileChanged += ObsOnProfileChanged;

            for (int i = 0; i < 10; i++) {
                var props = IPGlobalProperties.GetIPGlobalProperties();
                var eps = props.GetActiveTcpListeners();
                foreach (var ep in eps) {
                    if (ep.Port == _settings.ObsPort) {
                        _obs.Connect($"ws://{_settings.ObsServer}:{_settings.ObsPort}", _settings.ObsPassword);
                        Thread.Sleep(500);
                        if (_obs.IsConnected) {
                            break;
                        }
                    }
                }

                if (_obs.IsConnected) {
                    break;
                }

                Thread.Sleep(1000);
            }

            if (_obs.IsConnected) {
                _obs.Disconnected += (sender, args) => OnDisconnected();
                ObsOnConnected(null,null);
            } else {
                OnConnectTimeout();
            }
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
            OnConnected();
        }

        public ObsWebSocketApi Api => _obs?.Api;

        public ObsWebSocket WebSocket => _obs;

        protected virtual void OnDisconnected() {
            _obs = null;
            Disconnected?.Invoke();
            _timer.Change(1000, 1000);
        }

        protected virtual void OnConnected() {
            Connected?.Invoke();
        }

        protected virtual void OnConnectTimeout() {
            _resetTimer.Change(1000, 1000);
            ConnectTimeout?.Invoke();
        }
    }
}