namespace CorrinoEngine.Graphics.Shaders
{
	using OpenTK.Mathematics;

	public interface IShaderParameters
	{
		public void Bind(Matrix4 model, Matrix4 view, Matrix4 projection);
		public int CreateVertexArrayObject();
	}
}
