using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StreamDeck.Plugins.Keyboard {
    /// <summary>
    /// Interaktionslogik für SlotSettings.xaml
    /// </summary>
    public partial class SlotSettings : SlotSettingsControl<KeyboardSlotSettings> {

        public SlotSettings(PluginManagement pluginManagement, Guid slotID) : base(pluginManagement, slotID) {
            InitializeComponent();
            Grabber.SetPluginManagement(pluginManagement);
        }
    }
}