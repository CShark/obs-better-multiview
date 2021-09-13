using System.Windows;
using ObsMultiview.Data;

namespace ObsMultiview.Dialogs {
    /// <summary>
    /// Config Dialog for a profile / scene collection
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