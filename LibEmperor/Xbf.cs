namespace LibEmperor
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class Xbf
	{
		public readonly string[] Textures;
		public readonly List<XbfObject> Objects = new();

		public Xbf(Stream stream)
		{
			using var reader = new BinaryReader(stream);

			var version = reader.ReadInt32();

			if (version != 1)
				throw new Exception("Unknown version!");

			// TODO parse this! (but it seems the data is not required at all...?)
			var unk1Size = reader.ReadInt32();
			reader.BaseStream.Position += unk1Size;

			// Some textures have a 0x02 in front of the texture name.
			this.Textures = new string(reader.ReadChars(reader.ReadInt32()).Where(c => c != (char) 0x02).ToArray()).Split('\0').Where(s => s != "").ToArray();

			while (true)
			{
				var test = reader.ReadInt32();

				if (test == -1)
					break;

				reader.BaseStream.Position -= 4;
				this.Objects.Add(new XbfObject(reader));
			}

			if (reader.BaseStream.Position != reader.BaseStream.Length)
				throw new Exception("Missing data!");
		}
	}
}
