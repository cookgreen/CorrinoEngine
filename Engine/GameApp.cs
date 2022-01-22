using CorrinoEngine.Assets;
using CorrinoEngine.Cameras;
using CorrinoEngine.FileSystem;
using CorrinoEngine.Forms;
using CorrinoEngine.Core;
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
		private Game game;

		public GameApp(Argument argument)
			: base(GameWindowSettings.Default, NativeWindowSettings.Default)
		{
			game = new Game(argument);
			WindowState = WindowState.Maximized;
		}

        protected override void OnLoad()
		{
		}

        protected override void OnResize(ResizeEventArgs args)
		{
			GL.Viewport(0, 0, args.Width, args.Height);
			game.StartNewGame(args.Size, MouseState, KeyboardState);
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			game.RenderFrame();
			Context.SwapBuffers();
		}

		protected override void OnUpdateFrame(FrameEventArgs args)
		{
			game.Update(args);
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
