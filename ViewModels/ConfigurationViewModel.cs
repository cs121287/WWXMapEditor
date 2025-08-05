using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using WWXMapEditor.Services;
using Microsoft.Win32;

namespace WWXMapEditor.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly SettingsService _settingsService;
        private int _currentStepIndex = 0;
        private object _currentStepContent = null!;
        private string _progressText = "Step 1 of 4";
        private string _nextButtonText = "NEXT";
        private Visibility _previousButtonVisibility = Visibility.Collapsed;

        // Settings properties
        private string _theme = "Dark";
        private bool _showGrid = true;
        private bool _snapToGrid = true;
        private int _gridSize = 32;
        private int _autoSaveInterval = 5;
        private bool _showTooltips = true;
        private bool _enableSounds = true;
        private int _soundVolume = 50;
        private string _defaultMapSize = "Medium (50x50)";
        private bool _hardwareAcceleration = true;
        private string _textureQuality = "High";
        private bool _smoothZooming = true;
        private bool _autoSaveEnabled = true;
        private bool _startInFullscreen = true;
        private string _uiScaling = "100%";

        // File locations
        private string _defaultProjectDirectory = "";
        private string _defaultTilesetDirectory = "";
        private string _autoSaveLocation = "";

        // Theme properties
        private bool _isDarkTheme = true;
        private bool _isLightTheme = false;

        // Summary properties
        private string _themeSummary = "Dark";
        private string _fullscreenSummary = "Yes";
        private string _uiScalingSummary = "100%";
        private string _gridSummary = "Enabled";
        private string _gridSizeSummary = "32";
        private string _autoSaveSummary = "Enabled (every 5 minutes)";
        private string _fileLocationsSummary = "";

        public ObservableCollection<ConfigurationStep> ConfigurationSteps { get; }

        #region Step Navigation Properties

        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                if (SetProperty(ref _currentStepIndex, value))
                {
                    UpdateCurrentStep();
                }
            }
        }

        public object CurrentStepContent
        {
            get => _currentStepContent;
            set => SetProperty(ref _currentStepContent, value);
        }

        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        public string NextButtonText
        {
            get => _nextButtonText;
            set => SetProperty(ref _nextButtonText, value);
        }

        public Visibility PreviousButtonVisibility
        {
            get => _previousButtonVisibility;
            set => SetProperty(ref _previousButtonVisibility, value);
        }

        #endregion

        #region Settings Properties

        public string Theme
        {
            get => _theme;
            set
            {
                if (SetProperty(ref _theme, value))
                {
                    IsDarkTheme = value == "Dark";
                    IsLightTheme = value == "Light";
                    ThemeSummary = value;

                    // Apply theme immediately
                    ApplyThemeImmediately();
                }
            }
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                if (SetProperty(ref _showGrid, value))
                {
                    GridSummary = value ? "Enabled" : "Disabled";
                }
            }
        }

        public bool SnapToGrid
        {
            get => _snapToGrid;
            set => SetProperty(ref _snapToGrid, value);
        }

        public int GridSize
        {
            get => _gridSize;
            set
            {
                if (SetProperty(ref _gridSize, value))
                {
                    GridSizeSummary = value.ToString();
                }
            }
        }

        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set
            {
                if (SetProperty(ref _autoSaveInterval, value))
                {
                    UpdateAutoSaveSummary();
                }
            }
        }

        public bool ShowTooltips
        {
            get => _showTooltips;
            set => SetProperty(ref _showTooltips, value);
        }

        public bool EnableSounds
        {
            get => _enableSounds;
            set => SetProperty(ref _enableSounds, value);
        }

        public int SoundVolume
        {
            get => _soundVolume;
            set => SetProperty(ref _soundVolume, value);
        }

        public string DefaultMapSize
        {
            get => _defaultMapSize;
            set => SetProperty(ref _defaultMapSize, value);
        }

        public bool HardwareAcceleration
        {
            get => _hardwareAcceleration;
            set => SetProperty(ref _hardwareAcceleration, value);
        }

        public string TextureQuality
        {
            get => _textureQuality;
            set => SetProperty(ref _textureQuality, value);
        }

        public bool SmoothZooming
        {
            get => _smoothZooming;
            set => SetProperty(ref _smoothZooming, value);
        }

        public bool AutoSaveEnabled
        {
            get => _autoSaveEnabled;
            set
            {
                if (SetProperty(ref _autoSaveEnabled, value))
                {
                    UpdateAutoSaveSummary();
                }
            }
        }

        public bool StartInFullscreen
        {
            get => _startInFullscreen;
            set
            {
                if (SetProperty(ref _startInFullscreen, value))
                {
                    FullscreenSummary = value ? "Yes" : "No";
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
                    UIScalingSummary = value;

                    // Apply UI scaling immediately
                    ApplyUIScalingImmediately();
                }
            }
        }

        #endregion

        #region Theme Properties

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value) && value)
                {
                    Theme = "Dark";
                    IsLightTheme = false;
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
                    Theme = "Light";
                    IsDarkTheme = false;
                }
            }
        }

        #endregion

        #region File Location Properties

        public string DefaultProjectDirectory
        {
            get => _defaultProjectDirectory;
            set
            {
                if (SetProperty(ref _defaultProjectDirectory, value))
                {
                    UpdateFileLocationsSummary();
                }
            }
        }

        public string DefaultTilesetDirectory
        {
            get => _defaultTilesetDirectory;
            set
            {
                if (SetProperty(ref _defaultTilesetDirectory, value))
                {
                    UpdateFileLocationsSummary();
                }
            }
        }

        public string AutoSaveLocation
        {
            get => _autoSaveLocation;
            set
            {
                if (SetProperty(ref _autoSaveLocation, value))
                {
                    UpdateFileLocationsSummary();
                }
            }
        }

        #endregion

        #region Summary Properties

        public string ThemeSummary
        {
            get => _themeSummary;
            set => SetProperty(ref _themeSummary, value);
        }

        public string FullscreenSummary
        {
            get => _fullscreenSummary;
            set => SetProperty(ref _fullscreenSummary, value);
        }

        public string UIScalingSummary
        {
            get => _uiScalingSummary;
            set => SetProperty(ref _uiScalingSummary, value);
        }

        public string GridSummary
        {
            get => _gridSummary;
            set => SetProperty(ref _gridSummary, value);
        }

        public string GridSizeSummary
        {
            get => _gridSizeSummary;
            set => SetProperty(ref _gridSizeSummary, value);
        }

        public string AutoSaveSummary
        {
            get => _autoSaveSummary;
            set => SetProperty(ref _autoSaveSummary, value);
        }

        public string FileLocationsSummary
        {
            get => _fileLocationsSummary;
            set => SetProperty(ref _fileLocationsSummary, value);
        }

        #endregion

        #region Collections

        public ObservableCollection<string> Themes { get; }
        public ObservableCollection<string> MapSizes { get; }
        public ObservableCollection<string> TextureQualities { get; }
        public ObservableCollection<string> UIScalingOptions { get; }
        public ObservableCollection<int> GridSizeOptions { get; }
        public ObservableCollection<int> AutoSaveIntervalOptions { get; }

        #endregion

        #region Commands

        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand FinishCommand { get; }
        public ICommand SkipCommand { get; }
        public ICommand BrowseProjectDirectoryCommand { get; }
        public ICommand BrowseTilesetDirectoryCommand { get; }
        public ICommand BrowseAutoSaveLocationCommand { get; }

        #endregion

        public ConfigurationViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _settingsService = SettingsService.Instance;

            // Initialize configuration steps
            ConfigurationSteps = new ObservableCollection<ConfigurationStep>
            {
                new ConfigurationStep { Number = "1", Title = "Theme & Display", IsActive = true, IsCompleted = false },
                new ConfigurationStep { Number = "2", Title = "Editor Preferences", IsActive = false, IsCompleted = false },
                new ConfigurationStep { Number = "3", Title = "File Locations", IsActive = false, IsCompleted = false },
                new ConfigurationStep { Number = "4", Title = "Review & Finish", IsActive = false, IsCompleted = false }
            };

            // Initialize collections
            Themes = new ObservableCollection<string> { "Dark", "Light" };
            MapSizes = new ObservableCollection<string>
            {
                "Small (25x25)",
                "Medium (50x50)",
                "Large (100x100)",
                "Extra Large (200x200)"
            };
            TextureQualities = new ObservableCollection<string> { "Low", "Medium", "High", "Ultra" };
            UIScalingOptions = new ObservableCollection<string> { "75%", "100%", "125%", "150%", "200%" };
            GridSizeOptions = new ObservableCollection<int> { 8, 16, 24, 32, 48, 64 };
            AutoSaveIntervalOptions = new ObservableCollection<int> { 1, 3, 5, 10, 15, 30 };

            // Initialize commands
            PreviousCommand = new RelayCommand(ExecutePrevious, CanExecutePrevious);
            NextCommand = new RelayCommand(ExecuteNext, CanExecuteNext);
            FinishCommand = new RelayCommand(ExecuteFinish);
            SkipCommand = new RelayCommand(ExecuteSkip);
            BrowseProjectDirectoryCommand = new RelayCommand(ExecuteBrowseProjectDirectory);
            BrowseTilesetDirectoryCommand = new RelayCommand(ExecuteBrowseTilesetDirectory);
            BrowseAutoSaveLocationCommand = new RelayCommand(ExecuteBrowseAutoSaveLocation);

            // Load existing settings
            LoadSettings();

            // Set initial step
            UpdateCurrentStep();
        }

        private void LoadSettings()
        {
            var settings = _settingsService.Settings;
            Theme = settings.Theme;
            ShowGrid = settings.ShowGrid;
            SnapToGrid = settings.SnapToGrid;
            GridSize = settings.GridSize;
            AutoSaveInterval = settings.AutoSaveInterval;
            ShowTooltips = settings.ShowTooltips;
            HardwareAcceleration = settings.HardwareAcceleration;
            TextureQuality = settings.TextureQuality;
            SmoothZooming = settings.SmoothZooming;
            AutoSaveEnabled = settings.AutoSaveEnabled;
            StartInFullscreen = settings.StartInFullscreen;
            UIScaling = settings.UIScaling;
            DefaultProjectDirectory = settings.DefaultProjectDirectory;
            DefaultTilesetDirectory = settings.DefaultTilesetDirectory;
            AutoSaveLocation = settings.AutoSaveLocation;

            // Set default values for properties that don't exist in AppSettings
            EnableSounds = true;
            SoundVolume = 50;

            // Convert DefaultMapWidth/Height to DefaultMapSize string
            if (settings.DefaultMapWidth == 25 && settings.DefaultMapHeight == 25)
                DefaultMapSize = "Small (25x25)";
            else if (settings.DefaultMapWidth == 50 && settings.DefaultMapHeight == 50)
                DefaultMapSize = "Medium (50x50)";
            else if (settings.DefaultMapWidth == 100 && settings.DefaultMapHeight == 100)
                DefaultMapSize = "Large (100x100)";
            else if (settings.DefaultMapWidth == 200 && settings.DefaultMapHeight == 200)
                DefaultMapSize = "Extra Large (200x200)";
            else
                DefaultMapSize = "Medium (50x50)";

            // Update all summaries
            UpdateAutoSaveSummary();
            UpdateFileLocationsSummary();
        }

        private void UpdateCurrentStep()
        {
            // Update step states
            for (int i = 0; i < ConfigurationSteps.Count; i++)
            {
                ConfigurationSteps[i].IsActive = i == CurrentStepIndex;
                ConfigurationSteps[i].IsCompleted = i < CurrentStepIndex;
            }

            ProgressText = $"Step {CurrentStepIndex + 1} of {ConfigurationSteps.Count}";
            PreviousButtonVisibility = CurrentStepIndex > 0 ? Visibility.Visible : Visibility.Collapsed;
            NextButtonText = CurrentStepIndex == ConfigurationSteps.Count - 1 ? "FINISH" : "NEXT";

            // Update content based on current step
            switch (CurrentStepIndex)
            {
                case 0:
                    CurrentStepContent = new Views.Configuration.ThemeDisplayStepView { DataContext = this };
                    break;
                case 1:
                    CurrentStepContent = new Views.Configuration.EditorPreferencesStepView { DataContext = this };
                    break;
                case 2:
                    CurrentStepContent = new Views.Configuration.FileLocationsStepView { DataContext = this };
                    break;
                case 3:
                    CurrentStepContent = new Views.Configuration.ReviewFinishStepView { DataContext = this };
                    break;
            }
        }

        private void UpdateAutoSaveSummary()
        {
            if (AutoSaveEnabled)
            {
                AutoSaveSummary = $"Enabled (every {AutoSaveInterval} minute{(AutoSaveInterval > 1 ? "s" : "")})";
            }
            else
            {
                AutoSaveSummary = "Disabled";
            }
        }

        private void UpdateFileLocationsSummary()
        {
            FileLocationsSummary = $"• Projects: {DefaultProjectDirectory}\n" +
                                   $"• Tilesets: {DefaultTilesetDirectory}\n" +
                                   $"• Auto-save: {AutoSaveLocation}";
        }

        private void ApplyThemeImmediately()
        {
            // Update the settings and apply theme without saving to disk
            var tempSettings = _settingsService.Settings;
            tempSettings.Theme = Theme;
            _settingsService.ApplyTheme();
        }

        private void ApplyUIScalingImmediately()
        {
            // Update the settings and apply UI scaling without saving to disk
            var tempSettings = _settingsService.Settings;
            tempSettings.UIScaling = UIScaling;
            _settingsService.ApplyUIScaling();
        }

        private bool CanExecutePrevious(object? parameter)
        {
            return CurrentStepIndex > 0;
        }

        private void ExecutePrevious(object? parameter)
        {
            if (CurrentStepIndex > 0)
            {
                CurrentStepIndex--;
            }
        }

        private bool CanExecuteNext(object? parameter)
        {
            // On the last step, Next becomes Finish
            if (CurrentStepIndex == ConfigurationSteps.Count - 1)
            {
                return true; // Always allow finish
            }
            return CurrentStepIndex < ConfigurationSteps.Count - 1;
        }

        private void ExecuteNext(object? parameter)
        {
            if (CurrentStepIndex < ConfigurationSteps.Count - 1)
            {
                CurrentStepIndex++;
            }
            else
            {
                // If we're on the last step, execute finish
                ExecuteFinish(parameter);
            }
        }

        private void ExecuteFinish(object? parameter)
        {
            try
            {
                SaveSettings();

                // Navigate to main menu
                if (_mainWindowViewModel != null)
                {
                    _mainWindowViewModel.NavigateToMainMenu();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}", "Configuration Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSkip(object? parameter)
        {
            // Navigate to main menu without saving
            if (_mainWindowViewModel != null)
            {
                _mainWindowViewModel.NavigateToMainMenu();
            }
        }

        private void ExecuteBrowseProjectDirectory(object? parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Default Project Directory",
                SelectedPath = DefaultProjectDirectory,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DefaultProjectDirectory = dialog.SelectedPath;
            }
        }

        private void ExecuteBrowseTilesetDirectory(object? parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Default Tileset Directory",
                SelectedPath = DefaultTilesetDirectory,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DefaultTilesetDirectory = dialog.SelectedPath;
            }
        }

        private void ExecuteBrowseAutoSaveLocation(object? parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Auto-Save Location",
                SelectedPath = AutoSaveLocation,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AutoSaveLocation = dialog.SelectedPath;
            }
        }

        private void SaveSettings()
        {
            var settings = _settingsService.Settings;
            settings.Theme = Theme;
            settings.ShowGrid = ShowGrid;
            settings.SnapToGrid = SnapToGrid;
            settings.GridSize = GridSize;
            settings.AutoSaveInterval = AutoSaveInterval;
            settings.ShowTooltips = ShowTooltips;
            settings.HardwareAcceleration = HardwareAcceleration;
            settings.TextureQuality = TextureQuality;
            settings.SmoothZooming = SmoothZooming;
            settings.AutoSaveEnabled = AutoSaveEnabled;
            settings.StartInFullscreen = StartInFullscreen;
            settings.UIScaling = UIScaling;
            settings.DefaultProjectDirectory = DefaultProjectDirectory;
            settings.DefaultTilesetDirectory = DefaultTilesetDirectory;
            settings.AutoSaveLocation = AutoSaveLocation;

            // Parse DefaultMapSize to set width and height
            switch (DefaultMapSize)
            {
                case "Small (25x25)":
                    settings.DefaultMapWidth = 25;
                    settings.DefaultMapHeight = 25;
                    break;
                case "Medium (50x50)":
                    settings.DefaultMapWidth = 50;
                    settings.DefaultMapHeight = 50;
                    break;
                case "Large (100x100)":
                    settings.DefaultMapWidth = 100;
                    settings.DefaultMapHeight = 100;
                    break;
                case "Extra Large (200x200)":
                    settings.DefaultMapWidth = 200;
                    settings.DefaultMapHeight = 200;
                    break;
                default:
                    settings.DefaultMapWidth = 50;
                    settings.DefaultMapHeight = 50;
                    break;
            }

            settings.IsFirstRun = false;

            _settingsService.SaveSettings(settings);

            // Apply all settings after saving
            _settingsService.ApplyAllSettings();
        }
    }

    public class ConfigurationStep : ViewModelBase
    {
        private string _number = string.Empty;
        private string _title = string.Empty;
        private bool _isActive;
        private bool _isCompleted;

        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }
    }
}