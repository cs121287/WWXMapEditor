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
        private string _uiScaling = "100%";
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

        public string UIScaling
        {
            get => _uiScaling;
            set
            {
                if (SetProperty(ref _uiScaling, value))
                {
                    // Update settings
                    _settingsService.Settings.UIScaling = value;

                    // Parse and set the numeric scale value
                    if (value != null)
                    {
                        string scaleStr = value.Replace("%", "");
                        if (double.TryParse(scaleStr, out double scalePercent))
                        {
                            _settingsService.Settings.UiScale = scalePercent / 100.0;
                        }
                    }

                    // Apply the scaling immediately
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
                    // Update settings
                    _settingsService.Settings.FontSize = value;

                    // Map font size names to scaling factors
                    double fontScale = value switch
                    {
                        "Small" => 0.85,
                        "Medium" => 1.0,
                        "Large" => 1.15,
                        _ => 1.0
                    };

                    // Store font scale in settings if property exists
                    // Note: FontScale property needs to be added to AppSettings if not present

                    // Apply font scaling immediately
                    ApplyFontScaling(fontScale);
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
            var settings = _settingsService.Settings;

            _startInFullscreen = settings.StartInFullscreen;
            _rememberWindowPosition = settings.RememberWindowPosition;
            _multiMonitorBehavior = settings.MultiMonitorBehavior ?? "Primary monitor";
            _uiScaling = settings.UIScaling ?? "100%";
            _fontSize = settings.FontSize ?? "Medium";
            _showTooltips = settings.ShowTooltips;
            _tooltipDelay = settings.TooltipDelay;

            // Notify all properties
            OnPropertyChanged(nameof(StartInFullscreen));
            OnPropertyChanged(nameof(RememberWindowPosition));
            OnPropertyChanged(nameof(MultiMonitorBehavior));
            OnPropertyChanged(nameof(UIScaling));
            OnPropertyChanged(nameof(FontSize));
            OnPropertyChanged(nameof(ShowTooltips));
            OnPropertyChanged(nameof(TooltipDelay));
        }

        public override void SaveSettings()
        {
            // Settings are already updated in real-time via property setters
            // Just save to disk
            _settingsService.SaveSettings(_settingsService.Settings);
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

            // Save the reset settings
            SaveSettings();
        }

        private void ApplyFontScaling(double fontScale)
        {
            var app = System.Windows.Application.Current;
            if (app == null) return;

            app.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Get the current UI scale
                    double uiScale = _settingsService.CurrentScale;

                    // Apply combined scaling to fonts
                    UpdateFontResource("FontSizeSmall", 12 * uiScale * fontScale);
                    UpdateFontResource("FontSizeMedium", 14 * uiScale * fontScale);
                    UpdateFontResource("FontSizeLarge", 16 * uiScale * fontScale);
                    UpdateFontResource("FontSizeXLarge", 18 * uiScale * fontScale);
                    UpdateFontResource("FontSizeXXLarge", 24 * uiScale * fontScale);
                    UpdateFontResource("FontSizeTitle", 32 * uiScale * fontScale);
                    UpdateFontResource("FontSizeHeader", 36 * uiScale * fontScale);
                    UpdateFontResource("FontSizeMenu", 24 * uiScale * fontScale);

                    // Force layout update
                    foreach (System.Windows.Window window in app.Windows)
                    {
                        window.UpdateLayout();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error applying font scaling: {ex.Message}");
                }
            });
        }

        private void UpdateFontResource(string key, double value)
        {
            if (System.Windows.Application.Current?.Resources != null)
            {
                System.Windows.Application.Current.Resources[key] = value;
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
                    if (_showTooltips)
                    {
                        System.Windows.Controls.ToolTipService.SetIsEnabled(app.MainWindow, true);
                        System.Windows.Controls.ToolTipService.SetInitialShowDelay(app.MainWindow, _tooltipDelay);
                    }
                    else
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