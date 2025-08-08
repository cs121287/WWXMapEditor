namespace WWXMapEditor.Models
{
    public class Unit
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
    }
}