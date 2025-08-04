using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels.Settings
{
    public class GeneralSettingsViewModel : SettingsPageViewModelBase
    {
        private bool _isDarkMode = true;
        private bool _isLightMode;
        private bool _isCustomTheme;
        private SolidColorBrush _customThemeColor = new SolidColorBrush(Colors.DarkBlue);
        private string _selectedLanguage = "English";
        private bool _autoSaveEnabled = true;
        private int _autoSaveInterval = 5;
        private string _autoSaveLocation = @"C:\Users\cs121287\Documents\WWXMapEditor\AutoSave";
        private int _recentFilesCount = 10;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (SetProperty(ref _isDarkMode, value) && value)
                {
                    IsLightMode = false;
                    IsCustomTheme = false;
                    ThemeService.Instance.SetTheme(ThemeService.Theme.Dark);
                }
            }
        }

        public bool IsLightMode
        {
            get => _isLightMode;
            set
            {
                if (SetProperty(ref _isLightMode, value) && value)
                {
                    IsDarkMode = false;
                    IsCustomTheme = false;
                    ThemeService.Instance.SetTheme(ThemeService.Theme.Light);
                }
            }
        }

        public bool IsCustomTheme
        {
            get => _isCustomTheme;
            set
            {
                if (SetProperty(ref _isCustomTheme, value) && value)
                {
                    IsDarkMode = false;
                    IsLightMode = false;
                    ThemeService.Instance.SetCustomTheme((_customThemeColor.Color));
                }
            }
        }

        public SolidColorBrush CustomThemeColor
        {
            get => _customThemeColor;
            set
            {
                if (SetProperty(ref _customThemeColor, value) && IsCustomTheme)
                {
                    ThemeService.Instance.SetCustomTheme(value.Color);
                }
            }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public bool AutoSaveEnabled
        {
            get => _autoSaveEnabled;
            set => SetProperty(ref _autoSaveEnabled, value);
        }

        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set => SetProperty(ref _autoSaveInterval, value);
        }

        public string AutoSaveLocation
        {
            get => _autoSaveLocation;
            set => SetProperty(ref _autoSaveLocation, value);
        }

        public int RecentFilesCount
        {
            get => _recentFilesCount;
            set => SetProperty(ref _recentFilesCount, value);
        }

        public ObservableCollection<string> AvailableLanguages { get; }
        public ObservableCollection<int> AutoSaveIntervals { get; }
        public ObservableCollection<int> RecentFilesCounts { get; }

        public ICommand PickColorCommand { get; }
        public ICommand BrowseAutoSaveLocationCommand { get; }
        public ICommand ClearRecentFilesCommand { get; }

        public GeneralSettingsViewModel()
        {
            AvailableLanguages = new ObservableCollection<string> { "English", "Spanish", "French", "German", "Japanese", "Chinese" };
            AutoSaveIntervals = new ObservableCollection<int> { 1, 5, 10, 15, 30 };
            RecentFilesCounts = new ObservableCollection<int> { 5, 10, 15, 20 };

            PickColorCommand = new RelayCommand(ExecutePickColor);
            BrowseAutoSaveLocationCommand = new RelayCommand(ExecuteBrowseAutoSaveLocation);
            ClearRecentFilesCommand = new RelayCommand(ExecuteClearRecentFiles);

            // Set initial theme state based on current theme
            switch (ThemeService.Instance.CurrentTheme)
            {
                case ThemeService.Theme.Dark:
                    _isDarkMode = true;
                    break;
                case ThemeService.Theme.Light:
                    _isLightMode = true;
                    break;
                case ThemeService.Theme.Custom:
                    _isCustomTheme = true;
                    break;
            }
        }

        private void ExecutePickColor(object parameter)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CustomThemeColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    colorDialog.Color.A,
                    colorDialog.Color.R,
                    colorDialog.Color.G,
                    colorDialog.Color.B));
            }
        }

        private void ExecuteBrowseAutoSaveLocation(object parameter)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AutoSaveLocation = folderDialog.SelectedPath;
            }
        }

        private void ExecuteClearRecentFiles(object parameter)
        {
            // TODO: Clear recent files history
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
            IsDarkMode = true;
            IsLightMode = false;
            IsCustomTheme = false;
            CustomThemeColor = new SolidColorBrush(Colors.DarkBlue);
            SelectedLanguage = "English";
            AutoSaveEnabled = true;
            AutoSaveInterval = 5;
            AutoSaveLocation = @"C:\Users\cs121287\Documents\WWXMapEditor\AutoSave";
            RecentFilesCount = 10;
        }
    }
}