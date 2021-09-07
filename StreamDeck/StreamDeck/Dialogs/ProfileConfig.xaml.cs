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
using StreamDeck.Data;

namespace StreamDeck.Dialogs {
    /// <summary>
    /// Interaktionslogik für ProfileConfig.xaml
    /// </summary>
    public partial class ProfileConfig : Window {
        public static readonly DependencyProperty ConfigProperty = DependencyProperty.Register(
            nameof(Config), typeof(UserProfile.DSceneViewConfig), typeof(ProfileConfig), new PropertyMetadata(default(UserProfile.DSceneViewConfig)));

        public UserProfile.DSceneViewConfig Config {
            get { return (UserProfile.DSceneViewConfig) GetValue(ConfigProperty); }
            set { SetValue(ConfigProperty, value); }
        }

        public ProfileConfig() {
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