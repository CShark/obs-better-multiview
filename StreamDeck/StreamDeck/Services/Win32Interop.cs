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
    public record MonitorInfo(string Name, Point Size, Point Offset) {
    }

    public class Win32Interop {
        private readonly Settings _settings;

        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);


        #region Query Monitors

        private const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;

        private enum DisplayconfigVideoOutputTechnology : uint {
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_OTHER = 0xFFFFFFFF,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15 = 0,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO = 1,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO = 2,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO = 3,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI = 4,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI = 5,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_LVDS = 6,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_D_JPN = 8,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDI = 9,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EXTERNAL = 10,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED = 11,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EXTERNAL = 12,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED = 13,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDTVDONGLE = 14,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_MIRACAST = 15,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL = 0x80000000,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_FORCE_UINT32 = 0xFFFFFFFF
        }

        private enum DisplayconfigScanlineOrdering : uint {
            DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED = 0,
            DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE = 1,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED = 2,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST = DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST = 3,
            DISPLAYCONFIG_SCANLINE_ORDERING_FORCE_UINT32 = 0xFFFFFFFF
        }

        private enum DisplayconfigRotation : uint {
            DISPLAYCONFIG_ROTATION_IDENTITY = 1,
            DISPLAYCONFIG_ROTATION_ROTATE90 = 2,
            DISPLAYCONFIG_ROTATION_ROTATE180 = 3,
            DISPLAYCONFIG_ROTATION_ROTATE270 = 4,
            DISPLAYCONFIG_ROTATION_FORCE_UINT32 = 0xFFFFFFFF
        }

        private enum DisplayconfigScaling : uint {
            DISPLAYCONFIG_SCALING_IDENTITY = 1,
            DISPLAYCONFIG_SCALING_CENTERED = 2,
            DISPLAYCONFIG_SCALING_STRETCHED = 3,
            DISPLAYCONFIG_SCALING_ASPECTRATIOCENTEREDMAX = 4,
            DISPLAYCONFIG_SCALING_CUSTOM = 5,
            DISPLAYCONFIG_SCALING_PREFERRED = 128,
            DISPLAYCONFIG_SCALING_FORCE_UINT32 = 0xFFFFFFFF
        }

        private enum DisplayconfigPixelformat : uint {
            DISPLAYCONFIG_PIXELFORMAT_8BPP = 1,
            DISPLAYCONFIG_PIXELFORMAT_16BPP = 2,
            DISPLAYCONFIG_PIXELFORMAT_24BPP = 3,
            DISPLAYCONFIG_PIXELFORMAT_32BPP = 4,
            DISPLAYCONFIG_PIXELFORMAT_NONGDI = 5,
            DISPLAYCONFIG_PIXELFORMAT_FORCE_UINT32 = 0xffffffff
        }

        private enum DisplayconfigModeInfoType : uint {
            DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1,
            DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2,
            DISPLAYCONFIG_MODE_INFO_TYPE_FORCE_UINT32 = 0xFFFFFFFF
        }

        private enum DisplayconfigDeviceInfoType : uint {
            DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE = 3,
            DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME = 4,
            DISPLAYCONFIG_DEVICE_INFO_SET_TARGET_PERSISTENCE = 5,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_BASE_TYPE = 6,
            DISPLAYCONFIG_DEVICE_INFO_FORCE_UINT32 = 0xFFFFFFFF
        }

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
            private DisplayconfigVideoOutputTechnology outputTechnology;
            private DisplayconfigRotation rotation;
            private DisplayconfigScaling scaling;
            private DisplayconfigRational refreshRate;
            private DisplayconfigScanlineOrdering scanLineOrdering;
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
            public DisplayconfigScanlineOrdering scanLineOrdering;
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
            public DisplayconfigPixelformat pixelFormat;
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
            public DisplayconfigModeInfoType infoType;
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
            public DisplayconfigDeviceInfoType type;
            public uint size;
            public Luid adapterId;
            public uint id;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DisplayconfigTargetDeviceName {
            public DisplayconfigDeviceInfoHeader header;
            public DisplayconfigTargetDeviceNameFlags flags;
            public DisplayconfigVideoOutputTechnology outputTechnology;
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

        private const uint WM_GETTEXT = 0x000D;
        private const uint WM_CLOSE = 0x0010;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const int GWL_EXSTYLE = -20;

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

        public IEnumerable<MonitorInfo> GetMonitors() {
            var list = new List<MonitorInfo>();

            GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out var pathSize, out var modeSize);
            var pathInfo = new DisplayconfigPathInfo[pathSize];
            var modeInfo = new DisplayconfigModeInfo[modeSize];

            QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathSize, pathInfo, ref modeSize, modeInfo, IntPtr.Zero);

            foreach (var path in pathInfo) {
                var src = modeInfo.First(x => x.id == path.sourceInfo.id);
                var target = modeInfo.First(x => x.id == path.targetInfo.id);

                var deviceName = new DisplayconfigTargetDeviceName();
                deviceName.header.size = (uint) Marshal.SizeOf<DisplayconfigTargetDeviceName>();
                deviceName.header.id = target.id;
                deviceName.header.adapterId = target.adapterId;
                deviceName.header.type = DisplayconfigDeviceInfoType.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                DisplayConfigGetDeviceInfo(ref deviceName);

                list.Add(new MonitorInfo(deviceName.monitorFriendlyDeviceName,
                    new Point(src.modeInfo.sourceMode.width, src.modeInfo.sourceMode.height),
                    new Point(src.modeInfo.sourceMode.position.x, src.modeInfo.sourceMode.position.y)));
            }


            return list;
        }
    }
}