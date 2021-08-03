using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Core;
using Newtonsoft.Json;
using StreamDeck.Data;
using StreamDeck.Services;

namespace StreamDeck
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IContainer Container {get; private set; }

        protected override void OnStartup(StartupEventArgs e) {
            var builder = new ContainerBuilder();
            var settings = new Settings();

            if (File.Exists("settings.json")) {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            }

            builder.Register(x => settings).AsSelf().SingleInstance();
            builder.RegisterType<ObsWatchService>().AsSelf().SingleInstance();
            builder.RegisterType<ScenePreview>().AsSelf();

            Container = builder.Build();
            base.OnStartup(e);
        }
    }
}
