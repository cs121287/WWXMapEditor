using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WwXMapEditor.Models
{
    public class Map
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "UntitledMap";

        [JsonPropertyName("width")]
        public int Width { get; set; } = 100;

        [JsonPropertyName("height")]
        public int Height { get; set; } = 100;

        [JsonPropertyName("season")]
        public string Season { get; set; } = "Summer";

        [JsonPropertyName("tiles")]
        public List<Tile> Tiles { get; set; } = new();

        [JsonIgnore]
        public Tile[,] TileArray { get; set; }

        [JsonPropertyName("properties")]
        public List<Property> Properties { get; set; } = new();

        [JsonPropertyName("units")]
        public List<Unit> Units { get; set; } = new();

        [JsonPropertyName("players")]
        public List<Player> Players { get; set; } = new();

        [JsonPropertyName("weather")]
        public WeatherType Weather { get; set; } = WeatherType.Random;

        [JsonPropertyName("fogOfWarEnabled")]
        public bool FogOfWarEnabled { get; set; } = true;

        [JsonPropertyName("victoryConditions")]
        public VictoryConditions VictoryConditions { get; set; } = new();

        [JsonPropertyName("metadata")]
        public MapMetadata Metadata { get; set; } = new();

        public Map()
        {
            TileArray = new Tile[Width, Height];
        }

        public void BuildTileArray()
        {
            TileArray = new Tile[Width, Height];

            // First, initialize all tiles to ensure no null values
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    TileArray[x, y] = new Tile
                    {
                        X = x,
                        Y = y,
                        Terrain = TerrainType.Plain,
                        Traversable = true,
                        SpriteIndex = 0
                    };
                }
            }

            // Then override with loaded tiles
            foreach (var tile in Tiles)
            {
                if (tile != null && tile.X >= 0 && tile.X < Width && tile.Y >= 0 && tile.Y < Height)
                {
                    TileArray[tile.X, tile.Y] = tile;
                }
            }
        }

        public void FlattenTileArray()
        {
            Tiles.Clear();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var tile = TileArray[x, y];
                    if (tile != null)
                    {
                        // Ensure coordinates are correct
                        tile.X = x;
                        tile.Y = y;
                        Tiles.Add(tile);
                    }
                }
            }
        }
    }

    public enum WeatherType
    {
        Random,
        Clear,
        Rain,
        Snow
    }
}