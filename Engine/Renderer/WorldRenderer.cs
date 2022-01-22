using CorrinoEngine.Cameras;
using CorrinoEngine.Core;
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
        private static WorldRenderer instance;
        public static WorldRenderer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WorldRenderer();
                }
                return instance;
            }
        }

        private List<IRenderable> renderableObjects;
        public List<IRenderable> RenderableObjects
        {
            get { return renderableObjects; }
        }

        public WorldRenderer()
        {
            renderableObjects = new List<IRenderable>();
            instance = this;
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
