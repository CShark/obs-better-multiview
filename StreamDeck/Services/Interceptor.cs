using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interceptor;

namespace StreamDeck.Services {
    public class Interceptor {
        private Input _interceptor;
        public event Action<KeyPressedEventArgs> KeyPressed;

        public Interceptor() {
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

        public void SendKey(Keys key, KeyState state, int deviceId) {
            _interceptor.SendKey(key, state, deviceId);
        }

        protected virtual void OnKeyPressed(KeyPressedEventArgs obj) {
            KeyPressed?.Invoke(obj);
        }
    }
}
