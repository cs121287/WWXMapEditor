namespace WwXMapEditor.Models
{
    public class Player
    {
        public string Name { get; set; } = "Player";
        public string Country { get; set; } = "Unspecified";
        public bool IsAI { get; set; }
        public string Color { get; set; } = "Blue";
    }
}