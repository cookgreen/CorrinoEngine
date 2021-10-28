namespace CorrinoEngine.Assets
{
	using FileSystem;
	using Graphics.Mesh;
	using Graphics.Shaders;
	using Graphics.Vertices;
	using LibEmperor;
	using OpenTK.Mathematics;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Numerics;
	using Vector2 = OpenTK.Mathematics.Vector2;
	using Vector3 = OpenTK.Mathematics.Vector3;

	public class XbfMesh : Mesh
	{
		private readonly AssetManager assetManager;

		public XbfMesh(AssetManager assetManager, IReadableFileSystem fileSystem, string path)
		{
			this.assetManager = assetManager;

			var xbf = new Xbf(fileSystem.Read(path)!);

			this.Name = Path.GetFileNameWithoutExtension(path);

			// TODO: Some textures are using additional flags by prefixing their filenames:
			// TODO: = => apply player color to 0000?? pixels
			// TODO: ! => render additive
			// TODO: % might be ""
			// TODO: @ might be ""

			this.Children = xbf.Objects.Select(
					xbfObject => XbfMesh.LoadXbfObject(
						xbfObject,
						this.assetManager.Load<XbfShader>(this),
						xbf.Textures.Select(
								name =>
								{
									try
									{
										return this.assetManager.Load<Texture>(this, $"Textures/{name}").Id;
									}
									catch (Exception)
									{
										return this.assetManager.Load<Texture>(this, "Textures/white.tga").Id;
									}
								}
							)
							.ToArray()
					)
				)
				.ToArray();
		}

		private static Mesh LoadXbfObject(XbfObject xbfObject, XbfShader shader, IReadOnlyList<int> textures)
		{
			var allVertices = new Dictionary<int, List<VertexPositionNormalUv>>();
			var allIndices = new Dictionary<int, List<ShaderIndex>>();

			foreach (var triangle in xbfObject.Triangles)
			{
				if (triangle.Texture == -1)
					continue;

				if (!allVertices.ContainsKey(triangle.Texture))
				{
					allVertices.Add(triangle.Texture, new List<VertexPositionNormalUv>());
					allIndices.Add(triangle.Texture, new List<ShaderIndex>());
				}

				var vertices = allVertices[triangle.Texture];
				var indices = allIndices[triangle.Texture];

				for (var i = 0; i < 3; i++)
					vertices.Add(
						new VertexPositionNormalUv(
							XbfMesh.Convert(xbfObject.Vertices[triangle.Vertices[i]].Position),
							XbfMesh.Convert(xbfObject.Vertices[triangle.Vertices[i]].Normal),
							XbfMesh.Convert(triangle.Uv[i]) * new Vector2(1, -1)
						)
					);

				indices.Add(new ShaderIndex(vertices.Count - 3, vertices.Count - 2, vertices.Count - 1));
			}

			return new Mesh(null, null, null)
			{
				Name = xbfObject.Name,
				Transform = XbfMesh.Convert(xbfObject.Transform),
				TransformAnimation =
					xbfObject.ObjectAnimation != null && xbfObject.ObjectAnimation.Frames.Count > 0
						? XbfMesh.BuildAnimation(xbfObject.ObjectAnimation)
						: null,
				Children = allVertices.Keys
					.Select(
						texture => new Mesh(
							new XbfShader.XbfShaderParameters(shader) {Texture = textures[texture]},
							allVertices[texture].Select(vertex => vertex.Pack()).SelectMany(v => v).ToArray(),
							allIndices[texture].Select(index => index.Pack()).SelectMany(i => i).ToArray()
						)
					)
					.Concat(xbfObject.Children.Select(childXbfObject => XbfMesh.LoadXbfObject(childXbfObject, shader, textures)))
					.ToArray(),
			};
		}

		private static Matrix4[] BuildAnimation(XbfObjectAnimation xbfObjectAnimation)
		{
			var frames = new Matrix4[xbfObjectAnimation.Length];
			var first = xbfObjectAnimation.Frames.Keys.Min();
			var last = xbfObjectAnimation.Frames.Keys.Max();

			var lastFrame = 0;

			for (var i = 0; i < xbfObjectAnimation.Length; i++)
			{
				if (!xbfObjectAnimation.Frames.ContainsKey(i))
					continue;

				frames[i] = XbfMesh.Convert(xbfObjectAnimation.Frames[i]);

				if (i == first)
					for (var j = 0; j < i; j++)
						frames[j] = frames[i];
				else if (i > last)
					frames[i] = frames[last];
				else
					for (; lastFrame + 1 < i; lastFrame++)
						frames[lastFrame + 1] = frames[lastFrame] + (frames[i] - frames[lastFrame]) * (1f / (i - lastFrame));

				lastFrame = i;
			}

			return frames;
		}

		private static Vector2 Convert(System.Numerics.Vector2 v)
		{
			return new(v.X, v.Y);
		}

		private static Vector3 Convert(System.Numerics.Vector3 v)
		{
			return new(v.X, v.Y, v.Z);
		}

		private static Matrix4 Convert(Matrix4x4 m)
		{
			return new(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
		}

		public override void Dispose()
		{
			base.Dispose();
			this.assetManager.Unload(this);
			GC.SuppressFinalize(this);
		}
	}
}
