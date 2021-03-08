namespace LibEmperor
{
	using System.IO;
	using System.Numerics;

	public class XbfVertex
	{
		public readonly Vector3 Position;
		public readonly Vector3 Normal;

		public XbfVertex(BinaryReader reader)
		{
			this.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			this.Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}
	}
}
