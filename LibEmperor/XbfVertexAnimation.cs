namespace LibEmperor
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class XbfVertexAnimation
	{
		public readonly int Length;
		public readonly Dictionary<int, object[][]> Frames = new();

		public XbfVertexAnimation(BinaryReader reader)
		{
			this.Length = reader.ReadInt32();
			var entriesNegative = reader.ReadInt32();
			var usedFrames = reader.ReadInt32();

			var frameIds = new List<int>();

			for (var i = 0; i < usedFrames; i++)
				frameIds.Add(reader.ReadInt32());

			if (entriesNegative >= 0)
				return;

			// TODO what is this?
			var unk = reader.ReadInt16();
			var flags = reader.ReadUInt16();
			var entries = reader.ReadInt32();

			if (unk != 0 && unk != 2 && unk != 4 && unk != 5 && unk != 6 && unk != 7 && unk != 8 && unk != 9 && unk != 10)
				throw new Exception("Unknown unk!");

			if ((flags & 0b0111111111111111) != 0)
				throw new Exception("Unknown flags!");

			if (-entries != entriesNegative)
				throw new Exception("Wrong entries!");

			var numVertices = entries / usedFrames;

			for (var i = 0; i < usedFrames; i++)
			{
				var frameValues = new object[numVertices][];

				for (var j = 0; j < numVertices; j++)
				{
					// TODO find out what this is!
					// What we can animate using vertex animations: position, normal, uv
					var unk7a = reader.ReadSByte();
					var unk7b = reader.ReadSByte();
					var unk7c = reader.ReadSByte();
					var unk7d = reader.ReadSByte();
					var unk7e = reader.ReadSByte();
					var unk7f = reader.ReadSByte();
					var unk7g = reader.ReadSByte();
					var unk7h = reader.ReadSByte();

					frameValues[j] = new object[] {unk7a, unk7b, unk7c, unk7d, unk7e, unk7f, unk7g, unk7h};
					//Console.WriteLine($"Vertex: {unk7a} {unk7b} {unk7c} {unk7d} {unk7e} {unk7f} {unk7g} {unk7h}");
				}

				this.Frames.Add(frameIds[i], frameValues);
			}

			if (flags == 0)
				return;

			for (var i = 0; i < this.Length; i++)
			{
				// Every second is 0. Starts at 1, every next has has +(numVertices * 4). Max value is filled up till the end without a 0.
				// Example for 10 vertices:
				// 1,0,41,0,81,0,121,0,161,0,201,0,241,0,281,0,321,0,361,0,401,401,401,401,401,...
				// Could be to use as some kind of fast access pointer...?
				var unk8 = reader.ReadInt32();
				//Console.WriteLine($"Map: {unk8}");
			}
		}
	}
}
