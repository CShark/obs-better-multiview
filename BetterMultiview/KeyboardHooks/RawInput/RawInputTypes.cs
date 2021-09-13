using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// Contains some helpers for the RawInput hook

namespace KeyboardHooks.RawInput {
    internal enum HidUsagePage : ushort {
        UNDEFINED = 0x00, // Unknown usage page
        GENERIC = 0x01, // Generic desktop controls
        SIMULATION = 0x02, // Simulation controls
        VR = 0x03, // Virtual reality controls
        SPORT = 0x04, // Sports controls
        GAME = 0x05, // Games controls
        KEYBOARD = 0x07, // Keyboard controls
    }

    internal enum HidUsage : ushort {
        Undefined = 0x00, // Unknown usage
        Pointer = 0x01, // Pointer
        Mouse = 0x02, // Mouse
        Joystick = 0x04, // Joystick
        Gamepad = 0x05, // Game Pad
        Keyboard = 0x06, // Keyboard
        Keypad = 0x07, // Keypad
        SystemControl = 0x80, // Muilt-axis Controller
        Tablet = 0x80, // Tablet PC controls
        Consumer = 0x0C, // Consumer
    }

    [Flags]
    internal enum Flags : uint {
        Remove = 0x01,

        Exclude = 0x10,
        PageOnly = 0x20,
        NoLegacy = 0x30,

        InputSink = 0x100,
        CaptureMouse = 0x200,
        NoHotkeys = 0x200,
        AppKeys = 0x400,

        ExInputSink = 0x1000,
        DevNotify = 0x2000
    }

    internal enum RawDeviceType : int {
        HID = 2,
        Keyboard = 1,
        Mouse = 0
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RawInputDevice {
        public HidUsagePage UsagePage;
        public HidUsage Usage;
        public Flags Flags;
        public IntPtr Target;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point {
        public long X;
        public long Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Msg {
        public IntPtr Hwnd;
        public uint Message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint Time;
        public Point Point;
        public uint lPrivate;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RawInputHeader {
        public uint dwType;
        public uint dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }

    internal struct RawInputKeyboard {
        public ushort MakeCode;
        public ushort Flags;
        public ushort Reserved;
        public ushort VKey;
        public uint Message;
        public ulong ExtraInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RawInput {
        public RawInputHeader Header;
        public RawInputKeyboard Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RawInputDeviceInfo {
        public IntPtr hDevice;
        public RawDeviceType Type;
    }
}