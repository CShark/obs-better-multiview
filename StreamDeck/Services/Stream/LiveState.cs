using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeck.Services.Stream {
    class LiveState:StreamingStatus {
        public bool HasNextState => true;
        public bool HasPrevState => false;
        public string NextState => "Stream beenden";
        public string PrevState => "";
        public StreamState State => StreamState.Live;

        public StreamingStatus GoNext() {
            return new CompleteState().Apply();
        }

        public StreamingStatus GoPrev() {
            throw new NotImplementedException();
        }

        public StreamingStatus Apply() {
            return this;
        }
    }
}
