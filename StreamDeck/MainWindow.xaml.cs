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

namespace StreamDeck {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MultiviewOverlay _overlay;
        private Settings _settings;

        public MainWindow() {
            _settings = App.Container.Resolve<Settings>();
            _overlay = App.Container.Resolve<MultiviewOverlay>();
            
            InitializeComponent();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e) {
            _overlay.Close();
        }
    }
}