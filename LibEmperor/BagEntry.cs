namespace LibEmperor
{
	using System;
	using System.IO;
	using System.Text;

	public class BagEntry
	{
		[Flags]
		private enum Flags
		{
			Stereo = 1,
			Uncompressed = 2,
			Is16Bit = 4,
			Compressed = 8,
			Unk = 16,
			Mp3 = 32
		}

		private readonly BinaryReader reader;

		public readonly string Path;

		private readonly int offset;
		private readonly int length;
		private readonly int sampleRate;
		private readonly Flags flags;
		private readonly int unk;

		public BagEntry(BinaryReader reader)
		{
			this.reader = reader;
			this.Path = new string(reader.ReadChars(32)).Split('\0')[0];
			this.offset = reader.ReadInt32();
			this.length = reader.ReadInt32();
			this.sampleRate = reader.ReadInt32();
			this.flags = (Flags) reader.ReadInt32();
			this.unk = reader.ReadInt32(); // formatFlags

			if ((int) this.flags >> 6 != 0)
				throw new Exception("Unknown flags!");

			this.Path += (this.flags & Flags.Mp3) != 0 ? ".mp3" : ".wav";
		}

		public byte[] Read()
		{
			this.reader.BaseStream.Position = this.offset;

			if ((this.flags & Flags.Mp3) != 0)
			{
				// TODO formatFlags look like 2 shorts here.
				// The first one is either 4 on ingame tracks or 112 on menu and score. Possibly loop offset information?
				// The last one is always -13.

				return this.reader.ReadBytes(this.length);
			}

			var compressed = (this.flags & Flags.Compressed) != 0;
			var uncompressed = (this.flags & Flags.Uncompressed) != 0;

			if (compressed && uncompressed || !compressed && !uncompressed)
				throw new Exception("Unknown flags combination!");

			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);

			writer.Write(Encoding.ASCII.GetBytes("RIFF"));
			writer.Write(this.length * (compressed ? 4 : 1) + 36);
			writer.Write(Encoding.ASCII.GetBytes("WAVE"));
			writer.Write(Encoding.ASCII.GetBytes("fmt "));
			writer.Write(16);
			writer.Write((short) 1);
			writer.Write((short) ((this.flags & Flags.Stereo) != 0 ? 2 : 1));
			writer.Write(this.sampleRate);
			writer.Write(this.sampleRate * ((this.flags & Flags.Is16Bit) != 0 ? 2 : 1) * ((this.flags & Flags.Stereo) != 0 ? 2 : 1));
			writer.Write((short) ((this.flags & Flags.Is16Bit) != 0 ? 2 : 1));
			writer.Write((short) ((this.flags & Flags.Is16Bit) != 0 ? 16 : 8));
			writer.Write(Encoding.ASCII.GetBytes("data"));
			writer.Write(this.length * (compressed ? 4 : 1));

			if (compressed)
			{
				var currentSample = 0;
				var index = 0;

				for (var i = 0; i < this.length; i++)
				{
					var value = this.reader.ReadByte();
					writer.Write(BagEntry.Decode((byte) ((value & 0b00001111) >> 0), ref index, ref currentSample));
					writer.Write(BagEntry.Decode((byte) ((value & 0b11110000) >> 4), ref index, ref currentSample));
				}
			}
			else
				writer.Write(this.reader.ReadBytes(this.length));

			return stream.ToArray();
		}

		private static readonly int[] IndexTable = {-1, -1, -1, -1, 2, 4, 6, 8};

		private static readonly int[] StepTable =
		{
			7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143, 157, 173, 190,
			209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066, 2272, 2499,
			2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289, 16818, 18500, 20350,
			22385, 24623, 27086, 29794, 32767
		};

		private static short Decode(byte nibble, ref int index, ref int current)
		{
			var delta = nibble & 0b0111;
			var diff = BagEntry.StepTable[index] * (2 * delta + 1) / 16;

			if ((nibble & 0b1000) != 0)
				diff = -diff;

			current = Math.Clamp(current + diff, short.MinValue, short.MaxValue);
			index = Math.Clamp(index + BagEntry.IndexTable[delta], 0, BagEntry.StepTable.Length - 1);

			return (short) current;
		}
	}
}
