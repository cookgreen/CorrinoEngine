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
        private List<MeshInstance> meshInstances;
        private GameWindow wnd;
        private List<Actor> actors;


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

        public WorldRenderer()
        {
            meshInstances = new List<MeshInstance>();
            actors = new List<Actor>();
        }

        public void Init(GameWindow wnd)
        {
            this.wnd = wnd;
        }

        public void Loaded()
        {
        }

        public void AppendActor(Actor actor)
        {
            actors.Add(actor);
        }

        public void RenderModel(MeshInstance meshInstance)
        {
            meshInstances.Add(meshInstance);
        }

        public void UnloadCurrentModel()
        {
            if (meshInstances.Count > 0)
            {
                meshInstances.RemoveAt(0);
            }
        }

        public void RenderFrame(FrameEventArgs args, Camera camera)
        {
            foreach (var meshInstance in meshInstances)
            {
                meshInstance.Draw(camera);
            }
            foreach (var actor in actors)
            {
                actor.Draw(args, camera);
            }
        }

        public void UpdateFrame(FrameEventArgs args)
        {
            foreach(var meshInstance in meshInstances)
            {
                meshInstance.World *= Matrix4.CreateRotationY((float)args.Time / 5);
                meshInstance.Update((float)args.Time);
            }
            foreach (var actor in actors)
            {
                actor.Update(args);
            }
        }
    }
}
