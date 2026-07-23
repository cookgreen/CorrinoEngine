namespace LibEmperor
{
	using System;
	using System.IO;
	using System.Numerics;
	using System.Text;

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
			int vertexCount = reader.ReadInt32();
			var flags = (Flags) reader.ReadInt32();
			int triangleCount = reader.ReadInt32();
			int childCount = reader.ReadInt32();
			if (vertexCount < 0 || triangleCount < 0 || childCount < 0)
				throw new Exception("Invalid XBF object counts.");

			this.Vertices = new XbfVertex[vertexCount];
			this.Triangles = new XbfTriangle[triangleCount];
			this.Children = new XbfObject[childCount];
			this.Transform = ReadTransform(reader);
			this.Name = ReadName(reader);

			for (int i = 0; i < this.Children.Length; i++)
				this.Children[i] = new XbfObject(reader);

			for (int i = 0; i < this.Vertices.Length; i++)
				this.Vertices[i] = new XbfVertex(reader);

			for (int i = 0; i < this.Triangles.Length; i++)
				this.Triangles[i] = new XbfTriangle(reader);

			if ((flags & Flags.Unk1) != 0)
				SkipBytes(reader, this.Vertices.Length * 3, "vertex colors");

			if ((flags & Flags.Unk2) != 0)
				SkipBytes(reader, this.Triangles.Length * 4, "triangle extras");

			if ((flags & Flags.VertexAnimation) != 0)
				this.VertexAnimation = new XbfVertexAnimation(reader);

			if ((flags & Flags.ObjectAnimation) != 0)
				this.ObjectAnimation = new XbfObjectAnimation(reader);
		}

		private static Matrix4x4 ReadTransform(BinaryReader reader)
		{
			Xbf.EnsureRemaining(reader, 16 * sizeof(double), "object transform");
			return new Matrix4x4(
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble(),
				(float)reader.ReadDouble()
			);
		}

		private static string ReadName(BinaryReader reader)
		{
			Xbf.EnsureRemaining(reader, sizeof(int), "object name length");
			int nameLength = reader.ReadInt32();
			if (nameLength < 0)
				throw new Exception("Invalid XBF object name length.");

			Xbf.EnsureRemaining(reader, nameLength, "object name");
			byte[] bytes = reader.ReadBytes(nameLength);
			return Encoding.ASCII.GetString(bytes).Split('\0')[0];
		}

		private static void SkipBytes(BinaryReader reader, int byteCount, string sectionName)
		{
			Xbf.EnsureRemaining(reader, byteCount, sectionName);
			reader.BaseStream.Position += byteCount;
		}
	}
}
