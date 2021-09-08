﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeyboardHooks {
    public class KeyboardHook {
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


        private IntPtr _hook;
        private LowLevelKeyboardProc _handler;
        private GCHandle _gcHandler;

        public event Action<HookKeyEvent> KeyEvent;

        public KeyboardHook() {
            _handler = Handler;
            _gcHandler = GCHandle.Alloc(_handler);
        }

        ~KeyboardHook() {
            _gcHandler.Free();
        }

        public bool Hook() {
            if (_hook == IntPtr.Zero) {
                var modPtr = GetModuleHandle(null);

                _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _handler, modPtr, 0);
            }

            return _hook != IntPtr.Zero;
        }

        private IntPtr Handler(int code, IntPtr param, IntPtr lParam) {
            if (code >= 0) {
                var args = new HookKeyEvent(Marshal.ReadInt32(lParam),
                    param == (IntPtr) WM_KEYDOWN || param == (IntPtr) WM_SYSKEYDOWN);

                OnKeyEvent(args);

                if (args.Intercept) {
                    return (IntPtr) (-1);
                }
            }

            return CallNextHookEx(_hook, code, param, lParam);
        }

        public void Unhook() {
            if (_hook != IntPtr.Zero) {
                UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
            }
        }

        protected virtual void OnKeyEvent(HookKeyEvent obj) {
            KeyEvent?.Invoke(obj);
        }
    }
}