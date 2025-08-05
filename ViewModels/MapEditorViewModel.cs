using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WWXMapEditor.Models;
using WWXMapEditor.Services;

namespace WWXMapEditor.ViewModels
{
    public class MapEditorViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private Map _currentMap;
        private string _selectedTool = "Paint";
        private int _currentLayer = 0;
        private int _gridSize = 32;
        private double _zoomLevel = 100;
        private bool _showGrid = true;
        private bool _snapToGrid = true;

        // New layer system properties
        private MapLayer _selectedLayer;
        private int _brushSize = 1;
        private double _zoomScale = 1.0;
        private bool _showCollision = true;
        private bool _showCoordinates = true;
        private double _layerOpacity = 1.0;
        private bool _isDrawing = false;
        private System.Windows.Point _lastDrawPosition;
        private TileData _selectedTile;
        private string _selectedOwner = "Neutral";
        private bool _blockAircraft = true;

        // UI state properties
        private string _autoSaveStatus = "";
        private Visibility _autoSaveVisibility = Visibility.Collapsed;
        private string _coordinateDisplay = "";
        private string _currentTime = "";
        private string _memoryUsage = "";
        private string _statusMessage = "Ready";
        private double _canvasWidth;
        private double _canvasHeight;

        // Hover indicator
        private double _hoverIndicatorX;
        private double _hoverIndicatorY;
        private double _hoverIndicatorWidth;
        private double _hoverIndicatorHeight;
        private Visibility _hoverIndicatorVisibility = Visibility.Collapsed;

        // Ruler
        private double _rulerX1, _rulerY1, _rulerX2, _rulerY2;
        private string _rulerDistance = "";
        private double _rulerTextX, _rulerTextY;
        private Visibility _rulerVisibility = Visibility.Collapsed;
        private System.Windows.Point _rulerStartPoint;
        private bool _isRulerActive = false;

        // Mini-map
        private double _miniMapWidth = 200;
        private double _miniMapHeight = 200;
        private double _viewportX, _viewportY, _viewportWidth, _viewportHeight;
        private BitmapSource _miniMapImage;

        // Selected tile info
        private int _selectedTileX, _selectedTileY;
        private string _selectedTileTerrain = "";
        private string _selectedTileCollision = "";
        private string _selectedTileProperty = "";
        private string _selectedTileUnit = "";
        private Visibility _selectedTilePropertyVisibility = Visibility.Collapsed;
        private Visibility _selectedTileUnitVisibility = Visibility.Collapsed;

        // Tool selection
        private bool _isPaintToolSelected = true;
        private bool _isEraserToolSelected = false;
        private bool _isSelectToolSelected = false;
        private bool _isFillToolSelected = false;
        private bool _isRulerToolSelected = false;

        // Timers
        private readonly DispatcherTimer _autoSaveTimer;
        private readonly DispatcherTimer _clockTimer;
        private readonly UndoRedoManager _undoRedoManager;

        // Validation
        private Visibility _validationResultsVisibility = Visibility.Collapsed;

        // Collections
        public ObservableCollection<MapLayer> Layers { get; }
        public ObservableCollection<TileData> AvailableTiles { get; }
        public ObservableCollection<TileData> RecentTiles { get; }
        public ObservableCollection<string> AvailableOwners { get; }
        public ObservableCollection<MapTile> MapTiles { get; }
        public ObservableCollection<GridLine> GridLines { get; }
        public ObservableCollection<MapStatistic> MapStatistics { get; }
        public ObservableCollection<ValidationResult> ValidationResults { get; }

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

        public BitmapSource MiniMapImage
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

        // Commands - keeping all existing commands
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

        // New commands
        public ICommand TestMapCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand SelectPaintToolCommand { get; }
        public ICommand SelectEraserToolCommand { get; }
        public ICommand SelectFillToolCommand { get; }
        public ICommand SelectRulerToolCommand { get; }
        public ICommand SelectTileCommand { get; }
        public ICommand ValidateMapCommand { get; }
        public ICommand ClearLayerCommand { get; }
        public ICommand AutoTileCommand { get; }
        public ICommand BalanceResourcesCommand { get; }

