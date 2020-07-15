using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json;

namespace StreamDeck.Services.Obs {
    [JsonObject(MemberSerialization.OptOut)]
    public class AudioDevice {
        [JsonProperty("balance")]
        public float Balance { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("muted")]
        public bool Muted { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("volume")]
        public float Volume { get; set; }

        [JsonProperty("settings")]
        public AudioDeviceSettings Settings { get; set; }

        [JsonIgnore]
        public bool IsOutput => ID.ToLower().Contains("output");

        [JsonObject(MemberSerialization.OptOut)]
        public class AudioDeviceSettings {
            [JsonProperty("device_id")]
            public string DeviceID { get; set; }
        }

        public MMDevice GetDevice() {
            var enumerator = new MMDeviceEnumerator();
            var dataFlow = IsOutput ? DataFlow.Render : DataFlow.Capture;

            if (Settings.DeviceID == "default") {
                return enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia);
            } else {
                return enumerator.GetDevice(Settings.DeviceID);
            }
        }
    }
}