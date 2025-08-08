using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WWXMapEditor.Models;

namespace WWXMapEditor.Services
{
    public class MapValidationService
    {
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
            public List<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();

            public string GetSummary()
            {
                var summary = new StringBuilder();

                if (!IsValid)
                {
                    summary.AppendLine("Map validation failed:");
                    summary.AppendLine();

                    if (Errors.Any())
                    {
                        summary.AppendLine("ERRORS:");
                        foreach (var error in Errors)
                        {
                            summary.AppendLine($"  • {error.Message}");
                        }
                    }
                }

                if (Warnings.Any())
                {
                    if (!IsValid) summary.AppendLine();
                    summary.AppendLine("WARNINGS:");
                    foreach (var warning in Warnings)
                    {
                        summary.AppendLine($"  • {warning.Message}");
                    }
                }

                return summary.ToString();
            }
        }

        public class ValidationError
        {
            public string Code { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string? Details { get; set; }
        }

        public class ValidationWarning
        {
            public string Code { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string? Details { get; set; }
        }

        public ValidationResult ValidateMap(Map map)
        {
            var result = new ValidationResult { IsValid = true };

            if (map == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Code = "NULL_MAP",
                    Message = "Map object is null"
                });
                return result;
            }

            // Validate basic properties
            ValidateBasicProperties(map, result);

            // Validate dimensions
            ValidateDimensions(map, result);

            // Validate tiles
            ValidateTiles(map, result);

            // Validate game rules
            ValidateGameRules(map, result);

            // Validate HQs
            ValidateHQs(map, result);

            // Validate victory conditions
            ValidateVictoryConditions(map, result);

            // Check for performance issues
            CheckPerformanceIssues(map, result);

            return result;
        }

