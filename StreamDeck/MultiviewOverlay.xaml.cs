using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Google.Apis.YouTube.v3;
using NAudio.MediaFoundation;
using OBS.WebSocket.NET;
using OBS.WebSocket.NET.Types;
using Serilog;
using StreamDeck.Services;
using Path = System.IO.Path;
using StreamStatus = OBS.WebSocket.NET.Types.StreamStatus;

namespace StreamDeck {
    /// <summary>
    /// Interaktionslogik für MultiviewOverlay.xaml
    /// </summary>
    public partial class MultiviewOverlay : Window {
        private Settings _settings;
        private ProfileSettings _profile;


        #region Multiview Overlay in Front

        enum SystemEvents {
            EVENT_SYSTEM_FOREGROUND = 0x03
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }


        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(SystemEvents eventMin, SystemEvents eventMax, IntPtr hmodWinEventProc,
            SystemEventHandler lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);


        delegate void SystemEventHandler(IntPtr hWinEventHook, SystemEvents @event, IntPtr hwnd, int idObject,
            int idChild,
            uint dwEventThread, uint dwmsEventTime);

        private IntPtr _winEventHook;
        private SystemEventHandler _winEventHookHandler;
        private DispatcherTimer _windowWatcher;
        private bool _isHidden = true;

        private void WinEventHook(IntPtr hWinEventHook, SystemEvents @event, IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime) {
            StringBuilder buffer = new StringBuilder(512);
            int length = GetWindowText(hwnd, buffer, buffer.Capacity);

            if (buffer.ToString() == _settings.MultiviewWindowTitle) {
                Topmost = true;
            } else {
                Topmost = false;
            }
        }

        private void WatchMultiview(object sender, EventArgs args) {
            var windowHandle = FindWindow(null, _settings.MultiviewWindowTitle);
            var multiview = windowHandle != IntPtr.Zero;

            if (multiview && _isHidden) {
                Thread.Sleep(500);
                WindowState = WindowState.Normal;
                var screen = Screen.FromHandle(windowHandle);
                Top = screen.Bounds.Top;
                Left = screen.Bounds.Left;
                Width = screen.Bounds.Width;
                Height = screen.Bounds.Height;
                //WindowState = WindowState.Maximized;
                Show();
                _isHidden = false;
            } else if (!multiview && !_isHidden) {
                Hide();
                _isHidden = true;
            }
        }

        private void InitializeMultiviewHook() {
            _log.Debug("Initialize Window Hook");
            _winEventHookHandler = new SystemEventHandler(WinEventHook);
            _winEventHook = SetWinEventHook(SystemEvents.EVENT_SYSTEM_FOREGROUND, SystemEvents.EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero, _winEventHookHandler, 0, 0, 0);

            _windowWatcher = new DispatcherTimer();
            _windowWatcher.Tick += WatchMultiview;
            _windowWatcher.Interval = new TimeSpan(0, 0, 0, 0, 200);
            _windowWatcher.Start();

            Closing += (sender, args) => {
                UnhookWinEvent(_winEventHook);
                _windowWatcher.Stop();
            };
        }

        #endregion

        #region OBS WebSocket API

        private ObsService _obs;
        private bool _apiVolume;
        private bool _apiMute;
        private ILogger _log;

        private void InitializeOBS() {
            _log.Debug("Initializing OBS connection");
            var state = _obs.Api.GetStreamingStatus();
            _obs.Disconnected += DeInitializeObs;

            Dispatcher.Invoke(() => { StreamState = state.IsStreaming ? OutputState.Started : OutputState.Stopped; });

            _obs.WebSocket.StreamingStateChanged += ObsOnStreamingStateChanged;
            _obs.WebSocket.StreamStatus += ObsOnStreamStatus;
            _obs.WebSocket.SourceVolumeChanged += ObsOnSourceVolumeChanged;

            var data = _obs.Api.GetSpecialSources();
            AudioDevices = new ObservableCollection<string>(_obs.Profile.AvailableDevices());
            var view = CollectionViewSource.GetDefaultView(AudioDevices);
            if (AudioDevices.Contains(_profile.AudioDevice)) {
                view.MoveCurrentTo(_profile.AudioDevice);
            }

            VMeter1.Obs = _obs;
            VMeter1.AudioDevice = _obs.Profile.GetDevice(_profile.AudioDevice);

            var status = _obs.Api.GetStreamingStatus();
            _recording = status.IsRecording;
            _streaming = status.IsStreaming;
        }

        private void DeInitializeObs() {
            Dispatcher.Invoke(() => {
                _obs.Disconnected -= DeInitializeObs;
                Close();
            });
        }

        private void ObsOnSourceVolumeChanged(ObsWebSocket sender, string sourcename, float volume) {
            Dispatcher.Invoke(() => {
                if (sourcename == VMeter1.AudioDevice.Name) {
                    VMeter1.Volume = volume;
                }
            });
        }

