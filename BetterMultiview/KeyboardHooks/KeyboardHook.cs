using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardHooks {

    /// <summary>
    /// Base class for Keyboard Hooks
    /// </summary>
    public abstract class KeyboardHook {

        /// <summary>
        /// Fired, when a key gets pressed or released
        /// </summary>
        public event Action<KeyEventArgs> KeyEvent;

        /// <summary>
        /// Whether this hook can intercept keystrokes
        /// </summary>
        public abstract bool CanIntercept { get; }

        /// <summary>
        /// Whether this hook can distinguish between multiple Keyboards
        /// </summary>
        /// <remarks>Keyboard naming convention will vary between different methods</remarks>
        public abstract bool MultipleKeyboards { get; }

        /// <summary>
        /// Enable the Keyboard Hook
        /// </summary>
        /// <returns></returns>
        public abstract bool Hook();

        /// <summary>
        /// Disable the Keyboard hook
        /// </summary>
        public abstract void Unhook();

        protected virtual void OnKeyEvent(KeyEventArgs obj) {
            KeyEvent?.Invoke(obj);
        }
    }
}