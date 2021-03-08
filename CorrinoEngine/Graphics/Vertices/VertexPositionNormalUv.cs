namespace CorrinoEngine.Graphics.Vertices
{
	using OpenTK.Mathematics;

	public readonly struct VertexPositionNormalUv
	{
		private readonly Vector3 position;
		private readonly Vector3 normal;
		private readonly Vector2 uv;

		public VertexPositionNormalUv(Vector3 position, Vector3 normal, Vector2 uv)
		{
			this.position = position;
			this.normal = normal;
			this.uv = uv;
		}

		public float[] Pack()
		{
			return new[] {this.position.X, this.position.Y, this.position.Z, this.normal.X, this.normal.Y, this.normal.Z, this.uv.X, this.uv.Y};
		}
	}
}
