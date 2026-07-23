namespace LibEmperor
{
	using System;
	using System.IO;
	using System.IO.Compression;

	public class RfhEntry
	{
		private const int DataSectionPrefixSize = 6;

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
			int nameLength = headerReader.ReadInt32();
			int rawFileTime = headerReader.ReadInt32();
			this.flags = (Flags) headerReader.ReadInt32();
			this.compressedSize = headerReader.ReadInt32();
			this.uncompressedSize = headerReader.ReadInt32();
			this.offset = headerReader.ReadInt32();
			if (nameLength < 0)
				throw new Exception("Invalid RFH entry name length.");
			if (this.compressedSize < 0 || this.uncompressedSize < 0 || this.offset < 0)
				throw new Exception("Invalid RFH entry size or offset.");

			this.DateTime = TryReadFileTime(rawFileTime);
			this.Path = new string(headerReader.ReadChars(nameLength)).Split('\0')[0];

			if (this.flags != 0 && this.flags != Flags.Compressed)
				throw new Exception("Unknown flags!");
		}

		public byte[] Read()
		{
			long dataOffset = (long)this.offset + DataSectionPrefixSize;
			if (dataOffset < 0 || dataOffset > this.reader.BaseStream.Length)
				throw new EndOfStreamException($"RFH entry '{this.Path}' has invalid data offset {dataOffset}.");

			this.reader.BaseStream.Position = dataOffset;
			byte[] rawBytes = ReadExactBytes(this.reader, this.compressedSize, this.Path);

			if ((this.flags & Flags.Compressed) == 0)
				return rawBytes;

			using var compressedStream = new MemoryStream(rawBytes, writable: false);
			using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
			using var outputStream = new MemoryStream(this.uncompressedSize > 0 ? this.uncompressedSize : rawBytes.Length * 2);
			deflateStream.CopyTo(outputStream);
			byte[] bytes = outputStream.ToArray();
			if (this.uncompressedSize > 0 && bytes.Length != this.uncompressedSize)
				throw new EndOfStreamException($"RFH entry '{this.Path}' decompressed to {bytes.Length} bytes, expected {this.uncompressedSize}.");

			return bytes;
		}

		private static DateTime TryReadFileTime(int rawFileTime)
		{
			try
			{
				return DateTime.FromFileTime(rawFileTime);
			}
			catch
			{
				return DateTime.MinValue;
			}
		}

		private static byte[] ReadExactBytes(BinaryReader reader, int count, string path)
		{
			if (count == 0)
				return Array.Empty<byte>();

			byte[] bytes = reader.ReadBytes(count);
			if (bytes.Length != count)
				throw new EndOfStreamException($"RFH entry '{path}' expected {count} bytes but only read {bytes.Length}.");

			return bytes;
		}
	}
}
