namespace CorrinoEngine.Cameras
{
	using OpenTK.Mathematics;

	public class PerspectiveCamera : Camera
	{
		public float Fov = 90;

		public override void Update()
		{
			base.Update();
			this.View = Matrix4.LookAt(this.Position, this.Position + this.Direction, Vector3.UnitY);
			this.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(this.Fov), this.Size.X / this.Size.Y, 1, short.MaxValue);
		}
	}
}
