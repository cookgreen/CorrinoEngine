using CorrinoEngine.Assets;
using CorrinoEngine.Fields;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.Mods;
using CorrinoEngine.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Game
{
    public class World
    {
        private ModData modData;
        private AssetManager assetManager;

        public event Action<Actor> CreateActorFinished;

        public World(AssetManager assetManager, ModData modData)
        {
            this.assetManager = assetManager;
            this.modData = modData;
        }

        public Actor CreateActor(string actorTypeName)
        {
            ActorData actorData = modData.Manifest.ActorDataList.Where(o => o.TypeName == actorTypeName).FirstOrDefault();
            var actor = CreateActor(actorData);
            CreateActorFinished?.Invoke(actor);
            return actor;
        }

        public Actor CreateActor(ActorData actorData)
        {
            Mesh mesh = assetManager.Load<XbfMesh>(this, actorData["idle"].Resource);
            MeshInstance meshInstance = new MeshInstance(mesh);

            Actor actor = new Actor(meshInstance, actorData.DataField.Properties);
            WorldRenderer.Instance.AppendActor(actor);

            CreateActorFinished?.Invoke(actor);

            return actor;
        }
    }
}
