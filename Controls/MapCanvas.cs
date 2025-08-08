using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WWXMapEditor.Models;

namespace WWXMapEditor.Controls
{
    public class MapCanvas : Canvas
    {
        #region Visual Elements
        private readonly DrawingVisual _gridVisual = new DrawingVisual();
        private readonly DrawingVisual _tilesVisual = new DrawingVisual();
        private readonly DrawingVisual _overlayVisual = new DrawingVisual();
        private readonly Dictionary<System.Windows.Point, DrawingVisual> _tileVisuals = new Dictionary<System.Windows.Point, DrawingVisual>();
        private readonly VisualCollection _visuals;
        #endregion

        #region State
        private System.Windows.Point _lastMousePosition;
        private bool _isDrawing;
        private bool _needsFullRedraw = true;
        private readonly HashSet<System.Windows.Point> _dirtyTiles = new HashSet<System.Windows.Point>();
        private Rect _lastViewport = Rect.Empty;
        private readonly DispatcherTimer _renderTimer;
        private readonly Dictionary<string, System.Windows.Media.Brush> _terrainBrushCache = new Dictionary<string, System.Windows.Media.Brush>();
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(nameof(Map), typeof(Map), typeof(MapCanvas),
                new PropertyMetadata(null, OnMapChanged));

        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(MapCanvas),
                new PropertyMetadata(true, OnVisualPropertyChanged));

        public static readonly DependencyProperty GridSizeProperty =
            DependencyProperty.Register(nameof(GridSize), typeof(int), typeof(MapCanvas),
                new PropertyMetadata(32, OnVisualPropertyChanged));

        public static readonly DependencyProperty SelectedToolProperty =
            DependencyProperty.Register(nameof(SelectedTool), typeof(string), typeof(MapCanvas),
                new PropertyMetadata("Select"));

        public static readonly DependencyProperty SelectedTileProperty =
            DependencyProperty.Register(nameof(SelectedTile), typeof(TilePaletteItem), typeof(MapCanvas),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(MapCanvas),
                new PropertyMetadata(1.0, OnZoomLevelChanged));

        public static readonly DependencyProperty EnableViewportCullingProperty =
            DependencyProperty.Register(nameof(EnableViewportCulling), typeof(bool), typeof(MapCanvas),
                new PropertyMetadata(true));

        public Map Map
        {
            get => (Map)GetValue(MapProperty);
            set => SetValue(MapProperty, value);
        }

        public bool ShowGrid
        {
            get => (bool)GetValue(ShowGridProperty);
            set => SetValue(ShowGridProperty, value);
        }

        public int GridSize
        {
            get => (int)GetValue(GridSizeProperty);
            set => SetValue(GridSizeProperty, value);
        }

        public string SelectedTool
        {
            get => (string)GetValue(SelectedToolProperty);
            set => SetValue(SelectedToolProperty, value);
        }

        public TilePaletteItem SelectedTile
        {
            get => (TilePaletteItem)GetValue(SelectedTileProperty);
            set => SetValue(SelectedTileProperty, value);
        }

