using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ObsMultiview.Plugins.KNX {
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

        public SlotSettings(KnxPlugin plugin, CommandFacade commandFacade, Guid? slotID) : base(commandFacade, slotID) {
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

        private void ClearEntry_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var group = ((FrameworkElement) sender).DataContext as KnxSlotGroup;
            group.OnEntry = null;
        }

        private void ClearExit_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var group = ((FrameworkElement)sender).DataContext as KnxSlotGroup;
            group.OnExit = null;
        }
    }
}