using CorrinoEngine.Cameras;
using CorrinoEngine.Graphics.Mesh;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Renderer
{
    public class WorldRenderer
    {
        private List<MeshInstance> meshInstances;

        public WorldRenderer()
        {
            meshInstances = new List<MeshInstance>();
        }

        public void RenderModel(MeshInstance meshInstance)
        {
            meshInstances.Add(meshInstance);
        }

        public void RenderFrame(FrameEventArgs args, Camera camera)
        {
            foreach (var meshInstance in meshInstances)
            {
                meshInstance.Draw(camera);
            }
        }

        public void UpdateFrame(FrameEventArgs args)
        {
            foreach(var meshInstance in meshInstances)
            {
                meshInstance.World *= Matrix4.CreateRotationY((float)args.Time / 5);
                meshInstance.Update((float)args.Time);
            }
        }
    }
}
