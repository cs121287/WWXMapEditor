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
        private string _uiScaling = "100%";

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

        public ObservableCollection<string> UIScalingOptions { get; }

        public ThemeDisplayStepViewModel(AppSettings settings)
        {
            _settings = settings;
            UIScalingOptions = new ObservableCollection<string> { "75%", "100%", "125%", "150%", "200%" };

            // Load from settings
            IsDarkTheme = _settings.Theme == "Dark";
            IsLightTheme = _settings.Theme == "Light";
            StartInFullscreen = _settings.StartInFullscreen;
            ShowTooltips = _settings.ShowTooltips;
            UIScaling = _settings.UIScaling;
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
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    var scaleFactor = UIScaling.TrimEnd('%');
                    if (double.TryParse(scaleFactor, out double scale))
                    {
                        var scaleValue = scale / 100.0;
                        var transform = new System.Windows.Media.ScaleTransform(scaleValue, scaleValue);

                        // Apply transform to the window's content
                        var mainWindow = System.Windows.Application.Current.MainWindow;
                        if (mainWindow.Content is FrameworkElement content)
                        {
                            content.LayoutTransform = transform;

                            // Adjust window size to maintain aspect ratio
                            if (!mainWindow.WindowState.Equals(WindowState.Maximized))
                            {
                                mainWindow.Width = mainWindow.ActualWidth * scaleValue;
                                mainWindow.Height = mainWindow.ActualHeight * scaleValue;
                            }
                        }

                        // Force layout update
                        mainWindow.UpdateLayout();
                    }
                }
            });
        }

        public void UpdateSettings()
        {
            _settings.Theme = IsDarkTheme ? "Dark" : "Light";
            _settings.StartInFullscreen = StartInFullscreen;
            _settings.ShowTooltips = ShowTooltips;
            _settings.UIScaling = UIScaling;
        }
    }
}