namespace CorrinoEngine
{
	using Assets;
	using Cameras;
	using FileSystem;
	using Graphics.Mesh;
	using LibEmperor;
	using OpenTK.Graphics.OpenGL4;
	using OpenTK.Mathematics;
	using OpenTK.Windowing.Common;
	using OpenTK.Windowing.Desktop;
	using OpenTK.Windowing.GraphicsLibraryFramework;
	using System;
	using System.Collections.Generic;

	public class Application : GameWindow
	{
		private AssetManager assetManager;
		private List<string> models;
		private Camera camera;
		private string? model;
		private XbfMesh? mesh;
		private MeshInstance? meshInstance;

		public Application()
			: base(GameWindowSettings.Default, NativeWindowSettings.Default)
		{
		}

		protected override void OnLoad()
		{
			GL.ClearColor(0, 0, 0, 1);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			var fileSystem = new VirtualFileSystem();
			fileSystem.Add(new FolderFileSystem("C:/Westwood/Emperor"));

			foreach (var file in fileSystem.GetFiles())
			{
				if (file.EndsWith(".RFH", StringComparison.OrdinalIgnoreCase))
					fileSystem.Add(new RfhFileSystem(new Rfh(fileSystem.Read(file)!, fileSystem.Read(file.Substring(0, file.Length - 1) + "D")!)));
				else if (file.EndsWith(".BAG", StringComparison.OrdinalIgnoreCase))
					fileSystem.Add(new BagFileSystem(new Bag(fileSystem.Read(file)!)));
			}

			this.assetManager = new AssetManager(fileSystem);

			this.models = new List<string>();

			foreach (var file in fileSystem.GetFiles())
				if (file.EndsWith(".XBF", StringComparison.OrdinalIgnoreCase))
					this.models.Add(file);

			this.camera = new PerspectiveCamera
			{
				Size = new Vector2(this.Size.X, this.Size.Y), Direction = new Vector3(0, -1, 1).Normalized(), Position = new Vector3(0, 1, -1) * 128
			};
		}

		protected override void OnResize(ResizeEventArgs args)
		{
			GL.Viewport(0, 0, args.Width, args.Height);
			this.camera.Size = new Vector2(args.Width, args.Height);
		}

		protected override void OnUpdateFrame(FrameEventArgs args)
		{
			if (this.meshInstance != null)
			{
				this.meshInstance.World *= Matrix4.CreateRotationY((float) args.Time / 5);
				this.meshInstance.Update((float) args.Time);
			}

			if (this.KeyboardState.IsKeyPressed(Keys.Enter) || this.model == null)
				this.LoadXbf(this.models[this.model == null ? 0 : (this.models.IndexOf(this.model) + 1) % this.models.Count]);
		}

		private void LoadXbf(string model)
		{
			this.model = model;

			if (this.mesh != null)
				this.assetManager.Unload(this, this.mesh);

			this.mesh = this.assetManager.Load<XbfMesh>(this, model);

			this.meshInstance = new MeshInstance(this.mesh) {Speed = 20};
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			this.camera.Update();

			this.meshInstance?.Draw(this.camera);

			this.Context.SwapBuffers();
		}
	}
}