        private void ValidateBasicProperties(Map map, ValidationResult result)
        {
            // Map name validation
            if (string.IsNullOrWhiteSpace(map.Name))
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_NAME",
                    Message = "Map name cannot be empty"
                });
                result.IsValid = false;
            }
            else if (map.Name.Length > 100)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "NAME_TOO_LONG",
                    Message = "Map name exceeds maximum length of 100 characters"
                });
                result.IsValid = false;
            }

            // Description validation
            if (map.Description?.Length > 500)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "DESCRIPTION_LONG",
                    Message = "Map description is very long (over 500 characters)"
                });
            }

            // Number of players validation
            if (map.NumberOfPlayers < 2)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_PLAYER_COUNT",
                    Message = "Map must support at least 2 players"
                });
                result.IsValid = false;
            }
            else if (map.NumberOfPlayers > 8)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "TOO_MANY_PLAYERS",
                    Message = "Map cannot support more than 8 players"
                });
                result.IsValid = false;
            }
        }

        private void ValidateDimensions(Map map, ValidationResult result)
        {
            // Width validation
            if (map.Width < 10)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "WIDTH_TOO_SMALL",
                    Message = "Map width must be at least 10 tiles"
                });
                result.IsValid = false;
            }
            else if (map.Width > 500)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "WIDTH_TOO_LARGE",
                    Message = "Map width cannot exceed 500 tiles"
                });
                result.IsValid = false;
            }

            // Height validation
            if (map.Height < 10)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "HEIGHT_TOO_SMALL",
                    Message = "Map height must be at least 10 tiles"
                });
                result.IsValid = false;
            }
            else if (map.Height > 500)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "HEIGHT_TOO_LARGE",
                    Message = "Map height cannot exceed 500 tiles"
                });
                result.IsValid = false;
            }

            // Performance warning for large maps
            if (map.Width * map.Height > 10000)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "LARGE_MAP",
                    Message = $"Large map size ({map.Width}x{map.Height} = {map.Width * map.Height} tiles) may impact performance"
                });
            }
        }

        private void ValidateTiles(Map map, ValidationResult result)
        {
            if (map.Tiles == null)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "NULL_TILES",
                    Message = "Map tiles array is null"
                });
                result.IsValid = false;
                return;
            }

            // Check if tiles array matches dimensions
            if (map.Tiles.GetLength(0) != map.Width || map.Tiles.GetLength(1) != map.Height)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "TILES_DIMENSION_MISMATCH",
                    Message = $"Tiles array dimensions ({map.Tiles.GetLength(0)}x{map.Tiles.GetLength(1)}) don't match map dimensions ({map.Width}x{map.Height})"
                });
                result.IsValid = false;
                return;
            }

            // Validate individual tiles
            int nullTileCount = 0;
            int invalidTerrainCount = 0;
            var terrainDistribution = new Dictionary<string, int>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.Tiles[x, y];

                    if (tile == null)
                    {
                        nullTileCount++;
                        continue;
                    }

                    // Check tile coordinates
                    if (tile.X != x || tile.Y != y)
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            Code = "TILE_COORDINATE_MISMATCH",
                            Message = $"Tile at [{x},{y}] has mismatched coordinates [{tile.X},{tile.Y}]"
                        });
                    }

                    // Validate terrain type
                    if (string.IsNullOrWhiteSpace(tile.TerrainType))
                    {
                        invalidTerrainCount++;
                    }
                    else
                    {
                        if (!terrainDistribution.ContainsKey(tile.TerrainType))
                            terrainDistribution[tile.TerrainType] = 0;
                        terrainDistribution[tile.TerrainType]++;
                    }

                    // Check for invalid unit/property combinations
                    if (tile.Unit != null && tile.Property != null)
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            Code = "TILE_OVERLAP",
                            Message = $"Tile at [{x},{y}] has both a unit and a property"
                        });
                    }
                }
            }

            if (nullTileCount > 0)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "NULL_TILES_FOUND",
                    Message = $"Found {nullTileCount} null tiles in the map"
                });
                result.IsValid = false;
            }

            if (invalidTerrainCount > 0)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_TERRAIN",
                    Message = $"Found {invalidTerrainCount} tiles with invalid terrain types"
                });
                result.IsValid = false;
            }

            // Check terrain distribution
            if (terrainDistribution.Count == 1)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "UNIFORM_TERRAIN",
                    Message = "Map uses only one terrain type"
                });
            }
        }

        private void ValidateGameRules(Map map, ValidationResult result)
        {
            // Count properties by owner
            var propertyCount = new Dictionary<string, int>();
            int neutralProperties = 0;

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.Tiles?[x, y];
                    if (tile?.Property != null)
                    {
                        var owner = tile.Property.Owner ?? "Neutral";
                        if (owner == "Neutral")
                        {
                            neutralProperties++;
                        }
                        else
                        {
                            if (!propertyCount.ContainsKey(owner))
                                propertyCount[owner] = 0;
                            propertyCount[owner]++;
                        }
                    }
                }
            }

            // Check for balanced starting conditions
            if (propertyCount.Count > 0)
            {
                var minProperties = propertyCount.Values.Min();
                var maxProperties = propertyCount.Values.Max();

                if (maxProperties - minProperties > 2)
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        Code = "UNBALANCED_PROPERTIES",
                        Message = "Starting properties are not balanced between players"
                    });
                }
            }

            // Check for minimum playable content
            if (neutralProperties == 0 && propertyCount.Count == 0)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "NO_PROPERTIES",
                    Message = "Map has no capturable properties"
                });
            }
        }

        private void ValidateHQs(Map map, ValidationResult result)
        {
            if (map.HQs == null)
            {
                map.HQs = new List<HQ>();
            }

            // Check HQ count matches player count
            if (map.HQs.Count < map.NumberOfPlayers)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "INSUFFICIENT_HQS",
                    Message = $"Map has {map.HQs.Count} HQs but requires {map.NumberOfPlayers} for {map.NumberOfPlayers} players"
                });
                result.IsValid = false;
            }
            else if (map.HQs.Count > map.NumberOfPlayers)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "EXCESS_HQS",
                    Message = $"Map has {map.HQs.Count} HQs but only {map.NumberOfPlayers} players"
                });
            }

            // Validate individual HQs
            var hqOwners = new HashSet<string>();
            foreach (var hq in map.HQs)
            {
                // Check for duplicate owners
                if (!string.IsNullOrEmpty(hq.Owner))
                {
                    if (hqOwners.Contains(hq.Owner))
                    {
                        result.Errors.Add(new ValidationError
                        {
                            Code = "DUPLICATE_HQ_OWNER",
                            Message = $"Multiple HQs found for owner '{hq.Owner}'"
                        });
                        result.IsValid = false;
                    }
                    hqOwners.Add(hq.Owner);
                }

                // Validate HQ position
                if (hq.X < 0 || hq.X >= map.Width || hq.Y < 0 || hq.Y >= map.Height)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "HQ_OUT_OF_BOUNDS",
                        Message = $"HQ for '{hq.Owner}' is outside map boundaries at [{hq.X},{hq.Y}]"
                    });
                    result.IsValid = false;
                }
            }
        }

        private void ValidateVictoryConditions(Map map, ValidationResult result)
        {
            if (map.VictoryConditions == null)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "NO_VICTORY_CONDITIONS",
                    Message = "Map has no victory conditions defined"
                });
                return;
            }

            // Check if at least one victory condition is enabled
            bool hasVictoryCondition = map.VictoryConditions.CaptureHQ ||
                                       map.VictoryConditions.DefeatAllUnits ||
                                       map.VictoryConditions.CaptureProperties ||
                                       map.VictoryConditions.TurnLimit;

            if (!hasVictoryCondition)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "NO_ACTIVE_VICTORY_CONDITION",
                    Message = "Map must have at least one victory condition enabled"
                });
                result.IsValid = false;
            }

            // Validate turn limit if enabled
            if (map.VictoryConditions.TurnLimit && map.VictoryConditions.MaxTurns <= 0)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_TURN_LIMIT",
                    Message = "Turn limit victory condition is enabled but max turns is not set"
                });
                result.IsValid = false;
            }

            // Validate property capture if enabled
            if (map.VictoryConditions.CaptureProperties && map.VictoryConditions.RequiredProperties <= 0)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_PROPERTY_REQUIREMENT",
                    Message = "Property capture victory condition is enabled but required properties is not set"
                });
                result.IsValid = false;
            }
        }

        private void CheckPerformanceIssues(Map map, ValidationResult result)
        {
            // Count total entities
            int unitCount = 0;
            int propertyCount = 0;
            int blockedTiles = 0;

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.Tiles?[x, y];
                    if (tile != null)
                    {
                        if (tile.Unit != null) unitCount++;
                        if (tile.Property != null) propertyCount++;
                        if (tile.IsLandBlocked || tile.IsAirBlocked || tile.IsWaterBlocked) blockedTiles++;
                    }
                }
            }

            // Check for performance warnings
            if (unitCount > 100)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "HIGH_UNIT_COUNT",
                    Message = $"High number of units ({unitCount}) may impact performance"
                });
            }

            if (propertyCount > 50)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "HIGH_PROPERTY_COUNT",
                    Message = $"High number of properties ({propertyCount}) may impact game balance"
                });
            }

            if (blockedTiles > map.Width * map.Height * 0.7)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Code = "EXCESSIVE_BLOCKING",
                    Message = "Over 70% of tiles have movement restrictions"
                });
            }
        }
    }
}