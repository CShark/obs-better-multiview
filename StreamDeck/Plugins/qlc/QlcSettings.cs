using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StreamDeck.Plugins.qlc {
    public enum FunctionType {
        Widget,
        Function
    }

    public class FunctionInfo {
        public FunctionType Type { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }

        public FunctionInfo(string id, string name, FunctionType type) {
            Type = type;
            ID = id;
            Name = name;
        }

        public static bool operator ==(FunctionInfo a, FunctionInfo b) {
            return (a is null && b is null) ||
                   (a is not null && b is not null && a.ID == b.ID && a.Type == b.Type);
        }

        public static bool operator !=(FunctionInfo a, FunctionInfo b) {
            return !(a == b);
        }
    }

    public class SlotFunction {
        public byte Value { get; set; } = 255;

        [JsonIgnore]
        public bool Faulted { get; set; } = false;

        public FunctionInfo Function { get; set; }
    }

    public class QlcSettings {
        public string IP { get; set; }
        public int Port { get; set; }
    }

    public class QlcSlotSettings {
        public ObservableCollection<SlotFunction> Functions { get; set; } = new();

        public bool Reset { get; set; } = true;
    }
}