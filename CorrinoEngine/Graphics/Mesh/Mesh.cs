namespace CorrinoEngine.Graphics.Mesh
{
	using Cameras;
	using OpenTK.Graphics.OpenGL4;
	using OpenTK.Mathematics;
	using Shaders;
	using System;

	public class Mesh : IDisposable
	{
		public string Name;
		public Matrix4? Transform;
		public Matrix4[]? TransformAnimation;
		public Mesh[]? Children;

		protected IShaderParameters? ShaderParameters;
		protected int NumIndices;
		protected int VertexBufferObject;
		protected int IndexBufferObject;
		protected int VertexArrayObject;

		protected Mesh()
		{
		}

		public Mesh(IShaderParameters? shaderParameters, float[]? vertices, int[]? indices)
		{
			if (shaderParameters == null || vertices == null || indices == null)
				return;

			this.ShaderParameters = shaderParameters;
			this.NumIndices = indices.Length;

			this.VertexBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, this.VertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 4, vertices, BufferUsageHint.StaticDraw);

			this.IndexBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.IndexBufferObject);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * 4, indices, BufferUsageHint.StaticDraw);

			this.VertexArrayObject = shaderParameters.CreateVertexArrayObject();
		}

		public void Draw(Camera camera, Matrix4 world, float frame)
		{
			var model = world;

			if (this.TransformAnimation != null)
			{
				if (frame >= this.TransformAnimation.Length - 1)
					model = this.TransformAnimation[^1] * model;
				else
				{
					var lower = (int) frame;
					var upper = lower + 1;
					model = (this.TransformAnimation[lower] + (this.TransformAnimation[upper] - this.TransformAnimation[lower]) * (frame - lower)) * model;
				}
			}
			else if (this.Transform != null)
				model = this.Transform.Value * model;

			if (this.NumIndices > 0 && this.ShaderParameters != null)
			{
				this.ShaderParameters.Bind(model, camera.View, camera.Projection);

				GL.BindVertexArray(this.VertexArrayObject);
				GL.BindBuffer(BufferTarget.ArrayBuffer, this.VertexBufferObject);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.IndexBufferObject);

				GL.DrawElements(BeginMode.Triangles, this.NumIndices, DrawElementsType.UnsignedInt, 0);
			}

			if (this.Children == null)
				return;

			foreach (var child in this.Children)
				child.Draw(camera, model, frame);
		}

		public virtual void Dispose()
		{
			GL.DeleteBuffer(this.VertexArrayObject);
			GL.DeleteBuffer(this.IndexBufferObject);
			GL.DeleteBuffer(this.VertexBufferObject);

			if (this.Children != null)
				foreach (var child in this.Children)
					child.Dispose();

			GC.SuppressFinalize(this);
		}
	}
}
