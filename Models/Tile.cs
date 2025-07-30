namespace WwXMapEditor.Models
{
    public class Tile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TerrainType Terrain { get; set; } = TerrainType.Plain;
        public bool Traversable { get; set; } = true;
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