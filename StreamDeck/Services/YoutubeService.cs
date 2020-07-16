using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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


            var request = _youtube.LiveBroadcasts.List("snippet,status,contentDetails");
            request.Mine = true;
            var data = await request.ExecuteAsync();

            foreach (var stream in data.Items) {
                var streamReq = _youtube.LiveStreams.List("snippet,cdn");
                streamReq.Id = stream.ContentDetails.BoundStreamId;
                var streamData = await streamReq.ExecuteAsync();

                var streamObj = new LiveStream(_youtube, stream.Snippet.Title,
                    Convert.ToDateTime(stream.Snippet.ScheduledStartTime));

                streamObj.MonitorHTML = stream.ContentDetails.MonitorStream.EmbedHtml;
                if (streamData.Items.Count > 0) {
                    streamObj.StreamKey = streamData.Items[0].Cdn.IngestionInfo.StreamName;
                }

                Livestreams.Add(streamObj);
            }
        }
    }
}