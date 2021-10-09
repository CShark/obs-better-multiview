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
using ObsMultiview.Plugins.Extensions;

namespace ObsMultiview.Plugins.PelcoD {
    /// <summary>
    /// Interaktionslogik für SlotSettings.xaml
    /// </summary>
    public partial class SlotSettings : SlotSettingsControl<PelcoSlotSettings> {
        public static readonly DependencyProperty PresetsProperty = DependencyProperty.Register(
            nameof(Presets), typeof(List<Preset>), typeof(SlotSettings), new PropertyMetadata(default(List<Preset>)));

        public List<Preset> Presets {
            get { return (List<Preset>) GetValue(PresetsProperty); }
            set { SetValue(PresetsProperty, value); }
        }

        public SlotSettings(CommandFacade commandFacade, Guid slotID) : base(commandFacade, slotID) {
            var settings = commandFacade.RequestSettings<PelcoSettings>();
            Presets = settings.Presets;
            Presets.Insert(0,
                new Preset {CameraID = 0, PresetID = 0, Name = Localizer.Localize<string>("PelcoD", "NoCamera")});
            InitializeComponent();
        }

        public override void FetchSettings() {
            base.FetchSettings();
            var idx = Presets.FindIndex(x=>x.CameraID == Settings.Preset?.CameraID && x.PresetID == Settings.Preset?.PresetID);
            if (idx < 0) idx = 0;
            PelcoConfig.SelectedItem = Presets[idx];
        }

        public override void WriteSettings() {
            if (Settings.Preset?.CameraID == 0) {
                Settings.Preset = null;
            }
            base.WriteSettings();
        }
    }
}