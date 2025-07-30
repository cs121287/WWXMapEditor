using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WwXMapEditor.Models;

namespace WwXMapEditor.Services
{
    public class SpriteManager
    {
        private static SpriteManager? _instance;
        public static SpriteManager Instance => _instance ??= new SpriteManager();

        private readonly Dictionary<string, ImageSource> _spriteCache = new();
        private readonly Dictionary<string, Color> _fallbackColors = new();
        private readonly Dictionary<string, string> _fallbackPatterns = new();

        private SpriteManager()
        {
            InitializeFallbackColors();
            InitializeFallbackPatterns();
        }

        private void InitializeFallbackColors()
        {
            // Terrain fallback colors
            _fallbackColors["Plain"] = Colors.LightGreen;
            _fallbackColors["Forest"] = Colors.DarkGreen;
            _fallbackColors["Mountain"] = Colors.Brown;
            _fallbackColors["Road"] = Colors.Gray;
            _fallbackColors["Bridge"] = Colors.DarkGray;
            _fallbackColors["Sea"] = Colors.DarkBlue;
            _fallbackColors["Beach"] = Colors.SandyBrown;
            _fallbackColors["River"] = Colors.CornflowerBlue;
            _fallbackColors["City"] = Colors.LightGray;
            _fallbackColors["Factory"] = Colors.SlateGray;
            _fallbackColors["HQ"] = Colors.Gold;
            _fallbackColors["Airport"] = Colors.DimGray;
            _fallbackColors["Port"] = Colors.Navy;

            // Unit fallback colors
            _fallbackColors["Infantry"] = Colors.DarkOliveGreen;
            _fallbackColors["Mechanized"] = Colors.OliveDrab;
            _fallbackColors["Tank"] = Colors.DarkSlateGray;
            _fallbackColors["HeavyTank"] = Colors.Black;
            _fallbackColors["Artillery"] = Colors.Maroon;
            _fallbackColors["RocketLauncher"] = Colors.DarkRed;
            _fallbackColors["AntiAir"] = Colors.Olive;
            _fallbackColors["TransportVehicle"] = Colors.Khaki;
            _fallbackColors["SupplyTruck"] = Colors.BurlyWood;
            _fallbackColors["Helicopter"] = Colors.DarkKhaki;
            _fallbackColors["Fighter"] = Colors.SkyBlue;
            _fallbackColors["Bomber"] = Colors.DarkSlateBlue;
            _fallbackColors["Stealth"] = Colors.SlateGray;
            _fallbackColors["TransportHelicopter"] = Colors.Tan;
            _fallbackColors["Battleship"] = Colors.DarkGray;
            _fallbackColors["Cruiser"] = Colors.Gray;
            _fallbackColors["Submarine"] = Colors.DarkCyan;
            _fallbackColors["NavalTransport"] = Colors.Teal;
            _fallbackColors["Carrier"] = Colors.LightSlateGray;
            _fallbackColors["Lander"] = Colors.CadetBlue;

            // Owner colors
            _fallbackColors["Player"] = Colors.Blue;
            _fallbackColors["Neutral"] = Colors.Gray;
            _fallbackColors["Computer"] = Colors.Red;

            // Weather overlay colors
            _fallbackColors["Rain"] = Color.FromArgb(64, 100, 100, 150);
            _fallbackColors["Snow"] = Color.FromArgb(96, 220, 220, 255);
            _fallbackColors["Fog"] = Color.FromArgb(128, 180, 180, 180);
            _fallbackColors["Storm"] = Color.FromArgb(96, 80, 80, 120);
            _fallbackColors["Sandstorm"] = Color.FromArgb(96, 200, 180, 100);
        }

        private void InitializeFallbackPatterns()
        {
            _fallbackPatterns["terrain"] = "solid";
            _fallbackPatterns["property"] = "building";
            _fallbackPatterns["unit"] = "circle";
        }

