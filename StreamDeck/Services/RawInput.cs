using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RawInput_dll;

namespace StreamDeck.Services {
    public class RawInput {
        private RawInput_dll.RawInput _rawInput;

        private Thread _messageThread;

        public event Action<RawInputEventArg> KeyPressed;

        public RawInput() {
            _messageThread = new Thread(() => {
                _rawInput = new RawInput_dll.RawInput(false);
                _rawInput.KeyPressed += (sender, arg) => { OnKeyPressed(arg); };
                Application.Run();
            });
            _messageThread.IsBackground = true;
            _messageThread.Start();

            while (_rawInput == null) { }
        }

        public int NumberOfKeyboards => _rawInput.NumberOfKeyboards;

        public void Start() {
            _rawInput.AddMessageFilter();
        }

        protected virtual void OnKeyPressed(RawInputEventArg obj) {
            KeyPressed?.Invoke(obj);
        }
    }
}