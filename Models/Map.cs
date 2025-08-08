using System.Collections.Generic;

namespace WWXMapEditor.Models
{
    public class Map
    {
        public string Name { get; set; } = "Untitled Map";
        public string Description { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public string StartingTerrain { get; set; } = "Plains";
        public int NumberOfPlayers { get; set; } = 2;
        public Tile[,] Tiles { get; set; } = new Tile[0, 0];
        public List<HQ> HQs { get; set; } = new List<HQ>();
        public VictoryConditions? VictoryConditions { get; set; }
        public FogOfWarSettings? FogOfWarSettings { get; set; }
    }
}