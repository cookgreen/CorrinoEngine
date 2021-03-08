namespace CorrinoEngine.FileSystem
{
	using LibEmperor;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class BagFileSystem : IReadableFileSystem
	{
		private readonly Bag bag;

		public BagFileSystem(Bag bag)
		{
			this.bag = bag;
		}

		public bool Exists(string path)
		{
			var bagPath = path.Replace('/', '\\');

			return this.bag.Files.Any(bagEntry => string.Equals(bagEntry.Path, bagPath, StringComparison.OrdinalIgnoreCase));
		}

		public Stream? Read(string path)
		{
			var bagPath = path.Replace('/', '\\');
			var entry = this.bag.Files.FirstOrDefault(bagEntry => string.Equals(bagEntry.Path, bagPath, StringComparison.OrdinalIgnoreCase));

			return entry != null ? new MemoryStream(entry.Read()) : null;
		}

		public IEnumerable<string> GetFiles(string path = "")
		{
			var bagPath = path.Replace('/', '\\');

			return this.bag.Files.Where(bagEntry => bagEntry.Path.StartsWith(bagPath, StringComparison.OrdinalIgnoreCase))
				.Select(bagEntry => bagEntry.Path.Replace('\\', '/').Trim('/'));
		}

		public void Dispose()
		{
			this.bag.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
