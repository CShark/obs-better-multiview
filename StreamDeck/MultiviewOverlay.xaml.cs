using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NAudio.MediaFoundation;
using OBS.WebSocket.NET;
using OBS.WebSocket.NET.Types;
using StreamDeck.Services;
using StreamDeck.Services.Youtube;
using Path = System.IO.Path;

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
                WindowState = WindowState.Normal;
                var screen = Screen.FromHandle(windowHandle);
                Top = screen.Bounds.Top;
                Left = screen.Bounds.Left;
                Width = screen.Bounds.Width;
                Height = screen.Bounds.Height;
                WindowState = WindowState.Maximized;
                Show();
                _isHidden = false;
            } else if (!multiview && !_isHidden) {
                Hide();
                _isHidden = true;
            }
        }

        private void InitializeMultiviewHook() {
            _winEventHookHandler = new SystemEventHandler(WinEventHook);
            _winEventHook = SetWinEventHook(SystemEvents.EVENT_SYSTEM_FOREGROUND, SystemEvents.EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero, _winEventHookHandler, 0, 0, 0);

            _windowWatcher = new DispatcherTimer();
            _windowWatcher.Tick += WatchMultiview;
            _windowWatcher.Interval = new TimeSpan(0, 0, 0, 0, 00);
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

        private void InitializeOBS() {
            var state = _obs.Api.GetStreamingStatus();

            Dispatcher.Invoke(() => { StreamState = state.IsStreaming ? OutputState.Started : OutputState.Stopped; });

            _obs.WebSocket.StreamingStateChanged += ObsOnStreamingStateChanged;
            _obs.WebSocket.StreamStatus += ObsOnStreamStatus;
            _obs.WebSocket.SourceVolumeChanged += ObsOnSourceVolumeChanged;

            var data = _obs.Api.GetSpecialSources();

            VMeter1.Obs = _obs;
            VMeter1.AudioDevice = _obs.Profile.GetDevice(_profile.AudioDevice);
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
            });
        }

        private void ObsOnStreamingStateChanged(ObsWebSocket sender, OutputState type) {
            Dispatcher.Invoke(() => {
                StreamState = type;

                if (StreamState == OutputState.Stopped) {
                    FPS = 0;
                    KBits = 0;
                }
            });
        }

        #endregion

        #region Youtube

        private YoutubeService _youtube;

        public static readonly DependencyProperty LiveStreamsProperty = DependencyProperty.Register(
            nameof(LiveStreams), typeof(ObservableCollection<LiveStream>), typeof(MultiviewOverlay),
            new PropertyMetadata(default(ObservableCollection<LiveStream>)));

        public ObservableCollection<LiveStream> LiveStreams {
            get { return (ObservableCollection<LiveStream>) GetValue(LiveStreamsProperty); }
            set { SetValue(LiveStreamsProperty, value); }
        }

        private async void InitializeYoutube() {
            await _youtube.Authenticate();

            Dispatcher.Invoke(() => { LiveStreams = _youtube.Livestreams; });
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

        #endregion

        #region Commands

        public static RoutedUICommand ObsStreamCommand { get; set; }

        #endregion

        public MultiviewOverlay(Settings settings, ProfileSettings profile, ObsService obs, YoutubeService youtube) {
            _settings = settings;
            _obs = obs;
            _profile = profile;
            _youtube = youtube;

            ObsStreamCommand = new RoutedUICommand();

            InitializeComponent();
            InitializeMultiviewHook();
            InitializeOBS();
            InitializeYoutube();
        }

        private void ObsStream_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = StreamState != OutputState.Stopping;
        }

        private void ObsStream_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            switch (StreamState) {
                case OutputState.Started:
                case OutputState.Starting:
                    _obs.Api.StopStreaming();
                    break;
                case OutputState.Stopped:
                    _obs.Api.StartStreaming();
                    break;
            }
        }

        private void ActiveLivestream_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(ActiveLivestream.SelectedItem is LiveStream ls) {
                MonitoringPreview.NavigateToString(ls.MonitorHTML);
            }
        }
    }
}