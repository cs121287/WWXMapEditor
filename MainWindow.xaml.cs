using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WwXMapEditor.Models;
using WwXMapEditor.Services;

namespace WwXMapEditor
{
    public partial class MainWindow : Window
    {
        private Map? _currentMap;
        private string? _currentFilePath;
        private WriteableBitmap? _tilesBitmap;
        private DispatcherTimer? _debounceTimer;

        private bool _isPainting = false;
        private Point _lastPaintPoint = new Point(0, 0);

        private readonly UndoRedoManager _undoRedoManager = new UndoRedoManager();

        // Editing state
        private enum EditTool { Terrain, Property, Unit }
        private EditTool _currentTool = EditTool.Terrain;
        private string _selectedTerrain = "Plain";
        private string _selectedPropertyType = "City";
        private string _selectedUnitType = "Infantry";
        private string _selectedOwner = "Neutral"; // Owner option for property; default to Neutral

        // Canvas rendering and zoom
        private double _zoomLevel = 1.0; // 1.0 = 100%
        private const double ZoomStep = 0.2;
        private const double MinZoom = 0.2;
        private const double MaxZoom = 4.0;
        private int _tileSizeBase = 32;
        private int TileSize => Math.Max(1, (int)(_tileSizeBase * _zoomLevel));

        // Currently selected tile for details and highlight
        private int? _selectedTileX = null;
        private int? _selectedTileY = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMap();
            InitializePalettes();
            ToolSelector.SelectionChanged += ToolSelector_SelectionChanged;
            Loaded += MainWindow_Loaded;
        }

        public MainWindow(MapOptions options)
        {
            InitializeComponent();
            CreateNewMap(options);
            _undoRedoManager.Reset(_currentMap!);
            InitializePalettes();
            ToolSelector.SelectionChanged += ToolSelector_SelectionChanged;
            Loaded += MainWindow_Loaded;
        }

