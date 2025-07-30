using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WwXMapEditor.Models;

namespace WwXMapEditor.Controls
{
    public partial class MiniMapControl : UserControl
    {
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(nameof(Map), typeof(Map), typeof(MiniMapControl),
                new PropertyMetadata(null, OnMapChanged));

        public static readonly DependencyProperty ViewportXProperty =
            DependencyProperty.Register(nameof(ViewportX), typeof(int), typeof(MiniMapControl),
                new PropertyMetadata(0, OnViewportChanged));

        public static readonly DependencyProperty ViewportYProperty =
            DependencyProperty.Register(nameof(ViewportY), typeof(int), typeof(MiniMapControl),
                new PropertyMetadata(0, OnViewportChanged));

        public static readonly DependencyProperty ViewportWidthProperty =
            DependencyProperty.Register(nameof(ViewportWidth), typeof(int), typeof(MiniMapControl),
                new PropertyMetadata(30, OnViewportChanged));

        public static readonly DependencyProperty ViewportHeightProperty =
            DependencyProperty.Register(nameof(ViewportHeight), typeof(int), typeof(MiniMapControl),
                new PropertyMetadata(20, OnViewportChanged));

        // Add dependency property to force updates
        public static readonly DependencyProperty UpdateTriggerProperty =
            DependencyProperty.Register(nameof(UpdateTrigger), typeof(int), typeof(MiniMapControl),
                new PropertyMetadata(0, OnUpdateTriggerChanged));

        private WriteableBitmap? _miniMapBitmap;
        private bool _isUpdating = false;

        public Map? Map
        {
            get => (Map?)GetValue(MapProperty);
            set => SetValue(MapProperty, value);
        }

        public int ViewportX
        {
            get => (int)GetValue(ViewportXProperty);
            set => SetValue(ViewportXProperty, value);
        }

        public int ViewportY
        {
            get => (int)GetValue(ViewportYProperty);
            set => SetValue(ViewportYProperty, value);
        }

        public int ViewportWidth
        {
            get => (int)GetValue(ViewportWidthProperty);
            set => SetValue(ViewportWidthProperty, value);
        }

        public int ViewportHeight
        {
            get => (int)GetValue(ViewportHeightProperty);
            set => SetValue(ViewportHeightProperty, value);
        }

        public int UpdateTrigger
        {
            get => (int)GetValue(UpdateTriggerProperty);
            set => SetValue(UpdateTriggerProperty, value);
        }

        public event EventHandler<Point>? ViewportChangeRequested;

