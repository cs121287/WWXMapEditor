using System;

namespace WWXMapEditor.Models
{
    public class Map
    {
        public string Name { get; set; } = "Untitled Map";
        public string Description { get; set; } = "";
        public int Width { get; set; }
        public int Height { get; set; }
        public int NumberOfPlayers { get; set; }
        public string StartingTerrain { get; set; } = "Plains";
        public VictoryConditions VictoryConditions { get; set; } = new VictoryConditions();
        public FogOfWarSettings FogOfWarSettings { get; set; } = new FogOfWarSettings();
        public Tile[,] Tiles { get; set; } = new Tile[0, 0];
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class Tile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string TerrainType { get; set; } = "Plains";
        public int Elevation { get; set; }
        public Unit? Unit { get; set; }
        public Building? Building { get; set; }
        public bool HasCollision { get; set; }
        public bool AllowsAircraft { get; set; } = true;
    }

    public class Unit
    {
        public string Type { get; set; } = "";
        public int Player { get; set; }
        public int Health { get; set; }
    }

    public class Building
    {
        public string Type { get; set; } = "";
        public int Player { get; set; }
    }
}