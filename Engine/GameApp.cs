using CorrinoEngine.Assets;
using CorrinoEngine.Cameras;
using CorrinoEngine.FileSystem;
using CorrinoEngine.Forms;
using CorrinoEngine.Game;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.Mods;
using CorrinoEngine.Orders;
using CorrinoEngine.Renderer;
using CorrinoEngine.Scenes;
using CorrinoEngine.UI;
using LibEmperor;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CorrinoEngine
{
	public class GameApp : GameWindow
	{
		private World world;
		private bool isEditMode;
		private AssetManager assetManager;
		private Camera camera;
		private CameraController camController;
		private Argument argument;
		private WorldRenderer worldRenderer;
		private TerrainRenderer terrainRenderer;
		private string currentModel;
		private OrderManager orderManager;

		public GameApp(Argument argument)
			: base(GameWindowSettings.Default, NativeWindowSettings.Default)
		{
			this.argument = argument;
		}

		protected override void OnLoad()
		{
			GL.ClearColor(0, 0, 0, 1);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			ModData currentMod = null;
			if (argument.Length > 0 && argument.Contains("Mod"))
			{
				currentMod = ModManager.Instance.LoadSpecificMod(argument.GetArgumentParameter("Mod"));
			}
			else if (ModManager.Instance.Mods.Count > 0)
			{
				currentMod = ModManager.Instance.Mods.ElementAt(0).Value;
			}

			loadAssest(currentMod);

			WindowState = WindowState.Maximized;

			SceneManager.Instance.StartNewScene("InnerGame", this);
		}

        private void loadAssest(ModData currentMod)
		{
			if (currentMod != null)
			{
				var fileSystem = new VirtualFileSystem();

				foreach (var asset in currentMod.Manifest.Asset.Assets)
				{
					string assetFullPath = Path.Combine(Environment.CurrentDirectory, "Mods/" + currentMod.ID, asset);
					fileSystem.Add(new FolderFileSystem(assetFullPath));

					foreach (var file in fileSystem.GetFiles())
					{
						if (file.EndsWith(".RFH", StringComparison.OrdinalIgnoreCase))
						{
							fileSystem.Add(new RfhFileSystem(new Rfh(fileSystem.Read(file)!, fileSystem.Read(file.Substring(0, file.Length - 1) + "D")!)));
						}
						else if (file.EndsWith(".BAG", StringComparison.OrdinalIgnoreCase))
						{
							fileSystem.Add(new BagFileSystem(new Bag(fileSystem.Read(file)!)));
						}
					}
				}

				assetManager = new AssetManager(fileSystem);

				camera = new PerspectiveCamera
				{
					Size = new Vector2(this.Size.X, this.Size.Y),
					Direction = new Vector3(0, -1, 1).Normalized(),
					Position = new Vector3(0, 1, -1) * 128
				};

				camController = new RTSCameraController(camera);
				camController.InjectKeyborardState(KeyboardState);
				camController.InjectMouseState(MouseState);

				world = new World(assetManager, currentMod);
                world.CreateActorFinished += World_CreateActorFinished;

				orderManager = new OrderManager(world, camera, KeyboardState, MouseState);
				orderManager.OrderExecuted += OrderManager_OrderExecuted;
			}
		}

        private void World_CreateActorFinished(Actor actor)
        {
			worldRenderer.AppendActor(actor);
        }

        private void OrderManager_OrderExecuted(string arg1, object arg2)
        {
            switch (arg1)
            {
				case "PlaceBuilding":

					object[] newArgs = arg2 as object[];
					Vector3? actorPos = newArgs[0] as Vector3?;
					string actorName = newArgs[1].ToString();

					world.CreateActor(actorName);

					break;
				default:
					break;
            }
        }

        protected override void OnResize(ResizeEventArgs args)
		{
			WorldRenderer.Instance.Init(this);
			worldRenderer = WorldRenderer.Instance;
			terrainRenderer = new TerrainRenderer(this);

			GL.Viewport(0, 0, args.Width, args.Height);

			camera.Size = new Vector2(args.Width, args.Height);
		}

		protected override void OnUpdateFrame(FrameEventArgs args)
		{
			camController.Update();

			orderManager.Update();

			worldRenderer.UpdateFrame(args);

			if (/*isEditMode && */this.KeyboardState.IsKeyPressed(Keys.Enter))
			{
				frmModelSelector frmModelSelector = new frmModelSelector(assetManager, currentModel);
				if(frmModelSelector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					worldRenderer.UnloadCurrentModel();

					LoadXbf(frmModelSelector.SelectedModel);
                }
			}

			if (KeyboardState.IsKeyPressed(Keys.L))
			{
				frmRFHRFDFileListViewer fileListViewer = new frmRFHRFDFileListViewer(assetManager);
				fileListViewer.ShowDialog();
			}
		}

		private void LoadXbf(string model, Vector3 modelPos)
		{
			currentModel = model;

			var mesh = assetManager.Load<XbfMesh>(this, model);

			var meshInstance = new MeshInstance(mesh) { Speed = 20 };
			meshInstance.World = Matrix4.CreateTranslation(modelPos);

			worldRenderer.RenderModel(meshInstance);
		}

		private void LoadXbf(string model)
		{
			currentModel = model;

			var mesh = assetManager.Load<XbfMesh>(this, model);

			var meshInstance = new MeshInstance(mesh) {Speed = 20};

			worldRenderer.RenderModel(meshInstance);
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			camera.Update();

			worldRenderer.RenderFrame(args, camera);

			Context.SwapBuffers();
		}

        protected override void OnUnload()
        {
			System.Windows.Forms.Application.Exit();
        }
    }
}
