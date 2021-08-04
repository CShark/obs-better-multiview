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
using StreamDeck.Data;

namespace StreamDeck.Controls {
    /// <summary>
    /// Interaktionslogik für SceneSlot.xaml
    /// </summary>
    public partial class SceneSlot : UserControl {
        private readonly UserProfile.DSlot _slot;

        public static readonly DependencyProperty SceneNameProperty = DependencyProperty.Register(
            nameof(SceneName), typeof(string), typeof(SceneSlot), new PropertyMetadata(default(string)));

        public string SceneName {
            get { return (string) GetValue(SceneNameProperty); }
            set { SetValue(SceneNameProperty, value); }
        }

        public SceneSlot(UserProfile.DSlot slot) {
            _slot = slot;
            SceneName = _slot.Obs.Scene;
            InitializeComponent();
        }

        private void SceneSlot_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
        }
    }
}