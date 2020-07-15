using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace StreamDeck.Services {
    class KeyboardLedService {

        #region Win32 API
        [DllImport("kernel32.dll")]
        static extern bool DefineDosDevice(uint dwFlags, string lpDeviceName, string lpTargetPath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
            ref KeyboardIndicatorParameters data, uint nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize,
            out uint lpBytesReturned, IntPtr lpOverlapped);

        private const int DddRawTargetPath = 0x00000001;
        private const uint FileAnyAccess = 0;
        private const uint MethodBuffered = 0;
        private const uint FileDeviceKeyboard = 0x0000000b;

        private static uint IOCTL_KEYBOARD_SET_INDICATORS =
            ControlCode(FileDeviceKeyboard, 0x0002, MethodBuffered, FileAnyAccess);

        static uint ControlCode(uint deviceType, uint function, uint method, uint access) {
            return ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);
        }

        #endregion

        private Settings _settings;

        public KeyboardLedService(Settings settings) {
            _settings = settings;
        }

        struct KeyboardIndicatorParameters {
            public ushort UnitId;
            public Locks LedFlags;
        }

        [Flags]
        public enum Locks : ushort {
            None = 0,
            ScrollLock = 0x01,
            NumLock = 0x02,
            CapsLock = 0x04,
            Kana = 0x08,
            InjectLed = 0x80,
            Shadow = 0x40,
        }

        public bool SetStatusLEDs(Locks locks) {
            var success = DefineDosDevice(DddRawTargetPath, "StreamDeckKeyboard", _settings.KeyboardDevice);

            if (success) {
                var data = new KeyboardIndicatorParameters {
                    LedFlags=locks,
                    UnitId = 0,
                };

                var handle = CreateFile(@"\\.\StreamDeckKeyboard", FileAccess.Write, FileShare.Write, IntPtr.Zero,
                    FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

                if (!handle.IsInvalid) {
                    var size = Marshal.SizeOf(data);
                    uint bytesReturned;

                    DeviceIoControl(handle.DangerousGetHandle(), IOCTL_KEYBOARD_SET_INDICATORS, ref data, (uint) size,
                        IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);

                    handle.Close();

                    return true;
                }
            }

            return false;
        }
    }
}