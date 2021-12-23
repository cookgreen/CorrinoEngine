using CorrinoEngine.Cameras;
using CorrinoEngine.Fields;
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
        private ActorData actorData;
        private MeshInstance meshInstance;

        public ActorData ActorData
        {
            get { return actorData; }
        }

        public Actor(ActorData actorData)
        {
            this.actorData = actorData;
        }

        public IEnumerable<KeyValuePair<string, object>> GetFields(string fieldName)
        {
            return actorData.DataField.GetFields(fieldName);
        }

        public void Spawn(MeshInstance meshInstance)
        {
            this.meshInstance = meshInstance;
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
