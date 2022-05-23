using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using ObsMultiview.Data;

namespace ObsMultiview.Services {
    public record MonitorInfo(string Name, Point Size, Point Offset) {
    }

    /// <summary>
    /// Some Win32 interop functions
    /// </summary>

    public class Win32Interop {
        private readonly Settings _settings;

        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);


        #region Win32: Query Monitors

        private const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;
        private const uint DDI_GET_TARGET_NAME = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct Luid {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigPathSourceInfo {
            public Luid adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigPathTargetInfo {
            public Luid adapterId;
            public uint id;
            public uint modeInfoIdx;
            private uint outputTechnology;
            private uint rotation;
            private uint scaling;
            private DisplayconfigRational refreshRate;
            private uint scanLineOrdering;
            public bool targetAvailable;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigRational {
            public uint Numerator;
            public uint Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigPathInfo {
            public DisplayconfigPathSourceInfo sourceInfo;
            public DisplayconfigPathTargetInfo targetInfo;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Displayconfig2Dregion {
            public uint cx;
            public uint cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigVideoSignalInfo {
            public ulong pixelRate;
            public DisplayconfigRational hSyncFreq;
            public DisplayconfigRational vSyncFreq;
            public Displayconfig2Dregion activeSize;
            public Displayconfig2Dregion totalSize;
            public uint videoStandard;
            public uint scanLineOrdering;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigTargetMode {
            public DisplayconfigVideoSignalInfo targetVideoSignalInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTL {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigSourceMode {
            public uint width;
            public uint height;
            public uint pixelFormat;
            public POINTL position;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct DisplayconfigModeInfoUnion {
            [FieldOffset(0)]
            public DisplayconfigTargetMode targetMode;

            [FieldOffset(0)]
            public DisplayconfigSourceMode sourceMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigModeInfo {
            public uint infoType;
            public uint id;
            public Luid adapterId;
            public DisplayconfigModeInfoUnion modeInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigTargetDeviceNameFlags {
            public uint value;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DisplayconfigDeviceInfoHeader {
            public uint type;
            public uint size;
            public Luid adapterId;
            public uint id;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DisplayconfigTargetDeviceName {
            public DisplayconfigDeviceInfoHeader header;
            public DisplayconfigTargetDeviceNameFlags flags;
            public uint outputTechnology;
            public ushort edidManufactureId;
            public ushort edidProductCodeId;
            public uint connectorInstance;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string monitorFriendlyDeviceName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string monitorDevicePath;
        }

        [DllImport("user32.dll")]
        private static extern int GetDisplayConfigBufferSizes(
            uint Flags,
            out uint NumPathArrayElements,
            out uint NumModeInfoArrayElements
        );

        [DllImport("user32.dll")]
        private static extern int QueryDisplayConfig(
            uint Flags,
            ref uint NumPathArrayElements,
            [Out] DisplayconfigPathInfo[] PathInfoArray,
            ref uint NumModeInfoArrayElements,
            [Out] DisplayconfigModeInfo[] ModeInfoArray,
            IntPtr CurrentTopologyId
        );

        [DllImport("user32.dll")]
        private static extern int DisplayConfigGetDeviceInfo(
            ref DisplayconfigTargetDeviceName deviceName
        );

        #endregion

        #region Win32: Window Manipulation

        private const uint WM_GETTEXT = 0x000D;
        private const uint WM_CLOSE = 0x0010;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const int GWL_EXSTYLE = -20;

        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;

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

        #endregion

        public Win32Interop(Settings settings) {
            _settings = settings;
        }

        #region Window Manupulation

        public IEnumerable<(IntPtr handle, string title)> GetObsWindows(string titlePart) {
            var procs = Process.GetProcessesByName(_settings.Connection.Process);

            foreach (var proc in procs)
            foreach (ProcessThread thread in proc.Threads) {
                var windows = new List<IntPtr>();

                EnumThreadWindows(thread.Id, (wnd, param) => {
                    windows.Add(wnd);
                    return true;
                }, IntPtr.Zero);

                foreach (var window in windows) {
                    var title = new StringBuilder(512);
                    SendMessage(window, WM_GETTEXT, title.Capacity, title);

                    if (string.IsNullOrWhiteSpace(titlePart) ||
                        title.ToString().ToLower().Contains(titlePart.ToLower()))
                        yield return (window, title.ToString());
                }
            }
        }

        public void PlaceWindowOnScreen(IntPtr handle, MonitorInfo monitor) {
            SetWindowPos(handle, IntPtr.Zero, (int)monitor.Offset.X, (int)monitor.Offset.Y, (int)monitor.Size.X, (int)monitor.Size.Y,
                SWP_NOZORDER | SWP_NOACTIVATE);
        }

        public void CloseWindow(IntPtr window) {
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

        #endregion

        #region Monitor Info

        public IEnumerable<MonitorInfo> GetMonitors() {
            var list = new List<MonitorInfo>();

            GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out var pathSize, out var modeSize);
            var pathInfo = new DisplayconfigPathInfo[pathSize];
            var modeInfo = new DisplayconfigModeInfo[modeSize];

            QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathSize, pathInfo, ref modeSize, modeInfo, IntPtr.Zero);

            pathInfo = pathInfo.OrderBy(x => x.targetInfo.id).ToArray();
            foreach (var path in pathInfo) {
                var src = modeInfo.First(x => x.id == path.sourceInfo.id);
                var target = modeInfo.First(x => x.id == path.targetInfo.id);

                var deviceName = new DisplayconfigTargetDeviceName();
                deviceName.header.size = (uint) Marshal.SizeOf<DisplayconfigTargetDeviceName>();
                deviceName.header.id = target.id;
                deviceName.header.adapterId = target.adapterId;
                deviceName.header.type = DDI_GET_TARGET_NAME;
                DisplayConfigGetDeviceInfo(ref deviceName);

                list.Add(new MonitorInfo(deviceName.monitorFriendlyDeviceName,
                    new Point(src.modeInfo.sourceMode.width, src.modeInfo.sourceMode.height),
                    new Point(src.modeInfo.sourceMode.position.x, src.modeInfo.sourceMode.position.y)));
            }


            var primary = list.First(x => x.Offset.X == 0 && x.Offset.Y == 0);

            return Enumerable.Repeat(primary, 1).Concat(list.Where(x => x != primary));
        }

        #endregion
    }
}