        public MainWindow(string mapFilePath)
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            LoadMapAsync(mapFilePath);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RenderMapCanvas();
        }

        private void InitializeMap()
        {
            _currentMap = new Map
            {
                Name = "UntitledMap",
                Width = 100,
                Height = 100,
                Season = "Summer",
                Weather = "Clear",
                Metadata = new MapMetadata { Author = Environment.UserName, Created = DateTime.Now.ToString("yyyy-MM-dd") }
            };
            _currentMap.Tiles.Clear();
            _currentMap.TileArray = new Tile[_currentMap.Width, _currentMap.Height];
            for (int y = 0; y < _currentMap.Height; y++)
            {
                for (int x = 0; x < _currentMap.Width; x++)
                {
                    var tile = new Tile
                    {
                        X = x,
                        Y = y,
                        Terrain = "Plain",
                        Traversable = true // All tiles start traversable
                    };
                    _currentMap.TileArray[x, y] = tile;
                    _currentMap.Tiles.Add(tile);
                }
            }
            _undoRedoManager.Reset(_currentMap);
            RefreshUI();
        }

        private void CreateNewMap(MapOptions options)
        {
            _currentMap = new Map
            {
                Name = string.IsNullOrWhiteSpace(options.Name) ? "UntitledMap" : options.Name,
                Width = options.Width,
                Height = options.Length,
                Season = options.Season,
                Weather = "Clear",
                Metadata = new MapMetadata { Author = Environment.UserName, Created = DateTime.Now.ToString("yyyy-MM-dd") }
            };
            _currentMap.Tiles.Clear();
            _currentMap.TileArray = new Tile[_currentMap.Width, _currentMap.Height];
            for (int y = 0; y < _currentMap.Height; y++)
            {
                for (int x = 0; x < _currentMap.Width; x++)
                {
                    var tile = new Tile
                    {
                        X = x,
                        Y = y,
                        Terrain = options.Terrain,
                        Traversable = true // All tiles start traversable
                    };
                    _currentMap.TileArray[x, y] = tile;
                    _currentMap.Tiles.Add(tile);
                }
            }
            UpdateStatus($"New map created: {options.Width}x{options.Length}, Terrain: {options.Terrain}, Season: {options.Season}");
            RefreshUI();
        }

        private async void LoadMapAsync(string filePath)
        {
            try
            {
                var loadedMap = await Task.Run(() => MapService.LoadMap(filePath));
                _currentMap = loadedMap;
                _currentFilePath = filePath;
                _currentMap!.BuildTileArray();
                // Ensure traversable property exists for all tiles
                for (int y = 0; y < _currentMap.Height; y++)
                    for (int x = 0; x < _currentMap.Width; x++)
                        if (_currentMap.TileArray[x, y] != null && _currentMap.TileArray[x, y].Traversable == false)
                            continue;
                        else if (_currentMap.TileArray[x, y] != null)
                            _currentMap.TileArray[x, y].Traversable = true;
                _undoRedoManager.Reset(_currentMap);
                InitializePalettes();
                RefreshUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading map: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        #region Event Handlers

        private void NewMap_Click(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new NewMapOptionsWindow();
            optionsWindow.Owner = this;
            if (optionsWindow.ShowDialog() == true)
            {
                CreateNewMap(optionsWindow.MapOptions!);
                _undoRedoManager.Reset(_currentMap!);
                RefreshUI();
            }
        }

        private async void OpenMap_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JSON Map Files (*.json)|*.json|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() => LoadMapAsync(dlg.FileName));
                });
            }
        }

        private async void SaveMap_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                var dlg = new SaveFileDialog { Filter = "JSON Map Files (*.json)|*.json|All Files (*.*)|*.*" };
                if (dlg.ShowDialog() == true)
                {
                    _currentFilePath = dlg.FileName;
                }
                else
                {
                    return;
                }
            }
            try
            {
                _currentMap!.FlattenTileArray();
                await Task.Run(() => MapService.SaveMap(_currentMap, _currentFilePath!));
                UpdateStatus($"Map saved: {Path.GetFileName(_currentFilePath)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving map: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "JSON Map Files (*.json)|*.json|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _currentMap!.FlattenTileArray();
                    await Task.Run(() => MapService.SaveMap(_currentMap, dlg.FileName));
                    UpdateStatus($"Map exported as JSON: {Path.GetFileName(dlg.FileName)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting map: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmDiscardChanges())
                Application.Current.Shutdown();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (_undoRedoManager.CanUndo)
            {
                _currentMap = _undoRedoManager.Undo();
                RenderMapCanvas();
                UpdateStatus("Undo performed.");
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (_undoRedoManager.CanRedo)
            {
                _currentMap = _undoRedoManager.Redo();
                RenderMapCanvas();
                UpdateStatus("Redo performed.");
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("WorldWarX Map Editor\nVersion 1.0\nCreated by cs121287", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ToolSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((ToolSelector.SelectedItem as ComboBoxItem)?.Content as string)
            {
                case "Terrain":
                    _currentTool = EditTool.Terrain;
                    break;
                case "Property":
                    _currentTool = EditTool.Property;
                    break;
                case "Unit":
                    _currentTool = EditTool.Unit;
                    break;
            }
            UpdateStatus($"Current tool: {_currentTool}");
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (_zoomLevel < MaxZoom)
            {
                _zoomLevel += ZoomStep;
                RefreshUI();
            }
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (_zoomLevel > MinZoom && TileSize > 1)
            {
                double nextZoom = _zoomLevel - ZoomStep;
                int nextTileSize = (int)(_tileSizeBase * nextZoom);
                if (nextZoom >= MinZoom && nextTileSize > 0)
                {
                    _zoomLevel = nextZoom;
                    RefreshUI();
                }
            }
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            _zoomLevel = 1.0;
            RefreshUI();
        }

        #endregion

        #region Editing Tools, Palette Selection

        private void InitializePalettes()
        {
            TerrainPalette.Items.Clear();
            foreach (var terrain in new[] { "Plain", "Forest", "Mountain", "Sea", "Beach", "Water", "Road", "River" })
            {
                var item = new ListBoxItem { Content = terrain };
                item.Selected += (s, e) => { _selectedTerrain = terrain; UpdateStatus($"Selected terrain: {terrain}"); };
                TerrainPalette.Items.Add(item);
            }
            TerrainPalette.SelectedIndex = 0;

            PropertyPalette.Items.Clear();
            foreach (var prop in new[] { "City", "Factory", "HQ", "Airport", "Port" })
            {
                var item = new ListBoxItem { Content = prop };
                item.Selected += (s, e) => { _selectedPropertyType = prop; UpdateStatus($"Selected property: {prop}"); };
                PropertyPalette.Items.Add(item);
            }
            PropertyPalette.SelectedIndex = 0;

            // Owner selector for properties
            PropertyOwnerCombo.Items.Clear();
            foreach (var owner in new[] { "Player", "Neutral", "Computer" })
            {
                var item = new ComboBoxItem { Content = owner };
                PropertyOwnerCombo.Items.Add(item);
            }
            PropertyOwnerCombo.SelectedIndex = 1; // Default to Neutral
            PropertyOwnerCombo.SelectionChanged += (s, e) =>
            {
                var selected = (PropertyOwnerCombo.SelectedItem as ComboBoxItem)?.Content as string;
                if (!string.IsNullOrEmpty(selected))
                {
                    _selectedOwner = selected;
                    UpdateStatus($"Property owner: {_selectedOwner}");
                }
            };

            UnitPalette.Items.Clear();
            foreach (var unit in new[] { "Infantry", "Tank", "Artillery", "Recon", "Fighter", "Bomber", "Ship", "Transport" })
            {
                var item = new ListBoxItem { Content = unit };
                item.Selected += (s, e) => { _selectedUnitType = unit; UpdateStatus($"Selected unit: {unit}"); };
                UnitPalette.Items.Add(item);
            }
            UnitPalette.SelectedIndex = 0;
        }

        #endregion

        #region Map Rendering and Editing (Optimized Canvas, Zoomable, Selection, Layered Sprites)

        private void RefreshUI()
        {
            RenderMapCanvas();
            StatusText.Text = $"Map: {_currentMap?.Name ?? "UntitledMap"}";
            CoordsText.Text = "(0,0)";
            ZoomText.Text = $"{(int)(_zoomLevel * 100)}%";
            if (_selectedTileX.HasValue && _selectedTileY.HasValue)
                ShowTileDetails(_selectedTileX.Value, _selectedTileY.Value);
        }

        private void RenderMapCanvas()
        {
            if (_currentMap == null)
            {
                MapCanvas.Children.Clear();
                MapCanvas.Width = 100;
                MapCanvas.Height = 100;
                var blackRect = new System.Windows.Shapes.Rectangle
                {
                    Width = 100,
                    Height = 100,
                    Fill = Brushes.Black
                };
                MapCanvas.Children.Add(blackRect);
                return;
            }

            int visibleTileWidth = _currentMap.Width;
            int visibleTileHeight = _currentMap.Height;

            int canvasWidth = visibleTileWidth * TileSize;
            int canvasHeight = visibleTileHeight * TileSize;

            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                MapCanvas.Children.Clear();
                MapCanvas.Width = 100;
                MapCanvas.Height = 100;
                var blackRect = new System.Windows.Shapes.Rectangle
                {
                    Width = 100,
                    Height = 100,
                    Fill = Brushes.Black
                };
                MapCanvas.Children.Add(blackRect);
                return;
            }

            MapCanvas.Width = canvasWidth;
            MapCanvas.Height = canvasHeight;

            _tilesBitmap = new WriteableBitmap(canvasWidth, canvasHeight, 96, 96, PixelFormats.Pbgra32, null);

            _tilesBitmap.Lock();
            DrawAllTilesWithLayers();
            // Highlight selected tile if present
            if (_selectedTileX.HasValue && _selectedTileY.HasValue)
                HighlightSelectedTile(_selectedTileX.Value, _selectedTileY.Value);
            _tilesBitmap.Unlock();

            MapCanvas.Children.Clear();
            var img = new Image
            {
                Source = _tilesBitmap,
                Width = canvasWidth,
                Height = canvasHeight
            };
            MapCanvas.Children.Add(img);
        }

        // Draw tiles with layers: terrain (bottom), property (middle), unit (top)
        private void DrawAllTilesWithLayers()
        {
            if (_currentMap == null || _tilesBitmap == null) return;
            int width = _currentMap.Width;
            int height = _currentMap.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    DrawTileTerrainLayer(x, y);
                    DrawTilePropertyLayer(x, y);
                    DrawTileUnitLayer(x, y);
                    DrawGridLine(x, y);
                }
            }

            // Fill space outside the grid with black if zoomed out and canvas is larger than grid
            int canvasWidth = width * TileSize;
            int canvasHeight = height * TileSize;
            if (_tilesBitmap.PixelWidth > canvasWidth || _tilesBitmap.PixelHeight > canvasHeight)
            {
                var black = new byte[] { 0, 0, 0, 255 };
                // Right side
                for (int y = 0; y < _tilesBitmap.PixelHeight; y++)
                {
                    for (int x = canvasWidth; x < _tilesBitmap.PixelWidth; x++)
                    {
                        _tilesBitmap.WritePixels(new Int32Rect(x, y, 1, 1), black, 4, 0);
                    }
                }
                // Bottom side
                for (int y = canvasHeight; y < _tilesBitmap.PixelHeight; y++)
                {
                    for (int x = 0; x < _tilesBitmap.PixelWidth; x++)
                    {
                        _tilesBitmap.WritePixels(new Int32Rect(x, y, 1, 1), black, 4, 0);
                    }
                }
            }
        }

        private void DrawTileTerrainLayer(int x, int y)
        {
            if (_currentMap?.TileArray == null || _tilesBitmap == null) return;
            var terrain = _currentMap.TileArray[x, y]?.Terrain ?? "Plain";
            var season = _currentMap.Season ?? "Summer";
            var sprite = GetTileSprite(terrain, season, "terrain");

            var destRect = new Int32Rect(x * TileSize, y * TileSize, TileSize, TileSize);

            if (sprite is BitmapSource bmp)
            {
                var rect = new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
                var stride = bmp.PixelWidth * (bmp.Format.BitsPerPixel / 8);
                var buffer = new byte[stride * bmp.PixelHeight];
                bmp.CopyPixels(buffer, stride, 0);
                _tilesBitmap.WritePixels(destRect, buffer, stride, 0);
            }
            else
            {
                var color = Colors.LightGray;
                var pixels = new byte[TileSize * TileSize * 4];
                for (int py = 0; py < TileSize; py++)
                    for (int px = 0; px < TileSize; px++)
                    {
                        int idx = (py * TileSize + px) * 4;
                        pixels[idx + 0] = color.B;
                        pixels[idx + 1] = color.G;
                        pixels[idx + 2] = color.R;
                        pixels[idx + 3] = color.A;
                    }
                _tilesBitmap.WritePixels(destRect, pixels, TileSize * 4, 0);
            }
        }

        private void DrawTilePropertyLayer(int x, int y)
        {
            if (_currentMap == null || _tilesBitmap == null) return;
            var prop = _currentMap.Properties.Find(p => p.X == x && p.Y == y);
            if (prop == null) return;
            var sprite = GetTileSprite(prop.Type, prop.Owner, "property");
            var destRect = new Int32Rect(x * TileSize, y * TileSize, TileSize, TileSize);

            if (sprite is BitmapSource bmp)
            {
                var rect = new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
                var stride = bmp.PixelWidth * (bmp.Format.BitsPerPixel / 8);
                var buffer = new byte[stride * bmp.PixelHeight];
                bmp.CopyPixels(buffer, stride, 0);
                _tilesBitmap.WritePixels(destRect, buffer, stride, 0);
            }
        }

        private void DrawTileUnitLayer(int x, int y)
        {
            if (_currentMap == null || _tilesBitmap == null) return;
            var unit = _currentMap.Units.Find(u => u.X == x && u.Y == y);
            if (unit == null) return;
            var sprite = GetTileSprite(unit.Type, unit.Owner, "unit");
            var destRect = new Int32Rect(x * TileSize, y * TileSize, TileSize, TileSize);

            if (sprite is BitmapSource bmp)
            {
                var rect = new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
                var stride = bmp.PixelWidth * (bmp.Format.BitsPerPixel / 8);
                var buffer = new byte[stride * bmp.PixelHeight];
                bmp.CopyPixels(buffer, stride, 0);
                _tilesBitmap.WritePixels(destRect, buffer, stride, 0);
            }
        }

        private void DrawGridLine(int x, int y)
        {
            if (_currentMap == null || _tilesBitmap == null) return;
            var gridColor = Colors.Black;
            var gridBytes = new byte[] { gridColor.B, gridColor.G, gridColor.R, gridColor.A };

            // Right border
            if (x < _currentMap.Width - 1)
            {
                for (int py = 0; py < TileSize; py++)
                {
                    int bx = x * TileSize + (TileSize - 1);
                    int by = y * TileSize + py;
                    _tilesBitmap.WritePixels(
                        new Int32Rect(bx, by, 1, 1),
                        gridBytes, 4, 0);
                }
            }
            // Bottom border
            if (y < _currentMap.Height - 1)
            {
                for (int px = 0; px < TileSize; px++)
                {
                    int bx = x * TileSize + px;
                    int by = y * TileSize + (TileSize - 1);
                    _tilesBitmap.WritePixels(
                        new Int32Rect(bx, by, 1, 1),
                        gridBytes, 4, 0);
                }
            }
        }

        // Highlight selected tile visually with a red border
        private void HighlightSelectedTile(int x, int y)
        {
            if (_tilesBitmap == null) return;
            var highlightColor = Colors.Red;
            var highlightBytes = new byte[] { highlightColor.B, highlightColor.G, highlightColor.R, highlightColor.A };

            // Top border
            for (int px = 0; px < TileSize; px++)
            {
                int bx = x * TileSize + px;
                int by = y * TileSize;
                _tilesBitmap.WritePixels(new Int32Rect(bx, by, 1, 1), highlightBytes, 4, 0);
            }
            // Bottom border
            for (int px = 0; px < TileSize; px++)
            {
                int bx = x * TileSize + px;
                int by = y * TileSize + TileSize - 1;
                _tilesBitmap.WritePixels(new Int32Rect(bx, by, 1, 1), highlightBytes, 4, 0);
            }
            // Left border
            for (int py = 0; py < TileSize; py++)
            {
                int bx = x * TileSize;
                int by = y * TileSize + py;
                _tilesBitmap.WritePixels(new Int32Rect(bx, by, 1, 1), highlightBytes, 4, 0);
            }
            // Right border
            for (int py = 0; py < TileSize; py++)
            {
                int bx = x * TileSize + TileSize - 1;
                int by = y * TileSize + py;
                _tilesBitmap.WritePixels(new Int32Rect(bx, by, 1, 1), highlightBytes, 4, 0);
            }

            _tilesBitmap.AddDirtyRect(new Int32Rect(x * TileSize, y * TileSize, TileSize, TileSize));
        }

        // Sprite selection: terrain layer, property layer, unit layer
        private ImageSource? GetTileSprite(string type, string variant, string layer)
        {
            string fileName = "";
            if (layer == "terrain")
                fileName = $"{type}_{variant}.png";
            else if (layer == "property")
                fileName = $"{type}_{variant}_property.png"; // e.g. City_Neutral_property.png
            else if (layer == "unit")
                fileName = $"{type}_{variant}_unit.png"; // e.g. Infantry_Player_unit.png
            string resourcePath = $"pack://application:,,,/Resources/Images/Tiles/{fileName}";
            try
            {
                Uri uri = new Uri(resourcePath, UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch
            {
                return null;
            }
        }

        // Canvas Mouse Edit (Debounced) + Selection
        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentMap == null) return;
            var pos = e.GetPosition(MapCanvas);
            int x = (int)(pos.X / TileSize);
            int y = (int)(pos.Y / TileSize);

            if (x >= 0 && x < _currentMap.Width && y >= 0 && y < _currentMap.Height)
            {
                _isPainting = true;
                PaintTile(x, y);
                _lastPaintPoint = pos;

                // Set selected tile for details and highlight
                _selectedTileX = x;
                _selectedTileY = y;
                ShowTileDetails(x, y);
                RenderMapCanvas(); // Redraw with highlight
            }
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPainting && e.LeftButton == MouseButtonState.Pressed && _currentMap != null)
            {
                var pos = e.GetPosition(MapCanvas);
                int x = (int)(pos.X / TileSize);
                int y = (int)(pos.Y / TileSize);

                if (x >= 0 && x < _currentMap.Width && y >= 0 && y < _currentMap.Height)
                {
                    if (_debounceTimer == null)
                    {
                        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
                        _debounceTimer.Tick += (s, ev) =>
                        {
                            PaintTile(x, y);
                            _debounceTimer.Stop();
                        };
                    }
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                }
            }
        }

        private void MapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPainting = false;
        }

        private void PaintTile(int x, int y)
        {
            if (_currentMap == null || _tilesBitmap == null) return;
            _tilesBitmap.Lock();
            switch (_currentTool)
            {
                case EditTool.Terrain:
                    _currentMap.TileArray[x, y].Terrain = _selectedTerrain;
                    break;
                case EditTool.Property:
                    SetProperty(x, y, _selectedPropertyType, _selectedOwner);
                    break;
                case EditTool.Unit:
                    SetUnit(x, y, _selectedUnitType, _selectedOwner);
                    break;
            }
            DrawTileTerrainLayer(x, y);
            DrawTilePropertyLayer(x, y);
            DrawTileUnitLayer(x, y);
            DrawGridLine(x, y);
            _tilesBitmap.AddDirtyRect(new Int32Rect(x * TileSize, y * TileSize, TileSize, TileSize));
            _tilesBitmap.Unlock();

            _undoRedoManager.Push(_currentMap);
        }

        private void SetProperty(int x, int y, string type, string owner)
        {
            if (_currentMap == null) return;
            var prop = _currentMap.Properties.Find(p => p.X == x && p.Y == y);
            if (prop == null)
                _currentMap.Properties.Add(new Property { X = x, Y = y, Type = type, Owner = owner });
            else
            {
                prop.Type = type;
                prop.Owner = owner;
            }
        }

        private void SetUnit(int x, int y, string type, string owner)
        {
            if (_currentMap == null) return;
            var unit = _currentMap.Units.Find(u => u.X == x && u.Y == y);
            if (unit == null)
                _currentMap.Units.Add(new Unit { X = x, Y = y, Type = type, Owner = owner, HP = 10 });
            else
            {
                unit.Type = type;
                unit.Owner = owner;
                unit.HP = 10;
            }
        }

        private void ClearProperty(int x, int y)
        {
            if (_currentMap == null) return;
            var prop = _currentMap.Properties.Find(p => p.X == x && p.Y == y);
            if (prop != null)
            {
                _currentMap.Properties.Remove(prop);
                RefreshUI();
            }
        }

        private void ClearUnit(int x, int y)
        {
            if (_currentMap == null) return;
            var unit = _currentMap.Units.Find(u => u.X == x && u.Y == y);
            if (unit != null)
            {
                _currentMap.Units.Remove(unit);
                RefreshUI();
            }
        }

        private void SetTraversable(int x, int y, bool traversable)
        {
            if (_currentMap == null) return;
            if (_currentMap.TileArray[x, y] != null)
            {
                _currentMap.TileArray[x, y].Traversable = traversable;
            }
        }

        #endregion

        #region Details Panel

        private void ShowTileDetails(int x, int y)
        {
            if (_currentMap?.TileArray == null) return;
            DetailsPanel.Children.Clear();
            var tile = _currentMap.TileArray[x, y];
            var prop = _currentMap.Properties.Find(p => p.X == x && p.Y == y);
            var unit = _currentMap.Units.Find(u => u.X == x && u.Y == y);

            DetailsPanel.Children.Add(new TextBlock { Text = $"Coordinates: ({x}, {y})", FontWeight = FontWeights.Bold });
            DetailsPanel.Children.Add(new TextBlock { Text = $"Terrain: {tile?.Terrain ?? "Plain"}" });

            // Traversable option
            var traversablePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
            traversablePanel.Children.Add(new TextBlock { Text = "Traversable: ", VerticalAlignment = VerticalAlignment.Center });
            var traversableCheckBox = new CheckBox { IsChecked = tile?.Traversable ?? true, Content = "Allow unit movement" };
            traversableCheckBox.Checked += (s, e) => { SetTraversable(x, y, true); };
            traversableCheckBox.Unchecked += (s, e) => { SetTraversable(x, y, false); };
            traversablePanel.Children.Add(traversableCheckBox);
            DetailsPanel.Children.Add(traversablePanel);

            if (prop != null)
            {
                DetailsPanel.Children.Add(new TextBlock { Text = $"Property: {prop.Type}" });
                // Owner dropdown
                var ownerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
                ownerPanel.Children.Add(new TextBlock { Text = "Owner: ", VerticalAlignment = VerticalAlignment.Center });
                var ownerCombo = new ComboBox { Width = 100 };
                foreach (var owner in new[] { "Player", "Neutral", "Computer" })
                {
                    var item = new ComboBoxItem { Content = owner };
                    ownerCombo.Items.Add(item);
                    if (prop.Owner == owner)
                        ownerCombo.SelectedItem = item;
                }
                ownerCombo.SelectionChanged += (s, e) =>
                {
                    var selected = (ownerCombo.SelectedItem as ComboBoxItem)?.Content as string;
                    if (!string.IsNullOrEmpty(selected))
                    {
                        prop.Owner = selected;
                        RefreshUI();
                    }
                };
                ownerPanel.Children.Add(ownerCombo);
                DetailsPanel.Children.Add(ownerPanel);

                var removePropBtn = new Button { Content = "Remove Property" };
                removePropBtn.Margin = new Thickness(0, 5, 0, 0);
                removePropBtn.Click += (s, e) =>
                {
                    ClearProperty(x, y);
                };
                DetailsPanel.Children.Add(removePropBtn);
            }
            else
            {
                var disabledPropBtn = new Button { Content = "No Property To Remove", IsEnabled = false, Margin = new Thickness(0, 5, 0, 0) };
                DetailsPanel.Children.Add(disabledPropBtn);
            }

            if (unit != null)
            {
                DetailsPanel.Children.Add(new TextBlock { Text = $"Unit: {unit.Type} (Owner: {unit.Owner}, HP: {unit.HP})" });
                var removeUnitBtn = new Button { Content = "Remove Unit" };
                removeUnitBtn.Margin = new Thickness(0, 5, 0, 0);
                removeUnitBtn.Click += (s, e) =>
                {
                    ClearUnit(x, y);
                };
                DetailsPanel.Children.Add(removeUnitBtn);
            }
            else
            {
                var disabledUnitBtn = new Button { Content = "No Unit To Remove", IsEnabled = false, Margin = new Thickness(0, 5, 0, 0) };
                DetailsPanel.Children.Add(disabledUnitBtn);
            }
        }

        #endregion

        #region UI Helpers

        private bool ConfirmDiscardChanges()
        {
            var result = MessageBox.Show("Unsaved changes will be lost. Continue?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }

        #endregion
    }
}