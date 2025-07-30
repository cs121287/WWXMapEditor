using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WwXMapEditor.Models;

namespace WwXMapEditor.Services
{
    public static class MapService
    {
        private const int MIN_MAP_SIZE = 10;
        private const int MAX_MAP_SIZE = 2000;
        private const long MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB limit
        private const string COMPRESSED_EXTENSION = ".wwxz"; // Compressed map format
        private const string JSON_EXTENSION = ".json";

        // Optimized map format for smaller file sizes
        public class OptimizedMapData
        {
            public string Name { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string Season { get; set; }
            public WeatherType Weather { get; set; }
            public bool FogOfWarEnabled { get; set; }
            public List<OptimizedTile> Tiles { get; set; } // Only non-default tiles
            public List<Property> Properties { get; set; }
            public List<Unit> Units { get; set; }
            public List<Player> Players { get; set; }
            public VictoryConditions VictoryConditions { get; set; }
            public MapMetadata Metadata { get; set; }
            public TerrainType DefaultTerrain { get; set; } = TerrainType.Plain;
        }

        public class OptimizedTile
        {
            public int X { get; set; }
            public int Y { get; set; }
            public TerrainType T { get; set; } // Short property name
            public bool? Tr { get; set; } // Traversable - only if not default (true)
            public int? S { get; set; } // SpriteIndex - only if not default (0)
        }

