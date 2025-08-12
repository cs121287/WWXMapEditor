using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WWXMapEditor.Views;
using WWXMapEditor.Models;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object? _currentView;
        private Visibility _mainMenuVisibility = Visibility.Visible;
        private string _windowTitle = "WWX Map Editor";
        private readonly SettingsService _settingsService;
        private readonly ThemeService _themeService;

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
                UpdateWindowTitle();
            }
        }

        public Visibility MainMenuVisibility
        {
            get => _mainMenuVisibility;
            set
            {
                _mainMenuVisibility = value;
                OnPropertyChanged();
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                _windowTitle = value;
                OnPropertyChanged();
            }
        }

        // Theme properties for binding
        public bool IsDarkTheme
        {
            get => _settingsService.Settings.Theme == "Dark";
            set
            {
                if (value)
                {
                    _settingsService.Settings.Theme = "Dark";
                    _themeService.SetTheme(ThemeService.Theme.Dark);
                }
                else
                {
                    _settingsService.Settings.Theme = "Light";
                    _themeService.SetTheme(ThemeService.Theme.Light);
                }
                _settingsService.SaveSettings(_settingsService.Settings);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLightTheme));
            }
        }

        public bool IsLightTheme
        {
            get => _settingsService.Settings.Theme == "Light";
            set
            {
                if (value)
                {
                    _settingsService.Settings.Theme = "Light";
                    _themeService.SetTheme(ThemeService.Theme.Light);
                }
                else
                {
                    _settingsService.Settings.Theme = "Dark";
                    _themeService.SetTheme(ThemeService.Theme.Dark);
                }
                _settingsService.SaveSettings(_settingsService.Settings);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDarkTheme));
            }
        }

        // UI Scaling property
        public double UIScale
        {
            get => _settingsService.Settings.UiScale;
            set
            {
                if (Math.Abs(_settingsService.Settings.UiScale - value) > 0.01)
                {
                    _settingsService.Settings.UiScale = value;
                    _settingsService.ApplyUIScaling();
                    _settingsService.SaveSettings(_settingsService.Settings);
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NavigateCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand ApplyScalingCommand { get; }
        public ICommand CloseCommand { get; }

        public MainWindowViewModel()
        {
            _settingsService = SettingsService.Instance;
            _themeService = ThemeService.Instance;

            // Initialize commands
            NavigateCommand = new RelayCommand(ExecuteNavigate);
            ExitCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
            ApplyScalingCommand = new RelayCommand(ExecuteApplyScaling);
            // Close command used by views like AboutView that inherit this DataContext
            CloseCommand = new RelayCommand(_ => NavigateToMainMenu());

            // Apply current theme and scaling
            ApplyCurrentTheme();
            _settingsService.ApplyUIScaling();

            // Start with main menu
            NavigateToMainMenu();
        }

        private void ApplyCurrentTheme()
        {
            // Apply the theme from settings
            if (_settingsService.Settings.Theme == "Dark")
            {
                _themeService.SetTheme(ThemeService.Theme.Dark);
            }
            else
            {
                _themeService.SetTheme(ThemeService.Theme.Light);
            }
        }

        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }

        private void ExecuteApplyScaling(object? parameter)
        {
            if (parameter is double scale)
            {
                UIScale = scale;
            }
        }

        private void ExecuteNavigate(object? parameter)
        {
            if (parameter is string destination)
            {
                switch (destination)
                {
                    case "NewMap":
                    case "NewMapCreation":
                        NavigateToNewMapCreation();
                        break;
                    case "LoadMap":
                        NavigateToLoadMap();
                        break;
                    case "Configuration":
                        NavigateToConfiguration();
                        break;
                    case "Settings":
                        NavigateToSettings();
                        break;
                    case "About":
                        NavigateToAbout();
                        break;
                    case "MainMenu":
                        NavigateToMainMenu();
                        break;
                }
            }
        }

        public void NavigateToMainMenu()
        {
            // Show main menu
            MainMenuVisibility = Visibility.Visible;
            CurrentView = null;
            WindowTitle = "WWX Map Editor";

            // Ensure theme is applied when returning to main menu
            ApplyCurrentTheme();
        }

        public void NavigateToNewMap()
        {
            NavigateToNewMapCreation();
        }

        public void NavigateToNewMapCreation()
        {
            // Hide main menu and show new map view full screen
            MainMenuVisibility = Visibility.Collapsed;
            var newMapView = new NewMapView();
            var newMapViewModel = new NewMapViewModel(this);
            newMapView.DataContext = newMapViewModel;
            CurrentView = newMapView;
            WindowTitle = "WWX Map Editor - Create New Map";
        }

        public void NavigateToLoadMap()
        {
            // Hide main menu and show load map view
            MainMenuVisibility = Visibility.Collapsed;

            // Create a simple placeholder view if LoadMapView doesn't exist yet
            var loadMapView = new System.Windows.Controls.Border
            {
                Background = System.Windows.Media.Brushes.Transparent,
                Child = new System.Windows.Controls.TextBlock
                {
                    Text = "Load Map View - Coming Soon",
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    FontSize = 24,
                    Foreground = (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource("ForegroundBrush")
                }
            };

            CurrentView = loadMapView;
            WindowTitle = "WWX Map Editor - Load Map";
        }

        public void NavigateToConfiguration()
        {
            // Hide main menu and show configuration view
            MainMenuVisibility = Visibility.Collapsed;
            var configView = new ConfigurationView();
            var configViewModel = new ConfigurationViewModel(this);
            configView.DataContext = configViewModel;
            CurrentView = configView;
            WindowTitle = "WWX Map Editor - Configuration";
        }

        public void NavigateToSettings()
        {
            // Hide main menu and show settings view
            MainMenuVisibility = Visibility.Collapsed;
            var settingsView = new SettingsView();
            var settingsViewModel = new SettingsViewModel(this);
            settingsView.DataContext = settingsViewModel;
            CurrentView = settingsView;
            WindowTitle = "WWX Map Editor - Settings";
        }

        public void NavigateToAbout()
        {
            // Hide main menu and show AboutView (remove inline about content)
            MainMenuVisibility = Visibility.Collapsed;

            var aboutView = new AboutView();
            // Inherit this VM as DataContext so CloseCommand works (AboutView binds CloseCommand)
            aboutView.DataContext = this;

            CurrentView = aboutView;
            WindowTitle = "WWX Map Editor - About";
        }

        public void NavigateToMapEditor(Map map)
        {
            // Hide main menu and show map editor
            MainMenuVisibility = Visibility.Collapsed;
            var mapEditorView = new MapEditorView();
            var mapEditorViewModel = new MapEditorViewModel(this, map);
            mapEditorView.DataContext = mapEditorViewModel;
            CurrentView = mapEditorView;
            WindowTitle = $"WWX Map Editor - {map.Name}";
        }

        private void UpdateWindowTitle()
        {
            // Additional logic for updating window title based on current view
            if (CurrentView == null)
            {
                WindowTitle = "WWX Map Editor";
            }
            // Title is already set in navigation methods, but this allows for dynamic updates
        }

        // Method to refresh all views with current theme
        public void RefreshTheme()
        {
            ApplyCurrentTheme();

            // Force update of all bound properties
            OnPropertyChanged(nameof(IsDarkTheme));
            OnPropertyChanged(nameof(IsLightTheme));

            // If there's a current view, it will automatically use the new theme resources
            // due to DynamicResource bindings
        }

        // Method to handle settings changes from Configuration view
        public void OnSettingsChanged()
        {
            RefreshTheme();
            _settingsService.ApplyUIScaling();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}