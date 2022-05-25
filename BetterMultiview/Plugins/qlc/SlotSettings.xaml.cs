using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ObsMultiview.Plugins.qlc {
    /// <summary>
    /// Interaktionslogik für SlotSettings.xaml
    /// </summary>
    public partial class SlotSettings : SlotSettingsControl<QlcSlotSettings> {
        public static readonly DependencyProperty PluginProperty = DependencyProperty.Register(
            nameof(Plugin), typeof(QlcPlugin), typeof(SlotSettings), new PropertyMetadata(default(QlcPlugin)));

        public QlcPlugin Plugin {
            get { return (QlcPlugin)GetValue(PluginProperty); }
            set { SetValue(PluginProperty, value); }
        }

        public SlotSettings(QlcPlugin plugin, CommandFacade commandFacade, Guid slotID) : base(commandFacade, slotID) {
            Plugin = plugin;
            plugin.FetchInfo();
            InitializeComponent();
        }

        public override void FetchSettings() {
            base.FetchSettings();

            foreach (var fkt in Settings.EntryFunctions.Concat(Settings.ExitFunctions)) {
                var orig = Plugin.Functions.FirstOrDefault(x => x == fkt.Function);

                if (orig != null) {
                    fkt.Function.Name = orig.Name;
                } else {
                    fkt.Faulted = true;
                }
            }
        }


        private void DeleteFkt_OnClick(object sender, RoutedEventArgs e) {
            var fkt = ((Button)sender).DataContext as SlotFunction;

            if (fkt != null) {
                // Object instance will only belong to one of those lists, never both
                Settings.EntryFunctions.Remove(fkt);
                Settings.ExitFunctions.Remove(fkt);
            }
        }

        private void AddEntryFkt_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new FunctionSelect(Plugin.Functions);
            if (dlg.ShowDialog() == true) {
                if (dlg.SelectedItem != null) {
                    Settings.EntryFunctions.Add(new SlotFunction { Function = dlg.SelectedItem, Value = 255 });
                }
            }
        }

        private void AddExitFkt_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new FunctionSelect(Plugin.Functions);
            if (dlg.ShowDialog() == true) {
                if (dlg.SelectedItem != null) {
                    Settings.ExitFunctions.Add(new SlotFunction { Function = dlg.SelectedItem, Value = 0 });
                }
            }
        }
    }
}