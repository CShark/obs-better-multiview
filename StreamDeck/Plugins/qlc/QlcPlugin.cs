using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StreamDeck.Extensions;
using WebSocketSharp;
using WebSocket = WebSocketSharp.WebSocket;
using WebSocketState = WebSocketSharp.WebSocketState;

namespace StreamDeck.Plugins.qlc {
    /// <summary>
    /// A plugin for QLC+
    /// </summary>
    public class QlcPlugin : PluginBase {
        public override string Name => "QLC+";
        public override string Author => "Nathanael Schneider";
        public override string Version => "1.0";

        public override bool HasSettings => true;
        public override bool HasSlotSettings => true;

        private WebSocket _webSocket;
        private Thread _wsConnect;
        private bool _active;
        private QlcSettings _activeSettings;

        private List<FunctionInfo> _functions = new();

        public IReadOnlyList<FunctionInfo> Functions => _functions.AsReadOnly();

        public QlcPlugin() {
        }

        public override void OnEnabled() {
            // QLC+ communicates via Websocket
            Logger.LogInformation("Enabling QLC+");
            var settings = CommandFacade.RequestSettings<QlcSettings>();
            _activeSettings = settings;
            InfoMessage = "";

            if (!IPAddress.TryParse(settings.IP, out var ip)) {
                State = PluginState.Faulted;
                InfoMessage = Localizer.Localize<string>("Qlc", "InvalidIP");
                return;
            }

            _active = true;
            State = PluginState.Warning;
            if (_webSocket != null) _webSocket.OnMessage -= WebSocketOnOnMessage;
            _webSocket = new WebSocket($"ws://{settings.IP}:{settings.Port}/qlcplusWS");
            _webSocket.OnMessage += WebSocketOnOnMessage;

            _wsConnect = new Thread(async () => {
                while (_active) {
                    if (State == PluginState.Disabled) break;
                    State = PluginState.Warning;
                    InfoMessage = Localizer.Localize<string>("Qlc", "ConnectionPending");
                    while (_webSocket.ReadyState != WebSocketState.Open) {
                        try {
                            _webSocket.Connect();
                        } catch (Exception ex) {
                            Thread.Sleep(1000);
                        }
                    }

                    if (State == PluginState.Disabled) break;
                    State = PluginState.Active;
                    InfoMessage = "";
                    FetchInfo();
                    while (_webSocket.ReadyState == WebSocketState.Open) {
                        Thread.Sleep(1000);
                    }
                }
            });
            _wsConnect.IsBackground = true;
            _wsConnect.Start();
        }

        private void WebSocketOnOnMessage(object? sender, MessageEventArgs e) {
            Logger.LogDebug("QLC Message: " + e.Data);
            var data = e.Data.Split('|');

            // Parse function / widget response and populate list
            if (data[0] == "QLC+API" && data.Length > 1) {
                switch (data[1]) {
                    case "getFunctionsList":
                        for (int i = 2; i < data.Length; i += 2) {
                            _functions.Add(new FunctionInfo(data[i], data[i + 1], FunctionType.Function));
                        }

                        break;
                    case "getWidgetsList":
                        for (int i = 2; i < data.Length; i += 2) {
                            _functions.Add(new FunctionInfo(data[i], data[i + 1], FunctionType.Widget));
                        }

                        break;
                }
            } else {
                Logger.LogWarning($"Unknown message {e.Data}");
            }
        }

        public override void OnDisabled() {
            Logger.LogInformation("Disabling QLC+");
            _active = false;
            State = PluginState.Disabled;
            InfoMessage = "";

            try {
                _webSocket.CloseAsync();
            }catch{}
        }

        public void FetchInfo() {
            // Grab a list of all available functions and widgets
            try {
                _functions.Clear();
                _webSocket.Send("QLC+API|getFunctionsList");
                _webSocket.Send("QLC+API|getWidgetsList");
            } catch {
            }
        }

        public override void PausePlugin(bool pause) {
            if (pause == false) {
                // when settings change, check for different IP / port and reload if necessary
                var settings = CommandFacade.RequestSettings<QlcSettings>();
                if (_activeSettings.IP != settings.IP || _activeSettings.Port != settings.Port) {
                    OnDisabled();
                    OnEnabled();
                }
            }
        }


        public override SettingsControl GetGlobalSettings() {
            return new GlobalSettings(CommandFacade);
        }

        public override SettingsControl GetSlotSettings(Guid slot) {
            return new SlotSettings(this, CommandFacade, slot);
        }

        public override void UnapplySlot(Guid slot, Guid? next) {
            var settings = CommandFacade.RequestSlotSetting<QlcSlotSettings>(slot);
            
            // only reset functions that are not referenced in the next slot to avoid jitter
            QlcSlotSettings nextSettings = new QlcSlotSettings();
            if (next != null) {
                nextSettings = CommandFacade.RequestSlotSetting<QlcSlotSettings>(next.Value);
            }

            var fkts = settings.Functions.Where(x => !nextSettings.Functions.Any(y => x.Function == y.Function));

            foreach (var fkt in fkts) {
                SetFkt(fkt.Function, 0);
            }
        }

        public override void ApplySlot(Guid slot) {
            var settings = CommandFacade.RequestSlotSetting<QlcSlotSettings>(slot);

            foreach (var fkt in settings.Functions) {
                SetFkt(fkt.Function, fkt.Value);
            }
        }

        private void SetFkt(FunctionInfo fkt, byte value) {
            switch (fkt.Type) {
                case FunctionType.Function:
                    _webSocket.Send($"QLC+API|setFunctionStatus|{fkt.ID}|{(value > 0 ? 1 : 0)}");
                    break;
                case FunctionType.Widget:
                    _webSocket.Send($"{fkt.ID}|{(value == 0 ? 1 : value)}");
                    break;
            }
        }
    }
}