namespace WwXMapEditor.Models
{
    public class Tile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Terrain { get; set; } = "Plain";
        public bool Traversable { get; set; } = true;
    }
}