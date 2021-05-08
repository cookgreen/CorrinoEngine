using CorrinoEngine.Assets;
using CorrinoEngine.Cameras;
using CorrinoEngine.FileSystem;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.Mods;
using CorrinoEngine.Renderer;
using CorrinoEngine.Scenes;
using LibEmperor;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CorrinoEngine
{
	public class GameApp : GameWindow
	{
		private AssetManager assetManager;
		private Camera camera;
		private Argument argument;
		private WorldRenderer worldRenderer;

		public GameApp(Argument argument)
			: base(GameWindowSettings.Default, NativeWindowSettings.Default)
		{
			this.argument = argument;
			worldRenderer = new WorldRenderer();
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

			SceneManager.Instance.StartNewScene("MainMenu");
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
			}
		}

        protected override void OnResize(ResizeEventArgs args)
		{
			GL.Viewport(0, 0, args.Width, args.Height);
			camera.Size = new Vector2(args.Width, args.Height);
		}

		protected override void OnUpdateFrame(FrameEventArgs args)
		{
			worldRenderer.UpdateFrame(args);

			//if (this.KeyboardState.IsKeyPressed(Keys.Enter) || this.model == null)
			//	this.LoadXbf(this.models[this.model == null ? 0 : (this.models.IndexOf(this.model) + 1) % this.models.Count]);
		}

		private void LoadXbf(string model)
		{
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
	}
}
