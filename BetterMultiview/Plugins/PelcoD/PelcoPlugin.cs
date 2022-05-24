using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ObsMultiview.Plugins.PelcoD {
    public class PelcoPlugin : StatePluginBase {

        public override string Name => "Pelco-D";
        public override string Author => "Nathanael Schneider";
        public override string Version => "1.0";

        public override bool HasSettings => true;
        public override bool HasSlotSettings => true;

        private SerialPort _port;

        public override void OnEnabled() {
            Logger.LogInformation("Enabling Plugin");
            var settings = CommandFacade.RequestSettings<PelcoSettings>();
            _port = new SerialPort(settings.ComPort, settings.BaudRate, Parity.None, 8, StopBits.One);

            try {
                _port.Open();
                State = PluginState.Active;
            } catch (Exception ex) {
                Logger.LogError(ex, "Failed to open port");
                State = PluginState.Faulted;
            }
        }

        public override void OnDisabled() {
            Logger.LogInformation("Disabling Plugin");
            try {
                _port.Close();
            } catch {
            }

            State = PluginState.Disabled;
        }

        public override SettingsControl GetGlobalSettings() {
            return new GlobalSettings(CommandFacade);
        }

        public override SettingsControl GetSlotSettings(Guid slot) {
            return new SlotSettings(CommandFacade, slot);
        }

        public override void ActiveSlotChanged(Guid slot) {
            var settings = CommandFacade.RequestSlotSetting<PelcoSlotSettings>(slot);

            if (settings.Preset != null && settings.Preset.CameraID > 0) {
                byte[] message = new byte[7];
                message[0] = 0xFF;
                message[1] = settings.Preset.CameraID;
                message[2] = 0x00;
                message[3] = 0x07;
                message[4] = 0x00;
                message[5] = settings.Preset.PresetID;
                message[6] = message.Skip(1).Take(5).Aggregate((byte)0, (s, x) => (byte)(s + x));

                _port.Write(message, 0, 7);
            }
        }
    }
}