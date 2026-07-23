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

		public Vector3 ScreenPointToWorld(Vector2 position, float depth)
		{
			if (this.Size.X <= 0 || this.Size.Y <= 0)
				return Vector3.Zero;

			var clip = new Vector4(
				position.X / this.Size.X * 2f - 1f,
				1f - position.Y / this.Size.Y * 2f,
				depth * 2f - 1f,
				1f);

			var inverse = Matrix4.Invert(this.View * this.Projection);
			var world = Vector4.TransformRow(clip, inverse);
			if (System.MathF.Abs(world.W) > float.Epsilon)
				world /= world.W;

			return world.Xyz;
		}

		public Ray GetPickRay(Vector2 position)
		{
			Vector3 nearPoint = ScreenPointToWorld(position, 0f);
			Vector3 farPoint = ScreenPointToWorld(position, 1f);
			Vector3 direction = farPoint - nearPoint;
			if (direction.LengthSquared <= float.Epsilon)
				direction = this.Direction;
			else
				direction.Normalize();

			return new Ray(nearPoint, direction);
		}

		public bool TryProjectToGround(Vector2 position, float groundY, out Vector3 worldPosition)
		{
			Ray ray = GetPickRay(position);
			if (System.MathF.Abs(ray.Direction.Y) < float.Epsilon)
			{
				worldPosition = Vector3.Zero;
				return false;
			}

			float distance = (groundY - ray.Origin.Y) / ray.Direction.Y;
			if (distance < 0f)
			{
				worldPosition = Vector3.Zero;
				return false;
			}

			worldPosition = ray.Origin + ray.Direction * distance;
			return true;
		}

		public Vector3 ToScene(Vector2 position)
		{
			if (TryProjectToGround(position, 0f, out Vector3 worldPosition))
				return worldPosition;

			return Vector3.Zero;
		}

		public Vector2 ToViewport(Vector3 position)
		{
			if (this.Size.X <= 0 || this.Size.Y <= 0)
				return Vector2.Zero;

			Vector4 clip = Vector4.TransformRow(new Vector4(position, 1f), this.View * this.Projection);
			if (System.MathF.Abs(clip.W) > float.Epsilon)
				clip /= clip.W;

			return new Vector2(
				(clip.X + 1f) * this.Size.X * 0.5f,
				(1f - clip.Y) * this.Size.Y * 0.5f);
		}
	}

	public readonly struct Ray
	{
		public Vector3 Origin { get; }
		public Vector3 Direction { get; }

		public Ray(Vector3 origin, Vector3 direction)
		{
			Origin = origin;
			Direction = direction;
		}
	}
}
