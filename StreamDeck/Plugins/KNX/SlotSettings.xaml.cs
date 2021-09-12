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

namespace StreamDeck.Plugins.KNX {
    /// <summary>
    /// Interaktionslogik für SlotSettings.xaml
    /// </summary>
    public partial class SlotSettings : SlotSettingsControl<KnxSlotSettings> {
        public static readonly DependencyProperty PluginProperty = DependencyProperty.Register(
            nameof(Plugin), typeof(KnxPlugin), typeof(SlotSettings), new PropertyMetadata(default(KnxPlugin)));

        public KnxPlugin Plugin {
            get { return (KnxPlugin) GetValue(PluginProperty); }
            set { SetValue(PluginProperty, value); }
        }

        public SlotSettings(KnxPlugin plugin, CommandFacade commandFacade, Guid slotID) : base(commandFacade, slotID) {
            Plugin = plugin;
            InitializeComponent();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e) {
            var data = ((Button) sender).DataContext as KnxSlotGroup;

            Settings.Groups.Remove(data);
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e) {
            var settings = CommandFacade.RequestSettings<KnxSettings>();
            var dlg = new EditGroups(settings, true);
            dlg.Owner = Window.GetWindow(this);
            if (dlg.ShowDialog() == true) {
                Settings.Groups.Add(new KnxSlotGroup {Group = dlg.SelectedGroup});
            }
        }
    }
}