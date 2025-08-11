using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WWXMapEditor.Models
{
    /// <summary>
    /// Represents a terrain sprite with its metadata
    /// </summary>
    public class TerrainSprite
    {
        public string TerrainType { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public ImageSource? Image { get; set; }
        public bool IsDefault { get; set; }
        public string DisplayName => $"{TerrainType} - {VariantName}";
    }

    /// <summary>
    /// Manages terrain sprites and their organization
    /// </summary>
    public class TerrainSpriteManager
    {
        private static TerrainSpriteManager? _instance;
        public static TerrainSpriteManager Instance => _instance ??= new TerrainSpriteManager();

        // Sprite directory structure
        public const string SPRITES_ROOT = "Resources/Sprites/";
        public const string TERRAIN_SPRITES_DIR = "Resources/Sprites/Terrain/";
        public const string CUSTOM_SPRITES_DIR = "CustomSprites/Terrain/";
        
        // Sprite naming convention
        public const string SPRITE_PREFIX = "terrain_";
        public const string SPRITE_SIZE = "16x16";
        public const string SPRITE_EXTENSION = ".png";

        private Dictionary<string, List<TerrainSprite>> _terrainSprites;
        private Dictionary<string, System.Drawing.Color> _fallbackColors;

        public TerrainSpriteManager()
        {
            _terrainSprites = new Dictionary<string, List<TerrainSprite>>();
            InitializeFallbackColors();
            LoadDefaultSprites();
            LoadCustomSprites();
        }

        private void InitializeFallbackColors()
        {
            _fallbackColors = new Dictionary<string, System.Drawing.Color>
            {
                ["plains"] = System.Drawing.Color.FromArgb(144, 238, 144),
                ["mountain"] = System.Drawing.Color.FromArgb(139, 137, 137),
                ["forest"] = System.Drawing.Color.FromArgb(34, 139, 34),
                ["sand"] = System.Drawing.Color.FromArgb(238, 203, 173),
                ["sea"] = System.Drawing.Color.FromArgb(64, 164, 223),
                ["desert"] = System.Drawing.Color.FromArgb(237, 201, 175),
                ["snow"] = System.Drawing.Color.FromArgb(255, 250, 250),
                ["swamp"] = System.Drawing.Color.FromArgb(46, 125, 50),
                ["lava"] = System.Drawing.Color.FromArgb(255, 87, 34),
                ["tundra"] = System.Drawing.Color.FromArgb(176, 190, 197),
                ["road"] = System.Drawing.Color.FromArgb(105, 105, 105),
                ["bridge"] = System.Drawing.Color.FromArgb(139, 69, 19),
                ["city"] = System.Drawing.Color.FromArgb(192, 192, 192),
                ["ruins"] = System.Drawing.Color.FromArgb(128, 128, 128)
            };
        }

        private void LoadDefaultSprites()
        {
            // Default sprite variants for each terrain type
            var defaultVariants = new Dictionary<string, string[]>
            {
                ["plains"] = new[] { "grass", "meadow", "field", "savanna" },
                ["mountain"] = new[] { "rocky", "peaks", "cliffs", "volcanic" },
                ["forest"] = new[] { "deciduous", "pine", "jungle", "autumn" },
                ["sand"] = new[] { "beach", "dunes", "coast", "shore" },
                ["sea"] = new[] { "ocean", "shallow", "deep", "coral" },
                ["desert"] = new[] { "sandy", "rocky", "oasis", "dunes" },
                ["snow"] = new[] { "fresh", "packed", "ice", "glacier" },
                ["swamp"] = new[] { "marsh", "bog", "wetland", "mangrove" },
                ["road"] = new[] { "dirt", "paved", "cobblestone", "ancient" },
                ["bridge"] = new[] { "wooden", "stone", "steel", "rope" }
            };

            foreach (var terrainType in defaultVariants)
            {
                var sprites = new List<TerrainSprite>();
                
                // Add default variant first
                var defaultSprite = CreateTerrainSprite(terrainType.Key, "default", true);
                if (defaultSprite != null)
                {
                    sprites.Add(defaultSprite);
                }

                // Add other variants
                foreach (var variant in terrainType.Value)
                {
                    var sprite = CreateTerrainSprite(terrainType.Key, variant, false);
                    if (sprite != null)
                    {
                        sprites.Add(sprite);
                    }
                }

                _terrainSprites[terrainType.Key] = sprites;
            }
        }

        private void LoadCustomSprites()
        {
            // Get the application data folder
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WWXMapEditor",
                CUSTOM_SPRITES_DIR
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
                return;
            }

            // Load custom sprites from user directory
            var customFiles = Directory.GetFiles(appDataPath, $"{SPRITE_PREFIX}*{SPRITE_EXTENSION}");
            
            foreach (var file in customFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (ParseSpriteFileName(fileName, out var terrainType, out var variant))
                {
                    var sprite = new TerrainSprite
                    {
                        TerrainType = terrainType,
                        VariantName = variant + " (Custom)",
                        FileName = Path.GetFileName(file),
                        FullPath = file,
                        IsDefault = false
                    };

                    try
                    {
                        sprite.Image = new BitmapImage(new Uri(file));
                        
                        if (!_terrainSprites.ContainsKey(terrainType))
                        {
                            _terrainSprites[terrainType] = new List<TerrainSprite>();
                        }
                        
                        _terrainSprites[terrainType].Add(sprite);
                    }
                    catch
                    {
                        // Skip invalid image files
                    }
                }
            }
        }

        private TerrainSprite? CreateTerrainSprite(string terrainType, string variant, bool isDefault)
        {
            var fileName = $"{SPRITE_PREFIX}{terrainType}_{variant}_{SPRITE_SIZE}{SPRITE_EXTENSION}";
            var path = Path.Combine(TERRAIN_SPRITES_DIR, fileName);
            
            try
            {
                var uri = new Uri($"pack://application:,,,/{path}", UriKind.Absolute);
                var image = new BitmapImage(uri);
                
                return new TerrainSprite
                {
                    TerrainType = terrainType,
                    VariantName = variant,
                    FileName = fileName,
                    FullPath = path,
                    Image = image,
                    IsDefault = isDefault
                };
            }
            catch
            {
                // Sprite file doesn't exist, return null
                return null;
            }
        }

        private bool ParseSpriteFileName(string fileName, out string terrainType, out string variant)
        {
            terrainType = string.Empty;
            variant = string.Empty;

            if (!fileName.StartsWith(SPRITE_PREFIX))
                return false;

            var parts = fileName.Substring(SPRITE_PREFIX.Length).Split('_');
            if (parts.Length >= 3)
            {
                terrainType = parts[0];
                variant = parts[1];
                return true;
            }

            return false;
        }

        public List<TerrainSprite> GetSpritesForTerrain(string terrainType)
        {
            var key = terrainType.ToLower();
            return _terrainSprites.ContainsKey(key) 
                ? _terrainSprites[key] 
                : new List<TerrainSprite>();
        }

        public TerrainSprite? GetDefaultSprite(string terrainType)
        {
            var sprites = GetSpritesForTerrain(terrainType);
            return sprites.FirstOrDefault(s => s.IsDefault) ?? sprites.FirstOrDefault();
        }

        public System.Drawing.Color GetFallbackColor(string terrainType)
        {
            var key = terrainType.ToLower();
            return _fallbackColors.ContainsKey(key) 
                ? _fallbackColors[key] 
                : System.Drawing.Color.FromArgb(128, 128, 128);
        }

        public ImageBrush CreateTerrainBrush(string terrainType, string? variant = null)
        {
            TerrainSprite? sprite = null;
            
            if (!string.IsNullOrEmpty(variant))
            {
                var sprites = GetSpritesForTerrain(terrainType);
                sprite = sprites.FirstOrDefault(s => s.VariantName.Equals(variant, StringComparison.OrdinalIgnoreCase));
            }
            
            sprite ??= GetDefaultSprite(terrainType);
            
            if (sprite?.Image != null)
            {
                return new ImageBrush(sprite.Image)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new System.Windows.Rect(0, 0, 16, 16),
                    ViewportUnits = BrushMappingMode.Absolute,
                    Stretch = Stretch.None
                };
            }
            
            // Return solid color brush as fallback
            return new ImageBrush
            {
                TileMode = TileMode.None,
                Stretch = Stretch.Fill
            };
        }

        public void RefreshCustomSprites()
        {
            // Clear existing custom sprites
            foreach (var sprites in _terrainSprites.Values)
            {
                sprites.RemoveAll(s => s.VariantName.Contains("(Custom)"));
            }
            
            // Reload custom sprites
            LoadCustomSprites();
        }
    }
}