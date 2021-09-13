using System;
using System.Linq;
using KNXLib;
using Microsoft.Extensions.Logging;
using ObsMultiview.Plugins.Extensions;

namespace ObsMultiview.Plugins.KNX {
    /// <summary>
    /// A plugin to talk to a KNX/IP Interface
    /// </summary>
    public class KnxPlugin : PluginBase {
        public override string Name => "KNX";
        public override string Author => "Nathanael Schneider";
        public override string Version => "1.0";

        public override bool HasSettings => true;
        public override bool HasSlotSettings => true;

        private KnxConnection _knx;

        public override void OnEnabled() {
            Logger.LogInformation("Enabling Plugin");
            var settings = CommandFacade.RequestSettings<KnxSettings>();

            try {
                switch (settings.Mode) {
                    case KnxMode.Routing:
                        _knx = new KnxConnectionRouting(settings.IP, settings.Port);
                        break;
                    case KnxMode.Tunneling:
                        _knx = new KnxConnectionTunneling(settings.IP, settings.Port, settings.LocalIP,
                            settings.LocalPort);
                        break;
                }

                _knx.ThreeLevelGroupAddressing = settings.ThreeLevelGroupAdressing;
                _knx.Connect();
            } catch (Exception ex) {
                Logger.LogError(ex, "Failed to enable plugin");
                State = PluginState.Faulted;
                InfoMessage = Localizer.Localize<string>("KNX", "ConnectionFailed");
            }
        }

        public override void OnDisabled() {
            Logger.LogInformation("Disabling plugin");

            try {
                _knx.Disconnect();
            } catch {
            }
        }

        public override void PausePlugin(bool pause) {
            if (pause == false) {
                OnDisabled();
                OnEnabled();
            }
        }

        public override void UnapplySlot(Guid slot, Guid? next) {
            var config = CommandFacade.RequestSlotSetting<KnxSlotSettings>(slot);

            foreach (var group in config.Groups.Where(x => x.OnExit != null && x.OnExit.Length > 0)) {
                _knx.Action(group.Group.GroupAddress, group.OnExit);
            }
        }

        public override void ApplySlot(Guid slot) {
            var config = CommandFacade.RequestSlotSetting<KnxSlotSettings>(slot);

            foreach (var group in config.Groups.Where(x => x.OnEntry != null && x.OnEntry.Length > 0)) {
                _knx.Action(group.Group.GroupAddress, group.OnEntry);
            }
        }

        public override SettingsControl GetGlobalSettings() {
            return new GlobalSettings(CommandFacade);
        }

        public override SettingsControl GetSlotSettings(Guid slot) {
            return new SlotSettings(this, CommandFacade, slot);
        }
    }
}