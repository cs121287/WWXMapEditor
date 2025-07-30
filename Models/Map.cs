using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WwXMapEditor.Models
{
    public class Map
    {
        public string Name { get; set; } = "UntitledMap";
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;
        public string Season { get; set; } = "Summer";
        public List<Tile> Tiles { get; set; } = new();
        [JsonIgnore]
        public Tile[,] TileArray { get; set; }
        public List<Property> Properties { get; set; } = new();
        public List<Unit> Units { get; set; } = new();
        public List<Player> Players { get; set; } = new();
        public WeatherType Weather { get; set; } = WeatherType.Random;
        public bool FogOfWarEnabled { get; set; } = true;
        public VictoryConditions VictoryConditions { get; set; } = new();
        public MapMetadata Metadata { get; set; } = new();

        public void BuildTileArray()
        {
            TileArray = new Tile[Width, Height];
            foreach (var t in Tiles)
                if (t.X >= 0 && t.X < Width && t.Y >= 0 && t.Y < Height)
                    TileArray[t.X, t.Y] = t;
        }

        public void FlattenTileArray()
        {
            Tiles.Clear();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (TileArray[x, y] != null)
                        Tiles.Add(TileArray[x, y]);
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