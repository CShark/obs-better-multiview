using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeyboardHooks {
    /// <summary>
    /// A Key Event from a Keyboard Hook
    /// </summary>
    public sealed class KeyEventArgs {
        /// <summary>
        /// The Virtual Key Code of the Key Event
        /// </summary>
        public int KeyCode { get; }

        /// <summary>
        /// Whether the Key is pressed or released
        /// </summary>
        public bool IsDown { get; }

        /// <summary>
        /// The Keyboard which fired the event if applicable
        /// </summary>
        public string Keyboard { get; }

        /// <summary>
        /// Whether to intercept the keystroke. If null, keystroke cannot be intercepted
        /// </summary>
        public bool? Intercept { get; set; }

        public KeyEventArgs(int keyCode, bool isDown, string keyboard, bool canIntercept) {
            KeyCode = keyCode;
            IsDown = isDown;
            Keyboard = keyboard;
            Intercept = canIntercept ? false : null;
        }
    }
    
    /// <summary>
    /// Some Helpers for processing Key information
    /// </summary>
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