        public static void SaveMap(Map map, string filename, bool useCompression = true)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map), "Map cannot be null");
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Filename cannot be null or empty", nameof(filename));
            }

            try
            {
                // Validate map before saving
                ValidateMapForSave(map);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Determine if we should use compression based on extension or parameter
                bool shouldCompress = useCompression ||
                                    Path.GetExtension(filename).Equals(COMPRESSED_EXTENSION, StringComparison.OrdinalIgnoreCase);

                // Create backup if file already exists
                string? backupFile = null;
                if (File.Exists(filename))
                {
                    backupFile = filename + ".backup";
                    File.Copy(filename, backupFile, true);
                }

                try
                {
                    if (shouldCompress)
                    {
                        SaveCompressedMap(map, filename);
                    }
                    else
                    {
                        SaveJsonMap(map, filename);
                    }

                    // Delete backup on successful save
                    if (backupFile != null && File.Exists(backupFile))
                    {
                        File.Delete(backupFile);
                    }
                }
                catch
                {
                    // Restore from backup on failure
                    if (backupFile != null && File.Exists(backupFile))
                    {
                        File.Copy(backupFile, filename, true);
                        File.Delete(backupFile);
                    }
                    throw;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied when saving to '{filename}'. Please check file permissions.", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new InvalidOperationException($"Directory not found for '{filename}'.", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"IO error when saving map to '{filename}': {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to serialize map to JSON: {ex.Message}", ex);
            }
        }

        private static void SaveJsonMap(Map map, string filename)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(map, options);
            File.WriteAllText(filename, json);
        }

        private static void SaveCompressedMap(Map map, string filename)
        {
            // Convert to optimized format
            var optimizedMap = ConvertToOptimizedFormat(map);

            var options = new JsonSerializerOptions
            {
                WriteIndented = false, // No indentation for compressed format
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(optimizedMap, options);
            var bytes = Encoding.UTF8.GetBytes(json);

            // Compress using GZip
            using (var fileStream = new FileStream(filename, FileMode.Create))
            using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }
        }

        private static OptimizedMapData ConvertToOptimizedFormat(Map map)
        {
            // Find the most common terrain type to use as default
            var terrainCounts = new Dictionary<TerrainType, int>();
            foreach (var tile in map.Tiles)
            {
                if (terrainCounts.ContainsKey(tile.Terrain))
                    terrainCounts[tile.Terrain]++;
                else
                    terrainCounts[tile.Terrain] = 1;
            }

            var defaultTerrain = terrainCounts.Any()
                ? terrainCounts.OrderByDescending(kvp => kvp.Value).First().Key
                : TerrainType.Plain;

            var optimized = new OptimizedMapData
            {
                Name = map.Name,
                Width = map.Width,
                Height = map.Height,
                Season = map.Season,
                Weather = map.Weather,
                FogOfWarEnabled = map.FogOfWarEnabled,
                Properties = map.Properties,
                Units = map.Units,
                Players = map.Players,
                VictoryConditions = map.VictoryConditions,
                Metadata = map.Metadata,
                DefaultTerrain = defaultTerrain,
                Tiles = new List<OptimizedTile>()
            };

            // Only store tiles that differ from default
            foreach (var tile in map.Tiles)
            {
                if (tile.Terrain != defaultTerrain || !tile.Traversable || (tile.SpriteIndex ?? 0) != 0)
                {
                    var optTile = new OptimizedTile
                    {
                        X = tile.X,
                        Y = tile.Y,
                        T = tile.Terrain
                    };

                    // Only include non-default values
                    if (!tile.Traversable)
                        optTile.Tr = false;
                    if (tile.SpriteIndex.HasValue && tile.SpriteIndex.Value != 0)
                        optTile.S = tile.SpriteIndex.Value;

                    optimized.Tiles.Add(optTile);
                }
            }

            return optimized;
        }

        public static Map LoadMap(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Filename cannot be null or empty", nameof(filename));
            }

            try
            {
                // Check if file exists
                if (!File.Exists(filename))
                {
                    throw new FileNotFoundException($"Map file not found: '{filename}'", filename);
                }

                // Check file size
                var fileInfo = new FileInfo(filename);
                if (fileInfo.Length > MAX_FILE_SIZE)
                {
                    throw new InvalidOperationException($"Map file is too large. Maximum size is {MAX_FILE_SIZE / (1024 * 1024)}MB.");
                }

                if (fileInfo.Length == 0)
                {
                    throw new InvalidOperationException("Map file is empty.");
                }

                // Determine format by extension or by trying to read
                var extension = Path.GetExtension(filename);
                Map map;

                if (extension.Equals(COMPRESSED_EXTENSION, StringComparison.OrdinalIgnoreCase))
                {
                    map = LoadCompressedMap(filename);
                }
                else
                {
                    // Try compressed format first (in case extension is wrong)
                    try
                    {
                        map = LoadCompressedMap(filename);
                    }
                    catch
                    {
                        // Fall back to JSON format
                        map = LoadJsonMap(filename);
                    }
                }

                // Validate and fix loaded map
                ValidateAndFixLoadedMap(map);

                // Rebuild TileArray from Tiles list
                map.BuildTileArray();

                return map;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied when reading '{filename}'. Please check file permissions.", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"IO error when loading map from '{filename}': {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid JSON format in map file '{filename}': {ex.Message}", ex);
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                // Create default map as fallback
                return CreateDefaultMap(filename, ex.Message);
            }
        }

        private static Map LoadJsonMap(string filename)
        {
            var json = File.ReadAllText(filename);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("Map file contains no data.");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var map = JsonSerializer.Deserialize<Map>(json, options);

            if (map == null)
            {
                throw new InvalidOperationException("Failed to deserialize map file. File may be corrupted or in an invalid format.");
            }

            return map;
        }

        private static Map LoadCompressedMap(string filename)
        {
            string json;

            // Decompress the file
            using (var fileStream = new FileStream(filename, FileMode.Open))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
            {
                json = reader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("Decompressed map file contains no data.");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Try to deserialize as optimized format first
            try
            {
                var optimizedMap = JsonSerializer.Deserialize<OptimizedMapData>(json, options);
                if (optimizedMap != null)
                {
                    return ConvertFromOptimizedFormat(optimizedMap);
                }
            }
            catch
            {
                // Fall back to regular format
            }

            // Try regular format
            var map = JsonSerializer.Deserialize<Map>(json, options);

            if (map == null)
            {
                throw new InvalidOperationException("Failed to deserialize compressed map file.");
            }

            return map;
        }

        private static Map ConvertFromOptimizedFormat(OptimizedMapData optimizedMap)
        {
            var map = new Map
            {
                Name = optimizedMap.Name,
                Width = optimizedMap.Width,
                Height = optimizedMap.Height,
                Season = optimizedMap.Season,
                Weather = optimizedMap.Weather,
                FogOfWarEnabled = optimizedMap.FogOfWarEnabled,
                Properties = optimizedMap.Properties ?? new List<Property>(),
                Units = optimizedMap.Units ?? new List<Unit>(),
                Players = optimizedMap.Players ?? new List<Player>(),
                VictoryConditions = optimizedMap.VictoryConditions ?? new VictoryConditions(),
                Metadata = optimizedMap.Metadata ?? new MapMetadata(),
                Tiles = new List<Tile>(),
                TileArray = new Tile[optimizedMap.Width, optimizedMap.Height]
            };

            // Create a set of optimized tile positions for quick lookup
            var optimizedTileMap = new Dictionary<(int, int), OptimizedTile>();
            if (optimizedMap.Tiles != null)
            {
                foreach (var optTile in optimizedMap.Tiles)
                {
                    optimizedTileMap[(optTile.X, optTile.Y)] = optTile;
                }
            }

            // Reconstruct all tiles
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Tile tile;

                    if (optimizedTileMap.TryGetValue((x, y), out var optTile))
                    {
                        // Use optimized tile data
                        tile = new Tile
                        {
                            X = x,
                            Y = y,
                            Terrain = optTile.T,
                            Traversable = optTile.Tr ?? true,
                            SpriteIndex = optTile.S ?? 0
                        };
                    }
                    else
                    {
                        // Use default values
                        tile = new Tile
                        {
                            X = x,
                            Y = y,
                            Terrain = optimizedMap.DefaultTerrain,
                            Traversable = true,
                            SpriteIndex = 0
                        };
                    }

                    map.Tiles.Add(tile);
                    map.TileArray[x, y] = tile;
                }
            }

            return map;
        }

        // Get file size information
        public static (long originalSize, long compressedSize, double compressionRatio) GetFileSizeInfo(string filename)
        {
            if (!File.Exists(filename))
                return (0, 0, 0);

            var fileInfo = new FileInfo(filename);
            var compressedSize = fileInfo.Length;

            // If it's a compressed file, estimate original size
            if (Path.GetExtension(filename).Equals(COMPRESSED_EXTENSION, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var map = LoadCompressedMap(filename);
                    var tempJson = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
                    var originalSize = Encoding.UTF8.GetByteCount(tempJson);
                    var ratio = 1.0 - ((double)compressedSize / originalSize);
                    return (originalSize, compressedSize, ratio);
                }
                catch
                {
                    return (compressedSize, compressedSize, 0);
                }
            }

            return (compressedSize, compressedSize, 0);
        }

        private static void ValidateMapForSave(Map map)
        {
            // Validate map dimensions
            if (map.Width < MIN_MAP_SIZE || map.Width > MAX_MAP_SIZE)
            {
                throw new InvalidOperationException($"Map width must be between {MIN_MAP_SIZE} and {MAX_MAP_SIZE}.");
            }

            if (map.Height < MIN_MAP_SIZE || map.Height > MAX_MAP_SIZE)
            {
                throw new InvalidOperationException($"Map height must be between {MIN_MAP_SIZE} and {MAX_MAP_SIZE}.");
            }

            // Validate map name
            if (string.IsNullOrWhiteSpace(map.Name))
            {
                map.Name = "UntitledMap";
            }

            // Validate season
            var validSeasons = new[] { "Summer", "Winter" };
            if (!validSeasons.Contains(map.Season))
            {
                map.Season = "Summer";
            }

            // Ensure collections are initialized
            map.Tiles ??= new();
            map.Properties ??= new();
            map.Units ??= new();
            map.Players ??= new();
            map.VictoryConditions ??= new();
            map.Metadata ??= new MapMetadata { Author = Environment.UserName, Created = DateTime.Now.ToString("yyyy-MM-dd") };
        }

        private static void ValidateAndFixLoadedMap(Map map)
        {
            // Fix map dimensions
            if (map.Width < MIN_MAP_SIZE || map.Width > MAX_MAP_SIZE)
            {
                map.Width = Math.Clamp(map.Width, MIN_MAP_SIZE, MAX_MAP_SIZE);
            }

            if (map.Height < MIN_MAP_SIZE || map.Height > MAX_MAP_SIZE)
            {
                map.Height = Math.Clamp(map.Height, MIN_MAP_SIZE, MAX_MAP_SIZE);
            }

            // Fix map name
            if (string.IsNullOrWhiteSpace(map.Name))
            {
                map.Name = "UntitledMap";
            }

            // Fix season
            var validSeasons = new[] { "Summer", "Winter" };
            if (!validSeasons.Contains(map.Season))
            {
                map.Season = "Summer";
            }

            // Initialize collections if null
            map.Tiles ??= new();
            map.Properties ??= new();
            map.Units ??= new();
            map.Players ??= new();
            map.VictoryConditions ??= new();
            map.Metadata ??= new MapMetadata { Author = "Unknown", Created = "Unknown" };

            // Initialize TileArray
            map.TileArray = new Tile[map.Width, map.Height];

            // Validate tiles
            var validTiles = map.Tiles.Where(t => t != null &&
                                                  t.X >= 0 && t.X < map.Width &&
                                                  t.Y >= 0 && t.Y < map.Height).ToList();
            map.Tiles = validTiles;

            // Ensure all map positions have tiles
            var existingPositions = new bool[map.Width, map.Height];
            foreach (var tile in map.Tiles)
            {
                existingPositions[tile.X, tile.Y] = true;
            }

            // Create missing tiles with default terrain
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (!existingPositions[x, y])
                    {
                        map.Tiles.Add(new Tile
                        {
                            X = x,
                            Y = y,
                            Terrain = TerrainType.Plain,
                            Traversable = true,
                            SpriteIndex = 0
                        });
                    }
                }
            }

            // Validate properties - remove out of bounds
            map.Properties = map.Properties.Where(p => p != null &&
                                                       p.X >= 0 && p.X < map.Width &&
                                                       p.Y >= 0 && p.Y < map.Height).ToList();

            // Ensure properties have valid owners
            foreach (var property in map.Properties)
            {
                if (string.IsNullOrWhiteSpace(property.Owner))
                {
                    property.Owner = "Neutral";
                }
                // Ensure default values are set
                property.SetDefaultValues();
            }

            // Validate units - remove out of bounds
            map.Units = map.Units.Where(u => u != null &&
                                             u.X >= 0 && u.X < map.Width &&
                                             u.Y >= 0 && u.Y < map.Height).ToList();

            // Ensure units have valid owners and stats
            foreach (var unit in map.Units)
            {
                if (string.IsNullOrWhiteSpace(unit.Owner))
                {
                    unit.Owner = "Neutral";
                }
                // Ensure default values are set
                unit.SetDefaultValues();

                // Validate HP
                if (unit.HP <= 0 || unit.HP > 100)
                {
                    unit.HP = 100;
                }
            }

            // Validate players
            if (map.Players.Count == 0)
            {
                // Add default player if none exist
                map.Players.Add(new Player
                {
                    Name = "Player 1",
                    Country = "Unspecified",
                    IsAI = false,
                    Color = "Blue"
                });
            }

            // Ensure unique player names
            var playerNames = new System.Collections.Generic.HashSet<string>();
            foreach (var player in map.Players)
            {
                if (string.IsNullOrWhiteSpace(player.Name) || !playerNames.Add(player.Name))
                {
                    // Generate unique name
                    int counter = 1;
                    string baseName = string.IsNullOrWhiteSpace(player.Name) ? "Player" : player.Name;
                    while (!playerNames.Add($"{baseName} {counter}"))
                    {
                        counter++;
                    }
                    player.Name = $"{baseName} {counter}";
                }

                // Validate country
                if (string.IsNullOrWhiteSpace(player.Country))
                {
                    player.Country = "Unspecified";
                }

                // Validate color
                if (string.IsNullOrWhiteSpace(player.Color))
                {
                    player.Color = "Blue";
                }
            }
        }

        private static Map CreateDefaultMap(string filename, string errorMessage)
        {
            System.Diagnostics.Debug.WriteLine($"Creating default map due to load error: {errorMessage}");

            var map = new Map
            {
                Name = Path.GetFileNameWithoutExtension(filename) ?? "UntitledMap",
                Width = 100,
                Height = 100,
                Season = "Summer",
                Weather = WeatherType.Random,
                Tiles = new(),
                TileArray = new Tile[100, 100],
                Properties = new(),
                Units = new(),
                Players = new()
                {
                    new Player
                    {
                        Name = "Player 1",
                        Country = "Unspecified",
                        IsAI = false,
                        Color = "Blue"
                    }
                },
                VictoryConditions = new(),
                Metadata = new MapMetadata
                {
                    Author = Environment.UserName,
                    Created = DateTime.Now.ToString("yyyy-MM-dd")
                },
                FogOfWarEnabled = true
            };

            // Initialize tiles
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var tile = new Tile
                    {
                        X = x,
                        Y = y,
                        Terrain = TerrainType.Plain,
                        Traversable = true,
                        SpriteIndex = 0
                    };
                    map.Tiles.Add(tile);
                    map.TileArray[x, y] = tile;
                }
            }

            return map;
        }
    }
}