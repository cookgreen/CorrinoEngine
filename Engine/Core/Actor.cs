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
        private Queue<ProductionOrder> productionQueue;
        private Vector3 moveTarget;
        private bool isSelected;
        private float moveSpeed;
        private float selectionRadius;
        private Func<float, float, float> groundHeightProvider;
        private bool isUnderConstruction;
        private float constructionProgress;
        private float constructionDuration;
        private bool constructionCompletedThisFrame;

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
        public bool IsUnderConstruction
        {
            get { return isUnderConstruction; }
        }
        public bool IsOperational
        {
            get { return !isUnderConstruction; }
        }
        public float ConstructionProgress01
        {
            get
            {
                if (!isUnderConstruction || constructionDuration <= 0f)
                {
                    return 1f;
                }

                return Math.Clamp(constructionProgress / constructionDuration, 0f, 1f);
            }
        }
        public bool IsBuilding
        {
            get
            {
                string category = GetFieldValue("Category")?.ToString();
                if (string.IsNullOrWhiteSpace(category))
                {
                    category = GetFieldValue("ActorCategory")?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    return string.Equals(category, "building", StringComparison.OrdinalIgnoreCase);
                }

                return HasField("ProvideBuildings");
            }
        }
        public bool CanMove
        {
            get
            {
                return !IsBuilding;
            }
        }
        public IReadOnlyCollection<ProductionOrder> ProductionQueue
        {
            get { return productionQueue.ToArray(); }
        }

        public Actor(ActorData actorData)
        {
            this.actorData = actorData;
            productionQueue = new Queue<ProductionOrder>();
            moveTarget = Vector3.Zero;
            moveSpeed = 72;
            selectionRadius = 36;
            parseFields();
            if (IsBuilding)
            {
                selectionRadius = 72;
            }
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

        public void SetGroundHeightProvider(Func<float, float, float> provider)
        {
            groundHeightProvider = provider;
            if (meshInstance != null)
            {
                meshInstance.Position = SnapToGround(meshInstance.Position);
                moveTarget = SnapToGround(moveTarget);
            }
        }

        public void StartConstruction(float duration)
        {
            isUnderConstruction = true;
            constructionProgress = 0f;
            constructionDuration = Math.Max(0.25f, duration);
            constructionCompletedThisFrame = false;
        }

        public bool ConsumeConstructionCompleted()
        {
            bool completed = constructionCompletedThisFrame;
            constructionCompletedThisFrame = false;
            return completed;
        }

        public void EnqueueProduction(ProductionOrder order)
        {
            productionQueue.Enqueue(order);
        }

        public ProductionOrder PeekProduction()
        {
            if (productionQueue.Count == 0)
            {
                return null;
            }

            return productionQueue.Peek();
        }

        public ProductionOrder DequeueProduction()
        {
            if (productionQueue.Count == 0)
            {
                return null;
            }

            return productionQueue.Dequeue();
        }

        public void CancelProduction()
        {
            if (productionQueue.Count == 0)
            {
                return;
            }

            productionQueue.Dequeue();
        }

        public bool CancelProduction(Guid orderId, out ProductionOrder removedOrder)
        {
            removedOrder = null;
            if (productionQueue.Count == 0)
            {
                return false;
            }

            Queue<ProductionOrder> rebuiltQueue = new Queue<ProductionOrder>();
            bool removed = false;

            while (productionQueue.Count > 0)
            {
                ProductionOrder current = productionQueue.Dequeue();
                if (!removed && current.Id == orderId)
                {
                    removedOrder = current;
                    removed = true;
                    continue;
                }

                rebuiltQueue.Enqueue(current);
            }

            productionQueue = rebuiltQueue;
            return removed;
        }

        public void Update(FrameEventArgs args)
        {
            constructionCompletedThisFrame = false;
            if (meshInstance != null)
            {
                Vector2 currentXZ = new Vector2(meshInstance.Position.X, meshInstance.Position.Z);
                Vector2 targetXZ = new Vector2(moveTarget.X, moveTarget.Z);
                Vector2 deltaXZ = targetXZ - currentXZ;
                if (deltaXZ.LengthSquared > 1f)
                {
                    Vector2 stepXZ = deltaXZ.Normalized() * moveSpeed * (float)args.Time;
                    if (stepXZ.LengthSquared >= deltaXZ.LengthSquared)
                    {
                        meshInstance.Position = SnapToGround(moveTarget);
                    }
                    else
                    {
                        Vector2 nextXZ = currentXZ + stepXZ;
                        meshInstance.Position = SnapToGround(new Vector3(nextXZ.X, meshInstance.Position.Y, nextXZ.Y));
                    }
                }
                else
                {
                    meshInstance.Position = SnapToGround(meshInstance.Position);
                }
            }

            if (isUnderConstruction)
            {
                constructionProgress += (float)args.Time;
                if (constructionProgress >= constructionDuration)
                {
                    constructionProgress = constructionDuration;
                    isUnderConstruction = false;
                    constructionCompletedThisFrame = true;
                }
            }
        }

        private Vector3 SnapToGround(Vector3 position)
        {
            if (groundHeightProvider == null)
            {
                return position;
            }

            return new Vector3(position.X, groundHeightProvider(position.X, position.Z), position.Z);
        }
    }
}
