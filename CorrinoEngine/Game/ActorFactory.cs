using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Game
{
    public class ActorFactory
    {
        private static ActorFactory instance;
        public static ActorFactory Instance
        {
            get { if (instance == null) { instance = new ActorFactory(); } return instance; }
        }

        public Actor CreateActor(MeshInstance meshInstance, Dictionary<string, string> actorProperties)
        {
            Actor actor = new Actor(meshInstance, actorProperties);
            WorldRenderer.Instance.AppendActor(actor);
            return actor;
        }
    }
}
