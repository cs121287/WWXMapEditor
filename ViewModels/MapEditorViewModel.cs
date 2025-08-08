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
using Microsoft.Win32;

namespace WWXMapEditor.ViewModels
{
    public partial class MapEditorViewModel : ViewModelBase, IDisposable
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly MapService _mapService;
        private readonly MapValidationService _validationService;
        private Map _currentMap;
        private string? _currentFilePath;
        private bool _hasUnsavedChanges;
        private string _selectedTool = "Paint";
        private int _currentLayer = 0;
        private int _gridSize = 32;
        private double _zoomLevel = 100;
        private bool _showGrid = true;
        private bool _snapToGrid = true;
        private bool _disposed = false;

        // New layer system properties
        private MapLayer _selectedLayer;
        private int _brushSize = 1;
        private double _zoomScale = 1.0;
        private bool _showCollision = true;
        private bool _showCoordinates = true;
        private double _layerOpacity = 1.0;
        private bool _isDrawing = false;
        private System.Windows.Point _lastDrawPosition;
        private TileData _selectedTileData;
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
        private DispatcherTimer? _autoSaveTimer;
        private DispatcherTimer? _clockTimer;
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

        // New collections for tile palette integration
        public ObservableCollection<TilePaletteItem> TilePalette { get; }
        public ObservableCollection<int> BrushSizes { get; }

        // Additional properties for new functionality
        private TilePaletteItem? _selectedTilePaletteItem;
        private int _mouseTileX;
        private int _mouseTileY;

        // Tool active states for new UI
        private bool _isSelectToolActive = false;
        private bool _isBrushToolActive = true;
        private bool _isRectangleToolActive = false;
        private bool _isFillToolActive = false;
        private bool _isEraserToolActive = false;

        #region Constructor

        public MapEditorViewModel(MainWindowViewModel mainWindowViewModel, Map? map = null)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _mapService = new MapService();
            _validationService = new MapValidationService();

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

            // Initialize new collections
            TilePalette = new ObservableCollection<TilePaletteItem>();
            BrushSizes = new ObservableCollection<int> { 1, 2, 3, 4, 5 };

            // Initialize tile palette
            InitializeTilePalette();

            // Initialize services
            _undoRedoManager = new UndoRedoManager();

            // Initialize timers
            SetupTimers();

            // Initialize commands
            InitializeCommands();

            // Apply settings
            ApplySettings();

            // Initialize view
            _selectedLayer = Layers[0];
            InitializeMapCanvas();
            UpdateLayerUI();
            UpdateMapStatistics();
            UpdateAvailableOwners();

