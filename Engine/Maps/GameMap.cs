using CorrinoEngine.FileFormats;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System;

namespace CorrinoEngine.Maps
{
    public class GameMap
    {
        public GameMapManifest Manifest { get; set; }
        public GameMapMetadata Metadata { get; set; }

        public List<GameMapTile> Tiles { get; set; }
        public List<GameMapActor> Actors { get; set; }

        public GameMap()
        {
            Manifest = new GameMapManifest();
            Metadata = new GameMapMetadata();
            Tiles = new List<GameMapTile>();
            Actors = new List<GameMapActor>();
        }

        public void Load(string mapYamlFile)
        {
            Manifest.Parse(mapYamlFile);

            Tiles.Clear();
            Actors.Clear();

            MiniYaml miniYaml = new MiniYaml(mapYamlFile);
            foreach (var node in miniYaml.Nodes)
            {
                if (node.Name.StartsWith("Tile@"))
                {
                    Tiles.Add(ParseTile(node));
                }
                else if (node.Name.StartsWith("Actor@"))
                {
                    Actors.Add(ParseActor(node));
                }
                else if (node.Name == "Metadata")
                {
                    ParseMetadata(node);
                }
            }

            if (Tiles.Count == 0)
            {
                GenerateTiles();
            }
        }

        private void GenerateTiles()
        {
            for (int z = 0; z < Manifest.Height; z++)
            {
                for (int x = 0; x < Manifest.Width; x++)
                {
                    Tiles.Add(new GameMapTile()
                    {
                        Mesh = Manifest.TileMesh,
                        Material = Manifest.TileMesh,
                        X = x * Manifest.TileSize + Manifest.TileSize * 0.5f,
                        Y = 0,
                        Z = z * Manifest.TileSize + Manifest.TileSize * 0.5f
                    });
                }
            }
        }

        private static GameMapTile ParseTile(MiniYamlNode node)
        {
            GameMapTile tile = new GameMapTile();
            foreach (var subNode in node.ChildNodes)
            {
                if (subNode.Name == "Mesh")
                {
                    tile.Mesh = subNode.Value;
                }
                else if (subNode.Name == "Material")
                {
                    tile.Material = subNode.Value;
                }
                else if (subNode.Name == "Texture")
                {
                    tile.Texture = subNode.Value;
                }
                else if (subNode.Name == "Resource")
                {
                    tile.Resource = subNode.Value;
                }
                else if (subNode.Name == "UvScale")
                {
                    tile.UvScale = ParseFloat(subNode.Value);
                }
                else if (subNode.Name == "Buildable")
                {
                    tile.Buildable = ParseBool(subNode.Value);
                }
                else if (subNode.Name == "X")
                {
                    tile.X = ParseFloat(subNode.Value);
                }
                else if (subNode.Name == "Y")
                {
                    tile.Y = ParseFloat(subNode.Value);
                }
                else if (subNode.Name == "Z")
                {
                    tile.Z = ParseFloat(subNode.Value);
                }
            }

            return tile;
        }

        private static GameMapActor ParseActor(MiniYamlNode node)
        {
            GameMapActor actor = new GameMapActor();
            foreach (var subNode in node.ChildNodes)
            {
                if (subNode.Name == "Type")
                {
                    actor.Type = subNode.Value;
                }
                else if (subNode.Name == "X")
                {
                    actor.X = ParseFloat(subNode.Value);
                }
                else if (subNode.Name == "Y")
                {
                    actor.Y = ParseFloat(subNode.Value);
                }
                else if (subNode.Name == "Z")
                {
                    actor.Z = ParseFloat(subNode.Value);
                }
            }

            return actor;
        }

        private static float ParseFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
                ? result
                : 0f;
        }

        private static bool? ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (bool.TryParse(value, out bool boolValue))
            {
                return boolValue;
            }

            return null;
        }

        private void ParseMetadata(MiniYamlNode node)
        {
            foreach (var subNode in node.ChildNodes)
            {
                if (subNode.Name == "OriginalMapDir")
                {
                    Metadata.OriginalMapDir = subNode.Value;
                }
                else if (subNode.Name == "OriginalMapXbf")
                {
                    Metadata.OriginalMapXbf = subNode.Value;
                }
                else if (subNode.Name == "GroundColor")
                {
                    Metadata.GroundColor = subNode.Value;
                }
                else if (subNode.Name == "GroundPalette")
                {
                    Metadata.GroundPalette = subNode.Value;
                }
                else if (subNode.Name == "GroundLight")
                {
                    Metadata.GroundLight = subNode.Value;
                }
                else if (subNode.Name == "GroundLit")
                {
                    Metadata.GroundLit = subNode.Value;
                }
                else if (subNode.Name == "MapScale")
                {
                    Metadata.MapScale = ParseFloat(subNode.Value);
                }
            }
        }
    }

    public class GameMapTile
    {
        public string Mesh { get; set; }
        public string Material { get; set; }
        public string Texture { get; set; }
        public string Resource { get; set; }
        public float UvScale { get; set; } = 1;
        public bool? Buildable { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class GameMapActor
    {
        public string Type { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class GameMapManifest
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float TileSize { get; set; }
        public string TileMesh { get; set; }
        public string TileTexture { get; set; }
        public string TileResource { get; set; }
        public float TileUvScale { get; set; }

        public GameMapManifest()
        {
            Width = 12;
            Height = 12;
            TileSize = 48;
            TileUvScale = 1;
        }

        public void Parse(string mapYamlFile)
        {
            MiniYaml miniYaml = new MiniYaml(mapYamlFile);
            MiniYamlNode mapNode = miniYaml.Nodes.Where(o => o.Name == "Map").FirstOrDefault();
            if (mapNode == null)
            {
                return;
            }

            foreach (var node in mapNode.ChildNodes)
            {
                if (node.Name == "Name")
                {
                    Name = node.Value;
                }
                else if (node.Name == "Author")
                {
                    Author = node.Value;
                }
                else if (node.Name == "Width")
                {
                    Width = int.TryParse(node.Value, out int width) ? width : Width;
                }
                else if (node.Name == "Height")
                {
                    Height = int.TryParse(node.Value, out int height) ? height : Height;
                }
                else if (node.Name == "TileSize")
                {
                    TileSize = ParseManifestFloat(node.Value, TileSize);
                }
                else if (node.Name == "TileMesh")
                {
                    TileMesh = node.Value;
                }
                else if (node.Name == "TileTexture")
                {
                    TileTexture = node.Value;
                }
                else if (node.Name == "TileResource")
                {
                    TileResource = node.Value;
                }
                else if (node.Name == "TileUvScale")
                {
                    TileUvScale = ParseManifestFloat(node.Value, TileUvScale);
                }
            }
        }

        private static float ParseManifestFloat(string value, float fallback)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
                ? result
                : fallback;
        }
    }

    public class GameMapMetadata
    {
        public string OriginalMapDir { get; set; }
        public string OriginalMapXbf { get; set; }
        public string GroundColor { get; set; }
        public string GroundPalette { get; set; }
        public string GroundLight { get; set; }
        public string GroundLit { get; set; }
        public float MapScale { get; set; } = 0.0625f;
        public bool HasOriginalMapData =>
            !string.IsNullOrWhiteSpace(OriginalMapDir) || !string.IsNullOrWhiteSpace(OriginalMapXbf);
    }
}
