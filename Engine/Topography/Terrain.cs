using CorrinoEngine.Cameras;
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
        private TerrainRenderer renderer;

        public List<TerrainTile> Tiles
        {
            get { return tiles; }
        }

        public Terrain(TerrainRenderer renderer)
        {
            tiles = new List<TerrainTile>();
            this.renderer = renderer;
        }

        public Terrain(TerrainRenderer renderer, List<TerrainTile> tiles)
        {
            this.tiles = tiles;
            this.renderer = renderer;
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
            foreach (var tile in tiles)
            {
                tile.MeshInstance.Draw(camera);
            }
        }

        public void Update(FrameEventArgs args)
        {
            foreach (var tile in tiles)
            {
                tile.MeshInstance.Update((float)args.Time);
            }
        }
    }
}
