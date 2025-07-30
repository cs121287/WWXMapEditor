using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WwXMapEditor.Models;

namespace WwXMapEditor
{
    public class UndoRedoManager
    {
        private readonly Stack<Map> _undoStack = new();
        private readonly Stack<Map> _redoStack = new();
        private Map _current;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Reset(Map map)
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _current = DeepCopy(map);
        }

        public Map Undo()
        {
            if (!CanUndo) return _current;
            _redoStack.Push(DeepCopy(_current));
            _current = _undoStack.Pop();
            return DeepCopy(_current);
        }

        public Map Redo()
        {
            if (!CanRedo) return _current;
            _undoStack.Push(DeepCopy(_current));
            _current = _redoStack.Pop();
            return DeepCopy(_current);
        }

        public void Push(Map map)
        {
            _undoStack.Push(DeepCopy(_current));
            _current = DeepCopy(map);
            _redoStack.Clear();
        }

        private Map DeepCopy(Map map)
        {
            if (map == null) return null;

            // Create a new map instance with basic properties
            var newMap = new Map
            {
                Name = map.Name,
                Width = map.Width,
                Height = map.Height,
                Season = map.Season,
                Weather = map.Weather,
                FogOfWarEnabled = map.FogOfWarEnabled
            };

            // Deep copy metadata
            if (map.Metadata != null)
            {
                newMap.Metadata = new MapMetadata
                {
                    Author = map.Metadata.Author,
                    Created = map.Metadata.Created
                };
            }

            // Deep copy victory conditions
            if (map.VictoryConditions != null)
            {
                newMap.VictoryConditions = new VictoryConditions
                {
                    Type = map.VictoryConditions.Type,
                    TurnLimit = map.VictoryConditions.TurnLimit,
                    PointsTarget = map.VictoryConditions.PointsTarget,
                    CustomCondition = map.VictoryConditions.CustomCondition
                };
            }

            // Deep copy tiles
            newMap.Tiles = new List<Tile>();
            newMap.TileArray = new Tile[map.Width, map.Height];

            // Copy from TileArray if it exists and is populated
            if (map.TileArray != null)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        var originalTile = map.TileArray[x, y];
                        if (originalTile != null)
                        {
                            var newTile = new Tile
                            {
                                X = originalTile.X,
                                Y = originalTile.Y,
                                Terrain = originalTile.Terrain,
                                Traversable = originalTile.Traversable,
                                SpriteIndex = originalTile.SpriteIndex
                            };
                            newMap.TileArray[x, y] = newTile;
                            newMap.Tiles.Add(newTile);
                        }
                    }
                }
            }
            // If TileArray is not populated, copy from Tiles list
            else if (map.Tiles != null && map.Tiles.Count > 0)
            {
                foreach (var originalTile in map.Tiles)
                {
                    var newTile = new Tile
                    {
                        X = originalTile.X,
                        Y = originalTile.Y,
                        Terrain = originalTile.Terrain,
                        Traversable = originalTile.Traversable,
                        SpriteIndex = originalTile.SpriteIndex
                    };
                    newMap.Tiles.Add(newTile);

                    // Ensure bounds are valid
                    if (newTile.X >= 0 && newTile.X < map.Width &&
                        newTile.Y >= 0 && newTile.Y < map.Height)
                    {
                        newMap.TileArray[newTile.X, newTile.Y] = newTile;
                    }
                }
            }

            // Deep copy properties
            newMap.Properties = new List<Property>();
            if (map.Properties != null)
            {
                foreach (var originalProp in map.Properties)
                {
                    var newProp = new Property
                    {
                        X = originalProp.X,
                        Y = originalProp.Y,
                        Type = originalProp.Type,
                        Owner = originalProp.Owner,
                        VisionRange = originalProp.VisionRange,
                        Income = originalProp.Income
                    };
                    newMap.Properties.Add(newProp);
                }
            }

            // Deep copy units
            newMap.Units = new List<Unit>();
            if (map.Units != null)
            {
                foreach (var originalUnit in map.Units)
                {
                    var newUnit = new Unit
                    {
                        X = originalUnit.X,
                        Y = originalUnit.Y,
                        Type = originalUnit.Type,
                        Owner = originalUnit.Owner,
                        HP = originalUnit.HP,
                        Fuel = originalUnit.Fuel,
                        Ammo = originalUnit.Ammo,
                        MovementType = originalUnit.MovementType,
                        MovementRange = originalUnit.MovementRange,
                        VisionRange = originalUnit.VisionRange,
                        HasMoved = originalUnit.HasMoved,
                        HasAttacked = originalUnit.HasAttacked
                    };
                    newMap.Units.Add(newUnit);
                }
            }

            // Deep copy players
            newMap.Players = new List<Player>();
            if (map.Players != null)
            {
                foreach (var originalPlayer in map.Players)
                {
                    var newPlayer = new Player
                    {
                        Name = originalPlayer.Name,
                        Country = originalPlayer.Country,
                        IsAI = originalPlayer.IsAI,
                        Color = originalPlayer.Color
                    };
                    newMap.Players.Add(newPlayer);
                }
            }

            // Ensure TileArray is properly initialized even if empty
            if (newMap.TileArray == null)
            {
                newMap.TileArray = new Tile[newMap.Width, newMap.Height];
            }

            return newMap;
        }

        // Helper method to validate and ensure map integrity
        private void ValidateMap(Map map)
        {
            if (map == null) return;

            // Ensure TileArray is initialized
            if (map.TileArray == null)
            {
                map.TileArray = new Tile[map.Width, map.Height];
            }

            // Rebuild TileArray from Tiles list if needed
            if (map.Tiles != null && map.Tiles.Count > 0)
            {
                // Check if TileArray is empty
                bool isEmpty = true;
                for (int y = 0; y < map.Height && isEmpty; y++)
                {
                    for (int x = 0; x < map.Width && isEmpty; x++)
                    {
                        if (map.TileArray[x, y] != null)
                        {
                            isEmpty = false;
                        }
                    }
                }

                // If empty, rebuild from Tiles list
                if (isEmpty)
                {
                    foreach (var tile in map.Tiles)
                    {
                        if (tile.X >= 0 && tile.X < map.Width &&
                            tile.Y >= 0 && tile.Y < map.Height)
                        {
                            map.TileArray[tile.X, tile.Y] = tile;
                        }
                    }
                }
            }
        }

        // Public method to get the current state with validation
        public Map GetCurrentState()
        {
            if (_current != null)
            {
                ValidateMap(_current);
            }
            return _current;
        }
    }
}