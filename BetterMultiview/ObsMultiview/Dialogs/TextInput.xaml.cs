using System.Windows;

namespace ObsMultiview.Dialogs {
    /// <summary>
    /// Interaktionslogik für TextInput.xaml
    /// </summary>
    public partial class TextInput : Window {
        public static readonly DependencyProperty DialogTitleProperty = DependencyProperty.Register(
            nameof(DialogTitle), typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

        public string DialogTitle {
            get { return (string) GetValue(DialogTitleProperty); }
            set { SetValue(DialogTitleProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

        public string Message {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

        public string Value {
            get { return (string) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public TextInput(string dialogTitle, string message, string defaultValue = "") {
            DialogTitle = dialogTitle;
            Message = message;
            Value = defaultValue;
            InitializeComponent();

            input.Focus();
            input.SelectAll();
        }

        private void Confirm_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}