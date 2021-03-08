namespace LibEmperor
{
	using System;
	using System.IO;
	using System.IO.Compression;

	public class RfhEntry
	{
		[Flags]
		private enum Flags
		{
			Compressed = 2
		}

		private readonly BinaryReader reader;

		public readonly string Path;
		public readonly DateTime DateTime;

		private readonly Flags flags;
		private readonly int compressedSize;
		private readonly int uncompressedSize;
		private readonly int offset;

		public RfhEntry(BinaryReader headerReader, BinaryReader dataReader)
		{
			this.reader = dataReader;
			var nameLength = headerReader.ReadInt32();
			this.DateTime = DateTime.FromFileTime(headerReader.ReadInt32());
			this.flags = (Flags) headerReader.ReadInt32();
			this.compressedSize = headerReader.ReadInt32();
			this.uncompressedSize = headerReader.ReadInt32();
			this.offset = headerReader.ReadInt32();
			this.Path = new string(headerReader.ReadChars(nameLength)).Split('\0')[0];

			if (this.flags != 0 && this.flags != Flags.Compressed)
				throw new Exception("Unknown flags!");
		}

		public byte[] Read()
		{
			this.reader.BaseStream.Position = this.offset + 6;

			if ((this.flags & Flags.Compressed) == 0)
				return this.reader.ReadBytes(this.compressedSize);

			var deflateStream = new DeflateStream(this.reader.BaseStream, CompressionMode.Decompress);
			byte[] bytes = new byte[this.uncompressedSize];
			deflateStream.Read(bytes);

			return bytes;
		}
	}
}
