namespace CorrinoEngine.Cameras
{
	using OpenTK.Mathematics;

	public abstract class Camera
	{
		public Vector2 Size;
		public Vector3 Position;
		public Vector3 Direction = Vector3.UnitZ;

		public Matrix4 View = Matrix4.Identity;
		public Matrix4 Projection = Matrix4.Identity;

		public virtual void Update()
		{
			this.Direction.Normalize();
		}

		public Vector3 ToScene(Vector2 position)
		{
			var vector3 = Vector3.TransformVector(
				new Vector3(position.X / this.Size.X * 2 - 1, -(position.Y / this.Size.Y * 2 - 1), 0),
				Matrix4.Invert(Matrix4.Mult(this.View, this.Projection))
			);

			return vector3 - this.Direction / (this.Direction.Y / vector3.Y);
		}

		public Vector2 ToViewport(Vector3 position)
		{
			var vector3 = Vector3.TransformVector(position, Matrix4.Mult(this.View, this.Projection));

			return new Vector2((vector3.X + 1) * this.Size.X / 2, (-vector3.Y + 1) * this.Size.Y / 2);
		}
	}
}