        public MapEditorViewModel(MainWindowViewModel mainWindowViewModel, Map? map = null)
        {
            _mainWindowViewModel = mainWindowViewModel;

            // Use the provided map or create a default one
            _currentMap = map ?? CreateDefaultMap();

            // Initialize collections
            Layers = new ObservableCollection<MapLayer>
            {
                new MapLayer { Number = "1", Name = "Terrain", IsActive = true },
                new MapLayer { Number = "2", Name = "Collision", IsActive = false },
                new MapLayer { Number = "3", Name = "Properties", IsActive = false },
                new MapLayer { Number = "4", Name = "Units", IsActive = false }
            };

            AvailableTiles = new ObservableCollection<TileData>();
            RecentTiles = new ObservableCollection<TileData>();
            AvailableOwners = new ObservableCollection<string>();
            MapTiles = new ObservableCollection<MapTile>();
            GridLines = new ObservableCollection<GridLine>();
            MapStatistics = new ObservableCollection<MapStatistic>();
            ValidationResults = new ObservableCollection<ValidationResult>();

            // Initialize services
            _undoRedoManager = new UndoRedoManager();

            // Initialize timers
            _autoSaveTimer = new DispatcherTimer();
            _autoSaveTimer.Interval = TimeSpan.FromMinutes(5);
            _autoSaveTimer.Tick += OnAutoSave;
            _autoSaveTimer.Start();

            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += OnClockTick;
            _clockTimer.Start();

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

            // Apply settings
            ApplySettings();

            // Initialize view
            _selectedLayer = Layers[0];
            InitializeMapCanvas();
            UpdateLayerUI();
            UpdateMapStatistics();
            UpdateAvailableOwners();
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

        private void UpdateMapProperties()
        {
            OnPropertyChanged(nameof(MapName));
            OnPropertyChanged(nameof(MapWidth));
            OnPropertyChanged(nameof(MapHeight));
            OnPropertyChanged(nameof(NumberOfPlayers));
            OnPropertyChanged(nameof(DefaultTerrain));
        }

        private void InitializeMapCanvas()
        {
            if (CurrentMap == null) return;

            CanvasWidth = MapWidth * GridSize;
            CanvasHeight = MapHeight * GridSize;

            // Initialize grid lines
            GridLines.Clear();
            for (int x = 0; x <= MapWidth; x++)
            {
                GridLines.Add(new GridLine
                {
                    X1 = x * GridSize,
                    Y1 = 0,
                    X2 = x * GridSize,
                    Y2 = CanvasHeight
                });
            }

            for (int y = 0; y <= MapHeight; y++)
            {
                GridLines.Add(new GridLine
                {
                    X1 = 0,
                    Y1 = y * GridSize,
                    X2 = CanvasWidth,
                    Y2 = y * GridSize
                });
            }

            // Initialize map tiles
            MapTiles.Clear();
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    MapTiles.Add(new MapTile
                    {
                        X = x * GridSize,
                        Y = y * GridSize,
                        Width = GridSize,
                        Height = GridSize,
                        TerrainImage = GetDefaultTerrainImage(),
                        TerrainOpacity = 1.0,
                        CollisionOpacity = 0.5,
                        CollisionVisibility = Visibility.Collapsed
                    });
                }
            }

