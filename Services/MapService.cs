using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WwXMapEditor.Models;

namespace WwXMapEditor.Services
{
    public static class MapService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never, // Ensure nothing is ignored
            IncludeFields = false,
            Converters = { new JsonStringEnumConverter() }
        };

        public static void SaveMap(Map map, string filename, bool useCompression = false)
        {
            // Ensure tile array is flattened to list before saving
            map.FlattenTileArray();

            // Validate all tiles have their properties set
            foreach (var tile in map.Tiles)
            {
                if (tile == null) continue;
                // Ensure traversable has a value (default to true if not set)
                if (!map.Tiles.Exists(t => t.X == tile.X && t.Y == tile.Y))
                {
                    tile.Traversable = true;
                }
            }

            var json = JsonSerializer.Serialize(map, JsonOptions);

            if (useCompression || filename.EndsWith(".wwxz", StringComparison.OrdinalIgnoreCase))
            {
                SaveCompressed(json, filename);
            }
            else
            {
                File.WriteAllText(filename, json);
            }
        }

        public static Map LoadMap(string filename)
        {
            string json;

            if (filename.EndsWith(".wwxz", StringComparison.OrdinalIgnoreCase))
            {
                json = LoadCompressed(filename);
            }
            else
            {
                json = File.ReadAllText(filename);
            }

            var map = JsonSerializer.Deserialize<Map>(json, JsonOptions);

            if (map == null)
            {
                throw new InvalidOperationException("Failed to deserialize map");
            }

            // Ensure all required properties have default values
            if (string.IsNullOrEmpty(map.Name))
                map.Name = "UntitledMap";

            if (map.Width <= 0)
                map.Width = 100;

            if (map.Height <= 0)
                map.Height = 100;

            if (string.IsNullOrEmpty(map.Season))
                map.Season = "Summer";

            // Initialize collections if null
            map.Tiles ??= new();
            map.Properties ??= new();
            map.Units ??= new();
            map.Players ??= new();
            map.VictoryConditions ??= new();
            map.Metadata ??= new MapMetadata { Author = Environment.UserName, Created = DateTime.Now.ToString("yyyy-MM-dd") };

            // Build tile array from tile list
            map.BuildTileArray();

            // Ensure all tiles in the array have proper default values
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (map.TileArray[x, y] == null)
                    {
                        map.TileArray[x, y] = new Tile
                        {
                            X = x,
                            Y = y,
                            Terrain = TerrainType.Plain,
                            Traversable = true,
                            SpriteIndex = 0
                        };
                    }
                    else
                    {
                        // Ensure existing tiles have traversable set
                        // If it wasn't serialized (old format), default to true
                        var tile = map.TileArray[x, y];
                        if (tile.Terrain == TerrainType.Mountain || tile.Terrain == TerrainType.Sea)
                        {
                            // These terrains might have special traversability rules
                            // but we'll let the game logic handle that
                        }
                        // The property should already be set from deserialization
                        // No need to override it here
                    }
                }
            }

            return map;
        }

        private static void SaveCompressed(string json, string filename)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            using var fileStream = File.Create(filename);
            using var compressionStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            compressionStream.Write(bytes, 0, bytes.Length);
        }

        private static string LoadCompressed(string filename)
        {
            using var fileStream = File.OpenRead(filename);
            using var decompressionStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using var reader = new StreamReader(decompressionStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public static (long originalSize, long compressedSize, double ratio) GetFileSizeInfo(string filename)
        {
            try
            {
                var fileInfo = new FileInfo(filename);
                if (!fileInfo.Exists) return (0, 0, 0);

                if (filename.EndsWith(".wwxz", StringComparison.OrdinalIgnoreCase))
                {
                    // For compressed files, decompress to get original size
                    var json = LoadCompressed(filename);
                    var originalSize = Encoding.UTF8.GetByteCount(json);
                    var compressedSize = fileInfo.Length;
                    var ratio = 1.0 - (double)compressedSize / originalSize;
                    return (originalSize, compressedSize, ratio);
                }
                else
                {
                    // For uncompressed files
                    return (fileInfo.Length, fileInfo.Length, 0);
                }
            }
            catch
            {
                return (0, 0, 0);
            }
        }
    }
}