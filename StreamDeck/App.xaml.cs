using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autofac;
using StreamDeck.Services;

namespace StreamDeck {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {
        public static IContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e) {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new Settings()).AsSelf().SingleInstance();
            builder.RegisterInstance(new ProfileSettings()).AsSelf().SingleInstance();
            builder.RegisterType<ObsService>().AsSelf().SingleInstance();
            builder.RegisterType<KeyboardLedService>().AsSelf().SingleInstance();

            builder.RegisterType<MultiviewOverlay>().AsSelf();

            Container = builder.Build();

            

            base.OnStartup(e);
        }
    }
}
