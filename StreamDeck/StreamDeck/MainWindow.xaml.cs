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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using StreamDeck.Data;
using StreamDeck.Dialogs;
using StreamDeck.Extensions;
using StreamDeck.Services;
using WPFLocalizeExtension.Engine;

namespace StreamDeck {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly ObsWatchService _watcher;
        private readonly Settings _settings;
        private readonly Win32Interop _win32;
        private StreamView _view;
        private readonly PluginService _plugins;
        private readonly ILogger _logger;

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
            nameof(Screens), typeof(ObservableCollection<MonitorInfo>), typeof(MainWindow),
            new PropertyMetadata(default(ObservableCollection<MonitorInfo>)));

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

        public static readonly DependencyProperty PluginsProperty = DependencyProperty.Register(
            nameof(Plugins), typeof(IReadOnlyCollection<PluginInfo>), typeof(MainWindow),
            new PropertyMetadata(default(IReadOnlyCollection<PluginInfo>)));

        public IReadOnlyCollection<PluginInfo> Plugins {
            get { return (IReadOnlyCollection<PluginInfo>) GetValue(PluginsProperty); }
            set { SetValue(PluginsProperty, value); }
        }

        public MainWindow() {
            InitializeComponent();
            Closed += (sender, args) => _view?.Close();
            _logger = App.Container.Resolve<ILogger<MainWindow>>();

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

            _watcher.ObsConnectionError += (ex) => {
                Dispatcher.Invoke(() => {
                    if (ex is AuthFailureException) {
                        MessageBox.Show(this, Localizer.Localize<string>("MainWindow", "ObsError.Auth"),
                            Localizer.Localize<string>("MainWindow", "ObsError.Title"), MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    } else {
                        MessageBox.Show(this, Localizer.Localize<string>("MainWindow", "ObsError.Generic"),
                            Localizer.Localize<string>("MainWindow", "ObsError.Title"), MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                });
            };

            Dispatcher.Invoke(() => { SelectedProfile = ProfileManager.ActiveProfile?.Name ?? ""; });

            _settings = App.Container.Resolve<Settings>();
            _win32 = App.Container.Resolve<Win32Interop>();
            ActiveScreen = _settings.Screen;
            Screens = new ObservableCollection<MonitorInfo>(_win32.GetMonitors());
            if (ActiveScreen >= Screens.Count) {
                ActiveScreen = 0;
                _settings.Screen = 0;
            }

            _plugins = App.Container.Resolve<PluginService>();
            _plugins.Scan();
            Plugins = _plugins.Plugins;
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
            var input = new TextInput(Localizer.Localize<string>("Dialogs", "CreateProfile.Title"),
                Localizer.Localize<string>("Dialogs", "CreateProfile.Message"));
            input.Owner = this;

            if (input.ShowDialog() == true) {
                ProfileManager.CreateProfile(input.Value);
            }
        }

        private void RenameProfile_OnClick(object sender, RoutedEventArgs e) {
            if (SelectedProfile != null) {
                var input = new TextInput(Localizer.Localize<string>("Dialogs", "RenameProfile.Title"),
                    Localizer.Localize<string>("Dialogs", "RenameProfile.Message"), SelectedProfile);
                input.Owner = this;

                if (input.ShowDialog() == true) {
                    if (ProfileManager.RenameActiveProfile(input.Value)) {
                        SelectedProfile = input.Value;
                    } else {
                        MessageBox.Show(this, Localizer.Localize<string>("Dialogs", "RenameProfile.FailedMessage"),
                            Localizer.Localize<string>("Dialogs", "RenameProfile.Failed"), MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ScreenSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            _settings.Screen = ActiveScreen;
            if (_view != null) {
                _view.Close();
                _view = new StreamView();
                _view.Show();
            }
        }

        private void PluginSettings_OnClick(object sender, RoutedEventArgs e) {
            var info = ((Button) sender).DataContext as PluginInfo;
            var ctrl = info.Plugin.GetGlobalSettings();
            var config = new JObject();

            if (_settings.PluginSettings.ContainsKey(info.Plugin.Name)) {
                config = _settings.PluginSettings[info.Plugin.Name];
            }

            if (ctrl != null) {
                if (info.Active) {
                    info.Plugin.PausePlugin(true);
                }

                ctrl.FetchSettings();
                var window = new PluginConfig(ctrl);
                window.Owner = this;
                if (window.ShowDialog() == true) {
                    ctrl.WriteSettings();
                }

                if (info.Active) {
                    info.Plugin.PausePlugin(false);
                }
            }
        }

        private void ShowWindow_OnClick(object sender, RoutedEventArgs e) {
            if (_view.IsClosed) {
                _view = new StreamView();
                _view.Show();
            }
        }
    }
}