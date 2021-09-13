namespace ObsMultiview.Plugins.qlc {
    /// <summary>
    /// Interaktionslogik für GlobalSettings.xaml
    /// </summary>
    public partial class GlobalSettings : SettingsControl<QlcSettings> {
        public GlobalSettings(CommandFacade management) : base(management) {
            InitializeComponent();
        }
    }
}