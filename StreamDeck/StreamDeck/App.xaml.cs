﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;
using StreamDeck.Data;
using StreamDeck.Services;
using WPFLocalizeExtension.Engine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace StreamDeck {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static IContainer Container { get; private set; }

        private ILogger _logger;

        protected override void OnStartup(StartupEventArgs e) {
            var builder = new ContainerBuilder();
            var settings = new Settings();

            if (File.Exists("settings.json")) {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            }

            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Is(settings.LogLevel)
                .Enrich.FromLogContext()
                .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

            try {
                var culture = CultureInfo.GetCultureInfo(settings.Language);
                LocalizeDictionary.Instance.Culture = culture;
            } catch {
                LocalizeDictionary.Instance.Culture = CultureInfo.CurrentCulture;
            }

            builder.Register(x => settings).AsSelf().SingleInstance();
            builder.RegisterType<ObsWatchService>().AsSelf().SingleInstance();
            builder.RegisterType<ProfileManager>().AsSelf().SingleInstance();
            builder.RegisterType<ProfileWatcher>().AsSelf().SingleInstance();
            builder.RegisterType<Win32Interop>().AsSelf().SingleInstance();
            builder.RegisterType<SceneService>().AsSelf().SingleInstance();
            builder.RegisterType<PluginService>().AsSelf().SingleInstance();
            builder.RegisterSerilog(logConfig);

            Container = builder.Build();

            _logger = Container.Resolve<ILogger<App>>();
            _logger.LogInformation("-------------------");
            _logger.LogInformation("Application startup");

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                _logger.LogCritical(args.ExceptionObject as Exception, "Critical error, shutting down");
            };

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e) {
            var settings = Container.Resolve<Settings>();
            var text = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("settings.json", text);

            var profile = Container.Resolve<ProfileManager>();
            profile.SaveProfile();

            _logger.LogInformation("Shutting down");
            base.OnExit(e);
        }
    }
}