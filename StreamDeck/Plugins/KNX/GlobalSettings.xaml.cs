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

namespace StreamDeck.Plugins.KNX {
    /// <summary>
    /// Interaktionslogik für GlobalSettings.xaml
    /// </summary>
    public partial class GlobalSettings : SettingsControl<KnxSettings> {
        public GlobalSettings(CommandFacade management) : base(management) {
            InitializeComponent();
        }

        private void EditGroups_OnClick(object sender, RoutedEventArgs e) {
            var editor = new EditGroups(Settings);
            editor.Owner = Window.GetWindow(this);
            editor.ShowDialog();
        }
    }
}