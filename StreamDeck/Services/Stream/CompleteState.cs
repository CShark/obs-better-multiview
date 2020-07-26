using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeck.Services.Stream {
    class CompleteState :StreamingStatus {
        public bool HasNextState => false;
        public bool HasPrevState => false;
        public string NextState => "";
        public string PrevState => "";
        public StreamState State => StreamState.Completed;

        public StreamingStatus GoNext() {
            throw new NotImplementedException();
        }

        public StreamingStatus GoPrev() {
            throw new NotImplementedException();
        }

        public StreamingStatus Apply() {
            return this;
        }
    }
}
