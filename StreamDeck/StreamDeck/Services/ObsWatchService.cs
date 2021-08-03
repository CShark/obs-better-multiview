using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OBSWebsocketDotNet;
using StreamDeck.Data;

namespace StreamDeck.Services {
    public class ObsWatchService {
        private readonly Settings _settings;
        private readonly Thread _watcher;
        private readonly OBSWebsocket _socket;

        public event Action ObsConnected;
        public event Action ObsDisconnected;

        public bool IsObsConnected => _socket.IsConnected;
        public OBSWebsocket WebSocket => _socket;

        public ObsWatchService(Settings settings) {
            _settings = settings;
            _socket = new OBSWebsocket();
            _watcher = new Thread(Watcher) {IsBackground = true};
            _watcher.Start();
        }

        private void Watcher() {
            while (true) {
                WaitProcess();
            }
        }

        private void WaitProcess() {
            // Try to connect. If it fails 10 times, recheck port
            while (true) {
                _socket.Connect($"ws://{_settings.Connection.IP}:{_settings.Connection.Port}", _settings.Connection.Password);

                if (_socket.IsConnected) {
                    break;
                } else {
                    Thread.Sleep(500);
                }
            }

            if (!_socket.IsConnected) {
                return;
            }

            OnObsConnected();

            //wait till websocket looses connection
            while (_socket.IsConnected) {
                Thread.Sleep(1000);
            }

            OnObsDisconnected();
        }

        protected virtual void OnObsConnected() {
            ObsConnected?.Invoke();
        }

        protected virtual void OnObsDisconnected() {
            ObsDisconnected?.Invoke();
        }
    }
}