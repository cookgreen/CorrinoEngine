namespace LibEmperor
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Numerics;

	public class XbfObjectAnimation
	{
		public readonly int Length;
		public readonly Dictionary<int, Matrix4x4> Frames = new();

		public XbfObjectAnimation(BinaryReader reader)
		{
			this.Length = reader.ReadInt32() + 1;
			var usedFrames = reader.ReadInt32();

			if (usedFrames == -1)
				for (var i = 0; i < this.Length; i++)
					this.Frames.Add(
						i,
						new Matrix4x4(
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle()
						)
					);
			else if (usedFrames == -2)
				for (var i = 0; i < this.Length; i++)
					this.Frames.Add(
						i,
						new Matrix4x4(
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							0,
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							0,
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							0,
							reader.ReadSingle(),
							reader.ReadSingle(),
							reader.ReadSingle(),
							1
						)
					);
			else if (usedFrames == -3)
			{
				var matrices = new Matrix4x4[reader.ReadInt32()];
				var frames = new short[this.Length];

				for (var i = 0; i < frames.Length; i++)
					frames[i] = reader.ReadInt16();

				for (var i = 0; i < matrices.Length; i++)
					matrices[i] = new Matrix4x4(
						reader.ReadSingle(),
						reader.ReadSingle(),
						reader.ReadSingle(),
						0,
						reader.ReadSingle(),
						reader.ReadSingle(),
						reader.ReadSingle(),
						0,
						reader.ReadSingle(),
						reader.ReadSingle(),
						reader.ReadSingle(),
						0,
						reader.ReadSingle(),
						reader.ReadSingle(),
						reader.ReadSingle(),
						1
					);

				for (var i = 0; i < this.Length; i++)
					this.Frames.Add(i, matrices[frames[i]]);
			}
			else
				for (var i = 0; i < usedFrames; i++)
				{
					var frameId = reader.ReadInt16();
					var flags = reader.ReadInt16();

					if ((flags & 0b1000111111111111) != 0)
						throw new Exception("Unknown flags!");

					var frame = Matrix4x4.Identity;

					if (((flags >> 12) & 0b001) != 0)
						frame *= Matrix4x4.CreateFromQuaternion(
							new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
						);

					if (((flags >> 12) & 0b010) != 0)
						frame *= Matrix4x4.CreateScale(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

					if (((flags >> 12) & 0b100) != 0)
						frame *= Matrix4x4.CreateTranslation(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

					this.Frames.Add(frameId, frame);
				}
		}
	}
}
