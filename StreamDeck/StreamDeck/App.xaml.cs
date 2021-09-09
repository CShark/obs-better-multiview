using System;
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
using Newtonsoft.Json;
using StreamDeck.Data;
using StreamDeck.Services;
using WPFLocalizeExtension.Engine;

namespace StreamDeck {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static IContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e) {
            var builder = new ContainerBuilder();
            var settings = new Settings();

            if (File.Exists("settings.json")) {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            }

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

            Container = builder.Build();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e) {
            var settings = Container.Resolve<Settings>();
            var text = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("settings.json", text);

            var profile = Container.Resolve<ProfileManager>();
            profile.SaveProfile();

            base.OnExit(e);
        }
    }
}