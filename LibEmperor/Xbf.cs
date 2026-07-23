namespace LibEmperor
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;

	public class Xbf
	{
		public readonly string[] Textures;
		public readonly List<XbfObject> Objects = new();

		public Xbf(Stream stream)
		{
			using var memoryStream = new MemoryStream();
			if (stream.CanSeek)
				stream.Position = 0;

			stream.CopyTo(memoryStream);
			byte[] bytes = memoryStream.ToArray();
			if (bytes.Length < 12)
				throw new Exception("XBF is too small.");

			using var reader = new BinaryReader(new MemoryStream(bytes));

			int version = reader.ReadInt32();
			if (version != 1)
				throw new Exception("Unknown version!");

			int fxSectionSize = reader.ReadInt32();
			int fxSectionEnd = 8 + fxSectionSize;
			if (fxSectionEnd < 8 || fxSectionEnd + 4 > bytes.Length)
				throw new Exception("Invalid XBF FX section.");

			reader.BaseStream.Position = fxSectionEnd;
			int textureBlobLength = reader.ReadInt32();
			if (textureBlobLength < 0 || reader.BaseStream.Position + textureBlobLength > reader.BaseStream.Length)
				throw new Exception("Invalid XBF texture blob length.");

			this.Textures = ParseTextureBlob(reader.ReadBytes(textureBlobLength));

			while (true)
			{
				EnsureRemaining(reader, 4, "object marker");
				int marker = reader.ReadInt32();
				if (marker == -1)
					break;

				reader.BaseStream.Position -= 4;
				this.Objects.Add(new XbfObject(reader));
			}
		}

		private static string[] ParseTextureBlob(byte[] textureBlob)
		{
			var names = new List<string>();
			var currentName = new List<byte>();
			foreach (byte value in textureBlob)
			{
				if (value == 0)
				{
					if (currentName.Count > 0)
					{
						names.Add(Encoding.ASCII.GetString(currentName.ToArray()));
						currentName.Clear();
					}
				}
				else if (value != 0x02)
				{
					currentName.Add(value);
				}
			}

			if (currentName.Count > 0)
				names.Add(Encoding.ASCII.GetString(currentName.ToArray()));

			return names.Where(name => !string.IsNullOrWhiteSpace(name)).ToArray();
		}

		internal static void EnsureRemaining(BinaryReader reader, int byteCount, string sectionName)
		{
			if (byteCount < 0 || reader.BaseStream.Position + byteCount > reader.BaseStream.Length)
				throw new EndOfStreamException($"Unexpected end of XBF while reading {sectionName}.");
		}
	}
}
