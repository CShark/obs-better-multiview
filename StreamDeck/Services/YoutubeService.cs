using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using StreamDeck.Services.Youtube;

namespace StreamDeck.Services {
    public class YoutubeService {
        private Settings _settings;
        private YouTubeService _youtube;
        
        public ObservableCollection<LiveStream> Livestreams { get; private set; }

        public YoutubeService(Settings settings) {
            _settings = settings;
            Livestreams = new ObservableCollection<LiveStream>();
        }


        public async Task Authenticate() {
            UserCredential credential;

            using (var stream = new MemoryStream(Properties.Resources.client_secret)) {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] {YouTubeService.Scope.Youtube},
                    "user", CancellationToken.None, new FileDataStore(Directory.GetCurrentDirectory())
                );
            }


            _youtube = new YouTubeService(new BaseClientService.Initializer() {
                ApplicationName = "OBS Overlay",
                HttpClientInitializer = credential
            });


            // Playlist
            var playlistReq = _youtube.PlaylistItems.List("snippet,id");
            playlistReq.PlaylistId = _settings.GottesdienstPlaylist;
            playlistReq.MaxResults = 25;

            var items = await playlistReq.ExecuteAsync();

            // Livestreams
            var request = _youtube.LiveBroadcasts.List("snippet,status,contentDetails,id");
            request.Mine = true;
            var data = await request.ExecuteAsync();

            foreach (var stream in data.Items) {
                //if (stream.Status.LifeCycleStatus == "complete") continue;

                var streamReq = _youtube.LiveStreams.List("snippet,cdn,id");
                streamReq.Id = stream.ContentDetails.BoundStreamId;
                var streamData = await streamReq.ExecuteAsync();

                var streamObj = new LiveStream(_youtube, _settings, stream.Id, stream.Snippet.Title);

                streamObj.Airing = Convert.ToDateTime(stream.Snippet.ScheduledStartTime);
                streamObj.MonitorHTML = "<style>body{margin:0}</style>" + stream.ContentDetails.MonitorStream.EmbedHtml;
                if (streamData.Items.Count > 0) {
                    streamObj.StreamID = streamData.Items[0].Id;
                    streamObj.StreamKey = streamData.Items[0].Cdn.IngestionInfo.StreamName;
                }

                if (items.Items.Any(x => x.Snippet.Title == streamObj.Title)) {
                    streamObj.IsPlaylisted = true;
                }

                Livestreams.Add(streamObj);
            }
        }
    }
}