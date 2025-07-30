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

        public event EventHandler<Point>? ViewportChangeRequested;

        public MiniMapControl()
        {
            InitializeComponent();
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

        private void UpdateMiniMap()
        {
            if (Map == null) return;

            var bitmap = new WriteableBitmap(Map.Width, Map.Height, 96, 96, PixelFormats.Pbgra32, null);
            bitmap.Lock();

            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    var tile = Map.TileArray[x, y];
                    if (tile != null)
                    {
                        var color = GetTerrainColor(tile.Terrain);
                        var pixelData = new byte[] { color.B, color.G, color.R, color.A };
                        bitmap.WritePixels(new Int32Rect(x, y, 1, 1), pixelData, 4, 0);
                    }
                }
            }

            bitmap.Unlock();
            MiniMapImage.Source = bitmap;
            UpdateViewportIndicator();
        }

        private void UpdateViewportIndicator()
        {
            if (Map == null) return;

            var scaleX = MiniMapImage.ActualWidth / Map.Width;
            var scaleY = MiniMapImage.ActualHeight / Map.Height;

            ViewportRect.Width = ViewportWidth * scaleX;
            ViewportRect.Height = ViewportHeight * scaleY;
            Canvas.SetLeft(ViewportRect, ViewportX * scaleX);
            Canvas.SetTop(ViewportRect, ViewportY * scaleY);
        }

        private Color GetTerrainColor(string terrain)
        {
            return terrain switch
            {
                "Plain" => Colors.LightGreen,
                "Forest" => Colors.DarkGreen,
                "Mountain" => Colors.Brown,
                "Sea" => Colors.DarkBlue,
                "Beach" => Colors.SandyBrown,
                "Water" => Colors.LightBlue,
                "Road" => Colors.Gray,
                "River" => Colors.CornflowerBlue,
                _ => Colors.LightGray
            };
        }

        private void MiniMapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Map == null) return;

            var pos = e.GetPosition(MiniMapImage);
            var scaleX = Map.Width / MiniMapImage.ActualWidth;
            var scaleY = Map.Height / MiniMapImage.ActualHeight;

            var mapX = (int)(pos.X * scaleX) - ViewportWidth / 2;
            var mapY = (int)(pos.Y * scaleY) - ViewportHeight / 2;

            mapX = Math.Max(0, Math.Min(mapX, Map.Width - ViewportWidth));
            mapY = Math.Max(0, Math.Min(mapY, Map.Height - ViewportHeight));

            ViewportChangeRequested?.Invoke(this, new Point(mapX, mapY));
        }
    }
}