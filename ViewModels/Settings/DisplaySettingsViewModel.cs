using System.Collections.ObjectModel;

namespace WWXMapEditor.ViewModels.Settings
{
    public class DisplaySettingsViewModel : SettingsPageViewModelBase
    {
        private bool _startInFullscreen = true;
        private bool _rememberWindowPosition = true;
        private string _multiMonitorBehavior = "Primary monitor";
        private string _uiScaling = "75%";
        private string _fontSize = "Medium";
        private bool _showTooltips = true;
        private int _tooltipDelay = 500;

        public bool StartInFullscreen
        {
            get => _startInFullscreen;
            set => SetProperty(ref _startInFullscreen, value);
        }

        public bool RememberWindowPosition
        {
            get => _rememberWindowPosition;
            set => SetProperty(ref _rememberWindowPosition, value);
        }

        public string MultiMonitorBehavior
        {
            get => _multiMonitorBehavior;
            set => SetProperty(ref _multiMonitorBehavior, value);
        }

        public string UIScaling
        {
            get => _uiScaling;
            set => SetProperty(ref _uiScaling, value);
        }

        public string FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public bool ShowTooltips
        {
            get => _showTooltips;
            set => SetProperty(ref _showTooltips, value);
        }

        public int TooltipDelay
        {
            get => _tooltipDelay;
            set => SetProperty(ref _tooltipDelay, value);
        }

        public ObservableCollection<string> MultiMonitorBehaviors { get; }
        public ObservableCollection<string> UIScalings { get; }
        public ObservableCollection<string> FontSizes { get; }

        public DisplaySettingsViewModel()
        {
            MultiMonitorBehaviors = new ObservableCollection<string> { "Primary monitor", "Last used monitor", "All monitors" };
            UIScalings = new ObservableCollection<string> { "75%", "100%", "125%", "150%", "200%" };
            FontSizes = new ObservableCollection<string> { "Small", "Medium", "Large" };
        }

        public override void LoadSettings()
        {
            // TODO: Load settings from configuration
        }

        public override void SaveSettings()
        {
            // TODO: Save settings to configuration
        }

        public override void ResetToDefaults()
        {
            StartInFullscreen = true;
            RememberWindowPosition = true;
            MultiMonitorBehavior = "Primary monitor";
            UIScaling = "100%";
            FontSize = "Medium";
            ShowTooltips = true;
            TooltipDelay = 500;
        }
    }
}