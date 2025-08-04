namespace WWXMapEditor.Models
{
    public class MapProperties
    {
        public string Name { get; set; } = "Untitled Map";
        public string Description { get; set; } = "";
        public int Width { get; set; } = 50;
        public int Height { get; set; } = 50;
        public string StartingTerrain { get; set; } = "Plains";
        public int NumberOfPlayers { get; set; } = 2;
        public VictoryConditions VictoryConditions { get; set; } = new VictoryConditions();
        public FogOfWarSettings FogOfWarSettings { get; set; } = new FogOfWarSettings();
    }

    public class VictoryConditions
    {
        public bool Elimination { get; set; } = true;
        public bool CaptureObjectives { get; set; }
        public bool Survival { get; set; }
        public bool Economic { get; set; }
    }

    public class FogOfWarSettings
    {
        public bool Enabled { get; set; } = true;
        public string ShroudType { get; set; } = "Black";
        public double VisionPenaltyMultiplier { get; set; } = 1.0;
    }
}