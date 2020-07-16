using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using StreamDeck.Annotations;

namespace StreamDeck.Services.Youtube {
    public class LiveStream : INotifyPropertyChanged {
        private YouTubeService _youtube;

        public string Title { get; }
        public DateTime Airing { get; }

        public string StreamKey { get; set; }

        public string MonitorHTML { get; set; }

        public LiveStream(YouTubeService youtube, string title, DateTime airing) {
            _youtube = youtube;
            Title = title;
            Airing = airing;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}