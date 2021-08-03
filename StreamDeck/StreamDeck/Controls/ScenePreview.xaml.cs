using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autofac;

namespace StreamDeck.Controls {
    /// <summary>
    /// Interaktionslogik für ScenePreview.xaml
    /// </summary>
    public partial class ScenePreview : UserControl {
        public static readonly DependencyProperty SceneNameProperty = DependencyProperty.Register(
            nameof(SceneName), typeof(string), typeof(ScenePreview), new PropertyMetadata(default(string)));

        public string SceneName {
            get { return (string) GetValue(SceneNameProperty); }
            set { SetValue(SceneNameProperty, value); }
        }

        private readonly Services.ScenePreview _service;
        private readonly Timer _previewTimer;
        
        public ScenePreview() {
            InitializeComponent();

            _service = App.Container.Resolve<Services.ScenePreview>();

            _previewTimer = new Timer(state => {
                try {
                    string scene = "";
                    Dispatcher.Invoke(() => scene = SceneName);

                    var img = _service.GetSnapshot(scene);
                    if (img != null) {
                        Dispatcher.InvokeAsync(() => {
                            var bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.StreamSource = new MemoryStream(img);
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.EndInit();

                            PreviewImage.Source = bmp;
                        });
                    }
                } catch (Exception ex) {

                }
            }, null, 0,50);
        }


        private void ScenePreview_OnUnloaded(object sender, RoutedEventArgs e) {
            _previewTimer.Dispose();
        }
    }
}