using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using KeyboardHooks.Interceptor;

namespace KeyboardHooks {
    /// <summary>
    /// A Keyboard Hook using the Interception driver <see cref="!:https://github.com/oblitum/Interception"/>
    /// </summary>
    public sealed class DriverHook : KeyboardHook {
        private IntPtr _context;
        private Thread _callback;

        #region Win32 API

        private const uint MAP_SCAN_TO_VK = 1;

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint code, uint mapping);

        #endregion

        public bool IsEnabled => _context != IntPtr.Zero;

        public override bool CanIntercept => true;
        public override bool MultipleKeyboards => true;

        public override bool Hook() {
            if (_context == IntPtr.Zero) {
                try {
                    _context = InterceptionDriver.CreateContext();

                    if (_context != IntPtr.Zero) {
                        _callback = new Thread(MessagePull);
                        _callback.IsBackground = true;
                        _callback.Start();
                    }
                } catch (DllNotFoundException ex) {
                    return false;
                    throw;
                }
            }
            
            return _context != IntPtr.Zero;
        }

        /// <summary>
        /// A message loop for driver messages
        /// </summary>
        private void MessagePull() {
            InterceptionDriver.SetFilter(_context, InterceptionDriver.IsKeyboard,
                (int) (KeyboardFilterMode.All));

            var stroke = new Stroke();
            int deviceId;

            while (InterceptionDriver.Receive(_context, deviceId = InterceptionDriver.Wait(_context), ref stroke, 1) >
                   0) {
                var vk = MapVirtualKey((ushort) stroke.Key.Code, MAP_SCAN_TO_VK);
                vk = (uint) Helpers.FixVirtualKeyCode((int) vk, stroke.Key.Code, HasE0(stroke.Key.Code));
                var args = new KeyEventArgs((int) vk, stroke.Key.State.HasFlag(KeyState.Down), deviceId.ToString(),
                    true);

                OnKeyEvent(args);

                if (args.Intercept != true) {
                    InterceptionDriver.Send(_context, deviceId, ref stroke, 1);
                }
            }
        }

        /// <summary>
        /// Inject a key into the driver, which then is handled as a normal key press by the system
        /// </summary>
        /// <param name="device">The device id to inject from</param>
        /// <param name="scanCode">The scan code to inject</param>
        /// <param name="noRelease">Whether to not fire the Up-Key Event as well</param>
        public void InjectKey(int device, ushort scanCode, bool noRelease = false) {
            var stroke = new Stroke();
            stroke.Key.State = KeyState.Down;
            stroke.Key.Code = scanCode;
            InterceptionDriver.Send(_context, device, ref stroke, 1);

            if (!noRelease) {
                stroke.Key.State = KeyState.Up;
                InterceptionDriver.Send(_context, device, ref stroke, 1);
            }
        }

        public override void Unhook() {
            if (_context != IntPtr.Zero) {
                InterceptionDriver.DestroyContext(_context);
                _context = IntPtr.Zero;
            }
        }

        private bool HasE0(int scanCode) {
            return (scanCode & 0x20000) == 0x20000;
        }
    }
}