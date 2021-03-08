namespace LibEmperor
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class Rfh : IDisposable
	{
		private readonly Stream headerStream;
		private readonly Stream dataStream;

		public readonly List<RfhEntry> Files = new();

		public Rfh(Stream headerStream, Stream dataStream)
		{
			this.headerStream = headerStream;
			this.dataStream = dataStream;
			var headerReader = new BinaryReader(headerStream);
			var dataReader = new BinaryReader(dataStream);

			while (headerReader.BaseStream.Position < headerReader.BaseStream.Length)
				this.Files.Add(new RfhEntry(headerReader, dataReader));
		}

		public void Dispose()
		{
			this.headerStream.Dispose();
			this.dataStream.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
