using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KeyboardHooks.RawInput;

namespace KeyboardHooks {
    public class RawInputHook {
        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);
        private const uint WM_DESTROY = 2;
        private const uint WM_QUIT = 18;
        private const uint WM_INPUT = 255;
        private const uint WM_USB_DEVICECHANGE = 0x219;
        private const uint RID_INPUT = 0x10000003;
        private const uint RI_KEY_BREAK = 0x01;
        private const uint RI_KEY_E0 = 0x02;
        private const uint RIDI_DEVICENAME = 0x20000007;

        [DllImport("user32.dll")]
        private static extern IntPtr CreateWindowEx(uint exStyle, string className, string windowName, uint style,
            int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool GetMessage(ref Msg data, IntPtr window, uint filterMin, uint filterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref Msg data);

        [DllImport("user32.dll")]
        private static extern long DispatchMessage(ref Msg data);

        [DllImport("user32.dll")]
        private static extern bool RegisterRawInputDevices(RawInputDevice[] devices, uint count, uint size);

        [DllImport("user32.dll")]
        private static extern uint GetRawInputData(IntPtr input, uint command, IntPtr data, ref uint size,
            int headerSize);

        [DllImport("user32.dll")]
        private static extern uint GetRawInputData(IntPtr input, uint command, out RawInput.RawInput data,
            ref uint size,
            int headerSize);

        [DllImport("user32.dll")]
        private static extern uint GetRawInputDeviceList(IntPtr data, ref uint numDevices, uint size);

        [DllImport("user32.dll")]
        private static extern uint GetRawInputDeviceInfo(IntPtr device, uint command, IntPtr name, ref int size);

        public event Action<RawKeyEvent> KeyEvent;

        private IntPtr _window;
        private Thread _messagePump;

        private Dictionary<IntPtr, string> _deviceNames = new Dictionary<IntPtr, string>();

        public bool Hook() {
            if (_window == IntPtr.Zero) {
                _messagePump = new Thread(MessagePump);
                _messagePump.IsBackground = true;
                _messagePump.Start();
            }

            return true;
        }

        private void MessagePump() {
            var proc = Process.GetCurrentProcess();
            var modId = proc.MainModule.ModuleName;
            var modPtr = GetModuleHandle(modId);

            _window = CreateWindowEx(0, "STATIC", "RawInputWindow", 0, 0, 0, 0, 0, HWND_MESSAGE, IntPtr.Zero,
                modPtr,
                IntPtr.Zero);

            var rid = new RawInputDevice[1];
            rid[0].Target = _window;
            rid[0].Flags = Flags.InputSink | Flags.DevNotify;
            rid[0].UsagePage = HidUsagePage.GENERIC;
            rid[0].Usage = HidUsage.Keyboard;

            RegisterRawInputDevices(rid, 1, (uint) Marshal.SizeOf<RawInputDevice>());
            EnumerateDevices();

            var msg = new Msg();
            while (GetMessage(ref msg, _window, 0, 0)) {
                if (_window == IntPtr.Zero)
                    break;

                if (msg.Message == WM_DESTROY || msg.Message == WM_QUIT) {
                    break;
                }

                if (msg.Message == WM_INPUT) {
                    uint size = 0;
                    GetRawInputData(msg.lParam, RID_INPUT, IntPtr.Zero, ref size, Marshal.SizeOf<RawInputHeader>());
                    if (size == GetRawInputData(msg.lParam, RID_INPUT, out var input, ref size,
                        Marshal.SizeOf<RawInputHeader>())) {
                        var vk = Helpers.FixVirtualKeyCode(input.Keyboard.VKey, input.Keyboard.MakeCode,
                            (input.Keyboard.Flags & RI_KEY_E0) != 0);

                        var args = new RawKeyEvent(vk, (input.Keyboard.Flags & RI_KEY_BREAK) != 0,
                            _deviceNames[input.Header.hDevice]);
                        OnKeyEvent(args);
                    } else {
                        // Broken message
                    }
                } else if (msg.Message == WM_USB_DEVICECHANGE) {
                    EnumerateDevices();
                }

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        private void EnumerateDevices() {
            _deviceNames.Clear();

            _deviceNames.Add(IntPtr.Zero, "Global Keyboard");

            uint devices = 0;
            uint size = (uint) Marshal.SizeOf<RawInputDeviceInfo>();

            if (GetRawInputDeviceList(IntPtr.Zero, ref devices, size) == 0) {
                var list = Marshal.AllocHGlobal((int) (size * devices));
                GetRawInputDeviceList(list, ref devices, size);

                for (int i = 0; i < devices; i++) {
                    var device = Marshal.PtrToStructure<RawInputDeviceInfo>(new IntPtr(list.ToInt64() + size * i));

                    if (device.Type == RawDeviceType.Keyboard) {
                        var length = 0;
                        GetRawInputDeviceInfo(device.hDevice, RIDI_DEVICENAME, IntPtr.Zero, ref length);
                        var namePtr = Marshal.AllocHGlobal(length);
                        GetRawInputDeviceInfo(device.hDevice, RIDI_DEVICENAME, namePtr, ref length);
                        var name = Marshal.PtrToStringAnsi(namePtr);

                        _deviceNames.Add(device.hDevice, name);
                    }
                }
            }
        }

        public void Unhook() {
            if (_window != IntPtr.Zero) {
                DestroyWindow(_window);
                _window = IntPtr.Zero;
            }
        }

        protected virtual void OnKeyEvent(RawKeyEvent obj) {
            KeyEvent?.Invoke(obj);
        }
    }
}