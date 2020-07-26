using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeck.Services.Stream {
    public enum StreamState {
        Offline,
        Preparing,
        Live,
        Completed,
    }

    public interface StreamingStatus {
        bool HasNextState { get; }

        bool HasPrevState { get; }

        string NextState { get; }
        string PrevState { get; }

        StreamState State { get; }

        StreamingStatus GoNext();

        StreamingStatus GoPrev();

        StreamingStatus Apply();
    }
}