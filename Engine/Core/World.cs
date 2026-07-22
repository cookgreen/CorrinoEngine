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
            return clicked;
        }

        public bool ConsumeRightClick()
        {
            bool isDown = ms.IsButtonDown(MouseButton.Button2);
            bool clicked = isDown && !wasRightMouseDown;
            wasRightMouseDown = isDown;
            return clicked;
        }

        public void SelectActor(Actor actor)
        {
            if (selectedActor != null)
            {
                selectedActor.OnDeselect();
            }

            selectedActor = actor;

            if (selectedActor != null)
            {
                selectedActor.OnSelect();
            }
        }

        public void MoveSelectedActor(Vector3 target)
        {
            if (selectedActor == null)
            {
                return;
            }

            selectedActor.MoveTo(target);
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
	}
}
