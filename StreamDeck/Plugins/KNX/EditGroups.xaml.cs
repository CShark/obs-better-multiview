using System.Windows;

namespace ObsMultiview.Plugins.KNX {
    class DatapointType {
        public KnxDatapointType Type { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Interaktionslogik für EditGroups.xaml
    /// </summary>
    public partial class EditGroups : Window {
        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings), typeof(KnxSettings), typeof(EditGroups), new PropertyMetadata(default(KnxSettings)));

        public KnxSettings Settings {
            get { return (KnxSettings) GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
            nameof(ReadOnly), typeof(bool), typeof(EditGroups), new PropertyMetadata(default(bool)));

        public bool ReadOnly {
            get { return (bool) GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register(
            nameof(SelectedGroup), typeof(KnxGroup), typeof(EditGroups), new PropertyMetadata(default(KnxGroup)));

        public KnxGroup SelectedGroup {
            get { return (KnxGroup) GetValue(SelectedGroupProperty); }
            set { SetValue(SelectedGroupProperty, value); }
        }

        public EditGroups(KnxSettings settings, bool ro = false) {
            Settings = settings;
            ReadOnly = ro;
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
    }
}