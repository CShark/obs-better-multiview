using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Autofac;
using Newtonsoft.Json;
using StreamDeck.Data;
using StreamDeck.Services;

namespace StreamDeck.Dialogs {
    /// <summary>
    /// Interaktionslogik für SlotConfig.xaml
    /// </summary>
    public partial class SlotConfig : Window {
        public static readonly DependencyProperty SlotProperty = DependencyProperty.Register(
            nameof(Slot), typeof(UserProfile.DSlot), typeof(SlotConfig),
            new PropertyMetadata(default(UserProfile.DSlot)));

        public UserProfile.DSlot Slot {
            get { return (UserProfile.DSlot) GetValue(SlotProperty); }
            set { SetValue(SlotProperty, value); }
        }

        public static readonly DependencyProperty AvailableScenesProperty = DependencyProperty.Register(
            nameof(AvailableScenes), typeof(ObservableCollection<string>), typeof(SlotConfig),
            new PropertyMetadata(default(ObservableCollection<string>)));

        public ObservableCollection<string> AvailableScenes {
            get { return (ObservableCollection<string>) GetValue(AvailableScenesProperty); }
            set { SetValue(AvailableScenesProperty, value); }
        }

        private string _originalConfig;
        private readonly ObsWatchService _obs;

        public void SetSlot(UserProfile.DSlot slot) {
            _originalConfig = JsonConvert.SerializeObject(slot);
            Slot = slot;
        }

        public SlotConfig() {
            InitializeComponent();

            _obs = App.Container.Resolve<ObsWatchService>();
            var scenes = _obs.WebSocket.GetSceneList().Scenes.Select(x => x.Name)
                .Where(x => x != "multiview" && x != "preview");
            AvailableScenes = new ObservableCollection<string>(scenes);

            input.Focus();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            JsonConvert.PopulateObject(_originalConfig, Slot);
            Close();
        }

        private void SlotConfig_OnClosing(object sender, CancelEventArgs e) {
            if (DialogResult == null) {
                DialogResult = false;
                JsonConvert.PopulateObject(_originalConfig, Slot);
            }
        }

        private void Unlink_OnClick(object sender, RoutedEventArgs e) {
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(new UserProfile.DSlot()), Slot);
            DialogResult = true;
            Close();
        }
    }
}