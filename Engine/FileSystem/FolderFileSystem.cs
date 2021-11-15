namespace CorrinoEngine.FileSystem
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class FolderFileSystem : IWritableFileSystem
	{
		private readonly string path;

		public FolderFileSystem(string path)
		{
			this.path = path;
		}

		public bool Exists(string path)
		{
			return File.Exists(Path.Combine(this.path, path));
		}

		public Stream Read(string path)
		{
			try
			{
				return File.OpenRead(Path.Combine(this.path, path));
			}
			catch (Exception)
			{
				return null;
			}
		}

		public IEnumerable<string> GetFiles(string path)
		{
			return Directory.GetFiles(Path.Combine(this.path, path), "*", SearchOption.AllDirectories)
				.Select(file => file.Substring(this.path.Length).Replace('\\', '/').Trim('/'));
		}

		public IEnumerable<string> GetFilesByExtension(string extension = "")
		{
			return Directory.GetFiles(path, "*." + extension, SearchOption.AllDirectories);
		}

		public void Delete(string path)
		{
			File.Delete(Path.Combine(this.path, path));
		}

		public Stream Write(string path)
		{
			try
			{
				return File.Open(Path.Combine(this.path, path), FileMode.Create);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
    }
}
