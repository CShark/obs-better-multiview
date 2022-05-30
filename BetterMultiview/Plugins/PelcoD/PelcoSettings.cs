using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

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

        protected bool Equals(PelcoSlotSettings other) {
            if(Presets == other.Presets) return true;
            if(Presets == null) return false;
            if(other.Presets == null) return false;

            foreach (var preset in Presets) {
                if (!other.Presets.Any(x => x.CameraID == preset.CameraID && x.PresetID == preset.PresetID))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PelcoSlotSettings)obj);
        }

        public override int GetHashCode() {
            return Presets.GetHashCode() * 17;
        }
    }
}