        public ImageSource GetSprite(string type, string variant, string layer, int size)
        {
            string cacheKey = $"{type}_{variant}_{layer}_{size}";

            if (_spriteCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            string fileName = GenerateSpriteFileName(type, variant, layer);
            string resourcePath = $"pack://application:,,,/Resources/Images/Tiles/{fileName}";

            try
            {
                Uri uri = new Uri(resourcePath, UriKind.Absolute);
                var bitmap = new BitmapImage(uri);

                if (bitmap.PixelWidth != size || bitmap.PixelHeight != size)
                {
                    var scaledBitmap = new TransformedBitmap(bitmap,
                        new ScaleTransform(size / (double)bitmap.PixelWidth, size / (double)bitmap.PixelHeight));
                    _spriteCache[cacheKey] = scaledBitmap;
                    return scaledBitmap;
                }

                _spriteCache[cacheKey] = bitmap;
                return bitmap;
            }
            catch
            {
                var fallbackSprite = GenerateFallbackSprite(type, variant, layer, size);
                _spriteCache[cacheKey] = fallbackSprite;
                return fallbackSprite;
            }
        }

        public ImageSource GetWeatherOverlay(WeatherType weather, int width, int height)
        {
            string cacheKey = $"weather_{weather}_{width}_{height}";

            if (_spriteCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var overlay = GenerateWeatherOverlay(weather, width, height);
            _spriteCache[cacheKey] = overlay;
            return overlay;
        }

        private ImageSource GenerateWeatherOverlay(WeatherType weather, int width, int height)
        {
            var drawingGroup = new DrawingGroup();
            var drawingContext = drawingGroup.Open();

            Color overlayColor = Colors.Transparent;
            switch (weather)
            {
                case WeatherType.Rain:
                    overlayColor = _fallbackColors["Rain"];
                    break;
                case WeatherType.Snow:
                    overlayColor = _fallbackColors["Snow"];
                    break;
                case WeatherType.Fog:
                    overlayColor = _fallbackColors["Fog"];
                    break;
                case WeatherType.Storm:
                    overlayColor = _fallbackColors["Storm"];
                    break;
                case WeatherType.Sandstorm:
                    overlayColor = _fallbackColors["Sandstorm"];
                    break;
            }

            if (overlayColor != Colors.Transparent)
            {
                var brush = new SolidColorBrush(overlayColor);
                brush.Freeze();
                drawingContext.DrawRectangle(brush, null, new Rect(0, 0, width, height));

                // Add weather effects
                if (weather == WeatherType.Rain || weather == WeatherType.Storm)
                {
                    DrawRainEffect(drawingContext, width, height);
                }
                else if (weather == WeatherType.Snow)
                {
                    DrawSnowEffect(drawingContext, width, height);
                }
            }

            drawingContext.Close();

            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();
            return drawingImage;
        }

        private void DrawRainEffect(DrawingContext dc, int width, int height)
        {
            var rainPen = new Pen(new SolidColorBrush(Color.FromArgb(64, 200, 200, 255)), 1);
            rainPen.Freeze();

            var random = new Random(42); // Fixed seed for consistent pattern
            for (int i = 0; i < 20; i++)
            {
                int x = random.Next(width);
                int y = random.Next(height);
                dc.DrawLine(rainPen, new Point(x, y), new Point(x - 5, y + 10));
            }
        }

        private void DrawSnowEffect(DrawingContext dc, int width, int height)
        {
            var snowBrush = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
            snowBrush.Freeze();

            var random = new Random(42); // Fixed seed for consistent pattern
            for (int i = 0; i < 30; i++)
            {
                int x = random.Next(width);
                int y = random.Next(height);
                dc.DrawEllipse(snowBrush, null, new Point(x, y), 2, 2);
            }
        }

        private string GenerateSpriteFileName(string type, string variant, string layer)
        {
            if (layer == "terrain")
                return $"{type}_{variant}.png";
            else if (layer == "property")
                return $"{type}_property.png";
            else if (layer == "unit")
                return $"{type}_{variant}.png";
            return $"{type}.png";
        }

        private ImageSource GenerateFallbackSprite(string type, string variant, string layer, int size)
        {
            var drawingGroup = new DrawingGroup();
            var drawingContext = drawingGroup.Open();

            Color baseColor = _fallbackColors.ContainsKey(type) ? _fallbackColors[type] : Colors.Gray;
            Color ownerColor = _fallbackColors.ContainsKey(variant) ? _fallbackColors[variant] : Colors.Gray;

            if (layer == "terrain")
            {
                DrawTerrainFallback(drawingContext, baseColor, size, type);
            }
            else if (layer == "property")
            {
                DrawPropertyFallback(drawingContext, baseColor, ownerColor, size, type);
            }
            else if (layer == "unit")
            {
                DrawUnitFallback(drawingContext, baseColor, ownerColor, size, type);
            }

            drawingContext.Close();

            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();
            return drawingImage;
        }

        private void DrawTerrainFallback(DrawingContext dc, Color color, int size, string type)
        {
            var rect = new Rect(0, 0, size, size);
            var brush = new SolidColorBrush(color);
            brush.Freeze();

            dc.DrawRectangle(brush, null, rect);

            // Add texture patterns for specific terrain types
            if (type == "Forest")
            {
                var treeBrush = new SolidColorBrush(Color.FromRgb(0, 80, 0));
                treeBrush.Freeze();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var treeRect = new Rect(i * size / 3 + size / 12, j * size / 3 + size / 12, size / 6, size / 6);
                        dc.DrawEllipse(treeBrush, null, new Point(treeRect.X + treeRect.Width / 2, treeRect.Y + treeRect.Height / 2), treeRect.Width / 2, treeRect.Height / 2);
                    }
                }
            }
            else if (type == "Mountain")
            {
                var mountainBrush = new SolidColorBrush(Color.FromRgb(101, 67, 33));
                mountainBrush.Freeze();
                var points = new PointCollection
                {
                    new Point(size / 2, size / 4),
                    new Point(size / 4, size * 3 / 4),
                    new Point(size * 3 / 4, size * 3 / 4)
                };
                dc.DrawGeometry(mountainBrush, null, new PathGeometry(new[] { new PathFigure(points[0], new[] { new LineSegment(points[1], true), new LineSegment(points[2], true) }, true) }));
            }
            else if (type == "Sea" || type == "River")
            {
                var waveBrush = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
                waveBrush.Freeze();
                var pen = new Pen(waveBrush, 2);
                pen.Freeze();
                for (int i = 0; i < 3; i++)
                {
                    var y = (i + 1) * size / 4;
                    dc.DrawLine(pen, new Point(0, y), new Point(size, y));
                }
            }
            else if (type == "Road" || type == "Bridge")
            {
                var roadBrush = new SolidColorBrush(Colors.DarkGray);
                roadBrush.Freeze();
                dc.DrawRectangle(roadBrush, null, new Rect(size / 3, 0, size / 3, size));

                if (type == "Road")
                {
                    var dashBrush = new SolidColorBrush(Colors.Yellow);
                    dashBrush.Freeze();
                    for (int i = 0; i < 4; i++)
                    {
                        dc.DrawRectangle(dashBrush, null, new Rect(size / 2 - 2, i * size / 3, 4, size / 6));
                    }
                }
            }
            else if (type == "Beach")
            {
                var sandBrush = new SolidColorBrush(Colors.SandyBrown);
                sandBrush.Freeze();
                dc.DrawRectangle(sandBrush, null, rect);

                var waveBrush = new SolidColorBrush(Color.FromArgb(128, 100, 150, 200));
                waveBrush.Freeze();
                dc.DrawRectangle(waveBrush, null, new Rect(0, 0, size / 3, size));
            }
        }

