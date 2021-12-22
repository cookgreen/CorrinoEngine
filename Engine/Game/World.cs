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
        private Actor worldActor;
        private List<FactionInfo> factionInfos;

        public event Action<Actor> CreateActorFinished;

        public World(AssetManager assetManager, ModData modData)
        {
            this.assetManager = assetManager;
            this.modData = modData;

            factionInfos = new List<FactionInfo>();

            worldActor = CreateActor("World");
            parseFaction();
        }

        private void parseFaction()
        {
            var fieldValues = worldActor.GetFields("Faction");
            foreach(var fieldPair in fieldValues)
            {
                string[] tokens = fieldPair.Value.ToString().Split("^").Where(o => !string.IsNullOrEmpty(o)).ToArray();
                string[] line1 = tokens[0].Split(":");
                string[] line2 = tokens[1].Split(":");
                string[] line3 = tokens[2].Split(":");
                FactionInfo factionInfo = new FactionInfo(line1[1], line2[1], line3[1]);
                factionInfos.Add(factionInfo);
            }
        }

        public Actor CreateActor(string actorTypeName)
        {
            ActorData actorData = modData.Manifest.ActorDataList.Where(o => o.TypeName == actorTypeName).FirstOrDefault();
            Actor actor = new Actor(actorData);

            return actor;
        }

        public void SpawnActor(Actor actor)
        {
            Mesh mesh = assetManager.Load<XbfMesh>(this, actor.ActorData["idle"].Resource);
            MeshInstance meshInstance = new MeshInstance(mesh);

            actor.Spawn(meshInstance);

            CreateActorFinished?.Invoke(actor);
        }
    }
}
