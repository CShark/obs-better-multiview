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
using System.Windows.Shapes;
using StreamDeck.Plugins;

namespace StreamDeck.Dialogs {
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