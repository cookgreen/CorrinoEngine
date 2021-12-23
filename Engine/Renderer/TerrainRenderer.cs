using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorrinoEngine.Cameras;
using CorrinoEngine.Topography;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace CorrinoEngine.Renderer
{
    public class TerrainRenderer
    {
        private Terrain terrain;

        public TerrainRenderer()
        {
        }

        public TerrainRenderer(Terrain terrain)
        {
            this.terrain = terrain;
        }

        public void RenderTerrain(Terrain terrain)
        {
            this.terrain = terrain;
        }

        public void RenderFrame(FrameEventArgs args, Camera camera)
        {
            terrain.Draw(args, camera);
        }

        public void UpdateFrame(FrameEventArgs args)
        {
            terrain.Update(args);
        }
    }
}
