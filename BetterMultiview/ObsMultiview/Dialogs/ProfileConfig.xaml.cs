using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
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

        private Action<UserProfile.DSceneViewConfig> _replaceConfig;

        public ProfileConfig(Action<UserProfile.DSceneViewConfig> replaceConfigCallback) {
            _replaceConfig = replaceConfigCallback;
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

        private void Export_OnClick(object sender, RoutedEventArgs e) {
            var sfd = new SaveFileDialog {
                Title="Export multiview config",
                Filter="*.json|*.json"
            };

            if (sfd.ShowDialog() == true) {
                var json = JsonConvert.SerializeObject(Config);
                File.WriteAllText(sfd.FileName, json);
            }
        }

        private void Import_OnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                Title="Import multiview config",
                Filter="*.json|*.json"
            };

            if (ofd.ShowDialog() == true) {
                var json = File.ReadAllText(ofd.FileName);
                var obj = JsonConvert.DeserializeObject<UserProfile.DSceneViewConfig>(json);
                _replaceConfig?.Invoke(obj);
                DialogResult = true;
                Close();
            }
        }
    }
}