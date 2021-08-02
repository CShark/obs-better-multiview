using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace StreamDeck.Services {
    public class ProfileSettings {
        private ILogger _log;
        public ProfileSettings(ILogger logger) {
            _log = logger;
            _log.Information("Reading Profile Settings...");
            AudioDevice = Properties.Settings.Default.AudioDevice.Replace("-", "\u2010");

            _log.Debug("Profile parsed: {Profile}", this);
        }

        public string AudioDevice { get; set; } = "Desktop\u2010Audio";

        public override string ToString() {
            return $"{nameof(AudioDevice)}: {AudioDevice}";
        }
    }
}
