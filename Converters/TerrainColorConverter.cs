using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WWXMapEditor.Converters
{
    public class TerrainColorConverter : IValueConverter
    {
        // Default sprite paths following the naming convention
        private const string SPRITES_BASE_PATH = "Resources/Sprites/Terrain/";
        private const string SPRITE_EXTENSION = ".png";
        private const int SPRITE_SIZE = 16;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string terrain)
            {
                // Try to load the sprite first
                var spritePath = GetSpritePath(terrain, parameter?.ToString());
                if (TryLoadSprite(spritePath, out var sprite))
                {
                    return sprite;
                }

                // Fallback to solid colors if sprite not found
                return GetFallbackBrush(terrain);
            }
            return GetFallbackBrush("default");
        }

        private string GetSpritePath(string terrainType, string variant = null)
        {
            // Naming scheme: terrain_[type]_[variant]_16x16.png
            // Example: terrain_plains_grass_16x16.png, terrain_mountain_rocky_16x16.png
            var baseName = $"terrain_{terrainType.ToLower()}";
            
            if (!string.IsNullOrEmpty(variant))
            {
                baseName += $"_{variant.ToLower()}";
            }
            else
            {
                // Use default variant if none specified
                baseName += "_default";
            }

            baseName += $"_{SPRITE_SIZE}x{SPRITE_SIZE}{SPRITE_EXTENSION}";
            
            return Path.Combine(SPRITES_BASE_PATH, baseName);
        }

        private bool TryLoadSprite(string path, out ImageBrush brush)
        {
            brush = null;
            
            try
            {
                // Try to load from application resources
                var uri = new Uri($"pack://application:,,,/{path}", UriKind.Absolute);
                var bitmap = new BitmapImage(uri);
                
                brush = new ImageBrush(bitmap)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new System.Windows.Rect(0, 0, SPRITE_SIZE, SPRITE_SIZE),
                    ViewportUnits = BrushMappingMode.Absolute,
                    Stretch = Stretch.None
                };
                
                return true;
            }
            catch
            {
                // Sprite not found, will use fallback
                return false;
            }
        }

        private SolidColorBrush GetFallbackBrush(string terrain)
        {
            var color = terrain?.ToLower() switch
            {
                "plains" => System.Windows.Media.Color.FromRgb(144, 238, 144),    // Light green
                "mountain" => System.Windows.Media.Color.FromRgb(139, 137, 137),  // Gray
                "forest" => System.Windows.Media.Color.FromRgb(34, 139, 34),      // Dark green
                "sand" => System.Windows.Media.Color.FromRgb(238, 203, 173),      // Sandy beige
                "sea" => System.Windows.Media.Color.FromRgb(64, 164, 223),        // Ocean blue
                "desert" => System.Windows.Media.Color.FromRgb(237, 201, 175),    // Desert tan
                "snow" => System.Windows.Media.Color.FromRgb(255, 250, 250),      // Snow white
                "swamp" => System.Windows.Media.Color.FromRgb(46, 125, 50),       // Swamp green
                "lava" => System.Windows.Media.Color.FromRgb(255, 87, 34),        // Lava orange
                "tundra" => System.Windows.Media.Color.FromRgb(176, 190, 197),    // Cold gray
                _ => System.Windows.Media.Color.FromRgb(128, 128, 128)           // Default gray
            };

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}