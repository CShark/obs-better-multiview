using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using KeyboardHooks.Interceptor;

namespace KeyboardHooks {
    public class DriverHook {
        private IntPtr _context;
        private Thread _callback;

        public event Action<InterceptionKeyEvent> KeyEvent;

        private const uint MAP_SCAN_TO_VK = 1;

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint code, uint mapping);

        public bool Hook() {
            if (_context == IntPtr.Zero) {
                try {
                    _context = InterceptionDriver.CreateContext();

                    if (_context != IntPtr.Zero) {
                        _callback = new Thread(MessagePull);
                        _callback.IsBackground = true;
                        _callback.Start();
                    }
                } catch (DllNotFoundException ex) {
                    throw;
                }
            }

            return _context != IntPtr.Zero;
        }

        private void MessagePull() {
            InterceptionDriver.SetFilter(_context, InterceptionDriver.IsKeyboard,
                (int) (KeyboardFilterMode.All));

            var stroke = new Stroke();
            int deviceId;

            while (InterceptionDriver.Receive(_context, deviceId = InterceptionDriver.Wait(_context), ref stroke, 1) >
                   0) {
                var vk = MapVirtualKey((ushort) stroke.Key.Code, MAP_SCAN_TO_VK);
                vk = (uint) Helpers.FixVirtualKeyCode((int) vk, stroke.Key.Code, HasE0(stroke.Key.Code));
                var args = new InterceptionKeyEvent((int) vk, stroke.Key.State.HasFlag(KeyState.Down), deviceId);

                OnKeyEvent(args);

                if (!args.Intercept) {
                    InterceptionDriver.Send(_context, deviceId, ref stroke, 1);
                }
            }
        }

        public void InjectKey(int device, ushort scanCode) {
            var stroke = new Stroke();
            stroke.Key.State = KeyState.Down;
            stroke.Key.Code = scanCode;
            InterceptionDriver.Send(_context, device, ref stroke, 1);

            stroke.Key.State = KeyState.Up;
            InterceptionDriver.Send(_context, device, ref stroke, 1);
        }

    public void Unhook() {
            if (_context != IntPtr.Zero) {
                InterceptionDriver.DestroyContext(_context);
                _context = IntPtr.Zero;
            }
        }
        
        protected virtual void OnKeyEvent(InterceptionKeyEvent obj) {
            KeyEvent?.Invoke(obj);
        }

        private bool HasE0(int scanCode) {
            return (scanCode & 0x20000) == 0x20000;
        }
    }
}