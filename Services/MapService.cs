using System;
using System.IO;
using System.Text.Json;
using WWXMapEditor.Models;

namespace WWXMapEditor.Services
{
    public class MapService
    {
        public Map CreateNewMap(MapProperties properties)
        {
            var map = new Map
            {
                Name = properties.Name,
                Description = properties.Description,
                Width = properties.Width,
                Height = properties.Height,
                StartingTerrain = properties.StartingTerrain,
                NumberOfPlayers = properties.NumberOfPlayers,
                VictoryConditions = properties.VictoryConditions,
                FogOfWarSettings = properties.FogOfWarSettings,
                Tiles = new Tile[properties.Width, properties.Height],
                HQs = new System.Collections.Generic.List<HQ>()
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
                        TerrainType = properties.StartingTerrain,
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

        public bool SaveMap(Map map, string filePath)
        {
            try
            {
                var json = JsonSerializer.Serialize(map, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error saving map: {ex.Message}");
                return false;
            }
        }

        public Map? LoadMap(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    return null;
                }

                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<Map>(json);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error loading map: {ex.Message}");
                return null;
            }
        }

        public bool ExportMap(Map map, string filePath, string format)
        {
            try
            {
                switch (format.ToLower())
                {
                    case "json":
                        return SaveMap(map, filePath);
                    case "xml":
                        // TODO: Implement XML export
                        return false;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error exporting map: {ex.Message}");
                return false;
            }
        }

        public void PlaceUnit(Map map, Unit unit, int x, int y)
        {
            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                var tile = map.Tiles[x, y];
                if (tile.Unit == null)
                {
                    unit.X = x;
                    unit.Y = y;
                    tile.Unit = unit;
                }
            }
        }

        public void PlaceProperty(Map map, Property property, int x, int y)
        {
            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                var tile = map.Tiles[x, y];
                if (tile.Property == null)
                {
                    property.X = x;
                    property.Y = y;
                    tile.Property = property;
                }
            }
        }

        public void PlaceHQ(Map map, HQ hq, int x, int y)
        {
            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                hq.X = x;
                hq.Y = y;

                // Remove any existing HQ for this owner
                map.HQs.RemoveAll(h => h.Owner == hq.Owner);

                // Add the new HQ
                map.HQs.Add(hq);
            }
        }

        public void UpdateTileBlocking(Map map, int x, int y, bool landBlocked, bool airBlocked, bool waterBlocked)
        {
            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                var tile = map.Tiles[x, y];
                tile.IsLandBlocked = landBlocked;
                tile.IsAirBlocked = airBlocked;
                tile.IsWaterBlocked = waterBlocked;
            }
        }
    }
}