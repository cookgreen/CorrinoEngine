using CorrinoEngine.Cameras;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.Renderer;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Topography
{
    public class Terrain
    {
        private List<TerrainTile> tiles;
        private List<MeshInstance> layers;
        private TerrainRenderer renderer;

        public List<TerrainTile> Tiles
        {
            get { return tiles; }
        }
        public IReadOnlyList<MeshInstance> Layers
        {
            get { return layers; }
        }

        public Terrain(TerrainRenderer renderer)
        {
            tiles = new List<TerrainTile>();
            layers = new List<MeshInstance>();
            this.renderer = renderer;
        }

        public Terrain(TerrainRenderer renderer, List<TerrainTile> tiles)
        {
            this.tiles = tiles;
            this.layers = new List<MeshInstance>();
            this.renderer = renderer;
        }

        public void AddLayer(MeshInstance meshInstance)
        {
            if (meshInstance != null)
            {
                layers.Add(meshInstance);
            }
        }

        public void AppendTile(TerrainTile tile)
        {
            tiles.Add(tile);
        }

        public void DeleteTile(TerrainTile tile)
        {
            tiles.Remove(tile);
        }

        public void DeleteAt(int index)
        {
            tiles.RemoveAt(index);
        }

        public void Draw(FrameEventArgs args, Camera camera)
        {
            foreach (var layer in layers)
            {
                layer.Draw(camera);
            }

            foreach (var tile in tiles)
            {
                tile.MeshInstance?.Draw(camera);
            }
        }

        public void Update(FrameEventArgs args)
        {
            foreach (var layer in layers)
            {
                layer.Update((float)args.Time);
            }

            foreach (var tile in tiles)
            {
                tile.MeshInstance?.Update((float)args.Time);
            }
        }
    }
}
