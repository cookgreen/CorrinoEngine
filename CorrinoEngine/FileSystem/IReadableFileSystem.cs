namespace CorrinoEngine.FileSystem
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public interface IReadableFileSystem : IDisposable
	{
		public bool Exists(string path);
		public Stream Read(string path);
		public IEnumerable<string> GetFiles(string path = "");
	}
}
