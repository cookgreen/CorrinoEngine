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
		private AssetManager assetManager;
		private Argument argument;

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
			world = new World(assetManager, currentMod, Size, MouseState, KeyboardState);
			WindowState = WindowState.Maximized;
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
			}
		}

        protected override void OnResize(ResizeEventArgs args)
		{
			GL.Viewport(0, 0, args.Width, args.Height);
			world.OnResize(args);
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			world.RenderFrame();

			Context.SwapBuffers();
		}

		protected override void OnUpdateFrame(FrameEventArgs args)
		{
			world.Update(args);
		}

		//private void LoadXbf(string model, Vector3 modelPos)
		//{
		//	currentModel = model;
		//
		//	var mesh = assetManager.Load<XbfMesh>(this, model);
		//
		//	var meshInstance = new MeshInstance(mesh) { Speed = 20 };
		//	meshInstance.World = Matrix4.CreateTranslation(modelPos);
		//
		//	worldRenderer.RenderModel(meshInstance);
		//}
		//
		//private void LoadXbf(string model)
		//{
		//	currentModel = model;
		//
		//	var mesh = assetManager.Load<XbfMesh>(this, model);
		//
		//	var meshInstance = new MeshInstance(mesh) {Speed = 20};
		//
		//	worldRenderer.RenderModel(meshInstance);
		//}

        protected override void OnUnload()
        {
			System.Windows.Forms.Application.Exit();
        }
    }
}
