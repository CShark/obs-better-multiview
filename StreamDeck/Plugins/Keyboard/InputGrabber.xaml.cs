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
using System.Windows.Navigation;
using System.Windows.Shapes;
using KeyboardHooks;

namespace StreamDeck.Plugins.Keyboard {
    /// <summary>
    /// Records a keystroke for later use in a Keyboard Hook
    /// </summary>
    public partial class InputGrabber : UserControl {
        public static readonly DependencyProperty CapturingProperty = DependencyProperty.Register(
            nameof(Capturing), typeof(bool), typeof(InputGrabber), new PropertyMetadata(default(bool)));

        public bool Capturing {
            get { return (bool) GetValue(CapturingProperty); }
            set { SetValue(CapturingProperty, value); }
        }

        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(
            nameof(Error), typeof(bool), typeof(InputGrabber), new PropertyMetadata(default(bool)));

        public bool Error {
            get { return (bool) GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public static readonly DependencyProperty KeyboardProperty = DependencyProperty.Register(
            nameof(Keyboard), typeof(string), typeof(InputGrabber),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((InputGrabber) o).ToReadableKeyboard()));

        public string Keyboard {
            get { return (string) GetValue(KeyboardProperty); }
            set { SetValue(KeyboardProperty, value); }
        }

        public static readonly DependencyProperty ReadableKeyboardProperty = DependencyProperty.Register(
            nameof(ReadableKeyboard), typeof(string), typeof(InputGrabber), new PropertyMetadata(default(string)));

        public string ReadableKeyboard {
            get { return (string) GetValue(ReadableKeyboardProperty); }
            set { SetValue(ReadableKeyboardProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
            nameof(Key), typeof(string), typeof(InputGrabber), new PropertyMetadata(default(string)));

        public string Key {
            get { return (string) GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty VirtualKeyProperty = DependencyProperty.Register(
            nameof(VirtualKey), typeof(int), typeof(InputGrabber),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((InputGrabber) o).ToReadableKey()));

        public int VirtualKey {
            get { return (int) GetValue(VirtualKeyProperty); }
            set { SetValue(VirtualKeyProperty, value); }
        }

        public static readonly DependencyProperty DriverModeProperty = DependencyProperty.Register(
            nameof(DriverMode), typeof(bool), typeof(InputGrabber), new PropertyMetadata(default(bool)));

        public bool DriverMode {
            get { return (bool) GetValue(DriverModeProperty); }
            set { SetValue(DriverModeProperty, value); }
        }

        private KeyboardCore _keyboard;
        private KeyboardCoreSettings _settings;

        public InputGrabber() {
            InitializeComponent();
        }

        public void SetCommandFacade(CommandFacade management) {
            Unloaded += (sender, args) => { _keyboard.Disable(); };

            management.SettingsChanged += s => {
                if (s == "core") {
                    _settings = management.RequestSettings<KeyboardCoreSettings>("core");
                }
            };
            _settings = management.RequestSettings<KeyboardCoreSettings>("core");

            _keyboard = new KeyboardCore(management);
            _keyboard.KeyEvent += evt => {
                Dispatcher.Invoke(() => {
                    Keyboard = evt.Keyboard;
                    VirtualKey = evt.VirtualKey;

                    Capturing = false;
                    _keyboard.Disable();
                });

                evt.Cancel = true;
            };
        }

        private void InputGrabber_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Capturing = !Capturing;

            if (Capturing) {
                Error = !_keyboard.Enable(DriverMode);
                Capturing = _keyboard.IsEnabled;
            } else {
                Error = false;
                _keyboard.Disable();
            }
        }

        private void InputGrabber_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            Key = null;
            Keyboard = null;
        }

        private void ToReadableKey() {
            Key = KeyInterop.KeyFromVirtualKey(VirtualKey).ToString();
        }

        private void ToReadableKeyboard() {
            if (Keyboard != null) {
                ReadableKeyboard = _settings.KeyboardLabels.ContainsKey(Keyboard)
                    ? _settings.KeyboardLabels[Keyboard]
                    : Keyboard;
            } else {
                ReadableKeyboard = "";
            }
        }
    }
}