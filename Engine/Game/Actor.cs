using CorrinoEngine.Cameras;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.PathFind;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Game
{
    public class Actor
    {
        private IPathFind pathFinder;
        private MeshInstance meshInstance;
        private Dictionary<string, object> actorProperties;
        private VPos location;

        public Actor(MeshInstance meshInstance, Dictionary<string, object> actorProperties)
        {
            pathFinder = new AStarPathFind();
            this.actorProperties = actorProperties;
            this.meshInstance = meshInstance;
            location = new VPos();
        }

        public void Draw(FrameEventArgs args, Camera camera)
        {
            meshInstance.Draw(camera);
        }

        public void Update(FrameEventArgs args)
        {
            meshInstance.Update((float)args.Time);
        }
    }
}
