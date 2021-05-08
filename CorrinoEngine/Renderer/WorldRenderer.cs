using CorrinoEngine.Cameras;
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
        private ImGuiController imGuiController;
        private List<MeshInstance> meshInstances;
        private GameWindow wnd;

        public WorldRenderer(int width, int height, GameWindow wnd)
        {
            this.wnd = wnd;
            meshInstances = new List<MeshInstance>();
        }

        public void Loaded()
        {
            imGuiController = new ImGuiController(wnd.Size.X, wnd.Size.Y);
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

            imGuiController?.Render();
        }

        public void UpdateFrame(FrameEventArgs args)
        {
            foreach(var meshInstance in meshInstances)
            {
                meshInstance.World *= Matrix4.CreateRotationY((float)args.Time / 5);
                meshInstance.Update((float)args.Time);
            }

            imGuiController?.Update(wnd, (float)args.Time);
        }
    }
}
