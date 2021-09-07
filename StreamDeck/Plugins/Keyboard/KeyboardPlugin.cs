using Newtonsoft.Json.Linq;

namespace StreamDeck.Plugins.Keyboard {
    public class KeyboardPlugin : PluginBase {
        public override string Name => "Keyboard Shortcuts";
        public override string Author => "Nathanael Schneider";
        public override string Version => "1.0";

        public override bool HasSettings => true;
        public override bool HasSlotSettings => true;

        public override void OnEnabled() {
            State = PluginState.Active;
        }

        public override void OnDisabled() {
            State = PluginState.Disabled;
        }

        public override SettingsControl GetGlobalSettings() {
            return new GlobalSettings(PluginManagement);
        }

        public override SlotSettingsControl GetSlotSettings() {
            return new SlotSettings(PluginManagement);
        }
    }
}