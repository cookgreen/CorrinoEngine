namespace CorrinoEngine.Graphics.Shaders
{
	using OpenTK.Mathematics;

	public interface IShader
	{
		public int CreateVertexArrayObject();
		public void Bind(Matrix4 model, Matrix4 view, Matrix4 projection, IShaderParameters parameters);
	}
}
