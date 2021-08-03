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

        public MainWindow()
        {
            InitializeComponent();
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
        }
    }
}
