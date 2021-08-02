using System.Collections.Generic;
using System.Windows.Input;
using Newtonsoft.Json;
using Serilog;

namespace StreamDeck.Services {
    [JsonObject(MemberSerialization.OptOut)]
    public class Settings {
        private ILogger _log;

        public Settings(ILogger logger) {
            _log = logger;
            _log.Information("Parsing Settings...");

            MultiviewWindowTitle = Properties.Settings.Default.MultiviewWindowTitle;
            ObsServer = Properties.Settings.Default.ObsServer;
            ObsPort = Properties.Settings.Default.ObsPort;
            ObsPassword = Properties.Settings.Default.ObsPassword;
            ObsProcess = Properties.Settings.Default.ObsProcess;
            KeyboardDevice = Properties.Settings.Default.KeyboardDevice;

            _log.Information("Settings parsed: {Settings}", this);
        }

        public string MultiviewWindowTitle { get; set; } = "Multiview (Vollbild)";

        public string ObsServer { get; set; } = "localhost";

        public int ObsPort { get; set; } = 4444;

        public string ObsPassword { get; set; } = null;

        public string ObsProcess { get; set; } = "obs64";

        public string KeyboardDevice { get; set; } = @"\Device\KeyboardClass0";

        public override string ToString() {
            return $"{nameof(MultiviewWindowTitle)}: {MultiviewWindowTitle}, {nameof(ObsServer)}: {ObsServer}, {nameof(ObsPort)}: {ObsPort}, {nameof(ObsPassword)}: {ObsPassword}, {nameof(ObsProcess)}: {ObsProcess}, {nameof(KeyboardDevice)}: {KeyboardDevice}";
        }
    }
}