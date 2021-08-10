using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeyboardHooks {
    public class KeyEvent {
        public int KeyCode { get; }

        public bool IsDown { get; }

        public KeyEvent(int keyCode, bool isDown) {
            KeyCode = keyCode;
            IsDown = isDown;
        }
    }

    public class HookKeyEvent : KeyEvent {
        public HookKeyEvent(int keyCode, bool isDown) : base(keyCode, isDown) {
        }

        public bool Intercept { get; set; } = false;
    }

    public class RawKeyEvent : KeyEvent {
        public RawKeyEvent(int keyCode, bool isDown, string keyboard) : base(keyCode, isDown) {
            Keyboard = keyboard;
        }

        public string Keyboard { get; }
    }

    public class InterceptionKeyEvent : KeyEvent {
        public InterceptionKeyEvent(int keyCode, bool isDown, int keyboard) : base(keyCode, isDown) {
            Keyboard = keyboard;
        }

        public int Keyboard { get; }
        public bool Intercept { get; set; }
    }

    internal static class Helpers {
        public static int FixVirtualKeyCode(int vk, int scanCode, bool e0) {
            switch (vk) {
                case 17: // Ctrl
                    return e0 ? 163 : 162;
                case 16: // Shift
                    return (ushort) scanCode == 0x36 ? 161 : 160;
                case 18: // Alt
                    return e0 ? 165 : 164;
                case 36: // Home
                    return e0 ? 36 : 103;
                case 38: // Up
                    return e0 ? 38 : 104;
                case 33: // PageUp
                    return e0 ? 33 : 105;
                case 37: // Left
                    return e0 ? 37 : 100;
                case 12: // Clear
                    return e0 ? 12 : 101;
                case 39: // Right
                    return e0 ? 39 : 102;
                case 35: // End
                    return e0 ? 35 : 97;
                case 40: // Down
                    return e0 ? 40 : 98;
                case 34: // PageDown
                    return e0 ? 34 : 99;
                case 45: // Insert
                    return e0 ? 45 : 96;
                case 46: // Delete
                    return e0 ? 46 : 110;
                default:
                    return (int) vk;
            }
        }
    }
}