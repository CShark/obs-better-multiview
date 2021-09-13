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
            get { return (QlcPlugin) GetValue(PluginProperty); }
            set { SetValue(PluginProperty, value); }
        }

        public static readonly DependencyProperty SelectedFunctionProperty = DependencyProperty.Register(
            nameof(SelectedFunction), typeof(FunctionInfo), typeof(SlotSettings),
            new PropertyMetadata(default(FunctionInfo)));

        public FunctionInfo SelectedFunction {
            get { return (FunctionInfo) GetValue(SelectedFunctionProperty); }
            set { SetValue(SelectedFunctionProperty, value); }
        }

        public SlotSettings(QlcPlugin plugin, CommandFacade commandFacade, Guid slotID) : base(commandFacade, slotID) {
            Plugin = plugin;
            plugin.FetchInfo();
            InitializeComponent();
        }

        public override void FetchSettings() {
            base.FetchSettings();

            foreach (var fkt in Settings.Functions) {
                var orig = Plugin.Functions.FirstOrDefault(x => x == fkt.Function);

                if (orig != null) {
                    fkt.Function.Name = orig.Name;
                } else {
                    fkt.Faulted = true;
                }
            }
        }

        private void AddFkt_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (SelectedFunction != null) {
                if (!Settings.Functions.Any(x => x.Function == SelectedFunction)) {
                    Settings.Functions.Add(new SlotFunction {Function = SelectedFunction});
                }

                SelectedFunction = null;
            }
        }

        private void DeleteFkt_OnClick(object sender, RoutedEventArgs e) {
            var fkt = ((Button) sender).DataContext as SlotFunction;

            if (fkt != null) {
                Settings.Functions.Remove(fkt);
            }
        }
    }
}