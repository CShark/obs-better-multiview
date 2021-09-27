using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace ObsMultiview.Plugins.PelcoD {
    /// <summary>
    /// Interaktionslogik für GlobalSettings.xaml
    /// </summary>
    public partial class GlobalSettings : SettingsControl<PelcoSettings> {
        public static readonly DependencyProperty AvailablePortsProperty = DependencyProperty.Register(
            nameof(AvailablePorts), typeof(List<string>), typeof(GlobalSettings), new PropertyMetadata(default(List<string>)));

        public List<string> AvailablePorts {
            get { return (List<string>) GetValue(AvailablePortsProperty); }
            set { SetValue(AvailablePortsProperty, value); }
        }

        public GlobalSettings(CommandFacade management) : base(management) {
            AvailablePorts = SerialPort.GetPortNames().ToList();
            InitializeComponent();
        }
    }
}