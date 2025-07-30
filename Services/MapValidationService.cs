using System.Collections.Generic;
using System.Linq;
using WwXMapEditor.Models;

namespace WwXMapEditor.Services
{
    public class MapValidationService
    {
        public class ValidationError
        {
            public string Message { get; set; } = "";
            public ValidationErrorType Type { get; set; }
            public int? X { get; set; }
            public int? Y { get; set; }
        }

        public enum ValidationErrorType
        {
            NoHQ,
            MultipleHQs,
            NoPlayers,
            InvalidUnitPlacement,
            InvalidPropertyPlacement,
            UnreachableArea,
            NoProductionFacilities
        }

        public List<ValidationError> ValidateMap(Map map)
        {
            var errors = new List<ValidationError>();

            // Check for HQ presence
            var hqCount = map.Properties.Count(p => p.Type == PropertyType.HQ);
            if (hqCount == 0)
            {
                errors.Add(new ValidationError
                {
                    Message = "Map must have at least one HQ",
                    Type = ValidationErrorType.NoHQ
                });
            }

            // Check for players
            if (map.Players.Count == 0)
            {
                errors.Add(new ValidationError
                {
                    Message = "Map must have at least one player",
                    Type = ValidationErrorType.NoPlayers
                });
            }

            // Check for production facilities per player
            var playerNames = map.Players.Select(p => p.Name).ToList();
            foreach (var player in playerNames)
            {
                var hasProduction = map.Properties.Any(p =>
                    p.Owner == player &&
                    (p.Type == PropertyType.Factory || p.Type == PropertyType.Airport || p.Type == PropertyType.Port));

                if (!hasProduction)
                {
                    errors.Add(new ValidationError
                    {
                        Message = $"Player '{player}' has no production facilities",
                        Type = ValidationErrorType.NoProductionFacilities
                    });
                }
            }

            // Check unit placement
            foreach (var unit in map.Units)
            {
                if (unit.X < 0 || unit.X >= map.Width || unit.Y < 0 || unit.Y >= map.Height)
                {
                    errors.Add(new ValidationError
                    {
                        Message = $"Unit at ({unit.X}, {unit.Y}) is out of bounds",
                        Type = ValidationErrorType.InvalidUnitPlacement,
                        X = unit.X,
                        Y = unit.Y
                    });
                    continue;
                }

                var tile = map.TileArray[unit.X, unit.Y];
                if (tile == null) continue;

                // Check if unit type can be on this terrain
                bool validPlacement = IsValidUnitPlacement(unit, tile.Terrain);
                if (!validPlacement)
                {
                    errors.Add(new ValidationError
                    {
                        Message = $"{unit.Type} cannot be placed on {tile.Terrain} terrain",
                        Type = ValidationErrorType.InvalidUnitPlacement,
                        X = unit.X,
                        Y = unit.Y
                    });
                }
            }

            // Check property placement
            foreach (var property in map.Properties)
            {
                if (property.X < 0 || property.X >= map.Width || property.Y < 0 || property.Y >= map.Height)
                {
                    errors.Add(new ValidationError
                    {
                        Message = $"Property at ({property.X}, {property.Y}) is out of bounds",
                        Type = ValidationErrorType.InvalidPropertyPlacement,
                        X = property.X,
                        Y = property.Y
                    });
                }
            }

            return errors;
        }

        private bool IsValidUnitPlacement(Unit unit, TerrainType terrain)
        {
            switch (unit.MovementType)
            {
                case MovementType.Infantry:
                    return terrain != TerrainType.Sea;

                case MovementType.Wheeled:
                case MovementType.Treaded:
                    return terrain != TerrainType.Sea && terrain != TerrainType.Mountain;

                case MovementType.Ship:
                    return terrain == TerrainType.Sea || terrain == TerrainType.Port;

                case MovementType.Lander:
                    return terrain == TerrainType.Sea || terrain == TerrainType.Beach || terrain == TerrainType.Port;

                case MovementType.Air:
                    return true; // Air units can be placed anywhere

                default:
                    return true;
            }
        }
    }
}