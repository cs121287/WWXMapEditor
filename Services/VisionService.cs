using System;
using System.Collections.Generic;
using WwXMapEditor.Models;

namespace WwXMapEditor.Services
{
    public static class VisionService
    {
        public static int GetEffectiveVisionRange(Unit unit, TerrainType terrain, WeatherType weather)
        {
            int baseVision = unit.VisionRange;
            int terrainModifier = GetTerrainVisionModifier(unit.MovementType, terrain);
            int weatherPenalty = WeatherService.GetVisionPenalty(weather);
            
            return Math.Max(1, baseVision + terrainModifier - weatherPenalty);
        }

        public static int GetEffectiveVisionRange(Property property, WeatherType weather)
        {
            int baseVision = property.VisionRange;
            int weatherPenalty = WeatherService.GetVisionPenalty(weather);
            
            return Math.Max(1, baseVision - weatherPenalty);
        }

        private static int GetTerrainVisionModifier(MovementType movementType, TerrainType terrain)
        {
            switch (movementType)
            {
                case MovementType.Infantry:
                    switch (terrain)
                    {
                        case TerrainType.Mountain: return 4;
                        case TerrainType.Forest: return -2;
                        case TerrainType.Sea: return -5;
                        case TerrainType.City:
                        case TerrainType.HQ: return 2;
                        default: return 0;
                    }
                case MovementType.Wheeled:
                case MovementType.Treaded:
                    switch (terrain)
                    {
                        case TerrainType.Road:
                        case TerrainType.Bridge: return 2;
                        case TerrainType.Forest: return -3;
                        case TerrainType.Mountain: return -2;
                        default: return 0;
                    }
                case MovementType.Air:
                    switch (terrain)
                    {
                        case TerrainType.Airport: return 2;
                        case TerrainType.Mountain: return -1;
                        case TerrainType.Sea: return 1;
                        default: return 0;
                    }
                case MovementType.Ship:
                    switch (terrain)
                    {
                        case TerrainType.Sea: return 3;
                        case TerrainType.Port: return 2;
                        default: return -5;
                    }
                case MovementType.Lander:
                    switch (terrain)
                    {
                        case TerrainType.Beach: return 2;
                        case TerrainType.Sea: return 1;
                        case TerrainType.River: return 1;
                        default: return 0;
                    }
                default:
                    return 0;
            }
        }

        public static List<(int x, int y)> GetVisibleTiles(int centerX, int centerY, int visionRange, int mapWidth, int mapHeight)
        {
            var visibleTiles = new List<(int x, int y)>();
            
            for (int dx = -visionRange; dx <= visionRange; dx++)
            {
                for (int dy = -visionRange; dy <= visionRange; dy++)
                {
                    if (Math.Abs(dx) + Math.Abs(dy) <= visionRange) // Manhattan distance
                    {
                        int x = centerX + dx;
                        int y = centerY + dy;
                        
                        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                        {
                            visibleTiles.Add((x, y));
                        }
                    }
                }
            }
            
            return visibleTiles;
        }
    }

    public enum VisibilityState
    {
        Unseen,
        PreviouslySeen,
        Visible
    }
}