        public double ZoomLevel
        {
            get => (double)GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, value);
        }

        public bool EnableViewportCulling
        {
            get => (bool)GetValue(EnableViewportCullingProperty);
            set => SetValue(EnableViewportCullingProperty, value);
        }

        #endregion

        #region Constructor
        public MapCanvas()
        {
            ClipToBounds = true;
            Background = System.Windows.Media.Brushes.Transparent; // Important for mouse hit testing
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;

            // Initialize visual collection
            _visuals = new VisualCollection(this);

            // Add visual children
            _visuals.Add(_tilesVisual);
            _visuals.Add(_gridVisual);
            _visuals.Add(_overlayVisual);

            // Setup render timer for batched updates
            _renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _renderTimer.Tick += OnRenderTimerTick;

            // Subscribe to mouse events
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            MouseLeave += OnMouseLeave;

            // Initialize terrain brush cache
            InitializeBrushCache();
        }
        #endregion

        #region Visual Children Management
        protected override int VisualChildrenCount => _visuals.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visuals.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _visuals[index];
        }
        #endregion

        #region Property Changed Handlers
        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MapCanvas canvas)
            {
                canvas._needsFullRedraw = true;
                canvas._tileVisuals.Clear();
                canvas.UpdateCanvasSize();

                if (canvas.EnableViewportCulling)
                {
                    canvas.InvalidateVisual();
                }
                else
                {
                    // Use old rendering method for backward compatibility
                    canvas.DrawTiles();
                    canvas.DrawGrid();
                }
            }
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MapCanvas canvas)
            {
                if (canvas.EnableViewportCulling)
                {
                    canvas.InvalidateVisual();
                }
                else
                {
                    canvas.DrawGrid();
                }
            }
        }

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MapCanvas canvas)
            {
                canvas._needsFullRedraw = true;
                canvas.UpdateCanvasSize();
                canvas.InvalidateVisual();
            }
        }
        #endregion

        #region Canvas Management
        private void UpdateCanvasSize()
        {
            if (Map != null)
            {
                Width = Map.Width * GridSize * ZoomLevel;
                Height = Map.Height * GridSize * ZoomLevel;
            }
        }
        #endregion

        #region Optimized Rendering
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (!EnableViewportCulling)
            {
                // Use old rendering for backward compatibility
                return;
            }

            if (Map == null) return;

            var viewport = GetCurrentViewport();

            // Only render visible tiles
            RenderVisibleTiles(viewport);

            // Render grid if enabled
            if (ShowGrid)
            {
                RenderOptimizedGrid(viewport);
            }
        }

        private void RenderVisibleTiles(Rect viewport)
        {
            if (Map?.Tiles == null) return;

            var tileSize = GridSize * ZoomLevel;

            // Calculate visible tile range
            int startX = Math.Max(0, (int)(viewport.Left / tileSize));
            int startY = Math.Max(0, (int)(viewport.Top / tileSize));
            int endX = Math.Min(Map.Width - 1, (int)(viewport.Right / tileSize) + 1);
            int endY = Math.Min(Map.Height - 1, (int)(viewport.Bottom / tileSize) + 1);

            // Remove tiles that are no longer visible
            var tilesToRemove = new List<System.Windows.Point>();
            foreach (var kvp in _tileVisuals)
            {
                var point = kvp.Key;
                if (point.X < startX || point.X > endX || point.Y < startY || point.Y > endY)
                {
                    _visuals.Remove(kvp.Value);
                    tilesToRemove.Add(point);
                }
            }

            foreach (var point in tilesToRemove)
            {
                _tileVisuals.Remove(point);
            }

            // Add or update visible tiles
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    var point = new System.Windows.Point(x, y);
                    var tile = Map.Tiles[x, y];

                    if (tile == null) continue;

                    // Check if tile needs update
                    if (_needsFullRedraw || _dirtyTiles.Contains(point) || !_tileVisuals.ContainsKey(point))
                    {
                        RenderSingleTile(x, y, tile);
                    }
                }
            }

            _dirtyTiles.Clear();
            _needsFullRedraw = false;
        }

        private void RenderSingleTile(int x, int y, Tile tile)
        {
            var point = new System.Windows.Point(x, y);
            var tileSize = GridSize * ZoomLevel;

            // Remove old visual if exists
            if (_tileVisuals.TryGetValue(point, out var oldVisual))
            {
                _visuals.Remove(oldVisual);
                _tileVisuals.Remove(point);
            }

            // Create new visual
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var rect = new Rect(x * tileSize, y * tileSize, tileSize, tileSize);

                // Draw terrain
                var terrainBrush = GetCachedBrushForTerrain(tile.TerrainType ?? "Plains");
                dc.DrawRectangle(terrainBrush, null, rect);

                // Draw property if exists
                if (tile.Property != null)
                {
                    DrawProperty(dc, tile.Property, rect);
                }

                // Draw unit if exists
                if (tile.Unit != null)
                {
                    DrawUnit(dc, tile.Unit, rect);
                }

                // Draw blocking indicators
                if (tile.IsLandBlocked || tile.IsAirBlocked || tile.IsWaterBlocked)
                {
                    DrawBlockingIndicators(dc, tile, rect);
                }
            }

            _tileVisuals[point] = visual;
            _visuals.Insert(_visuals.Count - 2, visual); // Insert before grid and overlay
        }

        private void RenderOptimizedGrid(Rect viewport)
        {
            using (var dc = _gridVisual.RenderOpen())
            {
                var tileSize = GridSize * ZoomLevel;
                var pen = new System.Windows.Media.Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(64, 128, 128, 128)), 1);
                pen.Freeze();

                // Calculate visible grid lines
                int startX = Math.Max(0, (int)(viewport.Left / tileSize));
                int startY = Math.Max(0, (int)(viewport.Top / tileSize));
                int endX = Math.Min(Map.Width, (int)(viewport.Right / tileSize) + 2);
                int endY = Math.Min(Map.Height, (int)(viewport.Bottom / tileSize) + 2);

                // Draw vertical lines
                for (int x = startX; x <= endX; x++)
                {
                    var lineX = x * tileSize;
                    dc.DrawLine(pen, new System.Windows.Point(lineX, startY * tileSize), new System.Windows.Point(lineX, endY * tileSize));
                }

                // Draw horizontal lines
                for (int y = startY; y <= endY; y++)
                {
                    var lineY = y * tileSize;
                    dc.DrawLine(pen, new System.Windows.Point(startX * tileSize, lineY), new System.Windows.Point(endX * tileSize, lineY));
                }
            }
        }

        private Rect GetCurrentViewport()
        {
            // Get the visible area in canvas coordinates
            var scrollViewer = FindParentScrollViewer();
            if (scrollViewer != null)
            {
                return new Rect(
                    scrollViewer.HorizontalOffset,
                    scrollViewer.VerticalOffset,
                    scrollViewer.ViewportWidth,
                    scrollViewer.ViewportHeight
                );
            }

            return new Rect(0, 0, ActualWidth, ActualHeight);
        }

        private ScrollViewer? FindParentScrollViewer()
        {
            DependencyObject parent = this;
            while (parent != null)
            {
                if (parent is ScrollViewer scrollViewer)
                    return scrollViewer;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
        #endregion

        #region Original Rendering Methods (for backward compatibility)
        private void DrawGrid()
        {
            using (var dc = _gridVisual.RenderOpen())
            {
                if (ShowGrid && Map != null)
                {
                    var pen = new System.Windows.Media.Pen(
                        new SolidColorBrush(System.Windows.Media.Color.FromArgb(64, 128, 128, 128)), 1);
                    pen.Freeze();

                    var scaledGridSize = GridSize * ZoomLevel;

                    // Draw vertical lines
                    for (int x = 0; x <= Map.Width; x++)
                    {
                        var startPoint = new System.Windows.Point(x * scaledGridSize, 0);
                        var endPoint = new System.Windows.Point(x * scaledGridSize, Map.Height * scaledGridSize);
                        dc.DrawLine(pen, startPoint, endPoint);
                    }

                    // Draw horizontal lines
                    for (int y = 0; y <= Map.Height; y++)
                    {
                        var startPoint = new System.Windows.Point(0, y * scaledGridSize);
                        var endPoint = new System.Windows.Point(Map.Width * scaledGridSize, y * scaledGridSize);
                        dc.DrawLine(pen, startPoint, endPoint);
                    }
                }
            }
        }

        private void DrawTiles()
        {
            using (var dc = _tilesVisual.RenderOpen())
            {
                if (Map != null && Map.Tiles != null)
                {
                    var scaledGridSize = GridSize * ZoomLevel;

                    for (int x = 0; x < Map.Width; x++)
                    {
                        for (int y = 0; y < Map.Height; y++)
                        {
                            var tile = Map.Tiles[x, y];
                            if (tile != null)
                            {
                                var rect = new Rect(x * scaledGridSize, y * scaledGridSize, scaledGridSize, scaledGridSize);
                                var terrainType = tile.TerrainType ?? "Plains";
                                var brush = GetCachedBrushForTerrain(terrainType);
                                dc.DrawRectangle(brush, null, rect);

                                // Draw properties if present
                                if (tile.Property != null)
                                {
                                    DrawProperty(dc, tile.Property, rect);
                                }

                                // Draw units if present
                                if (tile.Unit != null)
                                {
                                    DrawUnit(dc, tile.Unit, rect);
                                }

                                // Draw blocking indicators
                                if (tile.IsLandBlocked || tile.IsAirBlocked || tile.IsWaterBlocked)
                                {
                                    DrawBlockingIndicators(dc, tile, rect);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Drawing Helpers
        private void InitializeBrushCache()
        {
            _terrainBrushCache["Plains"] = CreateFreezeBrush(144, 238, 144);
            _terrainBrushCache["Mountain"] = CreateFreezeBrush(139, 137, 137);
            _terrainBrushCache["Forest"] = CreateFreezeBrush(34, 139, 34);
            _terrainBrushCache["Sand"] = CreateFreezeBrush(238, 203, 173);
            _terrainBrushCache["Sea"] = CreateFreezeBrush(64, 164, 223);
            _terrainBrushCache["Default"] = CreateFreezeBrush(128, 128, 128);
        }

        private System.Windows.Media.Brush CreateFreezeBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        private System.Windows.Media.Brush GetCachedBrushForTerrain(string terrainType)
        {
            if (_terrainBrushCache.TryGetValue(terrainType, out var brush))
                return brush;

            // Fallback to old method for unknown terrain types
            return GetBrushForTerrain(terrainType);
        }

        private System.Windows.Media.Brush GetBrushForTerrain(string terrainType)
        {
            return terrainType switch
            {
                "Plains" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(144, 238, 144)),
                "Mountain" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 137, 137)),
                "Forest" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 139, 34)),
                "Sand" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(238, 203, 173)),
                "Sea" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 164, 223)),
                _ => new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 128, 128))
            };
        }

        private void DrawProperty(DrawingContext dc, Property property, Rect rect)
        {
            // Draw a simple indicator for properties
            var propertyBrush = property.Type switch
            {
                "City" => new SolidColorBrush(Colors.LightGray),
                "Factory" => new SolidColorBrush(Colors.DarkGray),
                "Airport" => new SolidColorBrush(Colors.LightBlue),
                "HQ" => new SolidColorBrush(Colors.Gold),
                _ => new SolidColorBrush(Colors.Gray)
            };

            var propertyRect = new Rect(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8);
            dc.DrawRectangle(propertyBrush, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Black, 1), propertyRect);
        }

        private void DrawUnit(DrawingContext dc, Unit unit, Rect rect)
        {
            // Draw a simple indicator for units
            var unitBrush = new SolidColorBrush(Colors.Red);
            var unitRect = new Rect(rect.X + 8, rect.Y + 8, rect.Width - 16, rect.Height - 16);
            dc.DrawEllipse(unitBrush, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Black, 1),
                new System.Windows.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2),
                (rect.Width - 16) / 2, (rect.Height - 16) / 2);
        }

        private void DrawBlockingIndicators(DrawingContext dc, Tile tile, Rect rect)
        {
            var indicatorSize = 6;
            var margin = 2;

            if (tile.IsLandBlocked)
            {
                var landRect = new Rect(rect.X + margin, rect.Y + margin, indicatorSize, indicatorSize);
                dc.DrawRectangle(System.Windows.Media.Brushes.Brown, null, landRect);
            }

            if (tile.IsAirBlocked)
            {
                var airRect = new Rect(rect.X + rect.Width - indicatorSize - margin, rect.Y + margin, indicatorSize, indicatorSize);
                dc.DrawRectangle(System.Windows.Media.Brushes.LightBlue, null, airRect);
            }

            if (tile.IsWaterBlocked)
            {
                var waterRect = new Rect(rect.X + margin, rect.Y + rect.Height - indicatorSize - margin, indicatorSize, indicatorSize);
                dc.DrawRectangle(System.Windows.Media.Brushes.Blue, null, waterRect);
            }
        }
        #endregion

        #region Mouse Handling
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _lastMousePosition = e.GetPosition(this);
            CaptureMouse();

            ProcessTool(_lastMousePosition);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var currentPosition = e.GetPosition(this);

            // Update hover overlay
            DrawHoverOverlay(currentPosition);

            // Process tool if drawing
            if (_isDrawing && e.LeftButton == MouseButtonState.Pressed)
            {
                ProcessTool(currentPosition);
            }

            _lastMousePosition = currentPosition;

            // Notify about tile position for status bar
            if (Map != null)
            {
                var scaledGridSize = GridSize * ZoomLevel;
                var tileX = (int)(currentPosition.X / scaledGridSize);
                var tileY = (int)(currentPosition.Y / scaledGridSize);

                if (tileX >= 0 && tileX < Map.Width && tileY >= 0 && tileY < Map.Height)
                {
                    RaiseEvent(new TilePositionEventArgs(tileX, tileY));
                }
            }
        }

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Clear hover overlay
            using (var dc = _overlayVisual.RenderOpen())
            {
                // Empty drawing context clears the visual
            }
        }

        private void DrawHoverOverlay(System.Windows.Point position)
        {
            using (var dc = _overlayVisual.RenderOpen())
            {
                if (Map != null && GridSize > 0)
                {
                    var scaledGridSize = GridSize * ZoomLevel;
                    var tileX = (int)(position.X / scaledGridSize);
                    var tileY = (int)(position.Y / scaledGridSize);

                    if (tileX >= 0 && tileX < Map.Width && tileY >= 0 && tileY < Map.Height)
                    {
                        var rect = new Rect(tileX * scaledGridSize, tileY * scaledGridSize, scaledGridSize, scaledGridSize);
                        var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(64, 255, 255, 255));
                        brush.Freeze();
                        dc.DrawRectangle(brush, null, rect);
                    }
                }
            }
        }

        private void ProcessTool(System.Windows.Point position)
        {
            if (Map == null || GridSize <= 0) return;

            var scaledGridSize = GridSize * ZoomLevel;
            var tileX = (int)(position.X / scaledGridSize);
            var tileY = (int)(position.Y / scaledGridSize);

            if (tileX < 0 || tileX >= Map.Width || tileY < 0 || tileY >= Map.Height) return;

            var tile = Map.Tiles[tileX, tileY];
            if (tile == null) return;

            bool tileModified = false;

            switch (SelectedTool)
            {
                case "Brush":
                case "Paint":
                    if (SelectedTile != null && tile.TerrainType != SelectedTile.TerrainType)
                    {
                        tile.TerrainType = SelectedTile.TerrainType;
                        tileModified = true;
                    }
                    break;

                case "Eraser":
                    if (tile.TerrainType != "Plains" || tile.Unit != null || tile.Property != null ||
                        tile.IsLandBlocked || tile.IsAirBlocked || tile.IsWaterBlocked)
                    {
                        tile.TerrainType = "Plains"; // Default terrain
                        tile.Unit = null;
                        tile.Property = null;
                        tile.IsLandBlocked = false;
                        tile.IsAirBlocked = false;
                        tile.IsWaterBlocked = false;
                        tileModified = true;
                    }
                    break;

                    // TODO: Implement other tools
            }

            if (tileModified)
            {
                if (EnableViewportCulling)
                {
                    // Mark tile as dirty for optimized rendering
                    InvalidateTile(tileX, tileY);
                }
                else
                {
                    // Use immediate redraw for backward compatibility
                    DrawTiles();
                }
            }
        }
        #endregion

        #region Public Methods
        public void InvalidateTile(int x, int y)
        {
            _dirtyTiles.Add(new System.Windows.Point(x, y));

            if (!_renderTimer.IsEnabled)
            {
                _renderTimer.Start();
            }
        }

        public void InvalidateRegion(int startX, int startY, int endX, int endY)
        {
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    _dirtyTiles.Add(new System.Windows.Point(x, y));
                }
            }

            if (!_renderTimer.IsEnabled)
            {
                _renderTimer.Start();
            }
        }

        private void OnRenderTimerTick(object? sender, EventArgs e)
        {
            _renderTimer.Stop();
            InvalidateVisual();
        }
        #endregion
    }

    #region Event Args
    public class TilePositionEventArgs : RoutedEventArgs
    {
        public int TileX { get; }
        public int TileY { get; }

        public TilePositionEventArgs(int tileX, int tileY)
        {
            TileX = tileX;
            TileY = tileY;
        }
    }
    #endregion
}