using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autofac;
using AutofacSerilogIntegration;
using Serilog;
using StreamDeck.Services;

namespace StreamDeck {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {
        public static IContainer Container { get; private set; }

        private ILogger _log;

        protected override void OnStartup(StartupEventArgs e) {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.File("logs/mainlog.log",
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u5}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            _log = Log.ForContext<App>();
            _log.Information("Starting Application");

            AppDomain.CurrentDomain.FirstChanceException += (sender, args) => {
                _log.Fatal(args.Exception, "Application shutdown unexpectedly");
            };

            var builder = new ContainerBuilder();
            builder.RegisterType<Settings>().AsSelf().SingleInstance();
            builder.RegisterType<ProfileSettings>().AsSelf().SingleInstance();
            builder.RegisterType<ObsService>().AsSelf().SingleInstance();
            builder.RegisterType<KeyboardLedService>().AsSelf().SingleInstance();

            builder.RegisterType<Services.Interceptor>().AsSelf().SingleInstance();
            builder.RegisterType<RawInput>().AsSelf().SingleInstance();
            builder.RegisterType<KeyboardService>().AsSelf().SingleInstance();

            builder.RegisterType<MultiviewOverlay>().AsSelf();

            builder.RegisterLogger();

            Container = builder.Build();

            base.OnStartup(e);
        }
    }
}