namespace LibEmperor
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class Bag : IDisposable
	{
		private readonly Stream stream;

		public readonly List<BagEntry> Files = new();

		public Bag(Stream stream)
		{
			this.stream = stream;
			var reader = new BinaryReader(stream);

			var magic = new string(reader.ReadChars(4));

			if (magic != "GABA")
				throw new Exception("Invalid magic!");

			var version = reader.ReadInt32();

			if (version != 4)
				throw new Exception("Unknown version!");

			var numFiles = reader.ReadInt32();
			var stride = reader.ReadInt32();
			var start = reader.BaseStream.Position;

			for (var i = 0; i < numFiles; i++)
			{
				reader.BaseStream.Position = start + i * stride;
				this.Files.Add(new BagEntry(reader));
			}
		}

		public void Dispose()
		{
			this.stream.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
