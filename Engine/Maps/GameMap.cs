using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Maps
{
    public class GameMap
    {
        public GameMapManifest Manifest { get; set; }

        public List<GameMapTile> Tiles { get; set; }

        public GameMap()
        {
            Manifest = new GameMapManifest();
            Tiles = new List<GameMapTile>();
        }
    }

    public class GameMapTile
    {
        public string Mesh { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class GameMapManifest
    {
        public string Name { get; set; }
        public string Author { get; set; }

        public void Parse(string mapYamlFile)
        {
            
        }
    }
}
