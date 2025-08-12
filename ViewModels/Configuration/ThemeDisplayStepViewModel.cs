using System.Collections.ObjectModel;
using System.Windows;
using WWXMapEditor.Models;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels
{
    public class ThemeDisplayStepViewModel : ViewModelBase
    {
        private readonly AppSettings _settings;
        private bool _isDarkTheme = true;
        private bool _isLightTheme;
        private bool _startInFullscreen = true;
        private bool _showTooltips = true;

        // New scaling options
        private string _scaleMode = "Automatic"; // Automatic | CustomFixed | SystemDpiOnly | LegacyPercent
        private string _uiScaling = "100%";      // used when ScaleMode == LegacyPercent
        private double _customFixedScale = 1.0;  // used when ScaleMode == CustomFixed
        private bool _useDensityBreakpoints = true;

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value) && value)
                {
                    _isLightTheme = false;
                    OnPropertyChanged(nameof(IsLightTheme));
                    ApplyThemeImmediately();
                }
            }
        }

        public bool IsLightTheme
        {
            get => _isLightTheme;
            set
            {
                if (SetProperty(ref _isLightTheme, value) && value)
                {
                    _isDarkTheme = false;
                    OnPropertyChanged(nameof(IsDarkTheme));
                    ApplyThemeImmediately();
                }
            }
        }

        public bool StartInFullscreen
        {
            get => _startInFullscreen;
            set => SetProperty(ref _startInFullscreen, value);
        }

        public bool ShowTooltips
        {
            get => _showTooltips;
            set => SetProperty(ref _showTooltips, value);
        }

        public string ScaleMode
        {
            get => _scaleMode;
            set
            {
                if (SetProperty(ref _scaleMode, value))
                {
                    ApplyScalingImmediately();
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
                    ApplyScalingImmediately();
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
                    ApplyScalingImmediately();
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
                    ApplyScalingImmediately();
                }
            }
        }

        public ObservableCollection<string> UIScalingOptions { get; }
        public ObservableCollection<string> ScaleModes { get; }

        public ThemeDisplayStepViewModel(AppSettings settings)
        {
            _settings = settings;
            UIScalingOptions = new ObservableCollection<string> { "75%", "100%", "125%", "150%", "200%" };
            ScaleModes = new ObservableCollection<string> { "Automatic", "SystemDpiOnly", "CustomFixed", "LegacyPercent" };

            // Load from settings
            IsDarkTheme = _settings.Theme == "Dark";
            IsLightTheme = _settings.Theme == "Light";
            StartInFullscreen = _settings.StartInFullscreen;
            ShowTooltips = _settings.ShowTooltips;
            ScaleMode = _settings.ScaleMode ?? "Automatic";
            UIScaling = _settings.UIScaling ?? "100%";
            CustomFixedScale = _settings.CustomFixedScale ?? 1.0;
            UseDensityBreakpoints = _settings.UseDensityBreakpoints;
        }

        private void ApplyThemeImmediately()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (IsDarkTheme)
                {
                    ThemeService.Instance.SetTheme(ThemeService.Theme.Dark);
                }
                else if (IsLightTheme)
                {
                    ThemeService.Instance.SetTheme(ThemeService.Theme.Light);
                }

                // Force the main window to refresh
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.InvalidateVisual();
                    System.Windows.Application.Current.MainWindow.UpdateLayout();
                }
            });
        }

        private void ApplyScalingImmediately()
        {
            var svc = SettingsService.Instance;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Update AppSettings fields
                svc.Settings.ScaleMode = ScaleMode;
                svc.Settings.UIScaling = UIScaling;
                if (ScaleMode == "LegacyPercent")
                {
                    var str = UIScaling.Replace("%", "");
                    if (double.TryParse(str, out double p))
                    {
                        svc.Settings.UiScale = p / 100.0;
                    }
                }
                svc.Settings.CustomFixedScale = CustomFixedScale;
                svc.Settings.UseDensityBreakpoints = UseDensityBreakpoints;

                // Apply via SettingsService -> ScaleService
                svc.ApplyUIScaling();
            });
        }

        public void UpdateSettings()
        {
            _settings.Theme = IsDarkTheme ? "Dark" : "Light";
            _settings.StartInFullscreen = StartInFullscreen;
            _settings.ShowTooltips = ShowTooltips;

            _settings.ScaleMode = ScaleMode;
            _settings.UIScaling = UIScaling;
            _settings.UseDensityBreakpoints = UseDensityBreakpoints;
            _settings.CustomFixedScale = CustomFixedScale;

            if (ScaleMode == "LegacyPercent")
            {
                var str = UIScaling.Replace("%", "");
                if (double.TryParse(str, out double p))
                {
                    _settings.UiScale = p / 100.0;
                }
            }
        }
    }
}