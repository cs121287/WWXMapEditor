namespace WWXMapEditor.Models
{
    public class HQ
    {
        public string Id { get; set; } = string.Empty;
        public string Owner { get; set; } = "Player 1";
        public int CapturePoints { get; set; } = 60;
        public int X { get; set; }
        public int Y { get; set; }
    }
}