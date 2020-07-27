using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeck.Services.Stream {
    class CompleteState :StreamingStatus {
        public override bool HasNextState => false;
        public override bool HasPrevState => false;
        public override bool StreamChangeable => true;
        public override string NextState => "";
        public override string PrevState => "";
        public override StreamState State => StreamState.Completed;

        public override StreamingStatus GoNext() {
            throw new NotImplementedException();
        }

        public override StreamingStatus GoPrev() {
            throw new NotImplementedException();
        }

        public override StreamingStatus Apply() {
            return this;
        }
    }
}
