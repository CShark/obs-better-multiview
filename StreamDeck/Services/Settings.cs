using System.Collections.Generic;
using Newtonsoft.Json;

namespace StreamDeck.Services {
    [JsonObject(MemberSerialization.OptOut)]
    public class Settings {
        public string MultiviewWindowTitle { get; set; } = "Multiview (Fenstermodus)";

        public string ObsServer { get; set; } = "localhost";

        public int ObsPort { get; set; } = 4444;

        public string ObsPassword { get; set; } = null;

        public string ObsProcess { get; set; } = "obs64";

        public string KeyboardDevice { get; set; } = @"\Device\KeyboardClass0";

        public string GottesdienstPlaylist { get; set; } = "PLCVjOY4dUWqcBJgw8kHL0x4uWFFdp3U_Q";
    }
}