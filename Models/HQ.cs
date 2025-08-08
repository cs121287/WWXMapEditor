namespace WWXMapEditor.Models
{
    public class HQ
    {
        public string Id { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public int CapturePoints { get; set; } = 20;
    }
}