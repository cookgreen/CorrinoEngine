namespace CorrinoEngine.Cameras
{
	using OpenTK.Mathematics;

	public class OrthogonalCamera : Camera
	{
		public float Zoom = 1;

		public override void Update()
		{
			base.Update();
			this.View = Matrix4.LookAt(this.Position, this.Position + this.Direction, Vector3.UnitY);
			this.Projection = Matrix4.CreateOrthographic(this.Size.X / this.Zoom, this.Size.Y / this.Zoom, 1, short.MaxValue);
		}
	}
}
