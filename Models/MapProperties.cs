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
        // Existing properties
        public bool Elimination { get; set; } = true;
        public bool CaptureObjectives { get; set; }
        public bool Survival { get; set; }
        public bool Economic { get; set; }

        // Additional properties needed by MapValidationService
        public bool CaptureHQ { get; set; } = true;
        public bool DefeatAllUnits { get; set; } = false;
        public bool CaptureProperties { get; set; } = false;
        public int RequiredProperties { get; set; } = 0;
        public bool TurnLimit { get; set; } = false;
        public int MaxTurns { get; set; } = 100;
    }

    public class FogOfWarSettings
    {
        // Existing properties
        public bool Enabled { get; set; } = true;
        public string ShroudType { get; set; } = "Black";
        public double VisionPenaltyMultiplier { get; set; } = 1.0;

        // Additional properties that might be useful
        public int InitialVisionRange { get; set; } = 3;
        public bool RevealTerrain { get; set; } = true;
        public bool PersistentVision { get; set; } = false;
    }
}