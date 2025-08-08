using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WWXMapEditor.Models;

namespace WWXMapEditor.Services
{
    public class MapService
    {
        public class MapServiceResult<T>
        {
            public bool Success { get; set; }
            public T? Data { get; set; }
            public string? ErrorMessage { get; set; }

            public static MapServiceResult<T> SuccessResult(T data)
            {
                return new MapServiceResult<T> { Success = true, Data = data };
            }

            public static MapServiceResult<T> FailureResult(string errorMessage)
            {
                return new MapServiceResult<T> { Success = false, ErrorMessage = errorMessage };
            }
        }

        public Map CreateNewMap(MapProperties properties)
        {
            try
            {
                // Validate input parameters
                if (properties == null)
                {
                    throw new ArgumentNullException(nameof(properties), "Map properties cannot be null");
                }

                if (properties.Width < 10 || properties.Width > 500)
                {
                    throw new ArgumentOutOfRangeException(nameof(properties.Width), "Map width must be between 10 and 500");
                }

                if (properties.Height < 10 || properties.Height > 500)
                {
                    throw new ArgumentOutOfRangeException(nameof(properties.Height), "Map height must be between 10 and 500");
                }

                var map = new Map
                {
                    Name = properties.Name ?? "Untitled Map",
                    Description = properties.Description ?? string.Empty,
                    Width = properties.Width,
                    Height = properties.Height,
                    StartingTerrain = properties.StartingTerrain ?? "Plains",
                    NumberOfPlayers = properties.NumberOfPlayers,
                    VictoryConditions = properties.VictoryConditions,
                    FogOfWarSettings = properties.FogOfWarSettings,
                    Tiles = new Tile[properties.Width, properties.Height],
                    HQs = new List<HQ>()
                };

                // Initialize all tiles with the starting terrain
                for (int x = 0; x < properties.Width; x++)
                {
                    for (int y = 0; y < properties.Height; y++)
                    {
                        map.Tiles[x, y] = new Tile
                        {
                            X = x,
                            Y = y,
                            TerrainType = properties.StartingTerrain ?? "Plains",
                            IsLandBlocked = false,
                            IsAirBlocked = false,
                            IsWaterBlocked = false,
                            Unit = null,
                            Property = null
                        };
                    }
                }

                return map;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating new map: {ex.Message}");
                throw new InvalidOperationException($"Failed to create new map: {ex.Message}", ex);
            }
        }

