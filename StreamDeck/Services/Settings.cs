using Newtonsoft.Json;

namespace StreamDeck.Services {
    [JsonObject(MemberSerialization.OptOut)]
    public class Settings {

        public string MultiviewWindowTitle { get; set; } = "Multiview (Fenstermodus)";

        public string ObsServer { get; set; } = "localhost";

        public int ObsPort { get; set; } = 4444;

        public string ObsPassword { get; set; } = null;

        public string KeyboardDevice { get; set; } = @"\Device\KeyboardClass0";

        public string YoutubeOAuthKey { get; set; } = "Gv0y631pi4qeHsR9KQs4BkIq";
    }
}