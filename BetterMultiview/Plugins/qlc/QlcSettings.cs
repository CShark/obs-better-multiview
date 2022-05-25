using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace ObsMultiview.Plugins.qlc {
    /// <summary>
    /// Type of QLC+ function
    /// </summary>
    public enum FunctionType {
        /// <summary>
        /// Widget as defined in the virtual console
        /// </summary>
        Widget,
        /// <summary>
        /// Function as defined under functions
        /// </summary>
        Function
    }

    public class FunctionInfo {
        /// <summary>
        /// The type of QLC+ function
        /// </summary>
        public FunctionType Type { get; set; }

        /// <summary>
        /// The ID of the function
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Human-Readable Name of the function
        /// </summary>
        public string Name { get; set; }

        public FunctionInfo(string id, string name, FunctionType type) {
            Type = type;
            ID = id;
            Name = name;
        }

        public static bool operator ==(FunctionInfo a, FunctionInfo b) {
            // Functions are uniquely identified by ID & Type. Name might be changed by user
            return (a is null && b is null) ||
                   (a is not null && b is not null && a.ID == b.ID && a.Type == b.Type);
        }

        public static bool operator !=(FunctionInfo a, FunctionInfo b) {
            return !(a == b);
        }
    }

    /// <summary>
    /// Combines a function with metadata
    /// </summary>
    public class SlotFunction {
        /// <summary>
        /// Which value to apply to this function when the slot is active
        /// </summary>
        public byte Value { get; set; } = 255;

        /// <summary>
        /// Whether this function cannot be found anymore
        /// </summary>
        [JsonIgnore]
        public bool Faulted { get; set; } = false;

        /// <summary>
        /// The function to apply the values to
        /// </summary>
        public FunctionInfo Function { get; set; }
    }

    /// <summary>
    /// Settings for the QLC+ WebSocket connection
    /// </summary>
    public class QlcSettings {
        public string IP { get; set; }
        public int Port { get; set; }
    }

    public class QlcSlotSettings {
        /// <summary>
        /// List of functions to apply for this slot on entry
        /// </summary>
        public ObservableCollection<SlotFunction> EntryFunctions { get; set; } = new();

        /// <summary>
        /// List of functions to apply for this slot on exit
        /// </summary>
        public ObservableCollection<SlotFunction> ExitFunctions { get; set; } = new();
    }
}