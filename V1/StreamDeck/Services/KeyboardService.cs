using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interceptor;
using RawInput_dll;

namespace StreamDeck.Services {
    public class KeyboardService {
        private Interceptor _interceptor;
        private RawInput _rawInput;
        private Settings _settings;

        public KeyboardService(Interceptor interceptor, RawInput rawInput, Settings settings) {
            _interceptor = interceptor;
            _rawInput = rawInput;
            _settings = settings;

            _rawInput.KeyPressed += RawInputOnKeyPressed;
            _interceptor.KeyPressed += InterceptorOnKeyPressed;

            _rawInput.Start();
            _interceptor.Start();
        }

        private void InterceptorOnKeyPressed(KeyPressedEventArgs obj) {
            
        }

        private void RawInputOnKeyPressed(RawInputEventArg obj) {
            
        }
    }
}
