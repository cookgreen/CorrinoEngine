namespace LibEmperor
{
	using System;
	using System.IO;
	using System.Numerics;

	public class XbfObject
	{
		[Flags]
		private enum Flags
		{
			Unk1 = 1,
			Unk2 = 2,
			VertexAnimation = 4,
			ObjectAnimation = 8
		}

		public readonly XbfVertex[] Vertices;
		public readonly XbfTriangle[] Triangles;
		public readonly XbfObject[] Children;
		public readonly Matrix4x4 Transform;
		public readonly string Name;
		public readonly XbfVertexAnimation? VertexAnimation;
		public readonly XbfObjectAnimation? ObjectAnimation;

		public XbfObject(BinaryReader reader)
		{
			this.Vertices = new XbfVertex[reader.ReadInt32()];

			var flags = (Flags) reader.ReadInt32();

			if ((int) flags >> 4 != 0)
				throw new Exception("Unknown flags!");

			this.Triangles = new XbfTriangle[reader.ReadInt32()];
			this.Children = new XbfObject[reader.ReadInt32()];

			this.Transform = new Matrix4x4(
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble(),
				(float) reader.ReadDouble()
			);

			this.Name = new string(reader.ReadChars(reader.ReadInt32())).Split('\0')[0];

			for (var i = 0; i < this.Children.Length; i++)
				this.Children[i] = new XbfObject(reader);

			for (var i = 0; i < this.Vertices.Length; i++)
				this.Vertices[i] = new XbfVertex(reader);

			for (var i = 0; i < this.Triangles.Length; i++)
				this.Triangles[i] = new XbfTriangle(reader);

			// TODO This could be AmbientLight. However, its always 255,255,255 and only present on these two files:
			// FRONTEND/arrowhighlight.xbf
			// FRONTEND/SCORE.XBF
			if ((flags & Flags.Unk1) != 0)
				for (var i = 0; i < this.Vertices.Length; i++)
					reader.ReadBytes(3);

			// TODO identify this. Bitmask?
			if ((flags & Flags.Unk2) != 0)
				for (var i = 0; i < this.Triangles.Length; i++)
					reader.ReadInt32();

			if ((flags & Flags.VertexAnimation) != 0)
				this.VertexAnimation = new XbfVertexAnimation(reader);

			if ((flags & Flags.ObjectAnimation) != 0)
				this.ObjectAnimation = new XbfObjectAnimation(reader);
		}
	}
}
