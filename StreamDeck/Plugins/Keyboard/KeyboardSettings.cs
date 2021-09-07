using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeck.Plugins.Keyboard {
    public class KeyboardSettings {
        public bool MultipleKeyboardSupport { get; set; } = false;

        public bool InterceptKeystrokes { get; set; } = true;

        public string NumberKeyboard { get; set; } = null;
    }

    public class KeyboardCoreSettings {
        public Dictionary<string, string> KeyboardGuid { get; set; } = new();

        public Dictionary<string, string> KeyboardLabels { get; set; } = new();
    }

    public class KeyboardSlotSettings {

    }
}