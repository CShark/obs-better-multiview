using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObsMultiview.Plugins.PelcoD {
    public class Preset {
        public byte CameraID { get; set; }
        
        public byte PresetID { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }
    }

    public class PelcoSettings {
        public string ComPort { get; set; }
        public int BaudRate { get; set; } = 9600;

        public List<Preset> Presets { get; set; } = new();
    }

    public class PelcoSlotSettings {
        public List<Preset> Presets { get; set; }
    }
}