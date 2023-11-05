﻿using Avalonia;
using System.Reflection.PortableExecutable;

namespace BetterMultiview.Design {
    class Program {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}