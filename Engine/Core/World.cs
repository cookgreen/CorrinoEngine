using CorrinoEngine.Assets;
using CorrinoEngine.Cameras;
using CorrinoEngine.Fields;
using CorrinoEngine.FileSystem;
using CorrinoEngine.Forms;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.Maps;
using CorrinoEngine.Mods;
using CorrinoEngine.Orders;
using CorrinoEngine.Renderer;
using CorrinoEngine.Scenes;
using CorrinoEngine.Topography;
using CorrinoEngine.UI;
using CorrinoEngine.Translation;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Core
{
    public class World
    {
        private string currentModel;
        private bool isEnableDebugMode;
        private ModData modData;
        private AssetManager assetManager;

        private Actor worldActor;
        private List<FactionInfo> factionInfos;
        private List<Actor> actors;
        private Actor selectedActor;
        private string selectedBuildActorTypeName;
        private bool suppressNextLeftClick;
        private bool wasLeftMouseDown;
        private bool wasRightMouseDown;

        private Camera camera;
        private CameraController camController;
        private WorldRenderer worldRenderer;
        private TerrainRenderer terrainRenderer;
        private Terrain terrain;
        private GameMap currentMap;

        private OrderManager orderManager;
        private SceneManager sceneManager;

        private KeyboardState ks;
        private MouseState ms;
        private int credits;
        private string buildFeedbackMessage;
        private float buildFeedbackTimeLeft;
        private Actor pendingPlacementActor;
        private Vector3 pendingPlacementPosition;
        private Vector2i pendingPlacementFootprint;

        public List<FactionInfo> FactionInfos
        {
            get { return factionInfos; }
        }
        public ModData ModData
        {
            get { return modData; }
        }
        public Actor SelectedActor
        {
            get { return selectedActor; }
        }
        public int ActorCount
        {
            get { return actors.Count; }
        }
        public int SelectedProductionQueueCount
        {
            get { return selectedActor?.ProductionQueue.Count ?? 0; }
        }
        public float MouseX
        {
            get { return ms.X; }
        }
        public float MouseY
        {
            get { return ms.Y; }
        }
        public float MouseScrollY
        {
            get { return ms.Scroll.Y; }
        }
        public KeyboardState KeyboardState
        {
            get { return ks; }
        }
        public int Credits
        {
            get { return credits; }
        }
        public string SelectedBuildActorTypeName
        {
            get { return selectedBuildActorTypeName; }
        }
        public string BuildFeedbackMessage
        {
            get { return buildFeedbackMessage; }
        }
        public float BuildFeedbackAlpha
        {
            get
            {
                if (buildFeedbackTimeLeft <= 0)
                {
                    return 0;
                }

                return Math.Clamp(buildFeedbackTimeLeft / 2.5f, 0, 1);
            }
        }
        public bool IsInBuildingPlacementMode
        {
            get { return pendingPlacementActor != null; }
        }
        public string PendingPlacementActorTypeName
        {
            get { return pendingPlacementActor?.ActorData?.TypeName; }
        }
        public Vector3 PendingPlacementPosition
        {
            get { return pendingPlacementPosition; }
        }
        public Vector2i PendingPlacementFootprint
        {
            get { return pendingPlacementFootprint; }
        }

        public bool IsEnableDebugMode
		{
            get { return isEnableDebugMode; }
        }

        public World(
            AssetManager assetManager, 
            ModData modData, 
            Vector2 viewportSize, 
            MouseState ms, 
            KeyboardState ks)
        {
            isEnableDebugMode = true;

			this.assetManager = assetManager;
            this.modData = modData;
            this.ms = ms;
            this.ks = ks;
            credits = 3000;
            buildFeedbackMessage = string.Empty;
            buildFeedbackTimeLeft = 0;
            pendingPlacementPosition = Vector3.Zero;
            pendingPlacementFootprint = Vector2i.One;

            FieldManager.Instance.Init(modData);

            factionInfos = new List<FactionInfo>();
            actors = new List<Actor>();

            worldActor = CreateActor("World");
            parseFaction();

            camera = new PerspectiveCamera
            {
                Size = viewportSize,
                Direction = new Vector3(0, -1, 1).Normalized(),
                Position = new Vector3(0, 1, -1) * 128
            };
            camera.Size = viewportSize;

            camController = new RTSCameraController(camera);
            camController.InjectKeyborardState(ks);
            camController.InjectMouseState(ms);

            orderManager = new OrderManager(this, camera, ks, ms);
            orderManager.OrderExecuted += OrderManager_OrderExecuted;

            sceneManager = new SceneManager(this, ms, ks);

            worldRenderer = new WorldRenderer();
            terrainRenderer = new TerrainRenderer();
        }

        public void Start()
        {
            LoadDefaultMap();
            sceneManager.StartNewScene("InnerGame");
        }

        private void OrderManager_OrderExecuted(string orderName, object orderParams)
        {
            switch(orderName)
            {
                case "SelectActor":
                    SelectActor(orderParams as Actor);
                    break;
                case "MoveActor":
                    object[] moveArgs = orderParams as object[];
                    MoveSelectedActor((Vector3)moveArgs[0]);
                    break;
                case "PlaceBuilding":
                    object[] arr = orderParams as object[];
                    Vector3 position = (Vector3)arr[0];
                    var actorName = arr[1].ToString();
                    SpawnActor(CreateActor(actorName), position);
                    break;
            }
        }

        public Actor CreateActor(string actorTypeName)
        {
            ActorData actorData = modData.Manifest.ActorDataList.Where(o => o.TypeName == actorTypeName).FirstOrDefault();
            Actor actor = new Actor(actorData);

            return actor;
        }

        public ActorData GetActorData(string actorTypeName)
        {
            return modData.Manifest.ActorDataList.FirstOrDefault(o => o.TypeName == actorTypeName);
        }

        public void SpawnActor(Actor actor)
        {
            SpawnActor(actor, Vector3.Zero);
        }

        public void SpawnActor(Actor actor, Vector3 position)
        {
            Mesh mesh = assetManager.Load<XbfMesh>(this, actor.ActorData["idle"].Resource);
            MeshInstance meshInstance = new MeshInstance(mesh);
            meshInstance.Position = position;

            actor.Spawn(meshInstance);
            actors.Add(actor);
            worldRenderer.RenderObject(meshInstance);
        }

        private void parseFaction()
        {
            var fieldValues = worldActor.GetFields("Faction");
            foreach (var fieldPair in fieldValues)
            {
                string[] tokens = fieldPair.Value.ToString().Split("^").Where(o => !string.IsNullOrEmpty(o)).ToArray();
                string[] line1 = tokens[0].Split(":");
                string[] line2 = tokens[1].Split(":");
                string[] line3 = tokens[2].Split(":");
                FactionInfo factionInfo = new FactionInfo(line1[1], line2[1], line3[1]);
                factionInfos.Add(factionInfo);
            }
        }

        public void RenderFrame()
        {
            if (terrain != null)
            {
                terrainRenderer.RenderFrame(default, camera);
            }
            worldRenderer.Render(
                camera,
                actors,
                selectedActor,
                pendingPlacementActor,
                pendingPlacementPosition,
                pendingPlacementFootprint,
                currentMap?.Manifest?.TileSize ?? 48f,
                IsPendingPlacementValid());
        }

        public void Update(FrameEventArgs args)
        {
            camController.Update();
            orderManager.Update();
            UpdateProductionQueues((float)args.Time);
            UpdateBuildFeedback((float)args.Time);
            UpdatePlacementPreview();
            if (terrain != null)
            {
                terrainRenderer.UpdateFrame(args);
            }
            worldRenderer.UpdateFrame(args);
            foreach (var actor in actors)
            {
                actor.Update(args);
            }
            sceneManager.Update();

            if(IsEnableDebugMode)
            {
                if(ks.IsKeyDown(Keys.M) && ks.IsKeyDown(Keys.LeftShift))
                {
                    frmModelSelector modelSelector = new frmModelSelector(assetManager);
                    if (modelSelector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        worldRenderer.RemoveCurrent();
                        LoadXbf(modelSelector.SelectedModel);
                    }
                }
            }
        }

		private void LoadXbf(string model, Vector3 modelPos)
		{
			currentModel = model;
			var mesh = assetManager.Load<XbfMesh>(this, model);
			var meshInstance = new MeshInstance(mesh) { Speed = 20 };
			meshInstance.Position = modelPos;
			worldRenderer.RenderObject(meshInstance);
		}
		
		private void LoadXbf(string model)
		{
            try
			{
				currentModel = model;
				var mesh = assetManager.Load<XbfMesh>(this, model);
				var meshInstance = new MeshInstance(mesh) { Speed = 20 };
				worldRenderer.RenderObject(meshInstance);
			}catch { }
		}

        private void LoadDefaultMap()
        {
            string mapsDir = modData.Manifest.MapsDir;
            if (string.IsNullOrWhiteSpace(mapsDir))
            {
                return;
            }

            string mapPath = Path.Combine(modData.FullPath, mapsDir, "default-map.yaml");
            if (!File.Exists(mapPath))
            {
                return;
            }

            currentMap = new GameMap();
            currentMap.Load(mapPath);

            terrain = BuildTerrain(currentMap);
            terrainRenderer.RenderTerrain(terrain);

            foreach (var actor in currentMap.Actors)
            {
                if (string.IsNullOrWhiteSpace(actor.Type))
                {
                    continue;
                }

                SpawnActor(CreateActor(actor.Type), new Vector3(actor.X, actor.Y, actor.Z));
            }
        }

        private Terrain BuildTerrain(GameMap map)
        {
            Terrain newTerrain = new Terrain(terrainRenderer);
            float tileSize = map.Manifest.TileSize;

            foreach (var tile in map.Tiles)
            {
                MeshInstance meshInstance = CreateTerrainMeshInstance(map, tile, tileSize);
                meshInstance.Position = new Vector3(tile.X, tile.Y, tile.Z);
                newTerrain.AppendTile(new TerrainTile(meshInstance, (int)tile.X, (int)tile.Y, (int)tile.Z, ResolveTileBuildable(tile)));
            }

            return newTerrain;
        }

        private Vector3 PickTileColor(GameMapTile tile)
        {
            if (!string.IsNullOrWhiteSpace(tile.Mesh))
            {
                int hash = Math.Abs(tile.Mesh.GetHashCode());
                return new Vector3(
                    0.45f + (hash % 20) / 100f,
                    0.38f + (hash % 15) / 120f,
                    0.22f + (hash % 10) / 140f);
            }

            return new Vector3(0.58f, 0.47f, 0.28f);
        }

        private MeshInstance CreateTerrainMeshInstance(GameMap map, GameMapTile tile, float tileSize)
        {
            string modelResource = ResolveTerrainModelPath(map, tile);
            if (!string.IsNullOrWhiteSpace(modelResource))
            {
                Mesh terrainMesh = assetManager.Load<XbfMesh>(this, modelResource);
                return new MeshInstance(terrainMesh);
            }

            string textureResource = ResolveTerrainTexturePath(map, tile);
            if (!string.IsNullOrWhiteSpace(textureResource))
            {
                Mesh terrainMesh = TerrainMeshFactory.CreateTextureTileMesh(
                    assetManager,
                    this,
                    tileSize,
                    textureResource,
                    ResolveTileUvScale(map, tile));
                return new MeshInstance(terrainMesh);
            }

            return new MeshInstance(TerrainMeshFactory.CreateColorTileMesh(tileSize, PickTileColor(tile)));
        }

        public Actor QueryActorAtCursor()
        {
            Vector3 scenePosition = camera.ToScene(new Vector2(ms.X, ms.Y));

            return actors
                .Where(o => o.MeshInstance != null)
                .OrderBy(o => Vector2.Distance(
                    new Vector2(o.Position.X, o.Position.Z),
                    new Vector2(scenePosition.X, scenePosition.Z)))
                .FirstOrDefault(o =>
                    Vector2.Distance(
                        new Vector2(o.Position.X, o.Position.Z),
                        new Vector2(scenePosition.X, scenePosition.Z)) <= o.SelectionRadius);
        }

        public Vector3 QueryGroundAtCursor()
        {
            return camera.ToScene(new Vector2(ms.X, ms.Y));
        }

        public bool ConsumeLeftClick()
        {
            bool isDown = ms.IsButtonDown(MouseButton.Button1);
            bool clicked = isDown && !wasLeftMouseDown;
            wasLeftMouseDown = isDown;

            if (clicked && suppressNextLeftClick)
            {
                suppressNextLeftClick = false;
                return false;
            }

            return clicked;
        }

        public void SuppressNextLeftClick()
        {
            suppressNextLeftClick = true;
        }

        public bool ConsumeRightClick()
        {
            bool isDown = ms.IsButtonDown(MouseButton.Button2);
            bool clicked = isDown && !wasRightMouseDown;
            wasRightMouseDown = isDown;
            return clicked;
        }

        public bool TryConfirmPendingPlacement()
        {
            if (!IsInBuildingPlacementMode)
            {
                return false;
            }

            if (!CanPlacePendingBuildingAt(pendingPlacementPosition))
            {
                SetBuildFeedback("Cannot place building here.");
                return true;
            }

            SpawnActor(pendingPlacementActor, pendingPlacementPosition);
            pendingPlacementActor = null;
            pendingPlacementPosition = Vector3.Zero;
            pendingPlacementFootprint = Vector2i.One;
            SetBuildFeedback("Building placed.");
            return true;
        }

        public bool CancelPendingPlacement()
        {
            if (!IsInBuildingPlacementMode)
            {
                return false;
            }

            int refund = ResolveBuildCost(pendingPlacementActor.ActorData.TypeName);
            credits += refund;
            pendingPlacementActor = null;
            pendingPlacementPosition = Vector3.Zero;
            pendingPlacementFootprint = Vector2i.One;
            SetBuildFeedback("Building placement cancelled.");
            return true;
        }

        public void SelectActor(Actor actor)
        {
            if (selectedActor != null)
            {
                selectedActor.OnDeselect();
            }

            selectedActor = actor;
            selectedBuildActorTypeName = null;

            if (selectedActor != null)
            {
                selectedActor.OnSelect();
            }

            if (CanActorProduce(selectedActor))
            {
                UIManager.Instance.StartUI("BuildQueueUI");
            }
            else
            {
                UIManager.Instance.CloseBuildQueueUI();
            }
        }

        public IEnumerable<ActorData> GetBuildableActors()
        {
            return GetBuildableActors(selectedActor);
        }

        public IEnumerable<ActorData> GetBuildableActors(Actor producer)
        {
            if (producer == null || producer.ActorData == null)
            {
                return Enumerable.Empty<ActorData>();
            }

            string producerTypeName = producer.ActorData.TypeName;
            string factionPrefix = producer.GetFieldValue("ProvideBuildings")?.ToString();

            return modData.Manifest.ActorDataList
                .Where(o => o.TypeName != producerTypeName)
                .Where(o => CanProducerBuildActor(producerTypeName, factionPrefix, o))
                .OrderBy(o => o.TypeName)
                .ToList();
        }

        public void SelectBuildActor(string actorTypeName)
        {
            selectedBuildActorTypeName = actorTypeName;
            buildFeedbackMessage = string.Empty;
            buildFeedbackTimeLeft = 0;
            UIManager.Instance.RefreshBuildQueueUI();
        }

        public string GetSelectedActorDisplayName()
        {
            if (selectedActor == null)
            {
                return "None";
            }

            return GetActorDisplayName(selectedActor.ActorData);
        }

        public string GetSelectedActorDescription()
        {
            if (selectedActor == null)
            {
                return "No actor selected";
            }

            return GetActorDescription(selectedActor.ActorData);
        }

        public void EnqueueBuild(string actorTypeName)
        {
            if (string.IsNullOrWhiteSpace(actorTypeName))
            {
                SetBuildFeedback("Select a build item first.");
                return;
            }

            if (selectedActor == null)
            {
                SetBuildFeedback("No producer selected.");
                return;
            }

            int cost = ResolveBuildCost(actorTypeName);
            if (credits < cost)
            {
                SetBuildFeedback($"Insufficient credits: need {cost}, have {credits}.");
                return;
            }

            credits -= cost;
            buildFeedbackMessage = string.Empty;
            buildFeedbackTimeLeft = 0;

            selectedActor.EnqueueProduction(new ProductionOrder
            {
                Id = Guid.NewGuid(),
                ActorTypeName = actorTypeName,
                Progress = 0,
                Duration = ResolveBuildDuration(actorTypeName),
                Cost = cost
            });
            selectedBuildActorTypeName = actorTypeName;
        }

        public void CancelSelectedProduction()
        {
            if (selectedActor == null)
            {
                return;
            }

            ProductionOrder current = selectedActor.PeekProduction();
            if (current == null)
            {
                return;
            }

            selectedActor.CancelProduction();
            credits += ResolveRefund(current, true);
        }

        public void CancelSelectedProduction(Guid orderId)
        {
            if (selectedActor == null)
            {
                return;
            }

            ProductionOrder current = selectedActor.PeekProduction();
            if (selectedActor.CancelProduction(orderId, out ProductionOrder removedOrder))
            {
                bool wasCurrent = current != null && removedOrder != null && current.Id == removedOrder.Id;
                credits += ResolveRefund(removedOrder, wasCurrent);
            }
        }

        public ProductionOrder GetSelectedProduction()
        {
            return selectedActor?.PeekProduction();
        }

        public float GetSelectedProductionProgress01()
        {
            ProductionOrder order = GetSelectedProduction();
            if (order == null || order.Duration <= 0)
            {
                return 0;
            }

            return Math.Clamp(order.Progress / order.Duration, 0, 1);
        }

        public IReadOnlyList<ProductionOrder> GetSelectedProductionQueue()
        {
            if (selectedActor == null)
            {
                return Array.Empty<ProductionOrder>();
            }

            return selectedActor.ProductionQueue.ToList();
        }

        public void MoveSelectedActor(Vector3 target)
        {
            if (selectedActor == null)
            {
                return;
            }

            if (!CanActorMove(selectedActor))
            {
                return;
            }

            selectedActor.MoveTo(target);
        }

        public bool IsPendingPlacementValid()
        {
            return IsInBuildingPlacementMode && CanPlacePendingBuildingAt(pendingPlacementPosition);
        }

        private float ResolveTileUvScale(GameMap map, GameMapTile tile)
        {
            if (tile.UvScale > 0)
            {
                return tile.UvScale;
            }

            if (map.Manifest.TileUvScale > 0)
            {
                return map.Manifest.TileUvScale;
            }

            return 1;
        }

        private string ResolveTerrainTexturePath(GameMap map, GameMapTile tile)
        {
            return ResolveTerrainAssetPath(
                tile.Texture,
                map.Manifest.TileTexture,
                tile.Material,
                tile.Mesh,
                "Textures",
                new string[] { ".tga", ".TGA" });
        }

        private string ResolveTerrainModelPath(GameMap map, GameMapTile tile)
        {
            return ResolveTerrainAssetPath(
                tile.Resource,
                map.Manifest.TileResource,
                tile.Material,
                tile.Mesh,
                string.Empty,
                new string[] { ".xbf", ".XBF" });
        }

        private string ResolveTerrainAssetPath(string explicitPath, string manifestPath, string material, string mesh, string rootFolder, string[] extensions)
        {
            string[] seeds = new string[]
            {
                explicitPath,
                manifestPath,
                material,
                mesh
            };

            foreach (string seed in seeds)
            {
                string resolved = ResolveAssetCandidate(seed, rootFolder, extensions);
                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    return resolved;
                }
            }

            return null;
        }

        private string ResolveAssetCandidate(string candidate, string rootFolder, string[] extensions)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return null;
            }

            List<string> guesses = new List<string>();
            guesses.Add(candidate);

            string normalizedRoot = rootFolder ?? string.Empty;
            string normalizedCandidate = candidate.Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(normalizedRoot) && !normalizedCandidate.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase))
            {
                guesses.Add(normalizedRoot + "/" + normalizedCandidate);
            }

            foreach (string guess in guesses.ToList())
            {
                foreach (string extension in extensions)
                {
                    if (!guess.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        guesses.Add(guess + extension);
                    }
                }
            }

            foreach (string guess in guesses.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    using Stream stream = assetManager.Read(guess);
                    if (stream != null)
                    {
                        return guess;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        public string GetActorDisplayName(ActorData actorData)
        {
            if (actorData == null)
            {
                return "Unknown";
            }

            object rawName = actorData.DataField.Properties.ContainsKey("Name")
                ? actorData.DataField.Properties["Name"]
                : actorData.TypeName;

            string rawValue = rawName?.ToString() ?? actorData.TypeName;
            if (rawValue.isTransableString())
            {
                return rawValue.ToTransableString().Translate("English");
            }

            return rawValue;
        }

        public string GetActorDescription(ActorData actorData)
        {
            if (actorData == null)
            {
                return string.Empty;
            }

            if (!actorData.DataField.Properties.ContainsKey("Description"))
            {
                return actorData.TypeName;
            }

            string rawValue = actorData.DataField.Properties["Description"]?.ToString() ?? actorData.TypeName;
            if (rawValue.isTransableString())
            {
                return rawValue.ToTransableString().Translate("English");
            }

            return rawValue;
        }

        public int GetActorCost(string actorTypeName)
        {
            return ResolveBuildCost(actorTypeName);
        }

        public bool CanAffordSelectedBuild()
        {
            if (string.IsNullOrWhiteSpace(selectedBuildActorTypeName))
            {
                return false;
            }

            return credits >= ResolveBuildCost(selectedBuildActorTypeName);
        }

        public bool CanActorProduce(Actor actor)
        {
            return GetBuildableActors(actor).Any();
        }

        public bool CanActorMove(Actor actor)
        {
            return actor != null && !CanActorProduce(actor);
        }

        private void SetBuildFeedback(string message)
        {
            buildFeedbackMessage = message ?? string.Empty;
            buildFeedbackTimeLeft = string.IsNullOrWhiteSpace(buildFeedbackMessage) ? 0 : 2.5f;
        }

        private void UpdateBuildFeedback(float deltaTime)
        {
            if (buildFeedbackTimeLeft <= 0)
            {
                return;
            }

            buildFeedbackTimeLeft = Math.Max(0, buildFeedbackTimeLeft - deltaTime);
            if (buildFeedbackTimeLeft <= 0)
            {
                buildFeedbackMessage = string.Empty;
            }
        }

        private void UpdateProductionQueues(float deltaTime)
        {
            foreach (var actor in actors.Where(o => o.ProductionQueue.Count > 0).ToList())
            {
                ProductionOrder order = actor.PeekProduction();
                if (order == null)
                {
                    continue;
                }

                order.Progress += deltaTime;
                if (order.Progress >= order.Duration)
                {
                    actor.DequeueProduction();
                    FinishProducedActor(actor, order.ActorTypeName);
                }
            }
        }

        private void FinishProducedActor(Actor producer, string actorTypeName)
        {
            if (string.IsNullOrWhiteSpace(actorTypeName))
            {
                return;
            }

            Actor producedActor = CreateActor(actorTypeName);
            bool isStructure = CanActorProduce(producedActor);
            if (isStructure)
            {
                pendingPlacementActor = producedActor;
                pendingPlacementFootprint = ResolveFootprint(actorTypeName);
                pendingPlacementPosition = SnapBuildingPosition(producer.Position + new Vector3(128, 0, 96));
                SetBuildFeedback("Place the building with LMB or cancel with RMB.");
                return;
            }

            Vector3 spawnPosition = producer.Position + new Vector3(48, 0, 48);
            SpawnActor(producedActor, spawnPosition);
        }

        private void UpdatePlacementPreview()
        {
            if (!IsInBuildingPlacementMode)
            {
                return;
            }

            pendingPlacementPosition = SnapBuildingPosition(QueryGroundAtCursor());
        }

        private Vector3 SnapBuildingPosition(Vector3 worldPosition)
        {
            const float gridSize = 32f;
            return new Vector3(
                MathF.Round(worldPosition.X / gridSize) * gridSize,
                0,
                MathF.Round(worldPosition.Z / gridSize) * gridSize);
        }

        private bool CanPlacePendingBuildingAt(Vector3 position)
        {
            if (!IsInBuildingPlacementMode)
            {
                return false;
            }

            float tileSize = currentMap?.Manifest?.TileSize > 0 ? currentMap.Manifest.TileSize : 48f;
            Vector2i footprint = pendingPlacementFootprint;
            var occupiedCells = GetFootprintCells(position, footprint, tileSize).ToList();
            if (occupiedCells.Count == 0)
            {
                return false;
            }

            foreach (var cell in occupiedCells)
            {
                TerrainTile terrainTile = FindTerrainTileAtCell(cell, tileSize);
                if (terrainTile == null || !terrainTile.IsBuildable)
                {
                    return false;
                }
            }

            foreach (Actor actor in actors.Where(o => o.MeshInstance != null))
            {
                foreach (var cell in occupiedCells)
                {
                    Vector2 cellCenter = CellToWorld(cell, tileSize);
                    float distance = Vector2.Distance(
                        new Vector2(actor.Position.X, actor.Position.Z),
                        cellCenter);
                    float combinedRadius = actor.SelectionRadius + tileSize * 0.45f;
                    if (distance < combinedRadius)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private Vector2i ResolveFootprint(string actorTypeName)
        {
            if (string.IsNullOrWhiteSpace(actorTypeName))
            {
                return Vector2i.One;
            }

            string lowerType = actorTypeName.ToLowerInvariant();
            if (lowerType.Contains("conyard"))
            {
                return new Vector2i(3, 3);
            }

            if (lowerType.Contains("barrack"))
            {
                return new Vector2i(2, 2);
            }

            return new Vector2i(2, 2);
        }

        private bool ResolveTileBuildable(GameMapTile tile)
        {
            if (tile.Buildable.HasValue)
            {
                return tile.Buildable.Value;
            }

            string surface = $"{tile.Material} {tile.Texture} {tile.Mesh} {tile.Resource}".ToLowerInvariant();
            if (surface.Contains("spice"))
            {
                return false;
            }

            if (surface.Contains("rock") || surface.Contains("concrete") || surface.Contains("slab"))
            {
                return true;
            }

            return surface.Contains("sand");
        }

        private IEnumerable<Vector2i> GetFootprintCells(Vector3 centerPosition, Vector2i footprint, float tileSize)
        {
            Vector2i centerCell = WorldToCell(centerPosition, tileSize);
            int startX = centerCell.X - footprint.X / 2;
            int startY = centerCell.Y - footprint.Y / 2;

            for (int z = 0; z < footprint.Y; z++)
            {
                for (int x = 0; x < footprint.X; x++)
                {
                    yield return new Vector2i(startX + x, startY + z);
                }
            }
        }

        private TerrainTile FindTerrainTileAtCell(Vector2i cell, float tileSize)
        {
            if (terrain == null)
            {
                return null;
            }

            Vector2 cellCenter = CellToWorld(cell, tileSize);
            return terrain.Tiles.FirstOrDefault(tile =>
                MathF.Abs(tile.X - cellCenter.X) <= tileSize * 0.25f &&
                MathF.Abs(tile.Z - cellCenter.Y) <= tileSize * 0.25f);
        }

        private Vector2i WorldToCell(Vector3 worldPosition, float tileSize)
        {
            return new Vector2i(
                (int)MathF.Round((worldPosition.X - tileSize * 0.5f) / tileSize),
                (int)MathF.Round((worldPosition.Z - tileSize * 0.5f) / tileSize));
        }

        private Vector2 CellToWorld(Vector2i cell, float tileSize)
        {
            return new Vector2(
                cell.X * tileSize + tileSize * 0.5f,
                cell.Y * tileSize + tileSize * 0.5f);
        }

        private bool CanProducerBuildActor(string producerTypeName, string factionPrefix, ActorData candidate)
        {
            string prerequisite = GetActorProperty(candidate, "Prerequisites");
            if (!string.IsNullOrWhiteSpace(prerequisite))
            {
                return string.Equals(prerequisite, producerTypeName, StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(factionPrefix))
            {
                return candidate.TypeName.StartsWith(factionPrefix + "-", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private string GetActorProperty(ActorData actorData, string propertyName)
        {
            if (actorData == null || actorData.DataField == null || !actorData.DataField.Properties.ContainsKey(propertyName))
            {
                return null;
            }

            return actorData.DataField.Properties[propertyName]?.ToString();
        }

        private float ResolveBuildDuration(string actorTypeName)
        {
            ActorData actorData = modData.Manifest.ActorDataList.FirstOrDefault(o => o.TypeName == actorTypeName);
            if (actorData == null)
            {
                return 2f;
            }

            if (actorData.DataField.Properties.ContainsKey("BuildTime"))
            {
                if (float.TryParse(actorData.DataField.Properties["BuildTime"]?.ToString(), out float buildTime) && buildTime > 0)
                {
                    return buildTime;
                }
            }

            if (actorData.DataField.Properties.ContainsKey("Cost"))
            {
                if (float.TryParse(actorData.DataField.Properties["Cost"]?.ToString(), out float cost) && cost > 0)
                {
                    return Math.Max(1.5f, cost / 100f);
                }
            }

            return 2f;
        }

        private int ResolveBuildCost(string actorTypeName)
        {
            ActorData actorData = modData.Manifest.ActorDataList.FirstOrDefault(o => o.TypeName == actorTypeName);
            if (actorData == null)
            {
                return 100;
            }

            if (actorData.DataField.Properties.ContainsKey("Cost"))
            {
                if (int.TryParse(actorData.DataField.Properties["Cost"]?.ToString(), out int cost) && cost > 0)
                {
                    return cost;
                }
            }

            return 100;
        }

        private int ResolveRefund(ProductionOrder order, bool wasCurrent)
        {
            if (order == null)
            {
                return 0;
            }

            if (!wasCurrent || order.Duration <= 0)
            {
                return order.Cost;
            }

            float remainingRatio = 1f - Math.Clamp(order.Progress / order.Duration, 0, 1);
            return (int)Math.Round(order.Cost * remainingRatio);
        }
	}
}
