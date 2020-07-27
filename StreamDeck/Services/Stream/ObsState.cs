using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;

namespace StreamDeck.Services.Stream {
    class ObsState :StreamingStatus {
        public override bool HasNextState => true;
        public override bool HasPrevState => true;
        public override bool StreamChangeable => false;
        public override string NextState => "Live schalten";
        public override string PrevState => "OBS stoppen";
        public override StreamState State => StreamState.Preparing;

        public override StreamingStatus GoNext() {
            LiveStream.SetState(LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Live);
            return Create<LiveState>().Apply();
        }

        public override StreamingStatus GoPrev() {
            LiveStream.SetState(LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.StatusUnspecified);
            Obs.Api.StopStreaming();
            return Create<OfflineState>().Apply();
        }

        public override StreamingStatus Apply() {
            return this;
        }
    }
}
