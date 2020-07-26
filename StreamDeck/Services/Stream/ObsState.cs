using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeck.Services.Stream {
    class ObsState :StreamingStatus {
        public bool HasNextState => true;
        public bool HasPrevState => true;
        public string NextState => "Live schalten";
        public string PrevState => "OBS stoppen";
        public StreamState State => StreamState.Preparing;

        public StreamingStatus GoNext() {
            return new LiveState().Apply();
        }

        public StreamingStatus GoPrev() {
            return new OfflineState().Apply();
        }

        public StreamingStatus Apply() {
            return this;
        }
    }
}
