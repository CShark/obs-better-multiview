using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Serilog;
using StreamDeck.Services;

namespace StreamDeck {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MultiviewOverlay _overlay;
        private ObsService _obs;
        private Settings _settings;
        private ILogger _log;

        public MainWindow() {
            _log = Log.ForContext<MainWindow>();

            Task.Run(() => {
                _settings = App.Container.Resolve<Settings>();
                _obs = App.Container.Resolve<ObsService>();

                _obs.Connected += () => {
                    Dispatcher.Invoke(() => {
                        using (var scope = App.Container.BeginLifetimeScope()) {
                            _overlay = scope.Resolve<MultiviewOverlay>();
                        }
                    });
                };
            });

            InitializeComponent();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e) {
            _log.Information("Shutting down");
            Properties.Settings.Default.Save();
            _overlay?.Close();
        }
    }
}