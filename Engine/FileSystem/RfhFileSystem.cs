namespace CorrinoEngine.FileSystem
{
	using LibEmperor;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class RfhFileSystem : IReadableFileSystem
	{
		private readonly Rfh rfh;

		public RfhFileSystem(Rfh rfh)
		{
			this.rfh = rfh;
		}

		public bool Exists(string path)
		{
			var rfhPath = path.Replace('/', '\\');

			return this.rfh.Files.Any(rfhEntry => string.Equals(rfhEntry.Path, rfhPath, StringComparison.OrdinalIgnoreCase));
		}

		public Stream Read(string path)
		{
			var rfhPath = path.Replace('/', '\\');
			var entry = this.rfh.Files.FirstOrDefault(rfhEntry => rfhEntry.Path.Equals(rfhPath, StringComparison.OrdinalIgnoreCase));

			return entry != null ? new MemoryStream(entry.Read()) : null;
		}

		public IEnumerable<string> GetAllFiles()
		{
			return this.rfh.Files.Select(rfhEntry => rfhEntry.Path.Replace('\\', '/').Trim('/'));
		}

		public IEnumerable<string> GetFiles(string path = "")
		{
			var rfhPath = path.Replace('/', '\\');

			return this.rfh.Files.Where(rfhEntry => rfhEntry.Path.StartsWith(rfhPath, StringComparison.OrdinalIgnoreCase))
				.Select(rfhEntry => rfhEntry.Path.Replace('\\', '/').Trim('/'));
		}

		public IEnumerable<string> GetFilesByExtension(string extension = "")
		{
			return this.rfh.Files.Where(rfhEntry=>Path.GetExtension(rfhEntry.Path).ToLower() == "."+extension)
				.Select(rfhEntry => rfhEntry.Path.Replace('\\', '/').Trim('/'));
		}

		public void Dispose()
		{
			this.rfh.Dispose();
			GC.SuppressFinalize(this);
		}
    }
}
