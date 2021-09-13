using System.Windows;

namespace ObsMultiview.Plugins.KNX {
    /// <summary>
    /// Interaktionslogik für GlobalSettings.xaml
    /// </summary>
    public partial class GlobalSettings : SettingsControl<KnxSettings> {
        public GlobalSettings(CommandFacade management) : base(management) {
            InitializeComponent();
        }

        private void EditGroups_OnClick(object sender, RoutedEventArgs e) {
            var editor = new EditGroups(Settings);
            editor.Owner = Window.GetWindow(this);
            editor.ShowDialog();
        }
    }
}