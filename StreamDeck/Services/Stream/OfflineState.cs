using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using StreamDeck.Annotations;

namespace StreamDeck.Services.Stream {
    class OfflineState :StreamingStatus{
        public override bool HasNextState => true;
        public override bool HasPrevState => false;
        public override bool StreamChangeable => true;
        public override string NextState => "OBS starten";
        public override string PrevState => "";
        public override StreamState State => StreamState.Offline;
        
        public override StreamingStatus GoNext() {
            Obs.Api.StartStreaming();
            LiveStream.SetState(LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Testing);
            return Create<ObsState>().Apply();
        }

        public override StreamingStatus GoPrev() {
            throw new NotImplementedException();
        }

        public override StreamingStatus Apply() {
            return this;
        }
    }
}
