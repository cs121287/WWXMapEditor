using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WWXMapEditor.Models;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels
{
    public partial class MapEditorViewModel
    {
        #region Properties

        public Map CurrentMap
        {
            get => _currentMap;
            set
            {
                if (SetProperty(ref _currentMap, value))
                {
                    UpdateMapProperties();
                    InitializeMapCanvas();
                }
            }
        }

        public string? CurrentFilePath
        {
            get => _currentFilePath;
            set => SetProperty(ref _currentFilePath, value);
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                if (SetProperty(ref _hasUnsavedChanges, value))
                {
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        public string WindowTitle
        {
            get
            {
                var title = "WWX Map Editor";
                if (CurrentMap != null)
                {
                    title += $" - {CurrentMap.Name}";
                    if (HasUnsavedChanges)
                    {
                        title += " *";
                    }
                }
                return title;
            }
        }

        public string MapName => CurrentMap?.Name ?? "Untitled Map";
        public int MapWidth => CurrentMap?.Width ?? 50;
        public int MapHeight => CurrentMap?.Height ?? 50;
        public int NumberOfPlayers => CurrentMap?.NumberOfPlayers ?? 2;
        public string DefaultTerrain => CurrentMap?.StartingTerrain ?? "Plains";

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
            set
            {
                if (SetProperty(ref _zoomLevel, value))
                {
                    ZoomScale = value / 100.0;
                }
            }
        }

        public double ZoomScale
        {
            get => _zoomScale;
            set => SetProperty(ref _zoomScale, value);
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                if (SetProperty(ref _showGrid, value))
                {
                    OnPropertyChanged(nameof(GridVisibility));
                }
            }
        }

        public bool SnapToGrid
        {
            get => _snapToGrid;
            set => SetProperty(ref _snapToGrid, value);
        }

        public MapLayer SelectedLayer
        {
            get => _selectedLayer;
            set
            {
                if (SetProperty(ref _selectedLayer, value))
                {
                    UpdateLayerUI();
                    CurrentLayer = Layers.IndexOf(value);
                }
            }
        }

        public int BrushSize
        {
            get => _brushSize;
            set
            {
                if (SetProperty(ref _brushSize, value))
                {
                    OnPropertyChanged(nameof(BrushSizeDisplay));
                    UpdateHoverIndicator();
                }
            }
        }

        public string BrushSizeDisplay => $"{BrushSize}×{BrushSize} tiles";

        public bool ShowCollision
        {
            get => _showCollision;
            set => SetProperty(ref _showCollision, value);
        }

        public bool ShowCoordinates
        {
            get => _showCoordinates;
            set
            {
                if (SetProperty(ref _showCoordinates, value))
                {
                    OnPropertyChanged(nameof(CoordinateDisplayVisibility));
                }
            }
        }

        public double LayerOpacity
        {
            get => _layerOpacity;
            set => SetProperty(ref _layerOpacity, value);
        }

        public string SelectedOwner
        {
            get => _selectedOwner;
            set => SetProperty(ref _selectedOwner, value);
        }

        public bool BlockAircraft
        {
            get => _blockAircraft;
            set
            {
                if (SetProperty(ref _blockAircraft, value))
                {
                    OnPropertyChanged(nameof(AllowAircraft));
                }
            }
        }

        public bool AllowAircraft
        {
            get => !_blockAircraft;
            set => BlockAircraft = !value;
        }

        // New properties for tile palette integration
        public TilePaletteItem? SelectedTile
        {
            get => _selectedTilePaletteItem;
            set => SetProperty(ref _selectedTilePaletteItem, value);
        }

        public int MouseTileX
        {
            get => _mouseTileX;
            set => SetProperty(ref _mouseTileX, value);
        }

        public int MouseTileY
        {
            get => _mouseTileY;
            set => SetProperty(ref _mouseTileY, value);
        }

        // New tool active state properties
        public bool IsSelectToolActive
        {
            get => _isSelectToolActive;
            set => SetProperty(ref _isSelectToolActive, value);
        }

        public bool IsBrushToolActive
        {
            get => _isBrushToolActive;
            set => SetProperty(ref _isBrushToolActive, value);
        }

        public bool IsRectangleToolActive
        {
            get => _isRectangleToolActive;
            set => SetProperty(ref _isRectangleToolActive, value);
        }

        public bool IsFillToolActive
        {
            get => _isFillToolActive;
            set => SetProperty(ref _isFillToolActive, value);
        }

        public bool IsEraserToolActive
        {
            get => _isEraserToolActive;
            set => SetProperty(ref _isEraserToolActive, value);
        }

        // UI State Properties
        public string AutoSaveStatus
        {
            get => _autoSaveStatus;
            set => SetProperty(ref _autoSaveStatus, value);
        }

        public Visibility AutoSaveVisibility
        {
            get => _autoSaveVisibility;
            set => SetProperty(ref _autoSaveVisibility, value);
        }

        public string CoordinateDisplay
        {
            get => _coordinateDisplay;
            set => SetProperty(ref _coordinateDisplay, value);
        }

        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public string MemoryUsage
        {
            get => _memoryUsage;
            set => SetProperty(ref _memoryUsage, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public double CanvasWidth
        {
            get => _canvasWidth;
            set => SetProperty(ref _canvasWidth, value);
        }

        public double CanvasHeight
        {
            get => _canvasHeight;
            set => SetProperty(ref _canvasHeight, value);
        }

        // Hover Indicator Properties
        public double HoverIndicatorX
        {
            get => _hoverIndicatorX;
            set => SetProperty(ref _hoverIndicatorX, value);
        }

        public double HoverIndicatorY
        {
            get => _hoverIndicatorY;
            set => SetProperty(ref _hoverIndicatorY, value);
        }

        public double HoverIndicatorWidth
        {
            get => _hoverIndicatorWidth;
            set => SetProperty(ref _hoverIndicatorWidth, value);
        }

        public double HoverIndicatorHeight
        {
            get => _hoverIndicatorHeight;
            set => SetProperty(ref _hoverIndicatorHeight, value);
        }

        public Visibility HoverIndicatorVisibility
        {
            get => _hoverIndicatorVisibility;
            set => SetProperty(ref _hoverIndicatorVisibility, value);
        }

        // Ruler Properties
        public double RulerX1
        {
            get => _rulerX1;
            set => SetProperty(ref _rulerX1, value);
        }

        public double RulerY1
        {
            get => _rulerY1;
            set => SetProperty(ref _rulerY1, value);
        }

        public double RulerX2
        {
            get => _rulerX2;
            set => SetProperty(ref _rulerX2, value);
        }

        public double RulerY2
        {
            get => _rulerY2;
            set => SetProperty(ref _rulerY2, value);
        }

        public string RulerDistance
        {
            get => _rulerDistance;
            set => SetProperty(ref _rulerDistance, value);
        }

        public double RulerTextX
        {
            get => _rulerTextX;
            set => SetProperty(ref _rulerTextX, value);
        }

        public double RulerTextY
        {
            get => _rulerTextY;
            set => SetProperty(ref _rulerTextY, value);
        }

        public Visibility RulerVisibility
        {
            get => _rulerVisibility;
            set => SetProperty(ref _rulerVisibility, value);
        }

        // Mini-map Properties
        public double MiniMapWidth
        {
            get => _miniMapWidth;
            set => SetProperty(ref _miniMapWidth, value);
        }

        public double MiniMapHeight
        {
            get => _miniMapHeight;
            set => SetProperty(ref _miniMapHeight, value);
        }

        public double ViewportX
        {
            get => _viewportX;
            set => SetProperty(ref _viewportX, value);
        }

        public double ViewportY
        {
            get => _viewportY;
            set => SetProperty(ref _viewportY, value);
        }

        public double ViewportWidth
        {
            get => _viewportWidth;
            set => SetProperty(ref _viewportWidth, value);
        }

        public double ViewportHeight
        {
            get => _viewportHeight;
            set => SetProperty(ref _viewportHeight, value);
        }

        public System.Windows.Media.Imaging.BitmapSource MiniMapImage
        {
            get => _miniMapImage;
            set => SetProperty(ref _miniMapImage, value);
        }

        // Selected Tile Properties
        public int SelectedTileX
        {
            get => _selectedTileX;
            set => SetProperty(ref _selectedTileX, value);
        }

        public int SelectedTileY
        {
            get => _selectedTileY;
            set => SetProperty(ref _selectedTileY, value);
        }

        public string SelectedTileTerrain
        {
            get => _selectedTileTerrain;
            set => SetProperty(ref _selectedTileTerrain, value);
        }

        public string SelectedTileCollision
        {
            get => _selectedTileCollision;
            set => SetProperty(ref _selectedTileCollision, value);
        }

        public string SelectedTileProperty
        {
            get => _selectedTileProperty;
            set => SetProperty(ref _selectedTileProperty, value);
        }

        public string SelectedTileUnit
        {
            get => _selectedTileUnit;
            set => SetProperty(ref _selectedTileUnit, value);
        }

        public Visibility SelectedTilePropertyVisibility
        {
            get => _selectedTilePropertyVisibility;
            set => SetProperty(ref _selectedTilePropertyVisibility, value);
        }

        public Visibility SelectedTileUnitVisibility
        {
            get => _selectedTileUnitVisibility;
            set => SetProperty(ref _selectedTileUnitVisibility, value);
        }

        // Tool Selection Properties
        public bool IsPaintToolSelected
        {
            get => _isPaintToolSelected;
            set
            {
                if (SetProperty(ref _isPaintToolSelected, value) && value)
                {
                    SelectedTool = "Paint";
                    StatusMessage = "Paint tool selected";
                    // Update new tool states
                    IsBrushToolActive = true;
                    IsSelectToolActive = false;
                    IsRectangleToolActive = false;
                    IsFillToolActive = false;
                    IsEraserToolActive = false;
                }
            }
        }

        public bool IsEraserToolSelected
        {
            get => _isEraserToolSelected;
            set
            {
                if (SetProperty(ref _isEraserToolSelected, value) && value)
                {
                    SelectedTool = "Eraser";
                    StatusMessage = "Eraser tool selected";
                    // Update new tool states
                    IsEraserToolActive = true;
                    IsBrushToolActive = false;
                    IsSelectToolActive = false;
                    IsRectangleToolActive = false;
                    IsFillToolActive = false;
                }
            }
        }

        public bool IsSelectToolSelected
        {
            get => _isSelectToolSelected;
            set
            {
                if (SetProperty(ref _isSelectToolSelected, value) && value)
                {
                    SelectedTool = "Select";
                    StatusMessage = "Selection tool selected";
                    // Update new tool states
                    IsSelectToolActive = true;
                    IsBrushToolActive = false;
                    IsRectangleToolActive = false;
                    IsFillToolActive = false;
                    IsEraserToolActive = false;
                }
            }
        }

        public bool IsFillToolSelected
        {
            get => _isFillToolSelected;
            set
            {
                if (SetProperty(ref _isFillToolSelected, value) && value)
                {
                    SelectedTool = "Fill";
                    StatusMessage = "Fill tool selected";
                    // Update new tool states
                    IsFillToolActive = true;
                    IsBrushToolActive = false;
                    IsSelectToolActive = false;
                    IsRectangleToolActive = false;
                    IsEraserToolActive = false;
                }
            }
        }

        public bool IsRulerToolSelected
        {
            get => _isRulerToolSelected;
            set
            {
                if (SetProperty(ref _isRulerToolSelected, value) && value)
                {
                    SelectedTool = "Ruler";
                    StatusMessage = "Ruler tool selected";
                    // Ruler doesn't map to new tool states
                    IsBrushToolActive = false;
                    IsSelectToolActive = false;
                    IsRectangleToolActive = false;
                    IsFillToolActive = false;
                    IsEraserToolActive = false;
                }
            }
        }

        // Visibility Properties
        public Visibility GridVisibility => ShowGrid ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CoordinateDisplayVisibility => ShowCoordinates ? Visibility.Visible : Visibility.Collapsed;
        public Visibility BrushSizeVisibility => (CurrentLayer == 0 || CurrentLayer == 1) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility TilesetVisibility => (CurrentLayer == 0 || CurrentLayer == 2 || CurrentLayer == 3) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AircraftOptionsVisibility => CurrentLayer == 1 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility OwnershipVisibility => (CurrentLayer == 2 || CurrentLayer == 3) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AutoTileVisibility => CurrentLayer == 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility BalanceResourcesVisibility => CurrentLayer == 2 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ValidationResultsVisibility
        {
            get => _validationResultsVisibility;
            set => SetProperty(ref _validationResultsVisibility, value);
        }

        // New visibility properties for integration
        public Visibility IsBrushSettingsVisible =>
            (SelectedTool == "Brush" || SelectedTool == "Paint" || SelectedTool == "Eraser") ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsSelectedTileVisible =>
            SelectedTile != null ? Visibility.Visible : Visibility.Collapsed;

        public string TilesetHeader
        {
            get
            {
                switch (CurrentLayer)
                {
                    case 0: return "TERRAIN TILES";
                    case 2: return "PROPERTIES";
                    case 3: return "UNITS";
                    default: return "TILES";
                }
            }
        }

        public string CurrentLayerStatus => SelectedLayer?.Name ?? "None";
        public string CurrentToolStatus => SelectedTool;

        // Background colors for tools
        public System.Windows.Media.Brush PaintToolBackground => IsPaintToolSelected ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70)) : System.Windows.Media.Brushes.Transparent;
        public System.Windows.Media.Brush EraserToolBackground => IsEraserToolSelected ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70)) : System.Windows.Media.Brushes.Transparent;
        public System.Windows.Media.Brush SelectToolBackground => IsSelectToolSelected ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70)) : System.Windows.Media.Brushes.Transparent;
        public System.Windows.Media.Brush FillToolBackground => IsFillToolSelected ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70)) : System.Windows.Media.Brushes.Transparent;
        public System.Windows.Media.Brush RulerToolBackground => IsRulerToolSelected ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70)) : System.Windows.Media.Brushes.Transparent;

        // Undo/Redo
        public bool CanUndo => _undoRedoManager?.CanUndo ?? false;
        public bool CanRedo => _undoRedoManager?.CanRedo ?? false;

        #endregion

        #region Commands

        // Commands - keeping all existing commands
        public ICommand FileMenuCommand { get; private set; }
        public ICommand EditMenuCommand { get; private set; }
        public ICommand ViewMenuCommand { get; private set; }
        public ICommand ToolsMenuCommand { get; private set; }
        public ICommand HelpMenuCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand SaveAsCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand CutCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand PasteCommand { get; private set; }
        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }
        public ICommand ResetZoomCommand { get; private set; }
        public ICommand ToggleGridCommand { get; private set; }
        public ICommand ToggleSnapCommand { get; private set; }
        public ICommand SelectToolCommand { get; private set; }

        // New commands
        public ICommand TestMapCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand SelectAllCommand { get; private set; }
        public ICommand SelectPaintToolCommand { get; private set; }
        public ICommand SelectEraserToolCommand { get; private set; }
        public ICommand SelectFillToolCommand { get; private set; }
        public ICommand SelectRulerToolCommand { get; private set; }
        public ICommand SelectTileCommand { get; private set; }
        public ICommand ValidateMapCommand { get; private set; }
        public ICommand ClearLayerCommand { get; private set; }
        public ICommand AutoTileCommand { get; private set; }
        public ICommand BalanceResourcesCommand { get; private set; }

        // Additional commands for new functionality
        public ICommand NewMapCommand { get; private set; }
        public ICommand OpenMapCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }

        private void InitializeCommands()
        {
            // Initialize commands - keeping all existing
            FileMenuCommand = new RelayCommand(ExecuteFileMenu);
            EditMenuCommand = new RelayCommand(ExecuteEditMenu);
            ViewMenuCommand = new RelayCommand(ExecuteViewMenu);
            ToolsMenuCommand = new RelayCommand(ExecuteToolsMenu);
            HelpMenuCommand = new RelayCommand(ExecuteHelpMenu);
            SaveCommand = new RelayCommand(ExecuteSave);
            SaveAsCommand = new RelayCommand(ExecuteSaveAs);
            ExitCommand = new RelayCommand(ExecuteExit);
            UndoCommand = new RelayCommand(ExecuteUndo, _ => CanUndo);
            RedoCommand = new RelayCommand(ExecuteRedo, _ => CanRedo);
            CutCommand = new RelayCommand(ExecuteCut);
            CopyCommand = new RelayCommand(ExecuteCopy);
            PasteCommand = new RelayCommand(ExecutePaste);
            ZoomInCommand = new RelayCommand(ExecuteZoomIn);
            ZoomOutCommand = new RelayCommand(ExecuteZoomOut);
            ResetZoomCommand = new RelayCommand(ExecuteResetZoom);
            ToggleGridCommand = new RelayCommand(ExecuteToggleGrid);
            ToggleSnapCommand = new RelayCommand(ExecuteToggleSnap);
            SelectToolCommand = new RelayCommand(ExecuteSelectTool);

            // Initialize new commands
            TestMapCommand = new RelayCommand(ExecuteTestMap);
            ExportCommand = new RelayCommand(ExecuteExport);
            SelectAllCommand = new RelayCommand(ExecuteSelectAll);
            SelectPaintToolCommand = new RelayCommand(_ => IsPaintToolSelected = true);
            SelectEraserToolCommand = new RelayCommand(_ => IsEraserToolSelected = true);
            SelectFillToolCommand = new RelayCommand(_ => IsFillToolSelected = true);
            SelectRulerToolCommand = new RelayCommand(_ => IsRulerToolSelected = true);
            SelectTileCommand = new RelayCommand(ExecuteSelectTile);
            ValidateMapCommand = new RelayCommand(ExecuteValidateMap);
            ClearLayerCommand = new RelayCommand(ExecuteClearLayer);
            AutoTileCommand = new RelayCommand(ExecuteAutoTile);
            BalanceResourcesCommand = new RelayCommand(ExecuteBalanceResources);

            // Initialize additional commands
            NewMapCommand = new RelayCommand(ExecuteNewMap);
            OpenMapCommand = new RelayCommand(ExecuteOpenMap);
            AboutCommand = new RelayCommand(ExecuteAbout);
        }

        #endregion

        #region Command Implementations

        // Keeping all existing command implementations
        private void ExecuteFileMenu(object? parameter) { }
        private void ExecuteEditMenu(object? parameter) { }
        private void ExecuteViewMenu(object? parameter) { }
        private void ExecuteToolsMenu(object? parameter) { }
        private void ExecuteHelpMenu(object? parameter) { }

        private void ExecuteSave(object? parameter)
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                ExecuteSaveAs(parameter);
                return;
            }

            // Validate before saving
            var validationResult = _validationService.ValidateMap(CurrentMap);
            if (!validationResult.IsValid)
            {
                var message = validationResult.GetSummary();
                var result = System.Windows.MessageBox.Show(
                    $"{message}\n\nDo you want to save anyway?",
                    "Map Validation Failed",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var saveResult = _mapService.SaveMap(CurrentMap, CurrentFilePath);
            if (saveResult.Success)
            {
                HasUnsavedChanges = false;
                StatusMessage = $"Saved: {System.IO.Path.GetFileName(CurrentFilePath)}";
                AddToRecentMaps(CurrentFilePath);
            }
            else
            {
                System.Windows.MessageBox.Show(
                    $"Failed to save map:\n{saveResult.ErrorMessage}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void ExecuteSaveAs(object? parameter)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "WWX Map Files (*.wwxmap)|*.wwxmap|JSON Files (*.json)|*.json",
                DefaultExt = ".wwxmap",
                FileName = CurrentMap.Name,
                Title = "Save Map As",
                InitialDirectory = SettingsService.Instance.Settings.DefaultProjectDirectory
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Validate before saving
                var validationResult = _validationService.ValidateMap(CurrentMap);
                if (!validationResult.IsValid)
                {
                    var message = validationResult.GetSummary();
                    var result = System.Windows.MessageBox.Show(
                        $"{message}\n\nDo you want to save anyway?",
                        "Map Validation Failed",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Warning);

                    if (result != System.Windows.MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                var saveResult = _mapService.SaveMap(CurrentMap, saveFileDialog.FileName);
                if (saveResult.Success)
                {
                    CurrentFilePath = saveFileDialog.FileName;
                    HasUnsavedChanges = false;
                    StatusMessage = $"Saved: {System.IO.Path.GetFileName(saveFileDialog.FileName)}";
                    AddToRecentMaps(saveFileDialog.FileName);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        $"Failed to save map:\n{saveResult.ErrorMessage}",
                        "Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteExit(object? parameter)
        {
            if (HasUnsavedChanges)
            {
                var result = System.Windows.MessageBox.Show(
                    "You have unsaved changes. Do you want to save before exiting?",
                    "Unsaved Changes",
                    System.Windows.MessageBoxButton.YesNoCancel,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    ExecuteSave(null);
                }
                else if (result == System.Windows.MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            Cleanup();
            _mainWindowViewModel.NavigateToMainMenu();
        }

        private void ExecuteUndo(object? parameter)
        {
            _undoRedoManager.Undo();
            StatusMessage = "Action undone";
        }

        private void ExecuteRedo(object? parameter)
        {
            _undoRedoManager.Redo();
            StatusMessage = "Action redone";
        }

        private void ExecuteCut(object? parameter)
        {
            // TODO: Implement cut functionality
            StatusMessage = "Cut functionality not yet implemented";
        }

        private void ExecuteCopy(object? parameter)
        {
            // TODO: Implement copy functionality
            StatusMessage = "Selection copied";
        }

        private void ExecutePaste(object? parameter)
        {
            // TODO: Implement paste functionality
            StatusMessage = "Selection pasted";
        }

        private void ExecuteZoomIn(object? parameter)
        {
            ZoomLevel = Math.Min(ZoomLevel * 1.2, 500);
            StatusMessage = $"Zoom: {ZoomLevel:F0}%";
        }

        private void ExecuteZoomOut(object? parameter)
        {
            ZoomLevel = Math.Max(ZoomLevel / 1.2, 10);
            StatusMessage = $"Zoom: {ZoomLevel:F0}%";
        }

        private void ExecuteResetZoom(object? parameter)
        {
            ZoomLevel = 100;
            StatusMessage = "Zoom: 100%";
        }

        private void ExecuteToggleGrid(object? parameter)
        {
            ShowGrid = !ShowGrid;
            StatusMessage = ShowGrid ? "Grid: On" : "Grid: Off";
        }

        private void ExecuteToggleSnap(object? parameter)
        {
            SnapToGrid = !SnapToGrid;
            StatusMessage = SnapToGrid ? "Snap to grid: On" : "Snap to grid: Off";
        }

        private void ExecuteSelectTool(object? parameter)
        {
            if (parameter is string tool)
            {
                SelectedTool = tool;
                StatusMessage = $"{tool} tool selected";

                // Update tool active states
                IsSelectToolActive = tool == "Select";
                IsBrushToolActive = tool == "Brush" || tool == "Paint";
                IsRectangleToolActive = tool == "Rectangle";
                IsFillToolActive = tool == "Fill";
                IsEraserToolActive = tool == "Eraser";

                // Update old tool selection states
                IsPaintToolSelected = tool == "Paint" || tool == "Brush";
                IsEraserToolSelected = tool == "Eraser";
                IsSelectToolSelected = tool == "Select";
                IsFillToolSelected = tool == "Fill";
                IsRulerToolSelected = tool == "Ruler";

                // Notify about visibility changes
                OnPropertyChanged(nameof(IsBrushSettingsVisible));
            }
        }

        // New command implementations
        private void ExecuteNewMap(object? parameter)
        {
            if (HasUnsavedChanges)
            {
                var result = System.Windows.MessageBox.Show(
                    "You have unsaved changes. Do you want to save before creating a new map?",
                    "Unsaved Changes",
                    System.Windows.MessageBoxButton.YesNoCancel,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    ExecuteSave(null);
                }
                else if (result == System.Windows.MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            Cleanup();
            _mainWindowViewModel.NavigateToNewMapCreation();
        }

        private void ExecuteOpenMap(object? parameter)
        {
            if (HasUnsavedChanges)
            {
                var result = System.Windows.MessageBox.Show(
                    "You have unsaved changes. Do you want to save before opening another map?",
                    "Unsaved Changes",
                    System.Windows.MessageBoxButton.YesNoCancel,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    ExecuteSave(null);
                }
                else if (result == System.Windows.MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WWX Map Files (*.wwxmap)|*.wwxmap|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = ".wwxmap",
                Title = "Open Map",
                InitialDirectory = SettingsService.Instance.Settings.DefaultProjectDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var result = _mapService.LoadMap(openFileDialog.FileName);
                if (result.Success && result.Data != null)
                {
                    CurrentMap = result.Data;
                    CurrentFilePath = openFileDialog.FileName;
                    HasUnsavedChanges = false;
                    StatusMessage = $"Loaded: {System.IO.Path.GetFileName(openFileDialog.FileName)}";
                    AddToRecentMaps(openFileDialog.FileName);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        $"Failed to load map:\n{result.ErrorMessage}",
                        "Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteAbout(object? parameter)
        {
            _mainWindowViewModel.NavigateToAbout();
        }

        private void ExecuteTestMap(object? parameter)
        {
            StatusMessage = "Launching map test...";
            // TODO: Implement map testing
        }

        private void ExecuteExport(object? parameter)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|XML Files (*.xml)|*.xml|PNG Image (*.png)|*.png",
                DefaultExt = ".json",
                FileName = CurrentMap.Name,
                Title = "Export Map",
                InitialDirectory = SettingsService.Instance.Settings.DefaultExportDirectory
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                var format = extension.TrimStart('.');

                var result = _mapService.ExportMap(CurrentMap, saveFileDialog.FileName, format);
                if (result.Success)
                {
                    StatusMessage = $"Exported: {System.IO.Path.GetFileName(saveFileDialog.FileName)}";
                    System.Windows.MessageBox.Show(
                        "Map exported successfully!",
                        "Export Complete",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        $"Failed to export map:\n{result.ErrorMessage}",
                        "Export Failed",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteSelectAll(object? parameter)
        {
            StatusMessage = "All tiles selected";
            // TODO: Implement select all
        }

        private void ExecuteSelectTile(object? parameter)
        {
            if (parameter is TileData tile)
            {
                // Deselect previous tile
                if (_selectedTileData != null)
                {
                    _selectedTileData.BorderBrush = System.Windows.Media.Brushes.Transparent;
                }

                // Select new tile
                _selectedTileData = tile;
                _selectedTileData.BorderBrush = System.Windows.Media.Brushes.Yellow;

                // Add to recent tiles
                if (!RecentTiles.Contains(tile))
                {
                    RecentTiles.Insert(0, tile);
                    if (RecentTiles.Count > 5)
                    {
                        RecentTiles.RemoveAt(RecentTiles.Count - 1);
                    }
                }
            }
            else if (parameter is TilePaletteItem paletteItem)
            {
                SelectTilePaletteItem(paletteItem);

                // Auto-switch to brush tool when selecting a tile
                if (SelectedTool != "Brush" && SelectedTool != "Paint" && SelectedTool != "Fill")
                {
                    ExecuteSelectTool("Brush");
                }
            }
        }

        #endregion
    }
}