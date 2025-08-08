using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WWXMapEditor.Models;

namespace WWXMapEditor.Controls
{
    public class MapCanvas : Canvas
    {
        private readonly DrawingVisual _gridVisual = new DrawingVisual();
        private readonly DrawingVisual _tilesVisual = new DrawingVisual();
        private readonly DrawingVisual _overlayVisual = new DrawingVisual();
        private System.Windows.Point _lastMousePosition;
        private bool _isDrawing;

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

        #endregion

        public MapCanvas()
        {
            ClipToBounds = true;
            Background = System.Windows.Media.Brushes.Transparent; // Important for mouse hit testing

            // Add visual children
            AddVisualChild(_tilesVisual);
            AddVisualChild(_gridVisual);
            AddVisualChild(_overlayVisual);

            // Subscribe to mouse events
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            MouseLeave += OnMouseLeave;
        }

        protected override int VisualChildrenCount => 3;

        protected override Visual GetVisualChild(int index)
        {
            return index switch
            {
                0 => _tilesVisual,
                1 => _gridVisual,
                2 => _overlayVisual,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }

        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MapCanvas canvas)
            {
                canvas.UpdateCanvasSize();
                canvas.DrawTiles();
                canvas.DrawGrid();
            }
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MapCanvas canvas)
            {
                canvas.DrawGrid();
            }
        }

        private void UpdateCanvasSize()
        {
            if (Map != null)
            {
                Width = Map.Width * GridSize;
                Height = Map.Height * GridSize;
            }
        }

        private void DrawGrid()
        {
            using (var dc = _gridVisual.RenderOpen())
            {
                if (ShowGrid && Map != null)
                {
                    var pen = new System.Windows.Media.Pen(
                        new SolidColorBrush(System.Windows.Media.Color.FromArgb(64, 128, 128, 128)), 1);
                    pen.Freeze();

                    // Draw vertical lines
                    for (int x = 0; x <= Map.Width; x++)
                    {
                        var startPoint = new System.Windows.Point(x * GridSize, 0);
                        var endPoint = new System.Windows.Point(x * GridSize, Map.Height * GridSize);
                        dc.DrawLine(pen, startPoint, endPoint);
                    }

                    // Draw horizontal lines
                    for (int y = 0; y <= Map.Height; y++)
                    {
                        var startPoint = new System.Windows.Point(0, y * GridSize);
                        var endPoint = new System.Windows.Point(Map.Width * GridSize, y * GridSize);
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
                    for (int x = 0; x < Map.Width; x++)
                    {
                        for (int y = 0; y < Map.Height; y++)
                        {
                            var tile = Map.Tiles[x, y];
                            if (tile != null)
                            {
                                var rect = new Rect(x * GridSize, y * GridSize, GridSize, GridSize);
                                var terrainType = tile.TerrainType ?? "Plains";
                                var brush = GetBrushForTerrain(terrainType);
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
                var tileX = (int)(currentPosition.X / GridSize);
                var tileY = (int)(currentPosition.Y / GridSize);

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
                    var tileX = (int)(position.X / GridSize);
                    var tileY = (int)(position.Y / GridSize);

                    if (tileX >= 0 && tileX < Map.Width && tileY >= 0 && tileY < Map.Height)
                    {
                        var rect = new Rect(tileX * GridSize, tileY * GridSize, GridSize, GridSize);
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

            var tileX = (int)(position.X / GridSize);
            var tileY = (int)(position.Y / GridSize);

            if (tileX < 0 || tileX >= Map.Width || tileY < 0 || tileY >= Map.Height) return;

            var tile = Map.Tiles[tileX, tileY];
            if (tile == null) return;

            switch (SelectedTool)
            {
                case "Brush":
                case "Paint":
                    if (SelectedTile != null)
                    {
                        tile.TerrainType = SelectedTile.TerrainType;
                        DrawTiles(); // Redraw tiles
                    }
                    break;

                case "Eraser":
                    tile.TerrainType = "Plains"; // Default terrain
                    tile.Unit = null;
                    tile.Property = null;
                    tile.IsLandBlocked = false;
                    tile.IsAirBlocked = false;
                    tile.IsWaterBlocked = false;
                    DrawTiles(); // Redraw tiles
                    break;

                    // TODO: Implement other tools
            }
        }
    }

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
}