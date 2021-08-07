using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using StreamDeck.Data;
using StreamDeck.Dialogs;
using StreamDeck.Services;

namespace StreamDeck {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly ObsWatchService _watcher;
        private readonly Settings _settings;
        private readonly Win32Interop _win32;
        private StreamView _view;

        public static readonly DependencyProperty ObsRunningProperty = DependencyProperty.Register(
            nameof(ObsRunning), typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));

        public bool ObsRunning {
            get { return (bool) GetValue(ObsRunningProperty); }
            set { SetValue(ObsRunningProperty, value); }
        }

        public static readonly DependencyProperty ProfileManagerProperty = DependencyProperty.Register(
            nameof(ProfileManager), typeof(ProfileManager), typeof(MainWindow),
            new PropertyMetadata(default(ProfileManager)));

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

        public static readonly DependencyProperty ScreensProperty = DependencyProperty.Register(
            nameof(Screens), typeof(ObservableCollection<MonitorInfo>), typeof(MainWindow), new PropertyMetadata(default(ObservableCollection<MonitorInfo>)));

        public ObservableCollection<MonitorInfo> Screens {
            get { return (ObservableCollection<MonitorInfo>) GetValue(ScreensProperty); }
            set { SetValue(ScreensProperty, value); }
        }

        public static readonly DependencyProperty ActiveScreenProperty = DependencyProperty.Register(
            nameof(ActiveScreen), typeof(int), typeof(MainWindow), new PropertyMetadata(default(int)));

        public int ActiveScreen {
            get { return (int) GetValue(ActiveScreenProperty); }
            set { SetValue(ActiveScreenProperty, value); }
        }

        public MainWindow() {
            InitializeComponent();
            Closed += (sender, args) => _view?.Close();

            ProfileManager = App.Container.Resolve<ProfileManager>();
            ProfileManager.ProfileChanged += () => {
                Dispatcher.Invoke(() => { SelectedProfile = ProfileManager.ActiveProfile?.Name ?? ""; });
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

            Dispatcher.Invoke(() => { SelectedProfile = ProfileManager.ActiveProfile?.Name ?? ""; });

            _settings = App.Container.Resolve<Settings>();
            _win32 = App.Container.Resolve<Win32Interop>();
            ActiveScreen = _settings.Screen;
            Screens = new ObservableCollection<MonitorInfo>(_win32.GetMonitors());
        }

        private void Profile_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (SelectedProfile != ProfileManager.ActiveProfile?.Name) {
                ProfileManager.LoadProfile(SelectedProfile);
            }
        }

        private void DeleteProfile_OnClick(object sender, RoutedEventArgs e) {
            if (SelectedProfile != null) {
                ProfileManager.DeleteActiveProfile();
            }
        }

        private void CreateProfile_OnClick(object sender, RoutedEventArgs e) {
            var input = new TextInput("Neues Profil anlegen", "Bitte gib einen Namen für das neue Profil ein:");
            input.Owner = this;

            if (input.ShowDialog() == true) {
                ProfileManager.CreateProfile(input.Value);
            }
        }

        private void RenameProfile_OnClick(object sender, RoutedEventArgs e) {
            if (SelectedProfile != null) {
                var input = new TextInput("Profil umbenennen", "Gib einen neuen Namen für das Profil ein:",
                    SelectedProfile);
                input.Owner = this;

                if (input.ShowDialog() == true) {
                    if (ProfileManager.RenameActiveProfile(input.Value)) {
                        SelectedProfile = input.Value;
                    } else {
                        MessageBox.Show(this,
                            "Das Profil kann nicht umbenannt werden, weil bereits ein Profil mit diesem Namen existiert",
                            "Umbennen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            _settings.Screen = ActiveScreen;
            if (_view != null) {
                _view.Close();
                _view = new StreamView();
                _view.Show();
            }
        }
    }
}