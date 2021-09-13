using System;

namespace ObsMultiview.Plugins.Keyboard {
    /// <summary>
    /// Interaktionslogik für SlotSettings.xaml
    /// </summary>
    public partial class SlotSettings : SlotSettingsControl<KeyboardSlotSettings> {

        public SlotSettings(CommandFacade commandFacade, Guid slotID) : base(commandFacade, slotID) {
            InitializeComponent();
            Grabber.SetCommandFacade(commandFacade);
        }
    }
}