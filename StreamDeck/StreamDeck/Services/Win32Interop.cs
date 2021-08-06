using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using StreamDeck.Data;

namespace StreamDeck.Services {
    public class Win32Interop {
        private readonly Settings _settings;

        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        private const uint WM_GETTEXT = 0x000D;
        private const uint WM_CLOSE = 0x0010;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const int GWL_EXSTYLE = (-20);

        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;


        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy,
            uint flags);


        public Win32Interop(Settings settings) {
            _settings = settings;
        }

        public IEnumerable<(IntPtr handle, string title)> GetObsWindows(string titlePart) {
            var procs = Process.GetProcessesByName(_settings.Connection.Process);

            foreach (var proc in procs) {
                foreach (ProcessThread thread in proc.Threads) {
                    List<IntPtr> windows = new List<IntPtr>();

                    EnumThreadWindows(thread.Id, (wnd, param) => {
                        windows.Add(wnd);
                        return true;
                    }, IntPtr.Zero);

                    foreach (var window in windows) {
                        var title = new StringBuilder(512);
                        SendMessage(window, WM_GETTEXT, title.Capacity, title);

                        if (string.IsNullOrWhiteSpace(titlePart) ||
                            title.ToString().ToLower().Contains(titlePart.ToLower())) {
                            yield return (window, title.ToString());
                        }
                    }
                }
            }
        }

        public void CloseWindow(IntPtr window) {
            //DestroyWindow(window);
            SendMessage(window, WM_CLOSE, 0, null);
        }

        public void ShowWindowBehind(IntPtr window, Window front) {
            var handle = new WindowInteropHelper(front).Handle;
            SetWindowPos(window, handle, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOMOVE);
        }

        public void HideAltTab(IntPtr window) {
            var style = (uint) GetWindowLong(window, GWL_EXSTYLE);
            style |= WS_EX_TOOLWINDOW;
            SetWindowLongPtr(window, GWL_EXSTYLE, (IntPtr) style);
        }
    }
}