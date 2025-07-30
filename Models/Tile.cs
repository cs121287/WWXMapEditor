using System.Text.Json.Serialization;

namespace WwXMapEditor.Models
{
    public class Tile
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("terrain")]
        public TerrainType Terrain { get; set; } = TerrainType.Plain;

        [JsonPropertyName("traversable")]
        public bool Traversable { get; set; } = true;

        [JsonPropertyName("spriteIndex")]
        public int? SpriteIndex { get; set; }

        public Tile()
        {
            // Set default values
            Terrain = TerrainType.Plain;
            Traversable = true;
            SpriteIndex = null;
        }
    }

    public enum TerrainType
    {
        Plain,
        Forest,
        Mountain,
        Road,
        Bridge,
        Sea,
        Beach,
        River,
        City,
        Factory,
        HQ,
        Airport,
        Port
    }
}