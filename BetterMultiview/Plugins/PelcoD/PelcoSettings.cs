using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObsMultiview.Plugins.PelcoD {
    public class PelcoSettings {
        public string ComPort { get; set; }
        public int BaudRate { get; set; } = 9600;
    }

    public class PelcoSlotSettings {
        public byte CameraID { get; set; }
        public byte PresetID { get; set; }
    }
}