        private void DrawPropertyFallback(DrawingContext dc, Color propertyColor, Color ownerColor, int size, string type)
        {
            // Draw terrain base first
            if (type == "City" || type == "Factory" || type == "HQ" || type == "Airport" || type == "Port")
            {
                var terrainBrush = new SolidColorBrush(Colors.LightGray);
                terrainBrush.Freeze();
                dc.DrawRectangle(terrainBrush, null, new Rect(0, 0, size, size));
            }

            // Draw building
            var baseRect = new Rect(size / 4, size / 3, size / 2, size / 2);
            var baseBrush = new SolidColorBrush(propertyColor);
            baseBrush.Freeze();
            dc.DrawRectangle(baseBrush, null, baseRect);

            // Draw property-specific features
            if (type == "HQ")
            {
                var flagPole = new Rect(size / 2 - 1, size / 6, 2, size / 3);
                var poleBrush = new SolidColorBrush(Colors.Black);
                poleBrush.Freeze();
                dc.DrawRectangle(poleBrush, null, flagPole);

                var flagRect = new Rect(size / 2 + 1, size / 6, size / 4, size / 6);
                var flagBrush = new SolidColorBrush(ownerColor);
                flagBrush.Freeze();
                dc.DrawRectangle(flagBrush, null, flagRect);
            }
            else if (type == "Factory")
            {
                var chimneyRect = new Rect(size * 3 / 5, size / 4, size / 8, size / 3);
                var chimneyBrush = new SolidColorBrush(Colors.DarkRed);
                chimneyBrush.Freeze();
                dc.DrawRectangle(chimneyBrush, null, chimneyRect);
            }
            else if (type == "Airport")
            {
                var runwayBrush = new SolidColorBrush(Colors.Black);
                runwayBrush.Freeze();
                dc.DrawRectangle(runwayBrush, null, new Rect(0, size * 2 / 3, size, size / 6));
            }
            else if (type == "Port")
            {
                var waterBrush = new SolidColorBrush(Colors.LightBlue);
                waterBrush.Freeze();
                dc.DrawRectangle(waterBrush, null, new Rect(0, size * 2 / 3, size, size / 3));
            }

            // Draw owner indicator
            var ownerRect = new Rect(size / 3, size / 2, size / 3, size / 6);
            var ownerBrush = new SolidColorBrush(ownerColor);
            ownerBrush.Freeze();
            dc.DrawRectangle(ownerBrush, null, ownerRect);
        }

