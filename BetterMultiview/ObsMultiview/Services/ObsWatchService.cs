using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using ObsMultiview.Data;
using OBSWebsocketDotNet;

namespace ObsMultiview.Services {
    /// <summary>
    /// Interaction with OBS WebSocket
    /// </summary>
    public class ObsWatchService {
        private readonly Settings _settings;
        private readonly Thread _watcher;
        private readonly OBSWebsocket _socket;
        private readonly ILogger _logger;

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
        /// Triggered when the obs connection encounters an error
        /// </summary>
        public event Action<Exception> ObsConnectionError;

        /// <summary>
        /// Whether OBS is connected
        /// </summary>
        public bool IsObsConnected => _socket.IsConnected;
        /// <summary>
        /// Whether all initialization calls have been executed after connecting
        /// </summary>
        public bool IsInitialized { get; private set; }

        public OBSWebsocket WebSocket => _socket;

        public ObsWatchService(Settings settings, ILogger<ObsWatchService> logger) {
            _settings = settings;
            _logger = logger;
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
            _logger.LogDebug("Trying to connect to OBS Websocket...");
            // Try to connect.
            try {
                _socket.ConnectAsync($"ws://{_settings.Connection.IP}:{_settings.Connection.Port}",
                    _settings.Connection.Password);

                while (true) {
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
            } catch (AuthFailureException ex) {
                _logger.LogError(ex,"Wrong credentials");
                OnObsConnectionError(ex);
                Thread.Sleep(10000);
            } catch (Exception ex) {
                _logger.LogError(ex, "Unknown exception with OBS");
                OnObsConnectionError(ex);
                Thread.Sleep(10000);
            }
        }

        protected virtual void OnObsConnected() {
            _logger.LogInformation("OBS Websocket connected");
            ObsConnected?.Invoke();
            OnObsInitialized();
        }

        protected virtual void OnObsDisconnected() {
            _logger.LogError("OBS Websocket connection lost");
            IsInitialized = false;
            ObsDisconnected?.Invoke();
        }

        protected virtual void OnObsInitialized() {
            _logger.LogInformation("OBS Websocket initialized");
            IsInitialized = true;
            ObsInitialized?.Invoke();
        }

        protected virtual void OnObsConnectionError(Exception obj) {
            ObsConnectionError?.Invoke(obj);
        }
    }
}