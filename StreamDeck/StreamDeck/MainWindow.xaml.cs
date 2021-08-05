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
using Autofac;
using StreamDeck.Services;

namespace StreamDeck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly ObsWatchService _watcher;
        private StreamView _view;

        public static readonly DependencyProperty ObsRunningProperty = DependencyProperty.Register(
            nameof(ObsRunning), typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));

        public bool ObsRunning {
            get { return (bool) GetValue(ObsRunningProperty); }
            set { SetValue(ObsRunningProperty, value); }
        }

        public static readonly DependencyProperty ProfileManagerProperty = DependencyProperty.Register(
            nameof(ProfileManager), typeof(ProfileManager), typeof(MainWindow), new PropertyMetadata(default(ProfileManager)));

        public ProfileManager ProfileManager {
            get { return (ProfileManager) GetValue(ProfileManagerProperty); }
            set { SetValue(ProfileManagerProperty, value); }
        }

        public static readonly DependencyProperty SelectedProfileProperty = DependencyProperty.Register(
            nameof(SelectedProfile), typeof(string), typeof(MainWindow), new PropertyMetadata(default(string)));

        public string SelectedProfile {
            get { return (string) GetValue(SelectedProfileProperty); }
            set { SetValue(SelectedProfileProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();
            Closed += (sender, args) => _view?.Close();

            ProfileManager = App.Container.Resolve<ProfileManager>();
            ProfileManager.ProfileChanged += () => {
                Dispatcher.Invoke(() => {
                    SelectedProfile = ProfileManager.ActiveProfile?.Name ?? "";
                });
            };

            _watcher = App.Container.Resolve<ObsWatchService>();
            
            _watcher.ObsConnected += () => {
                Dispatcher.Invoke(() => {
                    ObsRunning = true;
                    _view = new StreamView();
                    _view.Show();
                });
            };

            _watcher.ObsDisconnected += () => {
                Dispatcher.Invoke(() => {
                    ObsRunning = false;
                    _view.Close();
                });
            };

            Dispatcher.Invoke(() => {
                SelectedProfile = ProfileManager.ActiveProfile?.Name ?? "";
            });
        }

        private void Profile_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (SelectedProfile != ProfileManager.ActiveProfile?.Name) {
                ProfileManager.LoadProfile(SelectedProfile);
            }
        }
    }
}