        public MiniMapControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateMiniMap();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _miniMapBitmap = null;
        }

        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MiniMapControl control)
            {
                control.UpdateMiniMap();
            }
        }

        private static void OnViewportChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MiniMapControl control)
            {
                control.UpdateViewportIndicator();
            }
        }

        private static void OnUpdateTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MiniMapControl control)
            {
                control.UpdateMiniMap();
            }
        }

        public void ForceUpdate()
        {
            UpdateMiniMap();
        }

        private void UpdateMiniMap()
        {
            if (Map == null || _isUpdating) return;

            try
            {
                _isUpdating = true;

                // Create or reuse bitmap
                if (_miniMapBitmap == null || _miniMapBitmap.PixelWidth != Map.Width || _miniMapBitmap.PixelHeight != Map.Height)
                {
                    _miniMapBitmap = new WriteableBitmap(Map.Width, Map.Height, 96, 96, PixelFormats.Pbgra32, null);
                }

                _miniMapBitmap.Lock();

                // First pass: Draw terrain
                for (int y = 0; y < Map.Height; y++)
                {
                    for (int x = 0; x < Map.Width; x++)
                    {
                        var tile = Map.TileArray[x, y];
                        if (tile != null)
                        {
                            var color = GetTerrainColor(tile.Terrain);
                            var pixelData = new byte[] { color.B, color.G, color.R, color.A };
                            _miniMapBitmap.WritePixels(new Int32Rect(x, y, 1, 1), pixelData, 4, 0);
                        }
                    }
                }

                // Second pass: Overlay properties (if not already terrain properties)
                foreach (var property in Map.Properties)
                {
                    if (property.X >= 0 && property.X < Map.Width && property.Y >= 0 && property.Y < Map.Height)
                    {
                        var tile = Map.TileArray[property.X, property.Y];
                        // Only draw property if terrain is not already a property type
                        if (tile != null && !IsPropertyTerrain(tile.Terrain))
                        {
                            var color = GetPropertyColor(property.Type, property.Owner);
                            var pixelData = new byte[] { color.B, color.G, color.R, color.A };
                            _miniMapBitmap.WritePixels(new Int32Rect(property.X, property.Y, 1, 1), pixelData, 4, 0);
                        }
                    }
                }

                // Third pass: Overlay units
                foreach (var unit in Map.Units)
                {
                    if (unit.X >= 0 && unit.X < Map.Width && unit.Y >= 0 && unit.Y < Map.Height)
                    {
                        var color = GetUnitColor(unit.Type, unit.Owner);
                        // Make unit pixels slightly darker to stand out
                        color = Color.FromArgb(255,
                            (byte)(color.R * 0.7),
                            (byte)(color.G * 0.7),
                            (byte)(color.B * 0.7));
                        var pixelData = new byte[] { color.B, color.G, color.R, color.A };
                        _miniMapBitmap.WritePixels(new Int32Rect(unit.X, unit.Y, 1, 1), pixelData, 4, 0);
                    }
                }

                _miniMapBitmap.Unlock();
                MiniMapImage.Source = _miniMapBitmap;
                UpdateViewportIndicator();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateMiniMap error: {ex.Message}");
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private bool IsPropertyTerrain(TerrainType terrain)
        {
            return terrain == TerrainType.City ||
                   terrain == TerrainType.Factory ||
                   terrain == TerrainType.HQ ||
                   terrain == TerrainType.Airport ||
                   terrain == TerrainType.Port;
        }

        private void UpdateViewportIndicator()
        {
            if (Map == null || MiniMapImage.ActualWidth == 0 || MiniMapImage.ActualHeight == 0) return;

            var scaleX = MiniMapImage.ActualWidth / Map.Width;
            var scaleY = MiniMapImage.ActualHeight / Map.Height;

            // Calculate viewport rectangle size
            ViewportRect.Width = Math.Min(ViewportWidth * scaleX, MiniMapImage.ActualWidth);
            ViewportRect.Height = Math.Min(ViewportHeight * scaleY, MiniMapImage.ActualHeight);

            // Calculate viewport position
            var left = ViewportX * scaleX;
            var top = ViewportY * scaleY;

            // Clamp viewport to stay within minimap bounds
            left = Math.Max(0, Math.Min(left, MiniMapImage.ActualWidth - ViewportRect.Width));
            top = Math.Max(0, Math.Min(top, MiniMapImage.ActualHeight - ViewportRect.Height));

            Canvas.SetLeft(ViewportRect, left);
            Canvas.SetTop(ViewportRect, top);
        }

        private Color GetTerrainColor(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Plain => Colors.LightGreen,
                TerrainType.Forest => Colors.DarkGreen,
                TerrainType.Mountain => Colors.Brown,
                TerrainType.Road => Colors.Gray,
                TerrainType.Bridge => Colors.DarkGray,
                TerrainType.Sea => Colors.DarkBlue,
                TerrainType.Beach => Colors.SandyBrown,
                TerrainType.River => Colors.CornflowerBlue,
                TerrainType.City => Colors.LightGray,
                TerrainType.Factory => Colors.SlateGray,
                TerrainType.HQ => Colors.Gold,
                TerrainType.Airport => Colors.DimGray,
                TerrainType.Port => Colors.Navy,
                _ => Colors.LightGray
            };
        }

        private Color GetPropertyColor(PropertyType propertyType, string owner)
        {
            // Base color from property type
            var baseColor = propertyType switch
            {
                PropertyType.City => Colors.LightGray,
                PropertyType.Factory => Colors.SlateGray,
                PropertyType.HQ => Colors.Gold,
                PropertyType.Airport => Colors.DimGray,
                PropertyType.Port => Colors.Navy,
                _ => Colors.Gray
            };

            // Tint based on owner
            return owner switch
            {
                "Player" => Color.FromArgb(255, (byte)(baseColor.R * 0.7), (byte)(baseColor.G * 0.7), (byte)(baseColor.B * 1.0)),
                "Computer" => Color.FromArgb(255, (byte)(baseColor.R * 1.0), (byte)(baseColor.G * 0.7), (byte)(baseColor.B * 0.7)),
                _ => baseColor // Neutral
            };
        }

        private Color GetUnitColor(UnitType unitType, string owner)
        {
            // Owner-based primary color
            var ownerColor = owner switch
            {
                "Player" => Colors.Blue,
                "Computer" => Colors.Red,
                _ => Colors.Gray // Neutral
            };

            // Modify slightly based on unit type for variety
            if (unitType == UnitType.Infantry || unitType == UnitType.Mechanized)
            {
                return Color.FromArgb(255, (byte)(ownerColor.R * 0.8), (byte)(ownerColor.G * 0.8), (byte)(ownerColor.B * 0.8));
            }
            else if (unitType == UnitType.Fighter || unitType == UnitType.Bomber || unitType == UnitType.Helicopter)
            {
                return Color.FromArgb(255, (byte)(ownerColor.R * 0.9), (byte)(ownerColor.G * 0.9), (byte)(ownerColor.B * 1.0));
            }
            else if (unitType == UnitType.Battleship || unitType == UnitType.Cruiser || unitType == UnitType.Submarine)
            {
                return Color.FromArgb(255, (byte)(ownerColor.R * 0.7), (byte)(ownerColor.G * 0.8), (byte)(ownerColor.B * 1.0));
            }

            return ownerColor;
        }

        private void MiniMapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Map == null || MiniMapImage.ActualWidth == 0 || MiniMapImage.ActualHeight == 0) return;

            var pos = e.GetPosition(MiniMapImage);
            var scaleX = Map.Width / MiniMapImage.ActualWidth;
            var scaleY = Map.Height / MiniMapImage.ActualHeight;

            var mapX = (int)(pos.X * scaleX) - ViewportWidth / 2;
            var mapY = (int)(pos.Y * scaleY) - ViewportHeight / 2;

            mapX = Math.Max(0, Math.Min(mapX, Map.Width - ViewportWidth));
            mapY = Math.Max(0, Math.Min(mapY, Map.Height - ViewportHeight));

            ViewportChangeRequested?.Invoke(this, new Point(mapX, mapY));
        }

        private void MiniMapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                MiniMapCanvas_MouseLeftButtonDown(sender, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
            }
        }
    }
}