        private void ObsOnStreamStatus(ObsWebSocket sender, StreamStatus status) {
            Dispatcher.Invoke(() => {
                FPS = status.FPS;
                KBits = status.KbitsPerSec;
                Strain = status.Strain;
            });
        }

        private void ObsOnStreamingStateChanged(ObsWebSocket sender, OutputState type) {
            Dispatcher.Invoke(async () => {
                StreamState = type;

                if (StreamState == OutputState.Stopped) {
                    _log.Information("Stream stopped");
                    FPS = 0;
                    KBits = 0;
                    Strain = -1;
                }

                CommandManager.InvalidateRequerySuggested();
            });
        }

        #endregion


        #region Properties

        public static readonly DependencyProperty StreamStateProperty = DependencyProperty.Register(
            nameof(StreamState), typeof(OutputState), typeof(MultiviewOverlay),
            new PropertyMetadata(default(OutputState)));

        public OutputState StreamState {
            get { return (OutputState) GetValue(StreamStateProperty); }
            set { SetValue(StreamStateProperty, value); }
        }

        public static readonly DependencyProperty KBitsProperty = DependencyProperty.Register(
            nameof(KBits), typeof(float), typeof(MultiviewOverlay), new PropertyMetadata(default(float)));

        public float KBits {
            get { return (float) GetValue(KBitsProperty); }
            set { SetValue(KBitsProperty, value); }
        }

        public static readonly DependencyProperty FPSProperty = DependencyProperty.Register(
            nameof(FPS), typeof(float), typeof(MultiviewOverlay), new PropertyMetadata(default(float)));

        public float FPS {
            get { return (float) GetValue(FPSProperty); }
            set { SetValue(FPSProperty, value); }
        }


        public static readonly DependencyProperty StrainProperty = DependencyProperty.Register(
            nameof(Strain), typeof(float), typeof(MultiviewOverlay), new PropertyMetadata(-1f));

        public float Strain {
            get { return (float) GetValue(StrainProperty); }
            set { SetValue(StrainProperty, value); }
        }

        public static readonly DependencyProperty AudioDevicesProperty = DependencyProperty.Register(
            nameof(AudioDevices), typeof(ObservableCollection<string>), typeof(MultiviewOverlay),
            new PropertyMetadata(default(ObservableCollection<string>)));

        public ObservableCollection<string> AudioDevices {
            get { return (ObservableCollection<string>) GetValue(AudioDevicesProperty); }
            set { SetValue(AudioDevicesProperty, value); }
        }

        #endregion

        #region Commands

        public static RoutedUICommand AddStreamToPlaylist { get; set; }

        public static RoutedUICommand ObsStreaming { get; set; }

        public static RoutedUICommand ObsRecording { get; set; }

        public static readonly DependencyProperty StreamButtonProperty = DependencyProperty.Register(
            nameof(StreamButton), typeof(string), typeof(MultiviewOverlay), new PropertyMetadata("Stream Starten"));

        public string StreamButton {
            get { return (string) GetValue(StreamButtonProperty); }
            set { SetValue(StreamButtonProperty, value); }
        }

        public static readonly DependencyProperty RecordButtonProperty = DependencyProperty.Register(
            nameof(RecordButton), typeof(string), typeof(MultiviewOverlay), new PropertyMetadata("Aufnahme Starten"));

        public string RecordButton {
            get { return (string) GetValue(RecordButtonProperty); }
            set { SetValue(RecordButtonProperty, value); }
        }

        private bool _streaming, _recording;

        #endregion


        public MultiviewOverlay(Settings settings, ProfileSettings profile, ObsService obs, ILogger logger) {
            _settings = settings;
            _obs = obs;
            _profile = profile;
            _log = logger;

            AddStreamToPlaylist = new RoutedUICommand();
            ObsStreaming = new RoutedUICommand();
            ObsRecording = new RoutedUICommand();

            InitializeComponent();
            InitializeMultiviewHook();
            InitializeOBS();
        }

        private void Recording_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            _log.Information("Switching Recording State");

            if (!_recording) {
                _obs.Api.StartRecording();
                RecordButton = "Aufnahme Stoppen";
            } else {
                _obs.Api.StopRecording();
                RecordButton = "Aufnahme Starten";
            }

            _recording = !_recording;
        }

        private void Streaming_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            _log.Information("Switching Streaming State");

            if (!_streaming) {
                _obs.Api.StartStreaming();
                StreamButton = "Stream Stoppen";
            } else {
                _obs.Api.StopStreaming();
                StreamButton = "Stream Starten";
            }

            _streaming = !_streaming;
        }

        private void AudioDevice_OnSelected(object sender, RoutedEventArgs e) {
            var view = CollectionViewSource.GetDefaultView(AudioDevices);
            VMeter1.AudioDevice = _obs.Profile.GetDevice(view.CurrentItem as string);
            Properties.Settings.Default.AudioDevice = view.CurrentItem as string;
        }
    }
}