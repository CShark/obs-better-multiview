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
    /// <summary>
    /// Interaction with OBS WebSocket
    /// </summary>
    public class ObsWatchService {
        private readonly Settings _settings;
        private readonly Thread _watcher;
        private readonly OBSWebsocket _socket;

        /// <summary>
        /// Fired after OBS is connected
        /// </summary>
        public event Action ObsConnected;
        /// <summary>
        /// Fired when connection is lost
        /// </summary>
        public event Action ObsDisconnected;
        /// <summary>
        /// Fired after <see cref="ObsConnected"/> has finished
        /// </summary>
        public event Action ObsInitialized;

        /// <summary>
        /// Whether OBS is connected
        /// </summary>
        public bool IsObsConnected => _socket.IsConnected;
        /// <summary>
        /// Whether all initialization calls have been executed after connecting
        /// </summary>
        public bool IsInitialized { get; private set; }

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
            OnObsInitialized();
        }

        protected virtual void OnObsDisconnected() {
            IsInitialized = false;
            ObsDisconnected?.Invoke();
        }

        protected virtual void OnObsInitialized() {
            IsInitialized = true;
            ObsInitialized?.Invoke();
        }
    }
}