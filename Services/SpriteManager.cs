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

        private readonly Dictionary<string, BitmapSource> _spriteCache = new();
        private readonly Dictionary<string, Color> _fallbackColors = new();

        // Sprite sheets for each terrain type
        private readonly Dictionary<string, BitmapSource?> _terrainSheets = new();
        private readonly Dictionary<string, BitmapSource?> _propertySprites = new();
        private readonly Dictionary<string, BitmapSource?> _unitSprites = new();

        private const int SPRITE_SIZE = 32;
        private const int SPRITES_PER_ROW = 2;
        private const int SPRITES_PER_COLUMN = 4;
        private const int SPRITES_PER_SHEET = 8;

        private SpriteManager()
        {
            InitializeFallbackColors();
            LoadSpriteSheets();
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

            // Property owner colors for fallback
            _fallbackColors["Property_Player"] = Colors.Blue;
            _fallbackColors["Property_Neutral"] = Colors.Gray;
            _fallbackColors["Property_Computer"] = Colors.Red;

            // Unit owner colors for fallback
            _fallbackColors["Unit_Player"] = Colors.DarkBlue;
            _fallbackColors["Unit_Neutral"] = Colors.DarkGray;
            _fallbackColors["Unit_Computer"] = Colors.DarkRed;

            // Specific unit fallback colors
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
        }

        private void LoadSpriteSheets()
        {
            try
            {
                // Load terrain sprite sheets for each terrain type
                var terrainTypes = Enum.GetValues(typeof(TerrainType));
                foreach (TerrainType terrain in terrainTypes)
                {
                    string terrainName = terrain.ToString().ToLower();

                    // Load summer sheet
                    var summerKey = $"{terrainName}_summer";
                    var summerPath = $"Resources/Sprites/{terrainName}_summer.png";
                    _terrainSheets[summerKey] = LoadImage(summerPath);

                    // Load winter sheet
                    var winterKey = $"{terrainName}_winter";
                    var winterPath = $"Resources/Sprites/{terrainName}_winter.png";
                    _terrainSheets[winterKey] = LoadImage(winterPath);
                }

                // Load property sprites for each owner type
                LoadPropertySprites("City");
                LoadPropertySprites("Factory");
                LoadPropertySprites("HQ");
                LoadPropertySprites("Airport");
                LoadPropertySprites("Port");

                // Load unit sprites for each owner type
                LoadUnitSprites();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sprite sheets: {ex.Message}");
            }
        }

        private void LoadPropertySprites(string propertyType)
        {
            string[] owners = { "Player", "Neutral", "Computer" };
            string[] seasons = { "Summer", "Winter" };

            foreach (var owner in owners)
            {
                foreach (var season in seasons)
                {
                    var key = $"{propertyType}_{owner}_{season}";
                    var path = $"Resources/Sprites/{propertyType.ToLower()}_{owner.ToLower()}_{season.ToLower()}.png";
                    _propertySprites[key] = LoadImage(path);
                }
            }
        }

        private void LoadUnitSprites()
        {
            var unitTypes = Enum.GetNames(typeof(UnitType));
            string[] owners = { "Player", "Neutral", "Computer" };
            string[] seasons = { "Summer", "Winter" };

            foreach (var unitType in unitTypes)
            {
                foreach (var owner in owners)
                {
                    foreach (var season in seasons)
                    {
                        var key = $"{unitType}_{owner}_{season}";
                        var path = $"Resources/Sprites/{unitType.ToLower()}_{owner.ToLower()}_{season.ToLower()}.png";
                        _unitSprites[key] = LoadImage(path);
                    }
                }
            }
        }

        private BitmapSource? LoadImage(string path)
        {
            try
            {
                var uri = new Uri(path, UriKind.Relative);
                var bitmap = new BitmapImage(uri);

                // Force load the image to check if it exists
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                return bitmap;
            }
            catch
            {
                // Return null if image doesn't exist or can't be loaded
                return null;
            }
        }

        public BitmapSource GetTerrainSprite(TerrainType terrain, string season, int spriteIndex)
        {
            // Validate and normalize sprite index
            int validSpriteIndex = ValidateSpriteIndex(spriteIndex);

            // Check cache first
            var cacheKey = $"terrain_{terrain}_{season}_{validSpriteIndex}";
            if (_spriteCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            // Get the appropriate sprite sheet
            var sheetKey = $"{terrain.ToString().ToLower()}_{season.ToLower()}";

            if (_terrainSheets.TryGetValue(sheetKey, out var sheet) && sheet != null)
            {
                // Extract sprite from sheet
                var sprite = ExtractSpriteFromSheet(sheet, validSpriteIndex);
                _spriteCache[cacheKey] = sprite;
                return sprite;
            }

            // No sprite sheet loaded, create fallback
            var fallback = CreateFallbackSprite(terrain.ToString(), validSpriteIndex);
            _spriteCache[cacheKey] = fallback;
            return fallback;
        }

        public BitmapSource GetTerrainSpriteSheet(TerrainType terrain, string season)
        {
            var sheetKey = $"{terrain.ToString().ToLower()}_{season.ToLower()}";

            if (_terrainSheets.TryGetValue(sheetKey, out var sheet) && sheet != null)
            {
                return sheet;
            }

            // Return fallback sprite sheet
            return CreateFallbackSpriteSheet(terrain);
        }

        public BitmapSource GetPropertySprite(PropertyType propertyType, string owner, string season)
        {
            // Check cache first
            var cacheKey = $"property_{propertyType}_{owner}_{season}";
            if (_spriteCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var key = $"{propertyType}_{owner}_{season}";

            if (_propertySprites.TryGetValue(key, out var sprite) && sprite != null)
            {
                _spriteCache[cacheKey] = sprite;
                return sprite;
            }

            // Return fallback sprite with owner-specific color overlay
            var baseColor = _fallbackColors.GetValueOrDefault(propertyType.ToString(), Colors.Gray);
            var ownerColor = _fallbackColors.GetValueOrDefault($"Property_{owner}", Colors.Gray);
            var fallback = CreatePropertyFallbackSprite(propertyType.ToString(), baseColor, ownerColor);
            _spriteCache[cacheKey] = fallback;
            return fallback;
        }

        public BitmapSource GetUnitSprite(UnitType unitType, string owner, string season)
        {
            // Check cache first
            var cacheKey = $"unit_{unitType}_{owner}_{season}";
            if (_spriteCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var key = $"{unitType}_{owner}_{season}";

            if (_unitSprites.TryGetValue(key, out var sprite) && sprite != null)
            {
                _spriteCache[cacheKey] = sprite;
                return sprite;
            }

            // Return fallback sprite with unit-specific color and owner indicator
            var unitColor = _fallbackColors.GetValueOrDefault(unitType.ToString(), Colors.DarkGray);
            var ownerColor = _fallbackColors.GetValueOrDefault($"Unit_{owner}", Colors.DarkGray);
            var fallback = CreateUnitFallbackSprite(unitType.ToString(), unitColor, ownerColor);
            _spriteCache[cacheKey] = fallback;
            return fallback;
        }

        private int ValidateSpriteIndex(int spriteIndex)
        {
            // If sprite index is invalid (negative or out of bounds), default to 0
            if (spriteIndex < 0 || spriteIndex >= SPRITES_PER_SHEET)
            {
                return 0;
            }
            return spriteIndex;
        }

        private BitmapSource ExtractSpriteFromSheet(BitmapSource sheet, int spriteIndex)
        {
            // Ensure index is within bounds (already validated, but double-check)
            spriteIndex = ValidateSpriteIndex(spriteIndex);

            // Calculate row and column in 2x4 grid
            int col = spriteIndex % SPRITES_PER_ROW;
            int row = spriteIndex / SPRITES_PER_ROW;

            var cropRect = new Int32Rect(col * SPRITE_SIZE, row * SPRITE_SIZE, SPRITE_SIZE, SPRITE_SIZE);

            try
            {
                var croppedBitmap = new CroppedBitmap(sheet, cropRect);
                return croppedBitmap;
            }
            catch
            {
                return CreateFallbackSprite("Error", 0, Colors.Red);
            }
        }

        public int GetDefaultTerrainIndex(TerrainType terrain)
        {
            // Default to first sprite (index 0)
            return 0;
        }

        private BitmapSource CreateFallbackSprite(string type, int index = 0, Color? overrideColor = null)
        {
            var color = overrideColor ?? _fallbackColors.GetValueOrDefault(type, Colors.Gray);

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                // Draw base color
                context.DrawRectangle(new SolidColorBrush(color), null, new Rect(0, 0, SPRITE_SIZE, SPRITE_SIZE));

                // Add border for better visibility
                var borderPen = new Pen(new SolidColorBrush(Colors.Black), 1);
                context.DrawRectangle(null, borderPen, new Rect(0.5, 0.5, SPRITE_SIZE - 1, SPRITE_SIZE - 1));

                // Add a simple pattern or text to distinguish different types
                var textBrush = new SolidColorBrush(GetContrastColor(color));
                var typeface = new Typeface("Arial");

                // Show type abbreviation
                var typeText = type.Length > 3 ? type.Substring(0, 3) : type;
                var formattedText = new FormattedText(
                    typeText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    10,
                    textBrush,
                    VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                formattedText.TextAlignment = TextAlignment.Center;
                context.DrawText(formattedText, new Point(SPRITE_SIZE / 2, SPRITE_SIZE / 2 - 8));

                // Show sprite index
                var indexText = new FormattedText(
                    index.ToString(),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    8,
                    textBrush,
                    VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                indexText.TextAlignment = TextAlignment.Center;
                context.DrawText(indexText, new Point(SPRITE_SIZE / 2, SPRITE_SIZE / 2 + 2));
            }

            var bitmap = new RenderTargetBitmap(SPRITE_SIZE, SPRITE_SIZE, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            return bitmap;
        }

        private BitmapSource CreatePropertyFallbackSprite(string type, Color baseColor, Color ownerColor)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                // Draw base terrain
                context.DrawRectangle(new SolidColorBrush(Colors.LightGray), null, new Rect(0, 0, SPRITE_SIZE, SPRITE_SIZE));

                // Draw building
                var buildingRect = new Rect(SPRITE_SIZE / 4, SPRITE_SIZE / 3, SPRITE_SIZE / 2, SPRITE_SIZE / 2);
                context.DrawRectangle(new SolidColorBrush(baseColor), new Pen(Brushes.Black, 1), buildingRect);

                // Draw owner flag/indicator
                var flagRect = new Rect(SPRITE_SIZE / 3, SPRITE_SIZE / 2, SPRITE_SIZE / 3, SPRITE_SIZE / 6);
                context.DrawRectangle(new SolidColorBrush(ownerColor), new Pen(Brushes.Black, 1), flagRect);

                // Add property type indicator
                var textBrush = new SolidColorBrush(GetContrastColor(baseColor));
                var typeface = new Typeface("Arial");
                var formattedText = new FormattedText(
                    type.Substring(0, 1),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    8,
                    textBrush,
                    VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                formattedText.TextAlignment = TextAlignment.Center;
                context.DrawText(formattedText, new Point(SPRITE_SIZE / 2, SPRITE_SIZE / 3));
            }

            var bitmap = new RenderTargetBitmap(SPRITE_SIZE, SPRITE_SIZE, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            return bitmap;
        }

        private BitmapSource CreateUnitFallbackSprite(string type, Color unitColor, Color ownerColor)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                // Draw transparent background
                context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, SPRITE_SIZE, SPRITE_SIZE));

                // Draw unit shape based on type
                var unitBrush = new SolidColorBrush(unitColor);
                var outlinePen = new Pen(Brushes.Black, 1);

                if (type.Contains("Infantry") || type.Contains("Mechanized"))
                {
                    // Draw circle for infantry
                    context.DrawEllipse(unitBrush, outlinePen, new Point(SPRITE_SIZE / 2, SPRITE_SIZE / 2), SPRITE_SIZE / 3, SPRITE_SIZE / 3);
                }
                else if (type.Contains("Tank") || type.Contains("Artillery") || type.Contains("AntiAir"))
                {
                    // Draw rectangle for vehicles
                    var rect = new Rect(SPRITE_SIZE / 4, SPRITE_SIZE / 4, SPRITE_SIZE / 2, SPRITE_SIZE / 2);
                    context.DrawRectangle(unitBrush, outlinePen, rect);
                }
                else if (type.Contains("Fighter") || type.Contains("Bomber") || type.Contains("Helicopter") || type.Contains("Stealth"))
                {
                    // Draw triangle for air units
                    var points = new Point[]
                    {
                        new Point(SPRITE_SIZE / 2, SPRITE_SIZE / 4),
                        new Point(SPRITE_SIZE / 4, SPRITE_SIZE * 3 / 4),
                        new Point(SPRITE_SIZE * 3 / 4, SPRITE_SIZE * 3 / 4)
                    };
                    var figure = new PathFigure(points[0], new[] { new LineSegment(points[1], true), new LineSegment(points[2], true) }, true);
                    var geometry = new PathGeometry(new[] { figure });
                    context.DrawGeometry(unitBrush, outlinePen, geometry);
                }
                else if (type.Contains("Ship") || type.Contains("Naval") || type.Contains("Battleship") || type.Contains("Cruiser") || type.Contains("Submarine") || type.Contains("Carrier") || type.Contains("Lander"))
                {
                    // Draw hexagon for naval units
                    var points = new Point[6];
                    for (int i = 0; i < 6; i++)
                    {
                        var angle = i * Math.PI / 3;
                        points[i] = new Point(
                            SPRITE_SIZE / 2 + SPRITE_SIZE / 3 * Math.Cos(angle),
                            SPRITE_SIZE / 2 + SPRITE_SIZE / 3 * Math.Sin(angle)
                        );
                    }
                    var segments = new LineSegment[5];
                    for (int i = 0; i < 5; i++)
                    {
                        segments[i] = new LineSegment(points[i + 1], true);
                    }
                    var figure = new PathFigure(points[0], segments, true);
                    var geometry = new PathGeometry(new[] { figure });
                    context.DrawGeometry(unitBrush, outlinePen, geometry);
                }
                else
                {
                    // Default rectangle for other units
                    var rect = new Rect(SPRITE_SIZE / 4, SPRITE_SIZE / 4, SPRITE_SIZE / 2, SPRITE_SIZE / 2);
                    context.DrawRectangle(unitBrush, outlinePen, rect);
                }

                // Draw owner indicator (small circle in center)
                context.DrawEllipse(new SolidColorBrush(ownerColor), outlinePen, new Point(SPRITE_SIZE / 2, SPRITE_SIZE / 2), SPRITE_SIZE / 8, SPRITE_SIZE / 8);

                // Add unit type initial
                var textBrush = new SolidColorBrush(GetContrastColor(ownerColor));
                var typeface = new Typeface("Arial");
                var formattedText = new FormattedText(
                    type.Substring(0, 1),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    7,
                    textBrush,
                    VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                formattedText.TextAlignment = TextAlignment.Center;
                context.DrawText(formattedText, new Point(SPRITE_SIZE / 2, SPRITE_SIZE / 2 - 4));
            }

            var bitmap = new RenderTargetBitmap(SPRITE_SIZE, SPRITE_SIZE, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            return bitmap;
        }

        private BitmapSource CreateFallbackSpriteSheet(TerrainType terrain)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var color = _fallbackColors.GetValueOrDefault(terrain.ToString(), Colors.Gray);

                // Create a 2x4 grid of sprites
                for (int i = 0; i < SPRITES_PER_SHEET; i++)
                {
                    int col = i % SPRITES_PER_ROW;
                    int row = i / SPRITES_PER_ROW;

                    var rect = new Rect(col * SPRITE_SIZE, row * SPRITE_SIZE, SPRITE_SIZE, SPRITE_SIZE);

                    // Vary the shade slightly for each sprite
                    var spriteColor = Color.FromRgb(
                        (byte)Math.Min(255, color.R + (i * 10)),
                        (byte)Math.Min(255, color.G + (i * 10)),
                        (byte)Math.Min(255, color.B + (i * 10))
                    );

                    context.DrawRectangle(new SolidColorBrush(spriteColor), null, rect);

                    // Add border
                    var borderPen = new Pen(new SolidColorBrush(Colors.Black), 1);
                    context.DrawRectangle(null, borderPen, new Rect(col * SPRITE_SIZE + 0.5, row * SPRITE_SIZE + 0.5, SPRITE_SIZE - 1, SPRITE_SIZE - 1));

                    // Add text
                    var textBrush = new SolidColorBrush(GetContrastColor(spriteColor));
                    var typeface = new Typeface("Arial");

                    // Show terrain type abbreviation
                    var terrainText = terrain.ToString().Substring(0, Math.Min(3, terrain.ToString().Length));
                    var formattedText = new FormattedText(
                        terrainText,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        8,
                        textBrush,
                        VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                    formattedText.TextAlignment = TextAlignment.Center;
                    context.DrawText(formattedText, new Point(col * SPRITE_SIZE + SPRITE_SIZE / 2, row * SPRITE_SIZE + SPRITE_SIZE / 2 - 8));

                    // Show sprite index
                    var indexText = new FormattedText(
                        i.ToString(),
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        6,
                        textBrush,
                        VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                    indexText.TextAlignment = TextAlignment.Center;
                    context.DrawText(indexText, new Point(col * SPRITE_SIZE + SPRITE_SIZE / 2, row * SPRITE_SIZE + SPRITE_SIZE / 2 + 2));
                }
            }

            var bitmap = new RenderTargetBitmap(SPRITES_PER_ROW * SPRITE_SIZE, SPRITES_PER_COLUMN * SPRITE_SIZE, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            return bitmap;
        }

        private Color GetContrastColor(Color color)
        {
            // Calculate perceived brightness
            double brightness = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255;
            return brightness > 0.5 ? Colors.Black : Colors.White;
        }

        public void ClearCache()
        {
            _spriteCache.Clear();
        }
    }
}