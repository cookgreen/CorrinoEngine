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
			int entriesNegative = reader.ReadInt32();
			int usedFrames = reader.ReadInt32();
			if (usedFrames < 0)
				throw new Exception("Invalid XBF vertex animation frame count.");

			var frameIds = new List<int>(usedFrames);

			for (int i = 0; i < usedFrames; i++)
				frameIds.Add(reader.ReadInt32());

			if (entriesNegative >= 0)
				return;

			int kind = reader.ReadInt16();
			uint flags = reader.ReadUInt16();
			int entries = reader.ReadInt32();
			if (usedFrames == 0)
				return;
			if (-entries != entriesNegative)
				throw new Exception("Wrong entries!");

			int numVertices = entries / usedFrames;
			float coordinateDivisor = 1 << kind;
			var usedPositions = new List<object[][]>(usedFrames);

			for (int i = 0; i < usedFrames; i++)
			{
				var frameValues = new object[numVertices][];

				for (int j = 0; j < numVertices; j++)
				{
					float x = reader.ReadInt16() / coordinateDivisor;
					float y = reader.ReadInt16() / coordinateDivisor;
					float z = reader.ReadInt16() / coordinateDivisor;
					reader.ReadInt16();
					frameValues[j] = new object[] { x, y, z };
				}

				usedPositions.Add(frameValues);
			}

			if (flags != 0)
			{
				for (int i = 0; i < this.Length; i++)
				{
					int frameOffset = reader.ReadInt32();
					if (frameOffset <= 0)
						continue;

					int usedIndex = (frameOffset - 1) / Math.Max(1, numVertices * 4);
					if (usedIndex >= 0 && usedIndex < usedPositions.Count)
						this.Frames[i] = usedPositions[usedIndex];
				}
			}
			else
			{
				for (int i = 0; i < usedFrames; i++)
				{
					this.Frames[frameIds[i]] = usedPositions[i];
				}
			}
		}
	}
}
