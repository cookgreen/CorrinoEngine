using CorrinoEngine.Cameras;
using CorrinoEngine.Fields;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.PathFind;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Core
{
    public class Actor
    {
        private ActorData actorData;
        private MeshInstance meshInstance;
        private List<Field> fields;
        private Vector3 moveTarget;
        private bool isSelected;
        private float moveSpeed;
        private float selectionRadius;

        public ActorData ActorData
        {
            get { return actorData; }
        }
        public MeshInstance MeshInstance
        {
            get { return meshInstance; }
        }
        public Vector3 Position
        {
            get
            {
                if (meshInstance == null)
                {
                    return Vector3.Zero;
                }

                return meshInstance.Position;
            }
        }
        public float SelectionRadius
        {
            get { return selectionRadius; }
        }
        public bool IsSelected
        {
            get { return isSelected; }
        }

        public Actor(ActorData actorData)
        {
            this.actorData = actorData;
            moveTarget = Vector3.Zero;
            moveSpeed = 72;
            selectionRadius = 36;
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

        public bool HasField(string fieldName)
        {
            return actorData.DataField.Properties.ContainsKey(fieldName);
        }

        public object GetFieldValue(string fieldName)
        {
            if (actorData.DataField.Properties.ContainsKey(fieldName))
            {
                return actorData.DataField.Properties[fieldName];
            }

            return null;
        }

        public void Spawn(MeshInstance meshInstance)
        {
            this.meshInstance = meshInstance;
            moveTarget = meshInstance.Position;
        }

        public void Draw(FrameEventArgs args, Camera camera)
        {
            meshInstance.Draw(camera);
        }

        public void OnSelect()
        {
            isSelected = true;
            foreach (var field in fields)
            {
                if ((field as ISelectable) != null)
                {
                    field.Execute();
                }
            }
        }

        public void OnDeselect()
        {
            isSelected = false;
        }

        public void MoveTo(Vector3 target)
        {
            moveTarget = target;
        }

        public void Update(FrameEventArgs args)
        {
            if (meshInstance != null)
            {
                Vector3 delta = moveTarget - meshInstance.Position;
                if (delta.LengthSquared > 1f)
                {
                    Vector3 step = delta.Normalized() * moveSpeed * (float)args.Time;
                    if (step.LengthSquared >= delta.LengthSquared)
                    {
                        meshInstance.Position = moveTarget;
                    }
                    else
                    {
                        meshInstance.Position += step;
                    }
                }
            }
        }
    }
}
