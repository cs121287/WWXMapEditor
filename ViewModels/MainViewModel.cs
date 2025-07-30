using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WwXMapEditor.Commands;
using WwXMapEditor.Models;
using WwXMapEditor.Services;

namespace WwXMapEditor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly UndoRedoManager _undoRedoManager = new();
        private readonly DispatcherTimer _autoSaveTimer;
        private Map? _currentMap;
        private string? _currentFilePath;
        private string _statusText = "Ready";
        private string _coordsText = "(0,0)";
        private string _zoomText = "100%";
        private double _zoomLevel = 1.0;
        private int _viewportX = 0;
        private int _viewportY = 0;
        private int _viewportWidth = 30;
        private int _viewportHeight = 20;
        private bool _isPainting;
        private EditTool _currentTool = EditTool.Terrain;
        private TerrainType _selectedTerrain = TerrainType.Plain;
        private PropertyType _selectedPropertyType = PropertyType.City;
        private UnitType _selectedUnitType = UnitType.Infantry;
        private string _selectedOwner = "Neutral";
        private int? _selectedTileX;
        private int? _selectedTileY;
        private bool _isSelecting;
        private int _selectionStartX;
        private int _selectionStartY;
        private int _selectionEndX;
        private int _selectionEndY;

        public enum EditTool { Terrain, Property, Unit, Fill, Select }

        // Commands - initialized in constructor
        public ICommand NewMapCommand { get; private set; }
        public ICommand OpenMapCommand { get; private set; }
        public ICommand SaveMapCommand { get; private set; }
        public ICommand ExportJsonCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }
        public ICommand ZoomResetCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }
        public ICommand FillCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand PasteCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        // Properties
        public Map? CurrentMap
        {
            get => _currentMap;
            set => SetProperty(ref _currentMap, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string CoordsText
        {
            get => _coordsText;
            set => SetProperty(ref _coordsText, value);
        }

        public string ZoomText
        {
            get => _zoomText;
            set => SetProperty(ref _zoomText, value);
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (SetProperty(ref _zoomLevel, value))
                {
                    ZoomText = $"{(int)(value * 100)}%";
                    UpdateViewportSize();
                }
            }
        }

        public int ViewportX
        {
            get => _viewportX;
            set => SetProperty(ref _viewportX, value);
        }

        public int ViewportY
        {
            get => _viewportY;
            set => SetProperty(ref _viewportY, value);
        }

        public int ViewportWidth
        {
            get => _viewportWidth;
            set => SetProperty(ref _viewportWidth, value);
        }

        public int ViewportHeight
        {
            get => _viewportHeight;
            set => SetProperty(ref _viewportHeight, value);
        }

        public EditTool CurrentTool
        {
            get => _currentTool;
            set
            {
                if (SetProperty(ref _currentTool, value))
                {
                    StatusText = $"Current tool: {value}";
                }
            }
        }

        public TerrainType SelectedTerrain
        {
            get => _selectedTerrain;
            set => SetProperty(ref _selectedTerrain, value);
        }

        public PropertyType SelectedPropertyType
        {
            get => _selectedPropertyType;
            set => SetProperty(ref _selectedPropertyType, value);
        }

        public UnitType SelectedUnitType
        {
            get => _selectedUnitType;
            set => SetProperty(ref _selectedUnitType, value);
        }

        public string SelectedOwner
        {
            get => _selectedOwner;
            set => SetProperty(ref _selectedOwner, value);
        }

        public ObservableCollection<Player> Players { get; } = new();
        public ObservableCollection<TerrainType> TerrainTypes { get; } = new();
        public ObservableCollection<PropertyType> PropertyTypes { get; } = new();
        public ObservableCollection<UnitType> UnitTypes { get; } = new();
        public ObservableCollection<string> OwnerTypes { get; } = new();

        public bool CanUndo => _undoRedoManager.CanUndo;
        public bool CanRedo => _undoRedoManager.CanRedo;

        public MainViewModel()
        {
            InitializeCommands();
            InitializePalettes();
            InitializeAutoSave();

            _autoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            _autoSaveTimer.Tick += AutoSaveTimer_Tick;
            _autoSaveTimer.Start();
        }

        private void InitializeCommands()
        {
            NewMapCommand = new RelayCommand(ExecuteNewMap);
            OpenMapCommand = new RelayCommand(ExecuteOpenMap);
            SaveMapCommand = new RelayCommand(ExecuteSaveMap, CanExecuteSaveMap);
            ExportJsonCommand = new RelayCommand(ExecuteExportJson, CanExecuteExportJson);
            ExitCommand = new RelayCommand(ExecuteExit);
            UndoCommand = new RelayCommand(ExecuteUndo, CanExecuteUndo);
            RedoCommand = new RelayCommand(ExecuteRedo, CanExecuteRedo);
            ZoomInCommand = new RelayCommand(ExecuteZoomIn, CanExecuteZoomIn);
            ZoomOutCommand = new RelayCommand(ExecuteZoomOut, CanExecuteZoomOut);
            ZoomResetCommand = new RelayCommand(ExecuteZoomReset);
            AboutCommand = new RelayCommand(ExecuteAbout);
            FillCommand = new RelayCommand(ExecuteFill);
            SelectCommand = new RelayCommand(ExecuteSelect);
            CopyCommand = new RelayCommand(ExecuteCopy, CanExecuteCopy);
            PasteCommand = new RelayCommand(ExecutePaste, CanExecutePaste);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteDelete);
        }

        private void InitializePalettes()
        {
            // Terrain types from core game
            TerrainTypes.Add(TerrainType.Plain);
            TerrainTypes.Add(TerrainType.Forest);
            TerrainTypes.Add(TerrainType.Mountain);
            TerrainTypes.Add(TerrainType.Road);
            TerrainTypes.Add(TerrainType.Bridge);
            TerrainTypes.Add(TerrainType.Sea);
            TerrainTypes.Add(TerrainType.Beach);
            TerrainTypes.Add(TerrainType.River);

            // Property types from core game
            PropertyTypes.Add(PropertyType.City);
            PropertyTypes.Add(PropertyType.Factory);
            PropertyTypes.Add(PropertyType.HQ);
            PropertyTypes.Add(PropertyType.Airport);
            PropertyTypes.Add(PropertyType.Port);

            // Unit types from core game
            foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
            {
                UnitTypes.Add(unitType);
            }

            OwnerTypes.Add("Player");
            OwnerTypes.Add("Neutral");
            OwnerTypes.Add("Computer");
        }

        private void InitializeAutoSave()
        {
            // Auto-save setup is done in constructor
        }

        private void AutoSaveTimer_Tick(object? sender, EventArgs e)
        {
            if (CurrentMap != null && !string.IsNullOrEmpty(_currentFilePath))
            {
                try
                {
                    var autoSavePath = _currentFilePath + ".autosave";
                    CurrentMap.FlattenTileArray();
                    MapService.SaveMap(CurrentMap, autoSavePath);
                    StatusText = "Auto-saved";
                }
                catch
                {
                    // Silent fail for auto-save
                }
            }
        }

        public void CreateNewMap(MapOptions options)
        {
            CurrentMap = new Map
            {
                Name = string.IsNullOrWhiteSpace(options.Name) ? "UntitledMap" : options.Name,
                Width = options.Width,
                Height = options.Length,
                Season = options.Season,
                Weather = Enum.Parse<WeatherType>(options.Weather),
                FogOfWarEnabled = true,
                Metadata = new MapMetadata { Author = Environment.UserName, Created = DateTime.Now.ToString("yyyy-MM-dd") }
            };
            CurrentMap.Tiles.Clear();
            CurrentMap.TileArray = new Tile[CurrentMap.Width, CurrentMap.Height];
            for (int y = 0; y < CurrentMap.Height; y++)
            {
                for (int x = 0; x < CurrentMap.Width; x++)
                {
                    var tile = new Tile
                    {
                        X = x,
                        Y = y,
                        Terrain = Enum.Parse<TerrainType>(options.Terrain),
                        Traversable = true,
                        SpriteIndex = 0
                    };
                    CurrentMap.TileArray[x, y] = tile;
                    CurrentMap.Tiles.Add(tile);
                }
            }
            _undoRedoManager.Reset(CurrentMap);
            UpdatePlayers();
            UpdateViewportSize();
            StatusText = $"New map created: {options.Width}x{options.Length}";
        }

        public void LoadMap(string filePath)
        {
            try
            {
                var loadedMap = MapService.LoadMap(filePath);
                CurrentMap = loadedMap;
                _currentFilePath = filePath;
                ValidateMap();
                _undoRedoManager.Reset(CurrentMap);
                UpdatePlayers();
                UpdateViewportSize();
                StatusText = $"Map loaded: {System.IO.Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading map: {ex.Message}", ex);
            }
        }

        private void ValidateMap()
        {
            if (CurrentMap == null) return;

            var validationService = new MapValidationService();
            var errors = validationService.ValidateMap(CurrentMap);

            if (errors.Any())
            {
                StatusText = $"Map validation: {errors.Count} issues found";
            }
        }

        private void UpdatePlayers()
        {
            Players.Clear();
            if (CurrentMap?.Players != null)
            {
                foreach (var player in CurrentMap.Players)
                {
                    Players.Add(player);
                }
            }
        }

        private void UpdateViewportSize()
        {
            // Adjust viewport based on zoom level
            ViewportWidth = Math.Max(10, (int)(30 / ZoomLevel));
            ViewportHeight = Math.Max(10, (int)(20 / ZoomLevel));
        }

        public void PaintTile(int x, int y)
        {
            if (CurrentMap == null || x < 0 || x >= CurrentMap.Width || y < 0 || y >= CurrentMap.Height) return;

            switch (CurrentTool)
            {
                case EditTool.Terrain:
                    if (CurrentMap.TileArray[x, y] != null)
                    {
                        CurrentMap.TileArray[x, y].Terrain = SelectedTerrain;
                        // Preserve existing traversable value unless it's a terrain that should affect it
                        if (SelectedTerrain == TerrainType.Mountain || SelectedTerrain == TerrainType.Sea)
                        {
                            // Let the user decide, don't auto-change
                        }
                    }
                    break;
                case EditTool.Property:
                    PlaceProperty(x, y, SelectedPropertyType, SelectedOwner);
                    break;
                case EditTool.Unit:
                    PlaceUnit(x, y, SelectedUnitType, SelectedOwner);
                    break;
                case EditTool.Fill:
                    FillArea(x, y, SelectedTerrain);
                    break;
            }

            _undoRedoManager.Push(CurrentMap);
        }

        private void PlaceProperty(int x, int y, PropertyType type, string owner)
        {
            if (CurrentMap == null) return;
            var prop = CurrentMap.Properties.Find(p => p.X == x && p.Y == y);
            if (prop == null)
            {
                prop = new Property { X = x, Y = y, Type = type, Owner = owner };
                CurrentMap.Properties.Add(prop);
            }
            else
            {
                prop.Type = type;
                prop.Owner = owner;
            }
        }

        private void PlaceUnit(int x, int y, UnitType type, string owner)
        {
            if (CurrentMap == null) return;
            var unit = CurrentMap.Units.Find(u => u.X == x && u.Y == y);
            if (unit == null)
            {
                unit = new Unit { X = x, Y = y, Type = type, Owner = owner };
                CurrentMap.Units.Add(unit);
            }
            else
            {
                unit.Type = type;
                unit.Owner = owner;
            }
        }

        private void FillArea(int x, int y, TerrainType terrain)
        {
            if (CurrentMap == null || x < 0 || x >= CurrentMap.Width || y < 0 || y >= CurrentMap.Height) return;

            var originalTerrain = CurrentMap.TileArray[x, y]?.Terrain;
            if (originalTerrain == terrain) return;

            var visited = new bool[CurrentMap.Width, CurrentMap.Height];
            var stack = new System.Collections.Generic.Stack<(int, int)>();
            stack.Push((x, y));

            while (stack.Count > 0)
            {
                var (cx, cy) = stack.Pop();
                if (cx < 0 || cx >= CurrentMap.Width || cy < 0 || cy >= CurrentMap.Height || visited[cx, cy])
                    continue;

                if (CurrentMap.TileArray[cx, cy]?.Terrain != originalTerrain)
                    continue;

                visited[cx, cy] = true;
                CurrentMap.TileArray[cx, cy].Terrain = terrain;
                // Preserve traversable value during fill

                stack.Push((cx + 1, cy));
                stack.Push((cx - 1, cy));
                stack.Push((cx, cy + 1));
                stack.Push((cx, cy - 1));
            }
        }

        // Command implementations
        private void ExecuteNewMap(object? parameter) => OnNewMapRequested?.Invoke();
        private void ExecuteOpenMap(object? parameter) => OnOpenMapRequested?.Invoke();
        private void ExecuteSaveMap(object? parameter) => OnSaveMapRequested?.Invoke();
        private bool CanExecuteSaveMap(object? parameter) => CurrentMap != null;
        private void ExecuteExportJson(object? parameter) => OnExportJsonRequested?.Invoke();
        private bool CanExecuteExportJson(object? parameter) => CurrentMap != null;
        private void ExecuteExit(object? parameter) => OnExitRequested?.Invoke();
        private void ExecuteUndo(object? parameter)
        {
            if (_undoRedoManager.CanUndo)
            {
                CurrentMap = _undoRedoManager.Undo();
                StatusText = "Undo performed";
                OnMapChanged?.Invoke();
            }
        }
        private bool CanExecuteUndo(object? parameter) => _undoRedoManager.CanUndo;
        private void ExecuteRedo(object? parameter)
        {
            if (_undoRedoManager.CanRedo)
            {
                CurrentMap = _undoRedoManager.Redo();
                StatusText = "Redo performed";
                OnMapChanged?.Invoke();
            }
        }
        private bool CanExecuteRedo(object? parameter) => _undoRedoManager.CanRedo;
        private void ExecuteZoomIn(object? parameter)
        {
            if (ZoomLevel < 4.0)
            {
                ZoomLevel += 0.2;
                OnZoomChanged?.Invoke();
            }
        }
        private bool CanExecuteZoomIn(object? parameter) => ZoomLevel < 4.0;
        private void ExecuteZoomOut(object? parameter)
        {
            if (ZoomLevel > 0.2)
            {
                ZoomLevel -= 0.2;
                OnZoomChanged?.Invoke();
            }
        }
        private bool CanExecuteZoomOut(object? parameter) => ZoomLevel > 0.2;
        private void ExecuteZoomReset(object? parameter)
        {
            ZoomLevel = 1.0;
            OnZoomChanged?.Invoke();
        }
        private void ExecuteAbout(object? parameter) => OnAboutRequested?.Invoke();
        private void ExecuteFill(object? parameter) => CurrentTool = EditTool.Fill;
        private void ExecuteSelect(object? parameter) => CurrentTool = EditTool.Select;
        private void ExecuteCopy(object? parameter) => OnCopyRequested?.Invoke();
        private bool CanExecuteCopy(object? parameter) => _selectedTileX.HasValue && _selectedTileY.HasValue;
        private void ExecutePaste(object? parameter) => OnPasteRequested?.Invoke();
        private bool CanExecutePaste(object? parameter) => false; // Implement clipboard check
        private void ExecuteDelete(object? parameter) => OnDeleteRequested?.Invoke();
        private bool CanExecuteDelete(object? parameter) => _selectedTileX.HasValue && _selectedTileY.HasValue;

        // Events
        public event Action? OnNewMapRequested;
        public event Action? OnOpenMapRequested;
        public event Action? OnSaveMapRequested;
        public event Action? OnExportJsonRequested;
        public event Action? OnExitRequested;
        public event Action? OnAboutRequested;
        public event Action? OnMapChanged;
        public event Action? OnZoomChanged;
        public event Action? OnCopyRequested;
        public event Action? OnPasteRequested;
        public event Action? OnDeleteRequested;

        public void UpdateCoordinates(int x, int y)
        {
            CoordsText = $"({x},{y})";
        }

        public void SetSelectedTile(int? x, int? y)
        {
            _selectedTileX = x;
            _selectedTileY = y;
        }

        public void SaveCurrentMap(string filePath, bool useCompression = false)
        {
            if (CurrentMap == null) return;

            // Ensure all tiles are properly saved
            CurrentMap.FlattenTileArray();

            MapService.SaveMap(CurrentMap, filePath, useCompression);
            _currentFilePath = filePath;
            StatusText = $"Map saved: {System.IO.Path.GetFileName(filePath)}";
        }

        // Public method to trigger property change
        public void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}