        public MapServiceResult<bool> SaveMap(Map map, string filePath)
        {
            try
            {
                // Validate inputs
                if (map == null)
                {
                    return MapServiceResult<bool>.FailureResult("Cannot save null map");
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return MapServiceResult<bool>.FailureResult("File path cannot be empty");
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch (Exception ex)
                    {
                        return MapServiceResult<bool>.FailureResult($"Failed to create directory: {ex.Message}");
                    }
                }

                // Create backup if file exists
                string? backupPath = null;
                if (File.Exists(filePath))
                {
                    backupPath = $"{filePath}.backup";
                    try
                    {
                        File.Copy(filePath, backupPath, true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Could not create backup: {ex.Message}");
                    }
                }

                // Serialize and save
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(map, options);
                File.WriteAllText(filePath, json);

                // Remove backup after successful save
                if (backupPath != null && File.Exists(backupPath))
                {
                    try
                    {
                        File.Delete(backupPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                return MapServiceResult<bool>.SuccessResult(true);
            }
            catch (UnauthorizedAccessException ex)
            {
                return MapServiceResult<bool>.FailureResult($"Access denied: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                return MapServiceResult<bool>.FailureResult($"Directory not found: {ex.Message}");
            }
            catch (IOException ex)
            {
                return MapServiceResult<bool>.FailureResult($"IO error: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving map: {ex}");
                return MapServiceResult<bool>.FailureResult($"Unexpected error: {ex.Message}");
            }
        }

        public MapServiceResult<Map> LoadMap(string filePath)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return MapServiceResult<Map>.FailureResult("File path cannot be empty");
                }

                if (!File.Exists(filePath))
                {
                    return MapServiceResult<Map>.FailureResult($"File not found: {filePath}");
                }

                // Check file size (prevent loading huge files)
                var fileInfo = new FileInfo(filePath);
                const long maxFileSize = 50 * 1024 * 1024; // 50MB limit
                if (fileInfo.Length > maxFileSize)
                {
                    return MapServiceResult<Map>.FailureResult($"File is too large (max {maxFileSize / 1024 / 1024}MB)");
                }

                // Read and deserialize
                var json = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return MapServiceResult<Map>.FailureResult("File is empty");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var map = JsonSerializer.Deserialize<Map>(json, options);

                if (map == null)
                {
                    return MapServiceResult<Map>.FailureResult("Failed to deserialize map data");
                }

                // Validate loaded map
                if (map.Width <= 0 || map.Height <= 0)
                {
                    return MapServiceResult<Map>.FailureResult("Invalid map dimensions");
                }

                if (map.Tiles == null || map.Tiles.Length == 0)
                {
                    return MapServiceResult<Map>.FailureResult("Map contains no tiles");
                }

                // Initialize HQs list if null
                if (map.HQs == null)
                {
                    map.HQs = new List<HQ>();
                }

                return MapServiceResult<Map>.SuccessResult(map);
            }
            catch (UnauthorizedAccessException ex)
            {
                return MapServiceResult<Map>.FailureResult($"Access denied: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return MapServiceResult<Map>.FailureResult($"Invalid map file format: {ex.Message}");
            }
            catch (IOException ex)
            {
                return MapServiceResult<Map>.FailureResult($"IO error: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading map: {ex}");
                return MapServiceResult<Map>.FailureResult($"Unexpected error: {ex.Message}");
            }
        }

        public MapServiceResult<bool> ExportMap(Map map, string filePath, string format)
        {
            try
            {
                if (map == null)
                {
                    return MapServiceResult<bool>.FailureResult("Cannot export null map");
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return MapServiceResult<bool>.FailureResult("Export path cannot be empty");
                }

                switch (format?.ToLower())
                {
                    case "json":
                        return SaveMap(map, filePath);
                    case "xml":
                        return MapServiceResult<bool>.FailureResult("XML export is not yet implemented");
                    case "png":
                        return MapServiceResult<bool>.FailureResult("PNG export is not yet implemented");
                    default:
                        return MapServiceResult<bool>.FailureResult($"Unknown export format: {format}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting map: {ex}");
                return MapServiceResult<bool>.FailureResult($"Export failed: {ex.Message}");
            }
        }

        public void PlaceUnit(Map map, Unit unit, int x, int y)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (unit == null) throw new ArgumentNullException(nameof(unit));

            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                var tile = map.Tiles[x, y];
                if (tile != null && tile.Unit == null)
                {
                    unit.X = x;
                    unit.Y = y;
                    tile.Unit = unit;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot place unit at ({x}, {y}): tile is occupied or invalid");
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are outside map bounds");
            }
        }

        public void PlaceProperty(Map map, Property property, int x, int y)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (property == null) throw new ArgumentNullException(nameof(property));

            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                var tile = map.Tiles[x, y];
                if (tile != null && tile.Property == null)
                {
                    property.X = x;
                    property.Y = y;
                    tile.Property = property;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot place property at ({x}, {y}): tile already has a property or is invalid");
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are outside map bounds");
            }
        }

        public void PlaceHQ(Map map, HQ hq, int x, int y)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (hq == null) throw new ArgumentNullException(nameof(hq));

            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                hq.X = x;
                hq.Y = y;

                // Remove any existing HQ for this owner
                map.HQs.RemoveAll(h => h.Owner == hq.Owner);

                // Add the new HQ
                map.HQs.Add(hq);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are outside map bounds");
            }
        }

        public void UpdateTileBlocking(Map map, int x, int y, bool landBlocked, bool airBlocked, bool waterBlocked)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                var tile = map.Tiles[x, y];
                if (tile != null)
                {
                    tile.IsLandBlocked = landBlocked;
                    tile.IsAirBlocked = airBlocked;
                    tile.IsWaterBlocked = waterBlocked;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are outside map bounds");
            }
        }
    }
}