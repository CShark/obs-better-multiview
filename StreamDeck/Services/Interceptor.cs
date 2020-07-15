using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interceptor;

namespace StreamDeck.Services {
    class Interceptor {
        private Input _interceptor;
        public event Action<KeyPressedEventArgs> KeyPressed;

        public Interceptor(Settings settings) {
            _interceptor = new Input();
            _interceptor.KeyboardFilterMode = KeyboardFilterMode.All;
            _interceptor.OnKeyPressed += (sender, args) => { OnKeyPressed(args); };
        }

        public bool Start() {
            return _interceptor.Load();
        }

        public void Stop() {
            _interceptor.Unload();
        }

        protected virtual void OnKeyPressed(KeyPressedEventArgs obj) {
            KeyPressed?.Invoke(obj);
        }
    }
}