            // Update mini-map
            UpdateMiniMap();
        }

        private void UpdateLayerUI()
        {
            if (SelectedLayer == null) return;

            CurrentLayer = Layers.IndexOf(SelectedLayer);

            // Update visibility bindings
            OnPropertyChanged(nameof(BrushSizeVisibility));
            OnPropertyChanged(nameof(TilesetVisibility));
            OnPropertyChanged(nameof(AircraftOptionsVisibility));
            OnPropertyChanged(nameof(OwnershipVisibility));
            OnPropertyChanged(nameof(AutoTileVisibility));
            OnPropertyChanged(nameof(BalanceResourcesVisibility));
            OnPropertyChanged(nameof(TilesetHeader));
            OnPropertyChanged(nameof(CurrentLayerStatus));

            // Update available tiles based on layer
            UpdateAvailableTiles();
        }

        private void UpdateAvailableTiles()
        {
            AvailableTiles.Clear();

            switch (CurrentLayer)
            {
                case 0: // Terrain
                    AvailableTiles.Add(new TileData { Name = "Plains", ImageSource = "/Assets/Terrain/plains.png" });
                    AvailableTiles.Add(new TileData { Name = "Mountain", ImageSource = "/Assets/Terrain/mountain.png" });
                    AvailableTiles.Add(new TileData { Name = "Forest", ImageSource = "/Assets/Terrain/forest.png" });
                    AvailableTiles.Add(new TileData { Name = "Water", ImageSource = "/Assets/Terrain/water.png" });
                    AvailableTiles.Add(new TileData { Name = "Sand", ImageSource = "/Assets/Terrain/sand.png" });
                    break;

                case 2: // Properties
                    AvailableTiles.Add(new TileData { Name = "City", ImageSource = "/Assets/Properties/city.png" });
                    AvailableTiles.Add(new TileData { Name = "Factory", ImageSource = "/Assets/Properties/factory.png" });
                    AvailableTiles.Add(new TileData { Name = "Airport", ImageSource = "/Assets/Properties/airport.png" });
                    AvailableTiles.Add(new TileData { Name = "Seaport", ImageSource = "/Assets/Properties/seaport.png" });
                    AvailableTiles.Add(new TileData { Name = "HQ", ImageSource = "/Assets/Properties/hq.png" });
                    break;

                case 3: // Units
                    AvailableTiles.Add(new TileData { Name = "Infantry", ImageSource = "/Assets/Units/infantry.png" });
                    AvailableTiles.Add(new TileData { Name = "Tank", ImageSource = "/Assets/Units/tank.png" });
                    AvailableTiles.Add(new TileData { Name = "Artillery", ImageSource = "/Assets/Units/artillery.png" });
                    AvailableTiles.Add(new TileData { Name = "Fighter", ImageSource = "/Assets/Units/fighter.png" });
                    AvailableTiles.Add(new TileData { Name = "Bomber", ImageSource = "/Assets/Units/bomber.png" });
                    break;
            }

            // Select first tile if available
            if (AvailableTiles.Count > 0)
            {
                _selectedTile = AvailableTiles[0];
                _selectedTile.BorderBrush = System.Windows.Media.Brushes.Yellow;
            }
        }

        private void UpdateAvailableOwners()
        {
            AvailableOwners.Clear();

            if (CurrentLayer == 2) // Properties layer
            {
                AvailableOwners.Add("Neutral");
            }

            for (int i = 1; i <= NumberOfPlayers; i++)
            {
                AvailableOwners.Add($"Player {i}");
            }

            if (AvailableOwners.Count > 0 && !AvailableOwners.Contains(SelectedOwner))
            {
                SelectedOwner = AvailableOwners[0];
            }
        }

        private void UpdateMapStatistics()
        {
            MapStatistics.Clear();

            MapStatistics.Add(new MapStatistic { Label = "Total Tiles", Value = (MapWidth * MapHeight).ToString() });
            MapStatistics.Add(new MapStatistic { Label = "Terrain Tiles", Value = MapTiles.Count(t => t.TerrainImage != null).ToString() });
            MapStatistics.Add(new MapStatistic { Label = "Collision Tiles", Value = MapTiles.Count(t => t.CollisionVisibility == Visibility.Visible).ToString() });
            MapStatistics.Add(new MapStatistic { Label = "Properties", Value = MapTiles.Count(t => t.PropertyImage != null).ToString() });
            MapStatistics.Add(new MapStatistic { Label = "Units", Value = MapTiles.Count(t => t.UnitImage != null).ToString() });
        }

        private void UpdateMiniMap()
        {
            // TODO: Generate mini-map bitmap
            // For now, create a placeholder
            var bitmap = new WriteableBitmap(200, 200, 96, 96, PixelFormats.Bgr32, null);
            MiniMapImage = bitmap;
        }

        private void UpdateHoverIndicator()
        {
            if (_hoverIndicatorVisibility == Visibility.Visible)
            {
                HoverIndicatorWidth = BrushSize * GridSize;
                HoverIndicatorHeight = BrushSize * GridSize;
            }
        }

        private string GetDefaultTerrainImage()
        {
            switch (DefaultTerrain?.ToLower())
            {
                case "plains": return "/Assets/Terrain/plains.png";
                case "mountain": return "/Assets/Terrain/mountain.png";
                case "forest": return "/Assets/Terrain/forest.png";
                case "water": return "/Assets/Terrain/water.png";
                case "sand": return "/Assets/Terrain/sand.png";
                default: return "/Assets/Terrain/plains.png";
            }
        }

        #region Command Implementations

        // Keeping all existing command implementations
        private void ExecuteFileMenu(object parameter) { }
        private void ExecuteEditMenu(object parameter) { }
        private void ExecuteViewMenu(object parameter) { }
        private void ExecuteToolsMenu(object parameter) { }
        private void ExecuteHelpMenu(object parameter) { }

        private void ExecuteSave(object parameter)
        {
            // TODO: Implement save functionality
            StatusMessage = "Map saved";
            ShowAutoSaveNotification();
        }

        private void ExecuteSaveAs(object parameter)
        {
            // TODO: Implement save as functionality
        }

        private void ExecuteExit(object parameter)
        {
            _autoSaveTimer?.Stop();
            _clockTimer?.Stop();
            _mainWindowViewModel.NavigateToMainMenu();
        }

        private void ExecuteUndo(object parameter)
        {
            _undoRedoManager.Undo();
            StatusMessage = "Action undone";
        }

        private void ExecuteRedo(object parameter)
        {
            _undoRedoManager.Redo();
            StatusMessage = "Action redone";
        }

        private void ExecuteCut(object parameter)
        {
            // TODO: Implement cut functionality
        }

        private void ExecuteCopy(object parameter)
        {
            // TODO: Implement copy functionality
            StatusMessage = "Selection copied";
        }

        private void ExecutePaste(object parameter)
        {
            // TODO: Implement paste functionality
            StatusMessage = "Selection pasted";
        }

        private void ExecuteZoomIn(object parameter)
        {
            ZoomLevel = Math.Min(ZoomLevel * 1.2, 500);
        }

        private void ExecuteZoomOut(object parameter)
        {
            ZoomLevel = Math.Max(ZoomLevel / 1.2, 10);
        }

        private void ExecuteResetZoom(object parameter)
        {
            ZoomLevel = 100;
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

        // New command implementations
        private void ExecuteTestMap(object parameter)
        {
            StatusMessage = "Launching map test...";
            // TODO: Implement map testing
        }

        private void ExecuteExport(object parameter)
        {
            StatusMessage = "Exporting map...";
            // TODO: Implement map export
        }

        private void ExecuteSelectAll(object parameter)
        {
            StatusMessage = "All tiles selected";
            // TODO: Implement select all
        }

        private void ExecuteSelectTile(object parameter)
        {
            if (parameter is TileData tile)
            {
                // Deselect previous tile
                if (_selectedTile != null)
                {
                    _selectedTile.BorderBrush = System.Windows.Media.Brushes.Transparent;
                }

                // Select new tile
                _selectedTile = tile;
                _selectedTile.BorderBrush = System.Windows.Media.Brushes.Yellow;

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
        }

        private void ExecuteValidateMap(object parameter)
        {
            ValidationResults.Clear();

            // Check for HQ placement
            bool player1HasHQ = false;
            bool player2HasHQ = false;

            // TODO: Implement actual validation logic

            if (!player1HasHQ)
            {
                ValidationResults.Add(new ValidationResult
                {
                    Type = "Error",
                    Message = "Player 1 needs at least one HQ"
                });
            }

            if (NumberOfPlayers >= 2 && !player2HasHQ)
            {
                ValidationResults.Add(new ValidationResult
                {
                    Type = "Error",
                    Message = "Player 2 needs at least one HQ"
                });
            }

            if (ValidationResults.Count == 0)
            {
                ValidationResults.Add(new ValidationResult
                {
                    Type = "Info",
                    Message = "Map validation passed!"
                });
            }

            ValidationResultsVisibility = Visibility.Visible;
            StatusMessage = $"Validation complete: {ValidationResults.Count} issues found";
        }

        private void ExecuteClearLayer(object parameter)
        {
            StatusMessage = $"Cleared {SelectedLayer.Name} layer";
            // TODO: Implement clear layer
        }

        private void ExecuteAutoTile(object parameter)
        {
            StatusMessage = "Auto-tiling terrain...";
            // TODO: Implement auto-tiling
        }

        private void ExecuteBalanceResources(object parameter)
        {
            StatusMessage = "Balancing resources...";
            // TODO: Implement resource balancing
        }

        #endregion

        #region Public Methods

        public void UpdateMousePosition(double x, double y)
        {
            int tileX = (int)(x / GridSize);
            int tileY = (int)(y / GridSize);

            if (tileX >= 0 && tileX < MapWidth && tileY >= 0 && tileY < MapHeight)
            {
                CoordinateDisplay = $"X: {tileX}, Y: {tileY}";

                // Update hover indicator
                if (!_isRulerActive)
                {
                    HoverIndicatorX = tileX * GridSize;
                    HoverIndicatorY = tileY * GridSize;
                    HoverIndicatorWidth = BrushSize * GridSize;
                    HoverIndicatorHeight = BrushSize * GridSize;
                    HoverIndicatorVisibility = Visibility.Visible;
                }

                // Continue drawing if mouse is down
                if (_isDrawing)
                {
                    DrawAtPosition(x, y);
                }
            }
            else
            {
                HoverIndicatorVisibility = Visibility.Collapsed;
            }
        }

        public void StartDrawing(double x, double y)
        {
            if (SelectedTool == "Ruler")
            {
                _rulerStartPoint = new System.Windows.Point(x, y);
                _isRulerActive = true;
                RulerX1 = x;
                RulerY1 = y;
                RulerX2 = x;
                RulerY2 = y;
                RulerVisibility = Visibility.Visible;
            }
            else
            {
                _isDrawing = true;
                _lastDrawPosition = new System.Windows.Point(x, y);
                DrawAtPosition(x, y);
            }
        }

        public void StopDrawing()
        {
            if (_isRulerActive)
            {
                _isRulerActive = false;
                // Keep ruler visible until next action
            }
            else
            {
                _isDrawing = false;
            }
        }

        public void HideHoverIndicator()
        {
            HoverIndicatorVisibility = Visibility.Collapsed;
            CoordinateDisplay = "";
        }

        public void ShowContextMenu(double x, double y)
        {
            // TODO: Implement context menu
        }

        public void NavigateToMiniMapPosition(double x, double y)
        {
            // TODO: Implement mini-map navigation
        }

        #endregion

        #region Private Methods

        private void DrawAtPosition(double x, double y)
        {
            int tileX = (int)(x / GridSize);
            int tileY = (int)(y / GridSize);

            if (tileX < 0 || tileX >= MapWidth || tileY < 0 || tileY >= MapHeight)
                return;

            // TODO: Implement actual drawing logic based on current layer and tool

            UpdateMapStatistics();
        }

        private void OnAutoSave(object sender, EventArgs e)
        {
            ExecuteSave(null);
        }

        private void OnClockTick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");

            // Update memory usage
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / (1024 * 1024);
            MemoryUsage = $"Memory: {memoryMB} MB";
        }

        private void ShowAutoSaveNotification()
        {
            AutoSaveStatus = "Auto-saved";
            AutoSaveVisibility = Visibility.Visible;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) =>
            {
                AutoSaveVisibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        #endregion
    }

    #region Helper Classes

    public class MapLayer : ViewModelBase
    {
        private string _number = "";
        private string _name = "";
        private bool _isActive;

        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
    }

    public class TileData : ViewModelBase
    {
        private string _name = "";
        private string _imageSource = "";
        private System.Windows.Media.Brush _borderBrush = System.Windows.Media.Brushes.Transparent;
        private System.Windows.Media.Brush _background = System.Windows.Media.Brushes.White;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public System.Windows.Media.Brush BorderBrush
        {
            get => _borderBrush;
            set => SetProperty(ref _borderBrush, value);
        }

        public System.Windows.Media.Brush Background
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }
    }

    public class MapTile : ViewModelBase
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private string _terrainImage = "";
        private double _terrainOpacity = 1.0;
        private double _collisionOpacity = 0.5;
        private Visibility _collisionVisibility = Visibility.Collapsed;
        private string _propertyImage = "";
        private double _propertyOpacity = 1.0;
        private Visibility _propertyVisibility = Visibility.Collapsed;
        private string _unitImage = "";
        private double _unitOpacity = 1.0;
        private Visibility _unitVisibility = Visibility.Collapsed;
        private Visibility _selectionVisibility = Visibility.Collapsed;

        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public string TerrainImage
        {
            get => _terrainImage;
            set => SetProperty(ref _terrainImage, value);
        }

        public double TerrainOpacity
        {
            get => _terrainOpacity;
            set => SetProperty(ref _terrainOpacity, value);
        }

        public double CollisionOpacity
        {
            get => _collisionOpacity;
            set => SetProperty(ref _collisionOpacity, value);
        }

        public Visibility CollisionVisibility
        {
            get => _collisionVisibility;
            set => SetProperty(ref _collisionVisibility, value);
        }

        public string PropertyImage
        {
            get => _propertyImage;
            set => SetProperty(ref _propertyImage, value);
        }

        public double PropertyOpacity
        {
            get => _propertyOpacity;
            set => SetProperty(ref _propertyOpacity, value);
        }

        public Visibility PropertyVisibility
        {
            get => _propertyVisibility;
            set => SetProperty(ref _propertyVisibility, value);
        }

        public string UnitImage
        {
            get => _unitImage;
            set => SetProperty(ref _unitImage, value);
        }

        public double UnitOpacity
        {
            get => _unitOpacity;
            set => SetProperty(ref _unitOpacity, value);
        }

        public Visibility UnitVisibility
        {
            get => _unitVisibility;
            set => SetProperty(ref _unitVisibility, value);
        }

        public Visibility SelectionVisibility
        {
            get => _selectionVisibility;
            set => SetProperty(ref _selectionVisibility, value);
        }
    }

    public class GridLine
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
    }

    public class MapStatistic
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class ValidationResult
    {
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
    }

    // Simple UndoRedoManager implementation
    public class UndoRedoManager
    {
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Undo()
        {
            if (CanUndo)
            {
                var command = _undoStack.Pop();
                // TODO: Implement undo logic
                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var command = _redoStack.Pop();
                // TODO: Implement redo logic
                _undoStack.Push(command);
            }
        }

        public void ExecuteCommand(ICommand command)
        {
            // TODO: Implement command execution
            _undoStack.Push(command);
            _redoStack.Clear();
        }
    }

    #endregion
}