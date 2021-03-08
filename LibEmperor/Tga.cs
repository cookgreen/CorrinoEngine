namespace LibEmperor
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class Tga
	{
		public readonly ushort Width;
		public readonly ushort Height;
		public readonly byte[] Pixels;

		public Tga(Stream stream)
		{
			using var reader = new BinaryReader(stream);

			if (reader.ReadByte() != 0x00)
				throw new Exception("Unsupported IDLength");

			var colorMapType = reader.ReadByte();

			if (colorMapType != 0x00 && colorMapType != 0x01)
				throw new Exception("Unsupported ColorMapType");

			var imageType = reader.ReadByte();

			if (imageType != 0x00 && imageType != 0x01 && imageType != 0x02 && imageType != 0x0a)
				throw new Exception("Unsupported ImageType");

			var firstIndexEntry = 0;
			var colorMapLength = 0;
			var colorMapEntrySize = 0;

			// This is a custom tga format, as this imageType means originally "no image".
			if (imageType == 0x00)
				reader.ReadBytes(3);
			else
			{
				firstIndexEntry = reader.ReadUInt16();
				colorMapLength = reader.ReadUInt16();
				colorMapEntrySize = reader.ReadByte();

				if (colorMapType == 0x01)
				{
					if (firstIndexEntry + colorMapLength > 256 || colorMapLength == 0)
						throw new Exception("Unsupported ColorMap");

					if (colorMapEntrySize != 16 && colorMapEntrySize != 24 && colorMapEntrySize != 32)
						throw new Exception("Unsupported ColorMap");
				}
				else if (firstIndexEntry != 0 || colorMapLength != 0 || colorMapEntrySize != 0)
					throw new Exception("Unsupported ColorMap");

				if (reader.ReadUInt16() != 0x0000)
					throw new Exception("Unsupported XOrigin");

				if (reader.ReadUInt16() != 0x0000)
					throw new Exception("Unsupported YOrigin");
			}

			this.Width = reader.ReadUInt16();
			this.Height = reader.ReadUInt16();

			var pixelDepth = reader.ReadByte();

			if (colorMapType == 0x01)
			{
				if (pixelDepth != 8)
					throw new Exception("Unsupported PixelDepth");
			}
			else if (pixelDepth != 16 && pixelDepth != 24 && pixelDepth != 32)
				throw new Exception("Unsupported PixelDepth");

			// TODO implement alphaBits
			var alphaBits = 0;
			var flipHorizontal = false;
			var flipVertical = false;

			if (imageType != 0x00)
			{
				var imageDescriptor = reader.ReadByte();
				alphaBits = imageDescriptor & 0x0f;
				flipHorizontal = (imageDescriptor & 0x10) != 0x00;
				flipVertical = (imageDescriptor & 0x20) != 0x00;

				if (imageDescriptor >> 6 != 0x00)
					throw new Exception("Unsupported ImageDescriptor");

				// TODO what does it mean when alphaBits are 8?
				if (pixelDepth == 8 && alphaBits != 0 && alphaBits != 8)
					throw new Exception("Unsupported AlphaBits");

				// TODO what does it mean when alphaBits are 0?
				if (pixelDepth == 16 && alphaBits != 1 && alphaBits != 0)
					throw new Exception("Unsupported AlphaBits");

				if (pixelDepth == 24 && alphaBits != 0)
					throw new Exception("Unsupported AlphaBits");

				// TODO what does it mean when alphaBits are 0?
				if (pixelDepth == 32 && alphaBits != 8 && alphaBits != 0)
					throw new Exception("Unsupported AlphaBits");
			}

			this.Pixels = new byte[this.Width * this.Height * 4];

			var colorMap = new byte[colorMapLength * 4];

			if (colorMapType == 0x01)
				for (var i = 0; i < colorMapLength; i++)
					Array.Copy(Tga.ReadColor(colorMapEntrySize, reader), 0, colorMap, i * 4, 4);

			var pixelReader = reader;

			if (imageType == 0x0a)
			{
				var bytesPerPixel = pixelDepth / 8;
				var uncompressed = new byte[this.Width * this.Height * bytesPerPixel];

				for (var i = 0; i < uncompressed.Length;)
				{
					var info = reader.ReadByte();
					var mode = info >> 7;
					var count = (info & 0x7f) + 1;

					if (mode == 1)
					{
						var pixel = Tga.ReadColor(pixelDepth, reader);

						for (var j = 0; j < count; j++, i += bytesPerPixel)
							Array.Copy(pixel, 0, uncompressed, i, bytesPerPixel);
					}
					else
					{
						var numBytes = bytesPerPixel * count;
						Array.Copy(reader.ReadBytes(numBytes), 0, uncompressed, i, numBytes);
						i += numBytes;
					}
				}

				pixelReader = new BinaryReader(new MemoryStream(uncompressed));
			}

			for (var y = 0; y < this.Height; y++)
			for (var x = 0; x < this.Width; x++)
			{
				var rgba = new byte[4];

				if (colorMapType == 0x01)
					Array.Copy(colorMap, (pixelReader.ReadByte() - firstIndexEntry) * 4, rgba, 0, 4);
				else
					rgba = Tga.ReadColor(pixelDepth, pixelReader);

				if (rgba[0] == 0xff && rgba[1] == 0x00 && rgba[2] == 0xff && rgba[3] == 0xff)
					continue;

				Array.Copy(rgba, 0, this.Pixels, ((flipVertical ? this.Height - 1 - y : y) * this.Width + (flipHorizontal ? this.Width - 1 - x : x)) * 4, 4);
			}
		}

		private static byte[] ReadColor(int pixelDepth, BinaryReader reader)
		{
			switch (pixelDepth)
			{
				case 16:
				{
					var color16 = reader.ReadInt16();

					return Tga.FlipRgb(
						new[]
						{
							(byte) (((color16 >> 0) & 0x1f) * 0xff / 0x1f), (byte) (((color16 >> 5) & 0x1f) * 0xff / 0x1f),
							(byte) (((color16 >> 10) & 0x1f) * 0xff / 0x1f),

							// It seems the alpha value is not used here...
							//(byte) ((color16 >> 15) * 0xff)
							(byte) 0xff
						}
					);
				}

				case 24:
					return Tga.FlipRgb(new[] {reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), (byte) 0xff});

				case 32:
					return Tga.FlipRgb(new[] {reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()});

				default:
					return new byte[4];
			}
		}

		private static byte[] FlipRgb(IReadOnlyList<byte> flipped)
		{
			return new[] {flipped[2], flipped[1], flipped[0], flipped[3]};
		}
	}
}
