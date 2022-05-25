using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Xml.XPath;
using JetBrains.Annotations;
using ObsMultiview.Plugins.Extensions;

namespace ObsMultiview.Plugins.PelcoD {
    /// <summary>
    /// Interaktionslogik für SlotSettings.xaml
    /// </summary>
    public partial class SlotSettings : SlotSettingsControl<PelcoSlotSettings> {
        public static readonly DependencyProperty PresetsProperty = DependencyProperty.Register(
            nameof(Presets), typeof(List<PresetData>), typeof(SlotSettings),
            new PropertyMetadata(default(List<PresetData>)));

        public List<PresetData> Presets {
            get { return (List<PresetData>)GetValue(PresetsProperty); }
            set { SetValue(PresetsProperty, value); }
        }

        public SlotSettings(CommandFacade commandFacade, Guid slotID) : base(commandFacade, slotID) {
            Presets = new List<PresetData>();

            var settings = commandFacade.RequestSettings<PelcoSettings>();

            var camIds = settings.Presets.Select(x => x.CameraID).Distinct().OrderBy(x => x).ToList();

            foreach (var cam in camIds) {
                var data = new PresetData {
                    AvailablePresets = settings.Presets.Where(x => x.CameraID == cam).ToList(),
                    CameraID = cam,
                    Selected = null
                };

                data.AvailablePresets.Insert(0,
                    new Preset { CameraID = 0, PresetID = 0, Name = Localizer.Localize<string>("PelcoD", "NoCamera") });

                var view = CollectionViewSource.GetDefaultView(data.AvailablePresets);
                view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Preset.Group)));

                Presets.Add(data);
            }


            InitializeComponent();
        }

        public override void FetchSettings() {
            base.FetchSettings();

            foreach (var cam in Presets) {
                var preset = Settings.Presets?.FirstOrDefault(x => x.CameraID == cam.CameraID);
                if (preset == null) {
                    preset = cam.AvailablePresets[0];
                } else {
                    preset = cam.AvailablePresets.FirstOrDefault(x =>
                        x.CameraID == preset.CameraID && x.PresetID == preset.PresetID);
                }

                cam.Selected = preset;
            }
        }

        public override void WriteSettings() {
            foreach (var cam in Presets) {
                if (cam.Selected.CameraID == 0) cam.Selected = null;
            }

            Settings.Presets = Presets.Select(x => x.Selected).ToList();

            base.WriteSettings();
        }
    }

    public class PresetData : INotifyPropertyChanged {
        private Preset _selected;
        public List<Preset> AvailablePresets { get; set; }
        public int CameraID { get; set; }

        public Preset Selected {
            get => _selected;
            set {
                if (Equals(value, _selected)) return;
                _selected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}