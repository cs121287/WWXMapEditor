using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using WwXMapEditor.ViewModels;

namespace WwXMapEditor
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private WriteableBitmap? _tilesBitmap;
        private DispatcherTimer? _debounceTimer;
        private bool _isPainting = false;
        private Point _lastPaintPoint = new Point(0, 0);
        private int _tileSizeBase = 32;
        private int TileSize => Math.Max(1, (int)(_tileSizeBase * _viewModel.ZoomLevel));

        // Selection state
        private bool _isSelecting = false;
        private int _selectionStartX, _selectionStartY;
        private int _selectionEndX, _selectionEndY;
        private System.Windows.Shapes.Rectangle? _selectionRectangle;

        // Clipboard
        private List<Tile>? _clipboardTiles;
        private List<Property>? _clipboardProperties;
        private List<Unit>? _clipboardUnits;
        private int _clipboardWidth, _clipboardHeight;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            SetupEventHandlers();
            _viewModel.CreateNewMap(new MapOptions { Name = "UntitledMap", Width = 100, Length = 100, Terrain = "Plain", Season = "Summer" });
            Loaded += MainWindow_Loaded;
        }

        public MainWindow(MapOptions options)
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            SetupEventHandlers();
            _viewModel.CreateNewMap(options);
            Loaded += MainWindow_Loaded;
        }

        public MainWindow(string mapFilePath)
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            SetupEventHandlers();
            Loaded += (s, e) => LoadMapAsync(mapFilePath);
        }

        private void SetupEventHandlers()
        {
            _viewModel.OnNewMapRequested += ShowNewMapDialog;
            _viewModel.OnOpenMapRequested += ShowOpenMapDialog;
            _viewModel.OnSaveMapRequested += SaveMap;
            _viewModel.OnExportJsonRequested += ExportJson;
            _viewModel.OnExitRequested += Exit;
            _viewModel.OnAboutRequested += ShowAbout;
            _viewModel.OnMapChanged += RenderMapCanvas;
            _viewModel.OnZoomChanged += () =>
            {
                SpriteManager.Instance.ClearCache();
                RenderMapCanvas();
            };
            _viewModel.OnCopyRequested += CopySelection;
            _viewModel.OnPasteRequested += PasteSelection;
            _viewModel.OnDeleteRequested += DeleteSelection;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RenderMapCanvas();
        }

        private async void LoadMapAsync(string filePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        _viewModel.LoadMap(filePath);
                        RenderMapCanvas();
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading map: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        #region Event Handlers

        private void ShowNewMapDialog()
        {
            var optionsWindow = new NewMapOptionsWindow { Owner = this };
            if (optionsWindow.ShowDialog() == true)
            {
                _viewModel.CreateNewMap(optionsWindow.MapOptions!);
                RenderMapCanvas();
            }
        }

        private void ShowOpenMapDialog()
        {
            var dlg = new OpenFileDialog { Filter = "JSON Map Files (*.json)|*.json|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                LoadMapAsync(dlg.FileName);
            }
        }

        private void SaveMap()
        {
            var dlg = new SaveFileDialog { Filter = "JSON Map Files (*.json)|*.json|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _viewModel.SaveCurrentMap(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving map: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportJson()
        {
            var dlg = new SaveFileDialog { Filter = "JSON Map Files (*.json)|*.json|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _viewModel.SaveCurrentMap(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting map: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit()
        {
            if (MessageBox.Show("Unsaved changes will be lost. Continue?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void ShowAbout()
        {
            MessageBox.Show("WorldWarX Map Editor\nVersion 1.0\nCreated by cs121287", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ManagePlayers_Click(object sender, RoutedEventArgs e)
        {
            var playerWindow = new PlayerManagementWindow(_viewModel.Players) { Owner = this };
            if (playerWindow.ShowDialog() == true)
            {
                _viewModel.Players.Clear();
                foreach (var player in playerWindow.Players)
                {
                    _viewModel.Players.Add(player);
                }
                if (_viewModel.CurrentMap != null)
                {
                    _viewModel.CurrentMap.Players = _viewModel.Players.ToList();
                }
            }
        }

        private void ValidateMap_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CurrentMap == null) return;

            var validationService = new MapValidationService();
            var errors = validationService.ValidateMap(_viewModel.CurrentMap);

            if (errors.Any())
            {
                var errorMessage = string.Join("\n", errors.Take(10).Select(er => $"• {er.Message}"));
                if (errors.Count > 10)
                    errorMessage += $"\n... and {errors.Count - 10} more issues";

                MessageBox.Show($"Map validation found {errors.Count} issues:\n\n{errorMessage}", "Map Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Map validation successful! No issues found.", "Map Validation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FillTool_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CurrentTool = MainViewModel.EditTool.Fill;
            SelectToolButton.IsChecked = false;
            UpdateToolButtons();
        }

        private void SelectTool_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CurrentTool = MainViewModel.EditTool.Select;
            FillToolButton.IsChecked = false;
            UpdateToolButtons();
        }

        private void UpdateToolButtons()
        {
            FillToolButton.IsChecked = _viewModel.CurrentTool == MainViewModel.EditTool.Fill;
            SelectToolButton.IsChecked = _viewModel.CurrentTool == MainViewModel.EditTool.Select;
        }

        private void MiniMap_ViewportChangeRequested(object sender, Point e)
        {
            _viewModel.ViewportX = (int)e.X;
            _viewModel.ViewportY = (int)e.Y;
            UpdateScrollViewerPosition();
            RenderMapCanvas();
        }

        private void MapScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_viewModel.CurrentMap == null) return;

            var horizontalOffset = MapScrollViewer.HorizontalOffset;
            var verticalOffset = MapScrollViewer.VerticalOffset;

            _viewModel.ViewportX = (int)(horizontalOffset / TileSize);
            _viewModel.ViewportY = (int)(verticalOffset / TileSize);
        }

        private void UpdateScrollViewerPosition()
        {
            MapScrollViewer.ScrollToHorizontalOffset(_viewModel.ViewportX * TileSize);
            MapScrollViewer.ScrollToVerticalOffset(_viewModel.ViewportY * TileSize);
        }

        #endregion

        #region Map Rendering (Viewport-based)

        private void RenderMapCanvas()
        {
            if (_viewModel.CurrentMap == null)
            {
                MapCanvas.Children.Clear();
                MapCanvas.Width = 100;
                MapCanvas.Height = 100;
                return;
            }

            // Calculate visible area
            int startX = Math.Max(0, _viewModel.ViewportX);
            int startY = Math.Max(0, _viewModel.ViewportY);
            int endX = Math.Min(_viewModel.CurrentMap.Width, startX + _viewModel.ViewportWidth);
            int endY = Math.Min(_viewModel.CurrentMap.Height, startY + _viewModel.ViewportHeight);

            int visibleWidth = endX - startX;
            int visibleHeight = endY - startY;

            // Set canvas size to full map size for scrolling
            MapCanvas.Width = _viewModel.CurrentMap.Width * TileSize;
            MapCanvas.Height = _viewModel.CurrentMap.Height * TileSize;

            // Create bitmap for visible area only
            int bitmapWidth = visibleWidth * TileSize;
            int bitmapHeight = visibleHeight * TileSize;

            if (bitmapWidth <= 0 || bitmapHeight <= 0) return;

            _tilesBitmap = new WriteableBitmap(bitmapWidth, bitmapHeight, 96, 96, PixelFormats.Pbgra32, null);

            _tilesBitmap.Lock();

            // Draw only visible tiles
            for (int y = 0; y < visibleHeight; y++)
            {
                for (int x = 0; x < visibleWidth; x++)
                {
                    int mapX = startX + x;
                    int mapY = startY + y;
                    DrawTileAt(x, y, mapX, mapY);
                }
            }

            _tilesBitmap.Unlock();

            // Clear canvas and add the visible area image
            MapCanvas.Children.Clear();
            var img = new Image
            {
                Source = _tilesBitmap,
                Width = bitmapWidth,
                Height = bitmapHeight
            };
            Canvas.SetLeft(img, startX * TileSize);
            Canvas.SetTop(img, startY * TileSize);
            MapCanvas.Children.Add(img);

            // Re-add selection rectangle if selecting
            if (_isSelecting && _selectionRectangle != null)
            {
                MapCanvas.Children.Add(_selectionRectangle);
            }

            // Update mini-map
            MiniMap.Map = _viewModel.CurrentMap;
        }

        private void DrawTileAt(int bitmapX, int bitmapY, int mapX, int mapY)
        {
            if (_viewModel.CurrentMap == null || _tilesBitmap == null) return;

            var destRect = new Int32Rect(bitmapX * TileSize, bitmapY * TileSize, TileSize, TileSize);

            // Draw terrain layer
            DrawTileTerrainLayer(destRect, mapX, mapY);

            // Draw property layer
            DrawTilePropertyLayer(destRect, mapX, mapY);

            // Draw unit layer
            DrawTileUnitLayer(destRect, mapX, mapY);

            // Draw grid
            DrawGridLine(destRect, mapX, mapY);

            // Highlight selected tile
            if (_viewModel.CurrentMap != null &&
                mapX == _viewModel.GetSelectedTileX() &&
                mapY == _viewModel.GetSelectedTileY())
            {
                HighlightTile(destRect);
            }
        }

        private void DrawTileTerrainLayer(Int32Rect destRect, int x, int y)
        {
            if (_viewModel.CurrentMap?.TileArray == null || _tilesBitmap == null) return;
            var terrain = _viewModel.CurrentMap.TileArray[x, y]?.Terrain ?? "Plain";
            var season = _viewModel.CurrentMap.Season ?? "Summer";
            var sprite = SpriteManager.Instance.GetSprite(terrain, season, "terrain", TileSize);

            if (sprite is BitmapSource bmp)
            {
                try
                {
                    var scaledBitmap = new RenderTargetBitmap(TileSize, TileSize, 96, 96, PixelFormats.Pbgra32);
                    var drawingVisual = new DrawingVisual();
                    using (var drawingContext = drawingVisual.RenderOpen())
                    {
                        drawingContext.DrawImage(bmp, new Rect(0, 0, TileSize, TileSize));
                    }
                    scaledBitmap.Render(drawingVisual);

                    var buffer = new byte[TileSize * TileSize * 4];
                    scaledBitmap.CopyPixels(buffer, TileSize * 4, 0);
                    _tilesBitmap.WritePixels(destRect, buffer, TileSize * 4, 0);
                }
                catch
                {
                    DrawSolidColorTile(destRect, Colors.LightGray);
                }
            }
            else
            {
                DrawSolidColorTile(destRect, Colors.LightGray);
            }
        }

        private void DrawTilePropertyLayer(Int32Rect destRect, int x, int y)
        {
            if (_viewModel.CurrentMap == null || _tilesBitmap == null) return;
            var prop = _viewModel.CurrentMap.Properties.Find(p => p.X == x && p.Y == y);
            if (prop == null) return;

            var sprite = SpriteManager.Instance.GetSprite(prop.Type, prop.Owner, "property", TileSize);
            if (sprite is BitmapSource bmp)
            {
                try
                {
                    var scaledBitmap = new RenderTargetBitmap(TileSize, TileSize, 96, 96, PixelFormats.Pbgra32);
                    var drawingVisual = new DrawingVisual();
                    using (var drawingContext = drawingVisual.RenderOpen())
                    {
                        drawingContext.DrawImage(bmp, new Rect(0, 0, TileSize, TileSize));
                    }
                    scaledBitmap.Render(drawingVisual);

                    var buffer = new byte[TileSize * TileSize * 4];
                    scaledBitmap.CopyPixels(buffer, TileSize * 4, 0);
                    BlendPixels(destRect, buffer);
                }
                catch
                {
                    // Silent fail - property layer is optional
                }
            }
        }

        private void DrawTileUnitLayer(Int32Rect destRect, int x, int y)
        {
            if (_viewModel.CurrentMap == null || _tilesBitmap == null) return;
            var unit = _viewModel.CurrentMap.Units.Find(u => u.X == x && u.Y == y);
            if (unit == null) return;

            var sprite = SpriteManager.Instance.GetSprite(unit.Type, unit.Owner, "unit", TileSize);
            if (sprite is BitmapSource bmp)
            {
                try
                {
                    var scaledBitmap = new RenderTargetBitmap(TileSize, TileSize, 96, 96, PixelFormats.Pbgra32);
                    var drawingVisual = new DrawingVisual();
                    using (var drawingContext = drawingVisual.RenderOpen())
                    {
                        drawingContext.DrawImage(bmp, new Rect(0, 0, TileSize, TileSize));
                    }
                    scaledBitmap.Render(drawingVisual);

                    var buffer = new byte[TileSize * TileSize * 4];
                    scaledBitmap.CopyPixels(buffer, TileSize * 4, 0);
                    BlendPixels(destRect, buffer);
                }
                catch
                {
                    // Silent fail - unit layer is optional
                }
            }
        }

        private void DrawSolidColorTile(Int32Rect destRect, Color color)
        {
            var pixels = new byte[TileSize * TileSize * 4];
            for (int py = 0; py < TileSize; py++)
            {
                for (int px = 0; px < TileSize; px++)
                {
                    int idx = (py * TileSize + px) * 4;
                    pixels[idx + 0] = color.B;
                    pixels[idx + 1] = color.G;
                    pixels[idx + 2] = color.R;
                    pixels[idx + 3] = color.A;
                }
            }
            _tilesBitmap.WritePixels(destRect, pixels, TileSize * 4, 0);
        }

        private void BlendPixels(Int32Rect destRect, byte[] newPixels)
        {
            var existingPixels = new byte[TileSize * TileSize * 4];
            _tilesBitmap.CopyPixels(destRect, existingPixels, TileSize * 4, 0);

            for (int i = 0; i < existingPixels.Length; i += 4)
            {
                float alpha = newPixels[i + 3] / 255f;
                if (alpha > 0)
                {
                    existingPixels[i] = (byte)(existingPixels[i] * (1 - alpha) + newPixels[i] * alpha);
                    existingPixels[i + 1] = (byte)(existingPixels[i + 1] * (1 - alpha) + newPixels[i + 1] * alpha);
                    existingPixels[i + 2] = (byte)(existingPixels[i + 2] * (1 - alpha) + newPixels[i + 2] * alpha);
                    existingPixels[i + 3] = Math.Max(existingPixels[i + 3], newPixels[i + 3]);
                }
            }

            _tilesBitmap.WritePixels(destRect, existingPixels, TileSize * 4, 0);
        }

        private void DrawGridLine(Int32Rect destRect, int x, int y)
        {
            if (_viewModel.CurrentMap == null || _tilesBitmap == null) return;
            var gridColor = Colors.Black;
            var gridBytes = new byte[] { gridColor.B, gridColor.G, gridColor.R, gridColor.A };

            // Right border
            if (x < _viewModel.CurrentMap.Width - 1)
            {
                for (int py = 0; py < TileSize; py++)
                {
                    int bx = destRect.X + (TileSize - 1);
                    int by = destRect.Y + py;
                    if (bx >= 0 && bx < _tilesBitmap.PixelWidth && by >= 0 && by < _tilesBitmap.PixelHeight)
                    {
                        _tilesBitmap.WritePixels(new Int32Rect(bx, by, 1, 1), gridBytes, 4, 0);
                    }
                }
            }
            // Bottom border
            if (y < _viewModel.CurrentMap.Height - 1)
            {
                for (int px = 0; px < TileSize; px++)
                {
                    int bx = destRect.X + px;
                    int by = destRect.Y + (TileSize - 1);
                    if (bx >= 0 && bx < _tilesBitmap.PixelWidth && by >= 0 && by < _tilesBitmap.PixelHeight)
                    {
                        _tilesBitmap.WritePixels(new Int32Rect(bx, by, 1, 1), gridBytes, 4, 0);
                    }
                }
            }
        }

        private void HighlightTile(Int32Rect destRect)
        {
            if (_tilesBitmap == null) return;
            var highlightColor = Colors.Red;
            var highlightBytes = new byte[] { highlightColor.B, highlightColor.G, highlightColor.R, highlightColor.A };

            // Draw highlight border
            for (int i = 0; i < TileSize; i++)
            {
                // Top
                _tilesBitmap.WritePixels(new Int32Rect(destRect.X + i, destRect.Y, 1, 1), highlightBytes, 4, 0);
                // Bottom
                _tilesBitmap.WritePixels(new Int32Rect(destRect.X + i, destRect.Y + TileSize - 1, 1, 1), highlightBytes, 4, 0);
                // Left
                _tilesBitmap.WritePixels(new Int32Rect(destRect.X, destRect.Y + i, 1, 1), highlightBytes, 4, 0);
                // Right
                _tilesBitmap.WritePixels(new Int32Rect(destRect.X + TileSize - 1, destRect.Y + i, 1, 1), highlightBytes, 4, 0);
            }
        }

        #endregion

        #region Canvas Mouse Handling

        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.CurrentMap == null) return;
            var pos = e.GetPosition(MapCanvas);
            int x = (int)(pos.X / TileSize);
            int y = (int)(pos.Y / TileSize);

            if (x >= 0 && x < _viewModel.CurrentMap.Width && y >= 0 && y < _viewModel.CurrentMap.Height)
            {
                if (_viewModel.CurrentTool == MainViewModel.EditTool.Select)
                {
                    // Start selection
                    _isSelecting = true;
                    _selectionStartX = x;
                    _selectionStartY = y;
                    _selectionEndX = x;
                    _selectionEndY = y;
                    UpdateSelectionRectangle();
                }
                else
                {
                    _isPainting = true;
                    _viewModel.PaintTile(x, y);
                    _lastPaintPoint = pos;
                    _viewModel.SetSelectedTile(x, y);
                    ShowTileDetails(x, y);
                    RenderMapCanvas();
                }
            }
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);
            int x = (int)(pos.X / TileSize);
            int y = (int)(pos.Y / TileSize);

            if (_viewModel.CurrentMap != null && x >= 0 && x < _viewModel.CurrentMap.Width && y >= 0 && y < _viewModel.CurrentMap.Height)
            {
                _viewModel.UpdateCoordinates(x, y);

                if (_isSelecting && e.LeftButton == MouseButtonState.Pressed)
                {
                    _selectionEndX = x;
                    _selectionEndY = y;
                    UpdateSelectionRectangle();
                }
                else if (_isPainting && e.LeftButton == MouseButtonState.Pressed)
                {
                    if (_debounceTimer == null)
                    {
                        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
                        _debounceTimer.Tick += (s, ev) =>
                        {
                            _viewModel.PaintTile(x, y);
                            RenderMapCanvas();
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
            if (_isSelecting)
            {
                _isSelecting = false;
                // Store selection for copy/paste operations
                _viewModel.SetSelectedArea(_selectionStartX, _selectionStartY, _selectionEndX, _selectionEndY);
            }
            _isPainting = false;
        }

        private void MapCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Context menu or secondary action
            var pos = e.GetPosition(MapCanvas);
            int x = (int)(pos.X / TileSize);
            int y = (int)(pos.Y / TileSize);

            if (_viewModel.CurrentMap != null && x >= 0 && x < _viewModel.CurrentMap.Width && y >= 0 && y < _viewModel.CurrentMap.Height)
            {
                _viewModel.SetSelectedTile(x, y);
                ShowTileDetails(x, y);
            }
        }

        private void UpdateSelectionRectangle()
        {
            if (_selectionRectangle == null)
            {
                _selectionRectangle = new System.Windows.Shapes.Rectangle
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                    Fill = new SolidColorBrush(Color.FromArgb(64, 0, 0, 255))
                };
                MapCanvas.Children.Add(_selectionRectangle);
            }

            int minX = Math.Min(_selectionStartX, _selectionEndX);
            int minY = Math.Min(_selectionStartY, _selectionEndY);
            int maxX = Math.Max(_selectionStartX, _selectionEndX);
            int maxY = Math.Max(_selectionStartY, _selectionEndY);

            Canvas.SetLeft(_selectionRectangle, minX * TileSize);
            Canvas.SetTop(_selectionRectangle, minY * TileSize);
            _selectionRectangle.Width = (maxX - minX + 1) * TileSize;
            _selectionRectangle.Height = (maxY - minY + 1) * TileSize;
        }

        #endregion

        #region Copy/Paste/Delete Operations

        private void CopySelection()
        {
            if (_viewModel.CurrentMap == null) return;

            var selection = _viewModel.GetSelectedArea();
            if (selection == null) return;

            int minX = Math.Min(selection.Value.startX, selection.Value.endX);
            int minY = Math.Min(selection.Value.startY, selection.Value.endY);
            int maxX = Math.Max(selection.Value.startX, selection.Value.endX);
            int maxY = Math.Max(selection.Value.startY, selection.Value.endY);

            _clipboardWidth = maxX - minX + 1;
            _clipboardHeight = maxY - minY + 1;

            // Copy tiles
            _clipboardTiles = new List<Tile>();
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (_viewModel.CurrentMap.TileArray[x, y] != null)
                    {
                        _clipboardTiles.Add(new Tile
                        {
                            X = x - minX,
                            Y = y - minY,
                            Terrain = _viewModel.CurrentMap.TileArray[x, y].Terrain,
                            Traversable = _viewModel.CurrentMap.TileArray[x, y].Traversable
                        });
                    }
                }
            }

            // Copy properties
            _clipboardProperties = new List<Property>();
            foreach (var prop in _viewModel.CurrentMap.Properties.Where(p => p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY))
            {
                _clipboardProperties.Add(new Property
                {
                    X = prop.X - minX,
                    Y = prop.Y - minY,
                    Type = prop.Type,
                    Owner = prop.Owner
                });
            }

            // Copy units
            _clipboardUnits = new List<Unit>();
            foreach (var unit in _viewModel.CurrentMap.Units.Where(u => u.X >= minX && u.X <= maxX && u.Y >= minY && u.Y <= maxY))
            {
                _clipboardUnits.Add(new Unit
                {
                    X = unit.X - minX,
                    Y = unit.Y - minY,
                    Type = unit.Type,
                    Owner = unit.Owner,
                    HP = unit.HP
                });
            }

            _viewModel.StatusText = $"Copied {_clipboardWidth}x{_clipboardHeight} area to clipboard";
        }

        private void PasteSelection()
        {
            if (_viewModel.CurrentMap == null || _clipboardTiles == null) return;

            var selectedTile = _viewModel.GetSelectedTile();
            if (!selectedTile.HasValue) return;

            int pasteX = selectedTile.Value.x;
            int pasteY = selectedTile.Value.y;

            // Paste tiles
            foreach (var tile in _clipboardTiles)
            {
                int targetX = pasteX + tile.X;
                int targetY = pasteY + tile.Y;
                if (targetX >= 0 && targetX < _viewModel.CurrentMap.Width && targetY >= 0 && targetY < _viewModel.CurrentMap.Height)
                {
                    _viewModel.CurrentMap.TileArray[targetX, targetY].Terrain = tile.Terrain;
                    _viewModel.CurrentMap.TileArray[targetX, targetY].Traversable = tile.Traversable;
                }
            }

            // Paste properties
            if (_clipboardProperties != null)
            {
                foreach (var prop in _clipboardProperties)
                {
                    int targetX = pasteX + prop.X;
                    int targetY = pasteY + prop.Y;
                    if (targetX >= 0 && targetX < _viewModel.CurrentMap.Width && targetY >= 0 && targetY < _viewModel.CurrentMap.Height)
                    {
                        var existing = _viewModel.CurrentMap.Properties.Find(p => p.X == targetX && p.Y == targetY);
                        if (existing != null)
                            _viewModel.CurrentMap.Properties.Remove(existing);

                        _viewModel.CurrentMap.Properties.Add(new Property
                        {
                            X = targetX,
                            Y = targetY,
                            Type = prop.Type,
                            Owner = prop.Owner
                        });
                    }
                }
            }

            // Paste units
            if (_clipboardUnits != null)
            {
                foreach (var unit in _clipboardUnits)
                {
                    int targetX = pasteX + unit.X;
                    int targetY = pasteY + unit.Y;
                    if (targetX >= 0 && targetX < _viewModel.CurrentMap.Width && targetY >= 0 && targetY < _viewModel.CurrentMap.Height)
                    {
                        var existing = _viewModel.CurrentMap.Units.Find(u => u.X == targetX && u.Y == targetY);
                        if (existing != null)
                            _viewModel.CurrentMap.Units.Remove(existing);

                        _viewModel.CurrentMap.Units.Add(new Unit
                        {
                            X = targetX,
                            Y = targetY,
                            Type = unit.Type,
                            Owner = unit.Owner,
                            HP = unit.HP
                        });
                    }
                }
            }

            _viewModel.PushToUndoStack();
            RenderMapCanvas();
            _viewModel.StatusText = $"Pasted {_clipboardWidth}x{_clipboardHeight} area";
        }

        private void DeleteSelection()
        {
            if (_viewModel.CurrentMap == null) return;

            var selection = _viewModel.GetSelectedArea();
            if (selection == null) return;

            int minX = Math.Min(selection.Value.startX, selection.Value.endX);
            int minY = Math.Min(selection.Value.startY, selection.Value.endY);
            int maxX = Math.Max(selection.Value.startX, selection.Value.endX);
            int maxY = Math.Max(selection.Value.startY, selection.Value.endY);

            // Clear terrain to default
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (_viewModel.CurrentMap.TileArray[x, y] != null)
                    {
                        _viewModel.CurrentMap.TileArray[x, y].Terrain = "Plain";
                        _viewModel.CurrentMap.TileArray[x, y].Traversable = true;
                    }
                }
            }

            // Remove properties
            _viewModel.CurrentMap.Properties.RemoveAll(p => p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY);

            // Remove units
            _viewModel.CurrentMap.Units.RemoveAll(u => u.X >= minX && u.X <= maxX && u.Y >= minY && u.Y <= maxY);

            _viewModel.PushToUndoStack();
            RenderMapCanvas();
            _viewModel.StatusText = "Selection deleted";
        }

        #endregion

        #region Details Panel

        private void ShowTileDetails(int x, int y)
        {
            if (_viewModel.CurrentMap?.TileArray == null) return;
            DetailsPanel.Children.Clear();
            var tile = _viewModel.CurrentMap.TileArray[x, y];
            var prop = _viewModel.CurrentMap.Properties.Find(p => p.X == x && p.Y == y);
            var unit = _viewModel.CurrentMap.Units.Find(u => u.X == x && u.Y == y);

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
                DetailsPanel.Children.Add(new TextBlock { Text = $"Property: {prop.Type}", Margin = new Thickness(0, 10, 0, 0) });

                var ownerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
                ownerPanel.Children.Add(new TextBlock { Text = "Owner: ", VerticalAlignment = VerticalAlignment.Center });
                var ownerCombo = new ComboBox { Width = 100 };
                foreach (var owner in _viewModel.OwnerTypes)
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
                        RenderMapCanvas();
                    }
                };
                ownerPanel.Children.Add(ownerCombo);
                DetailsPanel.Children.Add(ownerPanel);

                var removePropBtn = new Button { Content = "Remove Property", Margin = new Thickness(0, 5, 0, 0) };
                removePropBtn.Click += (s, e) =>
                {
                    _viewModel.CurrentMap.Properties.Remove(prop);
                    _viewModel.PushToUndoStack();
                    ShowTileDetails(x, y);
                    RenderMapCanvas();
                };
                DetailsPanel.Children.Add(removePropBtn);
            }

            if (unit != null)
            {
                DetailsPanel.Children.Add(new TextBlock { Text = $"Unit: {unit.Type}", Margin = new Thickness(0, 10, 0, 0) });
                DetailsPanel.Children.Add(new TextBlock { Text = $"Owner: {unit.Owner}" });
                DetailsPanel.Children.Add(new TextBlock { Text = $"HP: {unit.HP}" });

                var removeUnitBtn = new Button { Content = "Remove Unit", Margin = new Thickness(0, 5, 0, 0) };
                removeUnitBtn.Click += (s, e) =>
                {
                    _viewModel.CurrentMap.Units.Remove(unit);
                    _viewModel.PushToUndoStack();
                    ShowTileDetails(x, y);
                    RenderMapCanvas();
                };
                DetailsPanel.Children.Add(removeUnitBtn);
            }
        }

        private void SetTraversable(int x, int y, bool traversable)
        {
            if (_viewModel.CurrentMap == null) return;
            if (_viewModel.CurrentMap.TileArray[x, y] != null)
            {
                _viewModel.CurrentMap.TileArray[x, y].Traversable = traversable;
                _viewModel.PushToUndoStack();
            }
        }

        #endregion
    }

    // Extension methods for MainViewModel
    public static class MainViewModelExtensions
    {
        private static int? _selectedTileX;
        private static int? _selectedTileY;
        private static (int startX, int startY, int endX, int endY)? _selectedArea;

        public static int? GetSelectedTileX(this MainViewModel vm) => _selectedTileX;
        public static int? GetSelectedTileY(this MainViewModel vm) => _selectedTileY;
        public static (int x, int y)? GetSelectedTile(this MainViewModel vm)
        {
            if (_selectedTileX.HasValue && _selectedTileY.HasValue)
                return (_selectedTileX.Value, _selectedTileY.Value);
            return null;
        }
        public static void SetSelectedTile(this MainViewModel vm, int? x, int? y)
        {
            _selectedTileX = x;
            _selectedTileY = y;
        }
        public static void SetSelectedArea(this MainViewModel vm, int startX, int startY, int endX, int endY)
        {
            _selectedArea = (startX, startY, endX, endY);
        }
        public static (int startX, int startY, int endX, int endY)? GetSelectedArea(this MainViewModel vm) => _selectedArea;
        public static void PushToUndoStack(this MainViewModel vm)
        {
            // This should be implemented in the ViewModel properly
            // For now, just trigger a property change
            vm.NotifyPropertyChanged(nameof(vm.CurrentMap));
        }
    }
}