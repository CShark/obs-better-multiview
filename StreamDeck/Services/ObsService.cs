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
using Serilog;
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

        private ILogger _log;

        public ObsService(Settings settings, ILogger logger) {
            _log = logger;
            _settings = settings;
            _obsSettingsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "obs-studio\\basic\\scenes");

            _timer = new Timer(state => {
                _log.Debug("Waiting for OBS...");
                if (Process.GetProcessesByName(_settings.ObsProcess).Length > 0) {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    SetupObs();
                }
            }, null, 0, 1000);

            _resetTimer = new Timer(state => {
                if (Process.GetProcessesByName(_settings.ObsProcess).Length == 0) {
                    _log.Information("Connection to OBS lost. Retrying...");
                    _timer.Change(1000, 1000);
                    _resetTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void SetupObs() {
            _obs = new ObsWebSocket();
            _obs.ProfileChanged += ObsOnProfileChanged;
            _log.Information("Configuring WebSocket...");

            for (int i = 0; i < 10; i++) {
                _log.Debug("Try {count} of 10...", i);
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
                _log.Information("OBS Connected");
                ObsOnProfileChanged(null, null);
                _obs.Disconnected += (sender, args) => OnDisconnected();
                ObsOnConnected(null,null);
            } else {
                _log.Warning("OBS Connection timeout");
                OnConnectTimeout();
            }
        }

        private void ObsOnProfileChanged(object sender, EventArgs e) {
            var profile = Api.GetCurrentSceneCollection();
            _log.Information("Scene changed to {Profile}", profile);

            var file = Path.Combine(_obsSettingsRoot, $"{profile}.json");

            if (File.Exists(file)) {
                var content = File.ReadAllText(file);
                Profile = JsonConvert.DeserializeObject<ObsProfile>(content);
            } else {
                _log.Error("Could not load scene Setup");
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