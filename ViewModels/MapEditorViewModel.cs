using System;
using System.Windows.Input;
using System.Windows.Media;
using WWXMapEditor.Models;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels
{
    public class MapEditorViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private Map _currentMap;
        private string _selectedTool = "Select";
        private int _currentLayer = 0;
        private int _gridSize = 32;
        private double _zoomLevel = 1.0;
        private bool _showGrid = true;
        private bool _snapToGrid = true;

        public Map CurrentMap
        {
            get => _currentMap;
            set => SetProperty(ref _currentMap, value);
        }

        public string SelectedTool
        {
            get => _selectedTool;
            set => SetProperty(ref _selectedTool, value);
        }

        public int CurrentLayer
        {
            get => _currentLayer;
            set => SetProperty(ref _currentLayer, value);
        }

        public int GridSize
        {
            get => _gridSize;
            set => SetProperty(ref _gridSize, value);
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

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

        // Commands
        public ICommand FileMenuCommand { get; }
        public ICommand EditMenuCommand { get; }
        public ICommand ViewMenuCommand { get; }
        public ICommand ToolsMenuCommand { get; }
        public ICommand HelpMenuCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetZoomCommand { get; }
        public ICommand ToggleGridCommand { get; }
        public ICommand ToggleSnapCommand { get; }
        public ICommand SelectToolCommand { get; }

        public MapEditorViewModel(MainWindowViewModel mainWindowViewModel, Map? map = null)
        {
            _mainWindowViewModel = mainWindowViewModel;

            // Use the provided map or create a default one
            _currentMap = map ?? CreateDefaultMap();

            // Initialize commands
            FileMenuCommand = new RelayCommand(ExecuteFileMenu);
            EditMenuCommand = new RelayCommand(ExecuteEditMenu);
            ViewMenuCommand = new RelayCommand(ExecuteViewMenu);
            ToolsMenuCommand = new RelayCommand(ExecuteToolsMenu);
            HelpMenuCommand = new RelayCommand(ExecuteHelpMenu);
            SaveCommand = new RelayCommand(ExecuteSave);
            SaveAsCommand = new RelayCommand(ExecuteSaveAs);
            ExitCommand = new RelayCommand(ExecuteExit);
            UndoCommand = new RelayCommand(ExecuteUndo);
            RedoCommand = new RelayCommand(ExecuteRedo);
            CutCommand = new RelayCommand(ExecuteCut);
            CopyCommand = new RelayCommand(ExecuteCopy);
            PasteCommand = new RelayCommand(ExecutePaste);
            ZoomInCommand = new RelayCommand(ExecuteZoomIn);
            ZoomOutCommand = new RelayCommand(ExecuteZoomOut);
            ResetZoomCommand = new RelayCommand(ExecuteResetZoom);
            ToggleGridCommand = new RelayCommand(ExecuteToggleGrid);
            ToggleSnapCommand = new RelayCommand(ExecuteToggleSnap);
            SelectToolCommand = new RelayCommand(ExecuteSelectTool);

            // Apply settings
            ApplySettings();
        }

        private Map CreateDefaultMap()
        {
            var defaultProperties = new MapProperties
            {
                Name = "Untitled Map",
                Width = 50,
                Height = 50,
                StartingTerrain = "Plains",
                NumberOfPlayers = 2
            };

            var mapService = new MapService();
            return mapService.CreateNewMap(defaultProperties);
        }

        private void ApplySettings()
        {
            var settings = SettingsService.Instance.Settings;
            GridSize = settings.GridSize;
            ShowGrid = settings.ShowGrid;
            SnapToGrid = settings.SnapToGrid;
        }

        // Command implementations
        private void ExecuteFileMenu(object parameter) { }
        private void ExecuteEditMenu(object parameter) { }
        private void ExecuteViewMenu(object parameter) { }
        private void ExecuteToolsMenu(object parameter) { }
        private void ExecuteHelpMenu(object parameter) { }

        private void ExecuteSave(object parameter)
        {
            // TODO: Implement save functionality
        }

        private void ExecuteSaveAs(object parameter)
        {
            // TODO: Implement save as functionality
        }

        private void ExecuteExit(object parameter)
        {
            _mainWindowViewModel.NavigateToMainMenu();
        }

        private void ExecuteUndo(object parameter)
        {
            // TODO: Implement undo functionality
        }

        private void ExecuteRedo(object parameter)
        {
            // TODO: Implement redo functionality
        }

        private void ExecuteCut(object parameter)
        {
            // TODO: Implement cut functionality
        }

        private void ExecuteCopy(object parameter)
        {
            // TODO: Implement copy functionality
        }

        private void ExecutePaste(object parameter)
        {
            // TODO: Implement paste functionality
        }

        private void ExecuteZoomIn(object parameter)
        {
            ZoomLevel = Math.Min(ZoomLevel * 1.2, 5.0);
        }

        private void ExecuteZoomOut(object parameter)
        {
            ZoomLevel = Math.Max(ZoomLevel / 1.2, 0.1);
        }

        private void ExecuteResetZoom(object parameter)
        {
            ZoomLevel = 1.0;
        }

        private void ExecuteToggleGrid(object parameter)
        {
            ShowGrid = !ShowGrid;
        }

        private void ExecuteToggleSnap(object parameter)
        {
            SnapToGrid = !SnapToGrid;
        }

        private void ExecuteSelectTool(object parameter)
        {
            if (parameter is string tool)
            {
                SelectedTool = tool;
            }
        }
    }
}