            // Select first tile by default
            if (TilePalette.Count > 0)
            {
                SelectTilePaletteItem(TilePalette[0]);
            }
        }

        #endregion

        #region Initialization Methods

        private void SetupTimers()
        {
            // Auto-save timer
            var settings = SettingsService.Instance.Settings;
            if (settings.AutoSaveEnabled && settings.AutoSaveInterval > 0)
            {
                _autoSaveTimer = new DispatcherTimer();
                _autoSaveTimer.Interval = TimeSpan.FromMinutes(settings.AutoSaveInterval);
                _autoSaveTimer.Tick += OnAutoSave;
                _autoSaveTimer.Start();
            }

            // Clock timer
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += OnClockTick;
            _clockTimer.Start();
        }

        private void InitializeTilePalette()
        {
            TilePalette.Add(new TilePaletteItem
            {
                Name = "Plains",
                TerrainType = "Plains",
                Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(144, 238, 144))
            });

            TilePalette.Add(new TilePaletteItem
            {
                Name = "Mountain",
                TerrainType = "Mountain",
                Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 137, 137))
            });

            TilePalette.Add(new TilePaletteItem
            {
                Name = "Forest",
                TerrainType = "Forest",
                Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 139, 34))
            });

            TilePalette.Add(new TilePaletteItem
            {
                Name = "Sand",
                TerrainType = "Sand",
                Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(238, 203, 173))
            });

            TilePalette.Add(new TilePaletteItem
            {
                Name = "Sea",
                TerrainType = "Sea",
                Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 164, 223))
            });
        }

        private void SelectTilePaletteItem(TilePaletteItem tile)
        {
            // Deselect all tiles
            foreach (var t in TilePalette)
            {
                t.IsSelected = false;
            }

            // Select the new tile
            tile.IsSelected = true;
            SelectedTile = tile;

            // Update the old tile data for compatibility
            if (_selectedTileData == null)
            {
                _selectedTileData = new TileData();
            }
            _selectedTileData.Name = tile.Name;
            _selectedTileData.ImageSource = "/Assets/Terrain/" + tile.Name.ToLower() + ".png";
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

            return _mapService.CreateNewMap(defaultProperties);
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
            OnPropertyChanged(nameof(WindowTitle));
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

        #endregion

        #region Public Methods

        public void UpdateMousePosition(double x, double y)
        {
            int tileX = (int)(x / GridSize);
            int tileY = (int)(y / GridSize);

            if (tileX >= 0 && tileX < MapWidth && tileY >= 0 && tileY < MapHeight)
            {
                CoordinateDisplay = $"X: {tileX}, Y: {tileY}";
                MouseTileX = tileX;
                MouseTileY = tileY;

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

        public void OnMapModified()
        {
            HasUnsavedChanges = true;
            _undoRedoManager.RecordState();
        }

        public void Cleanup()
        {
            if (!_disposed)
            {
                if (_autoSaveTimer != null)
                {
                    _autoSaveTimer.Stop();
                    _autoSaveTimer.Tick -= OnAutoSave;
                    _autoSaveTimer = null;
                }

                if (_clockTimer != null)
                {
                    _clockTimer.Stop();
                    _clockTimer.Tick -= OnClockTick;
                    _clockTimer = null;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Cleanup();
        }

        #endregion

        #region Private Methods

        private void DrawAtPosition(double x, double y)
        {
            int tileX = (int)(x / GridSize);
            int tileY = (int)(y / GridSize);

            if (tileX < 0 || tileX >= MapWidth || tileY < 0 || tileY >= MapHeight)
                return;

            // Apply brush size
            for (int bx = 0; bx < BrushSize; bx++)
            {
                for (int by = 0; by < BrushSize; by++)
                {
                    int targetX = tileX + bx;
                    int targetY = tileY + by;

                    if (targetX >= 0 && targetX < MapWidth && targetY >= 0 && targetY < MapHeight)
                    {
                        var tile = CurrentMap.Tiles[targetX, targetY];
                        if (tile != null)
                        {
                            switch (SelectedTool)
                            {
                                case "Paint":
                                case "Brush":
                                    if (SelectedTile != null && CurrentLayer == 0)
                                    {
                                        tile.TerrainType = SelectedTile.TerrainType;
                                        OnMapModified();
                                    }
                                    break;

                                case "Eraser":
                                    if (CurrentLayer == 0)
                                    {
                                        tile.TerrainType = DefaultTerrain;
                                    }
                                    else if (CurrentLayer == 2)
                                    {
                                        tile.Property = null;
                                    }
                                    else if (CurrentLayer == 3)
                                    {
                                        tile.Unit = null;
                                    }
                                    OnMapModified();
                                    break;

                                case "Fill":
                                    // TODO: Implement flood fill
                                    break;
                            }
                        }
                    }
                }
            }

            UpdateMapStatistics();
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
                _selectedTileData = AvailableTiles[0];
                _selectedTileData.BorderBrush = System.Windows.Media.Brushes.Yellow;
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

        private void AddToRecentMaps(string filePath)
        {
            try
            {
                var settings = SettingsService.Instance.Settings;

                // Remove if already exists
                settings.RecentMaps.Remove(filePath);

                // Add to beginning
                settings.RecentMaps.Insert(0, filePath);

                // Limit to configured count
                while (settings.RecentMaps.Count > settings.RecentFilesCount)
                {
                    settings.RecentMaps.RemoveAt(settings.RecentMaps.Count - 1);
                }

                // Save settings
                SettingsService.Instance.SaveSettings(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating recent maps: {ex.Message}");
            }
        }

        private void OnAutoSave(object? sender, EventArgs e)
        {
            if (HasUnsavedChanges && !string.IsNullOrEmpty(CurrentFilePath))
            {
                ExecuteAutoSave();
            }
        }

        private void ExecuteAutoSave()
        {
            try
            {
                var settings = SettingsService.Instance.Settings;
                var autoSavePath = System.IO.Path.Combine(
                    settings.AutoSaveLocation,
                    $"{CurrentMap.Name}_autosave_{DateTime.Now:yyyyMMdd_HHmmss}.wwxmap"
                );

                var result = _mapService.SaveMap(CurrentMap, autoSavePath);
                if (result.Success)
                {
                    ShowAutoSaveNotification();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-save failed: {ex.Message}");
            }
        }

        private void OnClockTick(object? sender, EventArgs e)
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
            StatusMessage = $"Auto-saved at {DateTime.Now:HH:mm:ss}";

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

        #region Additional Command Implementations

        private void ExecuteValidateMap(object? parameter)
        {
            ValidationResults.Clear();

            var result = _validationService.ValidateMap(CurrentMap);

            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ValidationResults.Add(new ValidationResult
                    {
                        Type = "Error",
                        Message = error.Message
                    });
                }
            }

            if (result.Warnings.Any())
            {
                foreach (var warning in result.Warnings)
                {
                    ValidationResults.Add(new ValidationResult
                    {
                        Type = "Warning",
                        Message = warning.Message
                    });
                }
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

        private void ExecuteClearLayer(object? parameter)
        {
            StatusMessage = $"Cleared {SelectedLayer.Name} layer";
            // TODO: Implement clear layer
        }

        private void ExecuteAutoTile(object? parameter)
        {
            StatusMessage = "Auto-tiling terrain...";
            // TODO: Implement auto-tiling
        }

        private void ExecuteBalanceResources(object? parameter)
        {
            StatusMessage = "Balancing resources...";
            // TODO: Implement resource balancing
        }

        #endregion
    }
}