using System.IO;
using System.Text.Json;
using WwXMapEditor.Models;

namespace WwXMapEditor.Services
{
    public static class MapService
    {
        public static void SaveMap(Map map, string filename)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(map, options);
            File.WriteAllText(filename, json);
        }

        public static Map LoadMap(string filename)
        {
            var json = File.ReadAllText(filename);
            var map = JsonSerializer.Deserialize<Map>(json);
            return map ?? new Map
            {
                Name = "UntitledMap",
                Width = 100,
                Height = 100,
                Season = "Summer",
                Weather = "Clear",
                Tiles = new(),
                TileArray = new Tile[100, 100],
                Properties = new(),
                Units = new(),
                Players = new(),
                VictoryConditions = new(),
                Metadata = new MapMetadata { Author = System.Environment.UserName, Created = System.DateTime.Now.ToString("yyyy-MM-dd") }
            };
        }
    }
}