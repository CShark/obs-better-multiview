using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Serilog;
using StreamDeck.Services;
using StreamDeck.Services.Obs;
using Math = System.Math;

namespace StreamDeck.ObsOverlay {
    /// <summary>
    /// Interaktionslogik für VMeter.xaml
    /// </summary>
    public partial class VMeter : UserControl {
        private MMDevice _device;
        private DispatcherTimer _meterUpdate;
        private ILogger _log;

        public static readonly DependencyProperty AudioDeviceProperty = DependencyProperty.Register(
            nameof(AudioDevice), typeof(AudioDevice), typeof(VMeter),
            new PropertyMetadata(default(AudioDevice), (o, args) => (o as VMeter).UpdateAudioDevice()));

        public AudioDevice AudioDevice {
            get { return (AudioDevice) GetValue(AudioDeviceProperty); }
            set { SetValue(AudioDeviceProperty, value); }
        }

        public static readonly DependencyProperty ObsProperty = DependencyProperty.Register(
            nameof(Obs), typeof(ObsService), typeof(VMeter), new PropertyMetadata(default(ObsService)));

        public ObsService Obs {
            get { return (ObsService) GetValue(ObsProperty); }
            set { SetValue(ObsProperty, value); }
        }

        private void UpdateAudioDevice() {
            _log.Debug("Updating Audio Device");
            _device = AudioDevice.GetDevice();
            var info = Obs.Api.GetVolume(AudioDevice.Name);
            Volume = info.Volume;
            Muted = info.Muted;
        }


        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register(
            nameof(Volume), typeof(float), typeof(VMeter), new PropertyMetadata(default(float)));

        public float Volume {
            get { return (float) GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        public static readonly DependencyProperty MutedProperty = DependencyProperty.Register(
            nameof(Muted), typeof(bool), typeof(VMeter), new PropertyMetadata(default(bool)));

        public bool Muted {
            get { return (bool) GetValue(MutedProperty); }
            set { SetValue(MutedProperty, value); }
        }

        public static readonly DependencyProperty MetersProperty = DependencyProperty.Register(
            nameof(Meters), typeof(ObservableCollection<float>), typeof(VMeter),
            new PropertyMetadata(default(ObservableCollection<float>)));

        public ObservableCollection<float> Meters {
            get { return (ObservableCollection<float>) GetValue(MetersProperty); }
            set { SetValue(MetersProperty, value); }
        }

        public VMeter() {
            _log = Log.ForContext<VMeter>();
            _log.Information("Initialize VMeter");

            Meters = new ObservableCollection<float>();

            _meterUpdate = new DispatcherTimer();
            _meterUpdate.Interval = TimeSpan.FromMilliseconds(50);
            _meterUpdate.Tick += (sender, args) => {
                if (_device != null) {
                    var length = _device.AudioMeterInformation.PeakValues.Count;

                    if (length != Meters.Count) {
                        Meters.Clear();

                        for (int i = 0; i < length; i++) {
                            Meters.Add(AudioMeterToDb(_device.AudioMeterInformation.PeakValues[i] * Volume));
                        }
                    } else {
                        for (int i = 0; i < length; i++) {
                            Meters[i] = AudioMeterToDb(_device.AudioMeterInformation.PeakValues[i] * Volume);
                        }
                    }
                }
            };
            _meterUpdate.Start();

            InitializeComponent();
        }

        private static float AudioMeterToDb(float meter) {
            return (float) (0.098 + 8.7197 * Math.Log(meter));
        }
    }
}