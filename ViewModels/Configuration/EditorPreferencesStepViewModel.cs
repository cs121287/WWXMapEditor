using System.Collections.ObjectModel;
using WWXMapEditor.Models;

namespace WWXMapEditor.ViewModels
{
    public class EditorPreferencesStepViewModel : ViewModelBase
    {
        private readonly AppSettings _settings;
        private bool _showGrid = true;
        private bool _snapToGrid = true;
        private int _gridSize = 32;
        private bool _autoSaveEnabled = true;
        private int _autoSaveInterval = 5;

        public bool ShowGrid
        {
            get => _showGrid;
            set => SetProperty(ref _showGrid, value);
        }

        public bool SnapToGrid
        {
            get => _snapToGrid;
            set => SetProperty(ref _snapToGrid, value);
        }

        public int GridSize
        {
            get => _gridSize;
            set => SetProperty(ref _gridSize, value);
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

        public ObservableCollection<int> GridSizeOptions { get; }
        public ObservableCollection<int> AutoSaveIntervalOptions { get; }

        public EditorPreferencesStepViewModel(AppSettings settings)
        {
            _settings = settings;
            GridSizeOptions = new ObservableCollection<int> { 16, 32, 64, 128 };
            AutoSaveIntervalOptions = new ObservableCollection<int> { 1, 5, 10, 15, 30 };

            // Load from settings
            ShowGrid = _settings.ShowGrid;
            SnapToGrid = _settings.SnapToGrid;
            GridSize = _settings.GridSize;
            AutoSaveEnabled = _settings.AutoSaveEnabled;
            AutoSaveInterval = _settings.AutoSaveInterval;
        }

        public void UpdateSettings()
        {
            _settings.ShowGrid = ShowGrid;
            _settings.SnapToGrid = SnapToGrid;
            _settings.GridSize = GridSize;
            _settings.AutoSaveEnabled = AutoSaveEnabled;
            _settings.AutoSaveInterval = AutoSaveInterval;
        }
    }
}