using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSCore.CoreAudioAPI;
using Newtonsoft.Json;

namespace StreamDeck.Services.Obs {
    [JsonObject(MemberSerialization.OptOut)]
    public class ObsProfile {
        public AudioDevice AuxAudioDevice1 { get; set; }
        public AudioDevice AuxAudioDevice2 { get; set; }
        public AudioDevice AuxAudioDevice3 { get; set; }
        public AudioDevice AuxAudioDevice4 { get; set; }
        public AudioDevice DesktopAudioDevice1 { get; set; }
        public AudioDevice DesktopAudioDevice2 { get; set; }

        public AudioDevice GetDevice(string name) {
            return GetType().GetProperties()
                .Where(x => x.PropertyType == typeof(AudioDevice))
                .Select(x => x.GetValue(this) as AudioDevice)
                .Where(x => x != null)
                .FirstOrDefault(x => x.Name == name);
        }

        public IEnumerable<string> AvailableDevices() {
            return GetType().GetProperties()
                .Where(x => x.PropertyType == typeof(AudioDevice))
                .Select(x => x.GetValue(this) as AudioDevice)
                .Where(x => x != null)
                .Select(x => x.Name);
        }
    }
}