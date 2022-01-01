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
        private List<Field> fields;

        public ActorData ActorData
        {
            get { return actorData; }
        }

        public Actor(ActorData actorData)
        {
            this.actorData = actorData;
            parseFields();
        }

        private void parseFields()
        {
            fields = new List<Field>();
            foreach(var property in actorData.DataField.Properties)
            {
                if(FieldManager.Instance.TryParse(property.Key))
                {
                    fields.Add(FieldManager.Instance.Parse(property.Key));
                }
            }
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

        public void OnSelect()
        {
            foreach (var field in fields)
            {
                if ((field as ISelectable) != null)
                {
                    field.Execute();
                }
            }
        }

        public void Update(FrameEventArgs args)
        {
            meshInstance.Update((float)args.Time);
        }
    }
}
