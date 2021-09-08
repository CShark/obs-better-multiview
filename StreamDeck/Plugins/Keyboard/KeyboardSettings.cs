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

        public int SwitchKey { get; set; } = 0;
        public string SwitchKeyboard { get; set; } = "";
    }

    public class KeyboardCoreSettings {
        public Dictionary<string, string> KeyboardGuid { get; set; } = new();

        public Dictionary<string, string> KeyboardLabels { get; set; } = new();
    }

    public class KeyboardSlotSettings {
        public bool NumpadMode { get; set; } = false;
        public int NumpadNumber { get; set; } = 0;
        public int ShortcutKey { get; set; } = 0;
        public string KeyboardId { get; set; } = "";
    }
}