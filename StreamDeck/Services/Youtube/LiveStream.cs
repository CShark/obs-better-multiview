using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using StreamDeck.Annotations;

namespace StreamDeck.Services.Youtube {
    public enum StreamStatus {
        Good,
        Ok,
        Bad,
        NoData
    }

    public class LiveStream : INotifyPropertyChanged {
        private YouTubeService _youtube;
        private Settings _settings;
        private bool _isPlaylisted;
        private Timer _timer;
        private bool _isStreaming;
        private StreamStatus _status;

        public string Title { get; }
        public DateTime Airing { get; set; }

        public string StreamKey { get; set; }

        public string MonitorHTML { get; set; }

        public string BroadcastID { get; }

        public string StreamID { get; set; }

        public bool IsPlaylisted {
            get => _isPlaylisted;
            set {
                if (value == _isPlaylisted) return;
                _isPlaylisted = value;
                OnPropertyChanged();
            }
        }

        public bool IsStreaming {
            get => _isStreaming;
            set {
                if (value == _isStreaming) return;
                _isStreaming = value;
                OnPropertyChanged();
            }
        }

        public StreamStatus Status {
            get => _status;
            set {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public async void TogglePlaylisted() {
            if (IsPlaylisted) {
                var reqPlaylist = _youtube.PlaylistItems.List("snippet,id");
                reqPlaylist.PlaylistId = _settings.GottesdienstPlaylist;
                ;

                var playlist = await reqPlaylist.ExecuteAsync();
                var item = playlist.Items.FirstOrDefault(x => x.Snippet.ResourceId.VideoId == BroadcastID);

                if (item != null) {
                    var reqDelete = _youtube.PlaylistItems.Delete(item.Id);
                    await reqDelete.ExecuteAsync();
                }

                IsPlaylisted = false;
            } else {
                var item = new PlaylistItem();
                item.Snippet.PlaylistId = _settings.GottesdienstPlaylist;
                item.Snippet.ResourceId = new ResourceId {VideoId = BroadcastID, Kind = "youtube#video"};

                var reqInsert = _youtube.PlaylistItems.Insert(item, "snippet");
                await reqInsert.ExecuteAsync();
                IsPlaylisted = true;
            }
        }

        public LiveStream(YouTubeService youtube, Settings settings, string broadcastID, string title) {
            _youtube = youtube;
            _settings = settings;
            Title = title;
            BroadcastID = broadcastID;

            _timer = new Timer(state => { UpdateStats(); }, null, Timeout.Infinite, Timeout.Infinite);
        }

        private async void UpdateStats() {
            var statReq = _youtube.LiveStreams.List("status");
            statReq.Mine = true;
            statReq.Id = StreamID;

            var stats = await statReq.ExecuteAsync();

            try {
                Status = stats.Items[0].Status.HealthStatus.Status switch {
                    "good" => StreamStatus.Good,
                    "ok" => StreamStatus.Ok,
                    "bad" => StreamStatus.Bad,
                    _ => StreamStatus.NoData
                };
            } catch {
                Status = StreamStatus.NoData;
            }
        }

        public void SetState(LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum status) {
            _youtube.LiveBroadcasts.Transition(BroadcastID,status, "");

            switch (status) {
                case LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Testing:
                case LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Live:
                    IsStreaming = true;
                    _timer.Change(1000, 1000);
                    break;
                default:
                    IsStreaming = false;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    Status = StreamStatus.NoData;
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}