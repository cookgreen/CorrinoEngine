using CorrinoEngine.Cameras;
using CorrinoEngine.Game;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.UI;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Renderer
{
    public class WorldRenderer
    {
        private List<IRenderable> renderableObjects;

        public WorldRenderer()
        {
            renderableObjects = new List<IRenderable>();
        }

        public void RenderObject(IRenderable renderableObject)
        {
            renderableObjects.Add(renderableObject);
        }

        public void Render(Camera camera)
        {
            foreach(var renderableObject in renderableObjects)
            {
                renderableObject.Draw(camera);
            }
        }

        public void UpdateFrame(FrameEventArgs args)
        {
            foreach(var renderableObject in renderableObjects)
            {
                renderableObject.Update((float)args.Time);
            }
        }
    }
}
