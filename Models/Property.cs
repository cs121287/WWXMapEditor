namespace WWXMapEditor.Models
{
    public class Property
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // City, Factory, Airport, etc.
        public string Owner { get; set; } = "Neutral";
        public int CapturePoints { get; set; } = 20;
        public int Income { get; set; } = 1000;
        public int X { get; set; }
        public int Y { get; set; }
    }
}