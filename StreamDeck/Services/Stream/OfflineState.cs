using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StreamDeck.Annotations;

namespace StreamDeck.Services.Stream {
    class OfflineState :StreamingStatus{
        public bool HasNextState => true;
        public bool HasPrevState => false;
        public string NextState => "OBS starten";
        public string PrevState => "";
        public StreamState State => StreamState.Offline;

        public StreamingStatus GoNext() {
            return new ObsState().Apply();
        }

        public StreamingStatus GoPrev() {
            throw new NotImplementedException();
        }

        public StreamingStatus Apply() {
            return this;
        }
    }
}
