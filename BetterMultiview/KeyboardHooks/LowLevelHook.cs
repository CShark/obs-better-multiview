using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeyboardHooks {
    /// <summary>
    /// A Keyboard Hook using the LowLevel Hook of the Win32 API
    /// </summary>
    public sealed class LowLevelHook : KeyboardHook {
        #region Win32 API
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);
        #endregion


        private IntPtr _hook;
        private LowLevelKeyboardProc _handler;
        private GCHandle _gcHandler;
        
        private int _lastKey;
        private DateTime _lastPress;

        public LowLevelHook() {
            // For Win32 Api calls, delegates need to be at a fixed position in memory
            _handler = Handler;
            _gcHandler = GCHandle.Alloc(_handler);
        }

        ~LowLevelHook() {
            _gcHandler.Free();
        }

        /// <inheritdoc/>
        public override bool CanIntercept => true;

        /// <inheritdoc/>
        public override bool MultipleKeyboards => false;
        
        /// <inheritdoc/>
        public override bool Hook() {
            if (_hook == IntPtr.Zero) {
                var modPtr = GetModuleHandle(null);

                _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _handler, modPtr, 0);
            }

            return _hook != IntPtr.Zero;
        }
        
        private IntPtr Handler(int code, IntPtr param, IntPtr lParam) {
            if (code >= 0) {
                var args = new KeyEventArgs(Marshal.ReadInt32(lParam),
                    param == (IntPtr) WM_KEYDOWN || param == (IntPtr) WM_SYSKEYDOWN, null, true);

                if (_lastKey == args.KeyCode && (DateTime.Now - _lastPress).TotalMilliseconds < 100) {
                    Console.WriteLine("Double detection");
                    return CallNextHookEx(_hook, code, param, lParam);
                }

                _lastKey = args.KeyCode;
                _lastPress = DateTime.Now;

                OnKeyEvent(args);

                if (args.Intercept == true) {
                    return (IntPtr) (-1);
                }
            }

            return CallNextHookEx(_hook, code, param, lParam);
        }

        /// <inheritdoc/>
        public override void Unhook() {
            if (_hook != IntPtr.Zero) {
                UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
            }
        }
    }
}