using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Autofac;
using StreamDeck.Services;

namespace StreamDeck.Controls {
    /// <summary>
    /// Displays the live feed from OBS
    /// </summary>
    public partial class LivePreview : UserControl {
        private readonly ObsWatchService _obs;
        
        public LivePreview() {
            // Display live feed using OBS' virtual camera and VLC
            InitializeComponent();
            _obs = App.Container.Resolve<ObsWatchService>();

            var status = _obs.WebSocket.GetVirtualCamStatus();
            if (!status.IsActive) {
                _obs.WebSocket.StartVirtualCam();
            }


            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            var options = new string[] {
                ":dshow-vdev=OBS Virtual Camera",
                @":dshow-aspect-ratio=16\:9",
                ":dshow-adev=none",
                ":live-caching=0",
            };

            Vlc.SourceProvider.CreatePlayer(libDirectory);
            Vlc.SourceProvider.MediaPlayer.Play(new Uri("dshow://"), options);
        }
    }
}