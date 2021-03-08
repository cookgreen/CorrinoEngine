namespace CorrinoEngine.Graphics.Shaders
{
	using OpenTK.Mathematics;

	public abstract class ShaderParameters<T> : IShaderParameters where T : IShader
	{
		private T shader;

		protected ShaderParameters(T shader)
		{
			this.shader = shader;
		}

		public int CreateVertexArrayObject() => this.shader.CreateVertexArrayObject();

		public void Bind(Matrix4 model, Matrix4 view, Matrix4 projection) => this.shader.Bind(model, view, projection, this);
	}
}
