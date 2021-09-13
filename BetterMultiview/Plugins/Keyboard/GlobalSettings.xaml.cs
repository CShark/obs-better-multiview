namespace ObsMultiview.Plugins.Keyboard
{
    /// <summary>
    /// Interaktionslogik für GlobalSettings.xaml
    /// </summary>
    public partial class GlobalSettings : SettingsControl<KeyboardSettings>
    {
        public GlobalSettings(CommandFacade management) : base(management) {
            InitializeComponent();
            InputGrabber.SetCommandFacade(management);
            InputGrabber2.SetCommandFacade(management);
        }
    }
}
