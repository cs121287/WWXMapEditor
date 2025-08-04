using System;
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
                NumberOfPlayers = properties.NumberOfPlayers,
                VictoryConditions = properties.VictoryConditions,
                FogOfWarSettings = properties.FogOfWarSettings,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            // Initialize tiles with the starting terrain
            map.Tiles = new Tile[properties.Width, properties.Height];
            for (int x = 0; x < properties.Width; x++)
            {
                for (int y = 0; y < properties.Height; y++)
                {
                    map.Tiles[x, y] = new Tile
                    {
                        X = x,
                        Y = y,
                        TerrainType = properties.StartingTerrain
                    };
                }
            }

            return map;
        }
    }
}