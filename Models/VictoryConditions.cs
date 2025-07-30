namespace WwXMapEditor.Models
{
    public class VictoryConditions
    {
        public string Type { get; set; } = "Elimination";
        public int? TurnLimit { get; set; }
        public int? PointsTarget { get; set; }
        public string? CustomCondition { get; set; }
    }
}