        private void DrawUnitFallback(DrawingContext dc, Color unitColor, Color ownerColor, int size, string type)
        {
            var center = new Point(size / 2, size / 2);
            var unitBrush = new SolidColorBrush(unitColor);
            unitBrush.Freeze();
            var ownerBrush = new SolidColorBrush(ownerColor);
            ownerBrush.Freeze();
            var outlinePen = new Pen(Brushes.Black, 1);
            outlinePen.Freeze();

            // Draw unit based on movement type
            if (type.Contains("Infantry") || type.Contains("Mechanized"))
            {
                dc.DrawEllipse(unitBrush, outlinePen, center, size / 3, size / 3);
            }
            else if (type.Contains("Tank") || type.Contains("Artillery") || type.Contains("AntiAir"))
            {
                var rect = new Rect(size / 4, size / 4, size / 2, size / 2);
                dc.DrawRectangle(unitBrush, outlinePen, rect);
            }
            else if (type.Contains("Fighter") || type.Contains("Bomber") || type.Contains("Helicopter") || type.Contains("Stealth"))
            {
                var points = new PointCollection
                {
                    new Point(size / 2, size / 4),
                    new Point(size / 4, size * 3 / 4),
                    new Point(size * 3 / 4, size * 3 / 4)
                };
                dc.DrawGeometry(unitBrush, outlinePen, new PathGeometry(new[] { new PathFigure(points[0], new[] { new LineSegment(points[1], true), new LineSegment(points[2], true) }, true) }));
            }
            else if (type.Contains("Ship") || type.Contains("Naval") || type.Contains("Battleship") || type.Contains("Cruiser") || type.Contains("Submarine") || type.Contains("Carrier") || type.Contains("Lander"))
            {
                var points = new PointCollection();
                for (int i = 0; i < 6; i++)
                {
                    var angle = i * Math.PI / 3;
                    points.Add(new Point(
                        center.X + size / 3 * Math.Cos(angle),
                        center.Y + size / 3 * Math.Sin(angle)
                    ));
                }
                var segments = new List<LineSegment>();
                for (int i = 1; i < points.Count; i++)
                {
                    segments.Add(new LineSegment(points[i], true));
                }
                dc.DrawGeometry(unitBrush, outlinePen, new PathGeometry(new[] { new PathFigure(points[0], segments, true) }));
            }
            else if (type.Contains("Transport") || type.Contains("Supply"))
            {
                var rect = new Rect(size / 5, size / 3, size * 3 / 5, size / 3);
                dc.DrawRectangle(unitBrush, outlinePen, rect);
            }

            // Draw owner indicator
            dc.DrawEllipse(ownerBrush, null, center, size / 8, size / 8);
        }

        public void ClearCache()
        {
            _spriteCache.Clear();
        }

        public void PreloadSprites(IEnumerable<(string type, string variant, string layer)> sprites, int size)
        {
            foreach (var (type, variant, layer) in sprites)
            {
                GetSprite(type, variant, layer, size);
            }
        }
    }
}