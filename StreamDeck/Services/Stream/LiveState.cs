using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;

namespace StreamDeck.Services.Stream {
    class LiveState : StreamingStatus {
        private bool _hasNextState = true;

        public override bool HasNextState => _hasNextState;

        public override bool HasPrevState => false;
        public override bool StreamChangeable => false;
        public override string NextState => "Stream beenden";
        public override string PrevState => "";
        public override StreamState State => StreamState.Live;

        public override StreamingStatus GoNext() {
            _hasNextState = false;
            OnPropertyChanged(nameof(HasNextState));
            Obs.Api.StopStreaming();
            //TODO: Delay
            LiveStream.SetState(LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Complete);
            return new CompleteState().Apply();
        }

        public override StreamingStatus GoPrev() {
            throw new NotImplementedException();
        }

        public override StreamingStatus Apply() {
            return this;
        }
    }
}
