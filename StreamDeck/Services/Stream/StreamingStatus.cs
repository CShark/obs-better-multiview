using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StreamDeck.Annotations;
using StreamDeck.Services.Youtube;

namespace StreamDeck.Services.Stream {
    public enum StreamState {
        Offline,
        Preparing,
        Live,
        Completed,
    }

    public abstract class StreamingStatus : INotifyPropertyChanged {
        protected ObsService Obs { get; private set; }
        protected YoutubeService Youtube { get; private set; }

        protected LiveStream LiveStream { get; private set; }

        public void Init(ObsService obs, YoutubeService youtube, LiveStream liveStream) {
            Obs = obs;
            Youtube = youtube;
            LiveStream = liveStream;
        }

        protected StreamingStatus() {
        }

        protected T Create<T>() where T : StreamingStatus, new() {
            var obj = new T();
            obj.Init(Obs,Youtube,LiveStream);

            return obj;
        }

        public abstract bool HasNextState { get; }

        public abstract bool HasPrevState { get; }

        public abstract bool StreamChangeable { get; }

        public abstract string NextState { get; }
        public abstract string PrevState { get; }

        public abstract StreamState State { get; }

        public abstract StreamingStatus GoNext();

        public abstract StreamingStatus GoPrev();

        public abstract StreamingStatus Apply();
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}