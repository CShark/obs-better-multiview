using System.Windows;
using ObsMultiview.Plugins;

namespace ObsMultiview.Dialogs {
    /// <summary>
    /// Configuration Dialog for global plugin settings
    /// </summary>
    public partial class PluginConfig : Window {

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings), typeof(SettingsControl), typeof(PluginConfig), new PropertyMetadata(default(SettingsControl)));

        public SettingsControl Settings {
            get { return (SettingsControl) GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        public PluginConfig(SettingsControl control) {
            Settings = control;
            InitializeComponent();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}