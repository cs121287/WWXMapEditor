using System.Collections.Generic;
using System.Linq;
using WwXMapEditor.Models;

namespace WwXMapEditor.Services
{
    public class MapValidationService
    {
        public class ValidationError
        {
            public string Type { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public int? X { get; set; }
            public int? Y { get; set; }
        }

        public List<ValidationError> ValidateMap(Map map)
        {
            var errors = new List<ValidationError>();

            // Validate map dimensions
            if (map.Width < 20 || map.Width > 2000)
            {
                errors.Add(new ValidationError
                {
                    Type = "Dimension",
                    Message = $"Map width {map.Width} is outside valid range (20-2000)"
                });
            }

            if (map.Height < 20 || map.Height > 2000)
            {
                errors.Add(new ValidationError
                {
                    Type = "Dimension",
                    Message = $"Map height {map.Height} is outside valid range (20-2000)"
                });
            }

            // Validate tiles
            if (map.TileArray == null)
            {
                errors.Add(new ValidationError
                {
                    Type = "Structure",
                    Message = "Map tile array is not initialized"
                });
                return errors;
            }

            // Check for null tiles
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (map.TileArray[x, y] == null)
                    {
                        errors.Add(new ValidationError
                        {
                            Type = "Tile",
                            Message = $"Null tile at position ({x},{y})",
                            X = x,
                            Y = y
                        });
                    }
                }
            }

            // Validate units
            foreach (var unit in map.Units)
            {
                if (unit.X < 0 || unit.X >= map.Width || unit.Y < 0 || unit.Y >= map.Height)
                {
                    errors.Add(new ValidationError
                    {
                        Type = "Unit",
                        Message = $"Unit at ({unit.X},{unit.Y}) is outside map bounds",
                        X = unit.X,
                        Y = unit.Y
                    });
                }

                if (unit.HP <= 0)
                {
                    errors.Add(new ValidationError
                    {
                        Type = "Unit",
                        Message = $"Unit at ({unit.X},{unit.Y}) has invalid HP: {unit.HP}",
                        X = unit.X,
                        Y = unit.Y
                    });
                }

                // Check if unit is on traversable terrain
                if (unit.X >= 0 && unit.X < map.Width && unit.Y >= 0 && unit.Y < map.Height)
                {
                    var tile = map.TileArray[unit.X, unit.Y];
                    if (tile != null && !tile.Traversable)
                    {
                        errors.Add(new ValidationError
                        {
                            Type = "Unit",
                            Message = $"Unit at ({unit.X},{unit.Y}) is on non-traversable terrain",
                            X = unit.X,
                            Y = unit.Y
                        });
                    }
                }
            }

            // Validate properties
            foreach (var property in map.Properties)
            {
                if (property.X < 0 || property.X >= map.Width || property.Y < 0 || property.Y >= map.Height)
                {
                    errors.Add(new ValidationError
                    {
                        Type = "Property",
                        Message = $"Property at ({property.X},{property.Y}) is outside map bounds",
                        X = property.X,
                        Y = property.Y
                    });
                }
            }

            // Check for duplicate units/properties at same position
            var unitPositions = map.Units.GroupBy(u => new { u.X, u.Y }).Where(g => g.Count() > 1);
            foreach (var pos in unitPositions)
            {
                errors.Add(new ValidationError
                {
                    Type = "Unit",
                    Message = $"Multiple units at position ({pos.Key.X},{pos.Key.Y})",
                    X = pos.Key.X,
                    Y = pos.Key.Y
                });
            }

            var propertyPositions = map.Properties.GroupBy(p => new { p.X, p.Y }).Where(g => g.Count() > 1);
            foreach (var pos in propertyPositions)
            {
                errors.Add(new ValidationError
                {
                    Type = "Property",
                    Message = $"Multiple properties at position ({pos.Key.X},{pos.Key.Y})",
                    X = pos.Key.X,
                    Y = pos.Key.Y
                });
            }

            // Validate players
            if (map.Players == null || map.Players.Count == 0)
            {
                errors.Add(new ValidationError
                {
                    Type = "Player",
                    Message = "Map has no players defined"
                });
            }

            // Check for at least one HQ per player
            var playerHQs = map.Properties.Where(p => p.Type == "HQ").GroupBy(p => p.Owner);
            foreach (var player in map.Players ?? new List<Player>())
            {
                if (!playerHQs.Any(g => g.Key == player.Name))
                {
                    errors.Add(new ValidationError
                    {
                        Type = "Victory",
                        Message = $"Player {player.Name} has no HQ"
                    });
                }
            }

            return errors;
        }

        public bool IsMapValid(Map map)
        {
            return !ValidateMap(map).Any();
        }
    }
}