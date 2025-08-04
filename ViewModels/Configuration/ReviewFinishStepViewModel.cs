using WWXMapEditor.Models;

namespace WWXMapEditor.ViewModels
{
    public class ReviewFinishStepViewModel : ViewModelBase
    {
        private readonly AppSettings _settings;
        private readonly ThemeDisplayStepViewModel _themeDisplayStep;
        private readonly EditorPreferencesStepViewModel _editorPreferencesStep;
        private readonly FileLocationsStepViewModel _fileLocationsStep;

        private string _themeSummary;
        private string _fullscreenSummary;
        private string _uiScalingSummary;
        private string _gridSummary;
        private string _gridSizeSummary;
        private string _autoSaveSummary;
        private string _fileLocationsSummary;

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

        public ReviewFinishStepViewModel(
            AppSettings settings,
            ThemeDisplayStepViewModel themeDisplayStep,
            EditorPreferencesStepViewModel editorPreferencesStep,
            FileLocationsStepViewModel fileLocationsStep)
        {
            _settings = settings;
            _themeDisplayStep = themeDisplayStep;
            _editorPreferencesStep = editorPreferencesStep;
            _fileLocationsStep = fileLocationsStep;
        }

        public void UpdateSummary()
        {
            // Get values from the actual step view models
            ThemeSummary = _themeDisplayStep.IsDarkTheme ? "Dark" : "Light";
            FullscreenSummary = _themeDisplayStep.StartInFullscreen ? "Yes" : "No";
            UIScalingSummary = _themeDisplayStep.UIScaling;
            GridSummary = _editorPreferencesStep.ShowGrid ? "Enabled" : "Disabled";
            GridSizeSummary = _editorPreferencesStep.GridSize.ToString();
            AutoSaveSummary = _editorPreferencesStep.AutoSaveEnabled ?
                $"Every {_editorPreferencesStep.AutoSaveInterval} minutes" : "Disabled";
            FileLocationsSummary = $"• Projects: {_fileLocationsStep.DefaultProjectDirectory}\n" +
                                  $"• Tilesets: {_fileLocationsStep.DefaultTilesetDirectory}\n" +
                                  $"• Auto-save: {_fileLocationsStep.AutoSaveLocation}";
        }

        public void RefreshAllProperties()
        {
            OnPropertyChanged(nameof(ThemeSummary));
            OnPropertyChanged(nameof(FullscreenSummary));
            OnPropertyChanged(nameof(UIScalingSummary));
            OnPropertyChanged(nameof(GridSummary));
            OnPropertyChanged(nameof(GridSizeSummary));
            OnPropertyChanged(nameof(AutoSaveSummary));
            OnPropertyChanged(nameof(FileLocationsSummary));
        }
    }
}