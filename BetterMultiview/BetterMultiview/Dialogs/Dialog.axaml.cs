using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using System.Threading.Tasks;

namespace BetterMultiview.Dialogs {
    public partial class Dialog : Window {
        public static readonly StyledProperty<object> DialogContentProperty =
            AvaloniaProperty.Register<Dialog, object>(nameof(DialogContent));

        public object DialogContent {
            get => GetValue(DialogContentProperty);
            set => SetValue(DialogContentProperty, value);
        }

        public static readonly StyledProperty<DataTemplate?> DialogTemplateProperty =
            AvaloniaProperty.Register<Dialog, DataTemplate?>(nameof(DialogTemplate));

        public DataTemplate? DialogTemplate {
            get => GetValue(DialogTemplateProperty);
            set => SetValue(DialogTemplateProperty, value);
        }

        public bool? DialogResult { get; private set; } = null;

        public Dialog() {
            InitializeComponent();
        }

        public void Apply_Click(object sender, RoutedEventArgs args) {
            if (Design.IsDesignMode) return;
            DialogResult = true;
            Close();
        }

        public void Cancel_Click(object sender, RoutedEventArgs args) {
            if (Design.IsDesignMode) return;
            DialogResult = false;
            Close();
        }

        public async Task<bool?> Show(Window owner, string title, object content, DataTemplate? template = null) {
            Title = title;
            DialogContent = content;
            DialogTemplate = template;

            await ShowDialog(owner);
            return DialogResult;
        }
    }
}
