using ObsInterop;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BetterMultiview {
    public class Plugin {
        private static nint _obsModulePointer;

        [UnmanagedCallersOnly(EntryPoint = "obs_module_set_pointer", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static void SetPointer(nint obsModulePointer) {
            _obsModulePointer = obsModulePointer;
        }

        [UnmanagedCallersOnly(EntryPoint = "obs_module_ver", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static uint GetVersion() {
            var maj = (uint)Obs.Version.Major;
            var min = (uint)Obs.Version.Minor;
            var build = (uint)Obs.Version.Build;

            return (maj << 24) | (min << 16) | (build);
        }

        [UnmanagedCallersOnly(EntryPoint = "obs_module_load", CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe bool ModuleLoad() {
            Log("Loaded!");
            var window = new Multiview();
            window.Show();
            return true;
        }

        private static unsafe void Log(string text) {
            text = $"[BetterMultiview]: {text}";
            var asciiBytes = Encoding.UTF8.GetBytes(text);
            fixed (byte* logMessagePtr = asciiBytes) {
                ObsBase.blog(ObsBase.LOG_INFO, (sbyte*)logMessagePtr);
            }
        }
    }
}