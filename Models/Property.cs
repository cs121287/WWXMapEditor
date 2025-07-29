namespace WwXMapEditor.Models
{
    public class Property
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; }
        public string Owner { get; set; } = "Neutral"; // Default owner
    }
}