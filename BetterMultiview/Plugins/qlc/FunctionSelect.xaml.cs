using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ObsMultiview.Plugins.qlc
{
    /// <summary>
    /// Interaktionslogik für FunctionSelect.xaml
    /// </summary>
    public partial class FunctionSelect : Window
    {

        public static readonly DependencyProperty SearchTermProperty = DependencyProperty.Register(
            nameof(SearchTerm), typeof(string), typeof(FunctionSelect), new PropertyMetadata(default(string),
                (o, args) => {
                    ((FunctionSelect)o).Refilter();
                }));

        public string SearchTerm {
            get { return (string)GetValue(SearchTermProperty); }
            set { SetValue(SearchTermProperty, value); }
        }

        public static readonly DependencyProperty ScenesProperty = DependencyProperty.Register(
            nameof(Scenes), typeof(CollectionViewSource), typeof(FunctionSelect), new PropertyMetadata(default(CollectionViewSource)));

        public CollectionViewSource Scenes {
            get { return (CollectionViewSource)GetValue(ScenesProperty); }
            set { SetValue(ScenesProperty, value); }
        }

        public static readonly DependencyProperty WidgetsProperty = DependencyProperty.Register(
            nameof(Widgets), typeof(CollectionViewSource), typeof(FunctionSelect), new PropertyMetadata(default(CollectionViewSource)));

        public CollectionViewSource Widgets {
            get { return (CollectionViewSource)GetValue(WidgetsProperty); }
            set { SetValue(WidgetsProperty, value); }
        }

        public FunctionInfo SelectedItem { get; private set; }

        public FunctionSelect(IEnumerable<FunctionInfo> functions) {
            var list = functions.ToList();

            Scenes = new CollectionViewSource();
            Scenes.Source = list.Where(x=>x.Type == FunctionType.Function).ToList();
            Scenes.IsLiveFilteringRequested = true;
            Scenes.Filter += (sender, args) => {
                if (args.Item is FunctionInfo f) {
                    args.Accepted = f.Name.ToLower().Contains(SearchTerm?.ToLower() ?? "");
                }
            };

            Widgets = new CollectionViewSource();
            Widgets.Source = list.Where(x => x.Type == FunctionType.Widget).ToList();
            Widgets.IsLiveFilteringRequested = true;
            Widgets.Filter += (sender, args) => {
                if (args.Item is FunctionInfo f) {
                    args.Accepted = f.Name.ToLower().Contains(SearchTerm?.ToLower() ?? "");
                }
            };

            InitializeComponent();

            txtSearch.Focus();
        }

        private void Refilter() {
            Widgets.View.Refresh();
            Scenes.View.Refresh();
        }

        private void Scenes_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var item = Scenes.View.CurrentItem as FunctionInfo;
            SelectedItem = item;
            DialogResult = true;
            Close();
        }

        private void Widgets_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var item = Widgets.View.CurrentItem as FunctionInfo;
            SelectedItem = item;
            DialogResult = true;
            Close();
        }
    }
}
