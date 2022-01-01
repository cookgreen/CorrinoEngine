namespace CorrinoEngine.Graphics.Mesh
{
	using Cameras;
    using CorrinoEngine.Game;
    using OpenTK.Mathematics;
	using System;
	using System.Linq;

	public class MeshInstance : IRenderable
	{
		private Actor actor;
		private float boundingRadius;
		private Vector3 position;

		private float frame;
		private readonly Mesh mesh;
		private readonly float totalFrames;
		private Matrix4 World = Matrix4.Identity;

		public float Speed = 1;

        public Vector3 Position
        {
            get { return position;}
            set { 
				position = value;
				World = Matrix4.CreateTranslation(position);
			}
        }
		public float BoundingRadius
        {
            get { return boundingRadius; }
        }

		public Actor Actor
        {
            get { return actor; }
        }

        public MeshInstance(Actor actor, Mesh mesh)
		{
			this.actor = actor;
			this.mesh = mesh;

			this.totalFrames = this.GetLongestAnimation(this.mesh);
			position = new Vector3();
			boundingRadius = 20;
		}

		private int GetLongestAnimation(Mesh mesh)
		{
			var frames = mesh.TransformAnimation?.Length ?? 1;

			if (mesh.Children != null && mesh.Children.Any())
				frames = Math.Max(frames, mesh.Children.Max(this.GetLongestAnimation));

			return frames;
		}

		public void Update(float delta)
		{
			this.frame = (this.frame + delta * this.Speed) % this.totalFrames;
		}

		public void Draw(Camera camera)
		{
			this.mesh.Draw(camera, this.World, this.frame);
		}

		// if startFrame = endFrame => Static Model
		// if startFrame -> endFrame => Model Animation loop play
		public void Draw(Camera camera, float startFrame, float endFrame)
		{
			this.mesh.Draw(camera, this.World, startFrame, endFrame);
		}
	}
}
