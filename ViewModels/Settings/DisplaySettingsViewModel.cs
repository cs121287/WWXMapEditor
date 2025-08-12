using System;
using System.Collections.ObjectModel;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels.Settings
{
    public class DisplaySettingsViewModel : SettingsPageViewModelBase
    {
        private readonly SettingsService _settingsService;

        private bool _startInFullscreen = true;
        private bool _rememberWindowPosition = true;
        private string _multiMonitorBehavior = "Primary monitor";

        // New scaling model fields (bound to AppSettings)
        private string _scaleMode = "Automatic"; // Automatic | CustomFixed | SystemDpiOnly | LegacyPercent
        private string _uiScaling = "100%";      // used only when ScaleMode == LegacyPercent
        private double _customFixedScale = 1.0;  // used only when ScaleMode == CustomFixed
        private double _minAutoScale = 0.75;
        private double _maxAutoScale = 1.65;
        private double _designWidth = 1920;
        private double _designHeight = 1080;
        private double _minFontScale = 0.85;
        private double _maxFontScale = 1.8;
        private bool _useDensityBreakpoints = true;

        // Font "size" as labels; sets FontScaleOverride via mapping
        private string _fontSize = "Medium";

        private bool _showTooltips = true;
        private int _tooltipDelay = 500;

        public bool StartInFullscreen
        {
            get => _startInFullscreen;
            set
            {
                if (SetProperty(ref _startInFullscreen, value))
                {
                    _settingsService.Settings.StartInFullscreen = value;
                }
            }
        }

        public bool RememberWindowPosition
        {
            get => _rememberWindowPosition;
            set
            {
                if (SetProperty(ref _rememberWindowPosition, value))
                {
                    _settingsService.Settings.RememberWindowPosition = value;
                }
            }
        }

        public string MultiMonitorBehavior
        {
            get => _multiMonitorBehavior;
            set
            {
                if (SetProperty(ref _multiMonitorBehavior, value))
                {
                    _settingsService.Settings.MultiMonitorBehavior = value;
                }
            }
        }

        public string ScaleMode
        {
            get => _scaleMode;
            set
            {
                if (SetProperty(ref _scaleMode, value))
                {
                    _settingsService.Settings.ScaleMode = value;
                    // Keep UIScaling string in sync for legacy percent mode
                    if (string.Equals(value, "LegacyPercent", StringComparison.OrdinalIgnoreCase))
                    {
                        // Ensure UIScaling string is valid
                        _settingsService.Settings.UIScaling = UIScaling;
                        UpdateUiScaleFromLegacyPercent();
                    }
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public string UIScaling
        {
            get => _uiScaling;
            set
            {
                if (SetProperty(ref _uiScaling, value))
                {
                    _settingsService.Settings.UIScaling = value;
                    UpdateUiScaleFromLegacyPercent();
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public double CustomFixedScale
        {
            get => _customFixedScale;
            set
            {
                if (SetProperty(ref _customFixedScale, value))
                {
                    _settingsService.Settings.CustomFixedScale = value;
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public double MinAutoScale
        {
            get => _minAutoScale;
            set
            {
                if (SetProperty(ref _minAutoScale, value))
                {
                    _settingsService.Settings.MinAutoScale = value;
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public double MaxAutoScale
        {
            get => _maxAutoScale;
            set
            {
                if (SetProperty(ref _maxAutoScale, value))
                {
                    _settingsService.Settings.MaxAutoScale = value;
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public double DesignWidth
        {
            get => _designWidth;
            set
            {
                if (SetProperty(ref _designWidth, value))
                {
                    _settingsService.Settings.DesignWidth = value;
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public double DesignHeight
        {
            get => _designHeight;
            set
            {
                if (SetProperty(ref _designHeight, value))
                {
                    _settingsService.Settings.DesignHeight = value;
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public double MinFontScale
        {
            get => _minFontScale;
            set
            {
                if (SetProperty(ref _minFontScale, value))
                {
                    _settingsService.Settings.MinFontScale = value;
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public double MaxFontScale
        {
            get => _maxFontScale;
            set
            {
                if (SetProperty(ref _maxFontScale, value))
                {
                    _settingsService.Settings.MaxFontScale = value;
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public bool UseDensityBreakpoints
        {
            get => _useDensityBreakpoints;
            set
            {
                if (SetProperty(ref _useDensityBreakpoints, value))
                {
                    _settingsService.Settings.UseDensityBreakpoints = value;
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public string FontSize
        {
            get => _fontSize;
            set
            {
                if (SetProperty(ref _fontSize, value))
                {
                    _settingsService.Settings.FontSize = value;

                    // Map to FontScaleOverride; null clears override
                    double? fontScaleOverride = value switch
                    {
                        "Small" => 0.90,
                        "Medium" => (double?)null, // let ScaleService compute gamma-based font scale
                        "Large" => 1.10,
                        _ => (double?)null
                    };
                    _settingsService.Settings.FontScaleOverride = fontScaleOverride;
                    _settingsService.ApplyUIScaling();
                }
            }
        }

        public bool ShowTooltips
        {
            get => _showTooltips;
            set
            {
                if (SetProperty(ref _showTooltips, value))
                {
                    _settingsService.Settings.ShowTooltips = value;
                    ApplyTooltipSettings();
                }
            }
        }

        public int TooltipDelay
        {
            get => _tooltipDelay;
            set
            {
                if (SetProperty(ref _tooltipDelay, value))
                {
                    _settingsService.Settings.TooltipDelay = value;
                    ApplyTooltipSettings();
                }
            }
        }

        public ObservableCollection<string> MultiMonitorBehaviors { get; }
        public ObservableCollection<string> ScaleModes { get; }
        public ObservableCollection<string> UIScalings { get; }
        public ObservableCollection<string> FontSizes { get; }

        public DisplaySettingsViewModel()
        {
            _settingsService = SettingsService.Instance;

            MultiMonitorBehaviors = new ObservableCollection<string>
            {
                "Primary monitor",
                "Last used monitor",
                "All monitors"
            };

            ScaleModes = new ObservableCollection<string>
            {
                "Automatic",
                "SystemDpiOnly",
                "CustomFixed",
                "LegacyPercent"
            };

            UIScalings = new ObservableCollection<string>
            {
                "50%",
                "75%",
                "100%",
                "125%",
                "150%",
                "175%",
                "200%"
            };

            FontSizes = new ObservableCollection<string>
            {
                "Small",
                "Medium",
                "Large"
            };

            LoadSettings();
        }

        public override void LoadSettings()
        {
            var s = _settingsService.Settings;

            _startInFullscreen = s.StartInFullscreen;
            _rememberWindowPosition = s.RememberWindowPosition;
            _multiMonitorBehavior = s.MultiMonitorBehavior ?? "Primary monitor";

            _scaleMode = s.ScaleMode ?? "Automatic";
            _uiScaling = s.UIScaling ?? "100%";
            _customFixedScale = s.CustomFixedScale ?? 1.0;
            _minAutoScale = s.MinAutoScale;
            _maxAutoScale = s.MaxAutoScale;
            _designWidth = s.DesignWidth;
            _designHeight = s.DesignHeight;
            _minFontScale = s.MinFontScale;
            _maxFontScale = s.MaxFontScale;
            _useDensityBreakpoints = s.UseDensityBreakpoints;

            _fontSize = s.FontSize ?? "Medium";
            _showTooltips = s.ShowTooltips;
            _tooltipDelay = s.TooltipDelay;

            // Notify all properties
            OnPropertyChanged(nameof(StartInFullscreen));
            OnPropertyChanged(nameof(RememberWindowPosition));
            OnPropertyChanged(nameof(MultiMonitorBehavior));

            OnPropertyChanged(nameof(ScaleMode));
            OnPropertyChanged(nameof(UIScaling));
            OnPropertyChanged(nameof(CustomFixedScale));
            OnPropertyChanged(nameof(MinAutoScale));
            OnPropertyChanged(nameof(MaxAutoScale));
            OnPropertyChanged(nameof(DesignWidth));
            OnPropertyChanged(nameof(DesignHeight));
            OnPropertyChanged(nameof(MinFontScale));
            OnPropertyChanged(nameof(MaxFontScale));
            OnPropertyChanged(nameof(UseDensityBreakpoints));

            OnPropertyChanged(nameof(FontSize));
            OnPropertyChanged(nameof(ShowTooltips));
            OnPropertyChanged(nameof(TooltipDelay));
        }

        public override void SaveSettings()
        {
            // Settings are live-updated via setters
            _settingsService.SaveSettings(_settingsService.Settings);
        }

        public override void ResetToDefaults()
        {
            StartInFullscreen = true;
            RememberWindowPosition = true;
            MultiMonitorBehavior = "Primary monitor";

            ScaleMode = "Automatic";
            UIScaling = "100%";
            CustomFixedScale = 1.0;
            MinAutoScale = 0.75;
            MaxAutoScale = 1.65;
            DesignWidth = 1920;
            DesignHeight = 1080;
            MinFontScale = 0.85;
            MaxFontScale = 1.8;
            UseDensityBreakpoints = true;

            FontSize = "Medium";
            ShowTooltips = true;
            TooltipDelay = 500;

            SaveSettings();
        }

        private void UpdateUiScaleFromLegacyPercent()
        {
            if (!string.Equals(ScaleMode, "LegacyPercent", StringComparison.OrdinalIgnoreCase))
                return;

            if (UIScaling != null)
            {
                string scaleStr = UIScaling.Replace("%", "");
                if (double.TryParse(scaleStr, out double scalePercent))
                {
                    _settingsService.Settings.UiScale = scalePercent / 100.0;
                }
            }
        }

        private void ApplyTooltipSettings()
        {
            var app = System.Windows.Application.Current;
            if (app == null) return;

            app.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Apply tooltip settings globally
                    if (_showTooltips && app.MainWindow != null)
                    {
                        System.Windows.Controls.ToolTipService.SetIsEnabled(app.MainWindow, true);
                        System.Windows.Controls.ToolTipService.SetInitialShowDelay(app.MainWindow, _tooltipDelay);
                    }
                    else if (app.MainWindow != null)
                    {
                        System.Windows.Controls.ToolTipService.SetIsEnabled(app.MainWindow, false);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error applying tooltip settings: {ex.Message}");
                }
            });
        }
    }
}