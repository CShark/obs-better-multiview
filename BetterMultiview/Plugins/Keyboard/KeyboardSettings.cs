using System.Collections.Generic;

namespace ObsMultiview.Plugins.Keyboard {
    /// <summary>
    /// Global settings for the keyboard plugin
    /// </summary>
    public class KeyboardSettings {
        /// <summary>
        /// Support distinguishing between multiple keyboards (i.e. use driver hook)
        /// </summary>
        public bool MultipleKeyboardSupport { get; set; } = false;

        /// <summary>
        /// Whether to intercept keystrokes that are mapped
        /// </summary>
        public bool InterceptKeystrokes { get; set; } = true;

        /// <summary>
        /// Which keyboard to use for numbers when in driver mode
        /// </summary>
        public string NumberKeyboard { get; set; } = null;

        /// <summary>
        /// Key used to switch between live and preview
        /// </summary>
        public int SwitchKey { get; set; } = 0;

        /// <summary>
        /// Keyboard used to switch between live and preview
        /// </summary>
        public string SwitchKeyboard { get; set; } = "";
    }

    /// <summary>
    /// Settings for the KeyboardCore
    /// </summary>
    public class KeyboardCoreSettings {
        /// <summary>
        /// Maps device IDs to more persistent GUIDs
        /// </summary>
        public Dictionary<string, string> KeyboardGuid { get; set; } = new();

        /// <summary>
        /// Map GUIDs to human readable labels
        /// </summary>
        public Dictionary<string, string> KeyboardLabels { get; set; } = new();
    }

    /// <summary>
    /// Settings for a slot
    /// </summary>
    public class KeyboardSlotSettings {
        /// <summary>
        /// Whether this slots reacts to numbers or keyboard shortcuts
        /// </summary>
        public bool NumpadMode { get; set; } = false;

        /// <summary>
        /// The number associated with this slot
        /// </summary>
        public int NumpadNumber { get; set; } = 0;

        /// <summary>
        /// The shortcut key for this slot
        /// </summary>
        public int ShortcutKey { get; set; } = 0;

        /// <summary>
        /// The keyboard guid for this slot
        /// </summary>
        public string KeyboardId { get; set; } = "";
    }
}