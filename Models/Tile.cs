namespace WWXMapEditor.Models
{
    public class Tile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string TerrainType { get; set; } = "Plains";
        public bool IsLandBlocked { get; set; }
        public bool IsAirBlocked { get; set; }
        public bool IsWaterBlocked { get; set; }
        public Unit? Unit { get; set; }
        public Property? Property { get; set; }
    }
}