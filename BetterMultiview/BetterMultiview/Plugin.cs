using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using BetterMultiview.Data;
using BetterMultiview.Dialogs;
using ObsInterop;

namespace BetterMultiview.Desktop;

class Plugin {
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .SetupWithoutStarting();

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

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

        Log("Initializing Avalonia");
        Trace.Listeners.Add(new LogListener());
        try {
            Main(new string[0]);
        } catch (Exception ex) {
            Log(ex.Message);
            if (ex.InnerException != null) {
                Log(ex.InnerException.Message);
                Log(ex.InnerException.StackTrace);
            }
        }
        
        Trace.WriteLine("Initializing Trace logger");

        var window = new Multiview();
        window.Show();

        return true;
    }

    public static unsafe void Log(string text) {
        try {
            text = $"[BetterMultiview]: {text}";
            fixed (sbyte* logMessagePtr = text.GetBytes()) {
                ObsBase.blog(ObsBase.LOG_INFO, logMessagePtr);
            }
        } catch {
            Console.WriteLine(text);
        }
    }

    private class LogListener : TraceListener {
        public override void Write(string? message) {
            Log(message ?? "");
        }

        public override void WriteLine(string? message) {
            Log(message ?? "");
        }
    }
}
