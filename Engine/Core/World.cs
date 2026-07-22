using CorrinoEngine.Assets;
using CorrinoEngine.Cameras;
using CorrinoEngine.Fields;
using CorrinoEngine.Forms;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.Maps;
using CorrinoEngine.Mods;
using CorrinoEngine.Orders;
using CorrinoEngine.Renderer;
using CorrinoEngine.Scenes;
using CorrinoEngine.Topography;
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

        public List<FactionInfo> FactionInfos
        {
            get { return factionInfos; }
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

            FieldManager.Instance.Init(modData);

            factionInfos = new List<FactionInfo>();

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
            worldRenderer.Render(camera);
        }

        public void Update(FrameEventArgs args)
        {
            camController.Update();
            orderManager.Update();
            if (terrain != null)
            {
                terrainRenderer.UpdateFrame(args);
            }
            worldRenderer.UpdateFrame(args);
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
                MeshInstance meshInstance = new MeshInstance(TerrainMeshFactory.CreateTileMesh(tileSize, PickTileColor(tile)));
                meshInstance.Position = new Vector3(tile.X, tile.Y, tile.Z);
                newTerrain.AppendTile(new TerrainTile(meshInstance, (int)tile.X, (int)tile.Y, (int)tile.Z));
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
	}
}
