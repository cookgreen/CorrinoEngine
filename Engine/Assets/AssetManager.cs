namespace CorrinoEngine.Assets
{
	using FileSystem;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class AssetManager
	{
		private readonly IReadableFileSystem fileSystem;
		private readonly Dictionary<string, object> assetForPath = new();
		private readonly Dictionary<object, string> pathForAssets = new();
		private readonly Dictionary<object, HashSet<object>> holdersForAsset = new();
		private readonly Dictionary<object, HashSet<object>> assetsForHolder = new();

		public AssetManager(IReadableFileSystem fileSystem)
		{
			this.fileSystem = fileSystem;
		}

		public IEnumerable<string> GetAllFiles()
		{
			return fileSystem.GetAllFiles();
		}

		public IEnumerable<string> GetFiles(string path)
		{
			return fileSystem.GetFiles(path);
		}

		public IEnumerable<string> GetFilesByExtension(string extension)
        {
			return fileSystem.GetFilesByExtension(extension);
        }

		public T Load<T>(object holder) where T : class
		{
			return this.Load<T>(holder, typeof(T).FullName!);
		}

		public T Load<T>(object holder, string path) where T : class
		{
			if (!this.assetForPath.ContainsKey(path))
			{
				this.assetForPath.Add(path, Activator.CreateInstance(typeof(T), this, this.fileSystem, path)!);
				this.pathForAssets.Add(this.assetForPath[path], path);
				this.holdersForAsset.Add(this.assetForPath[path], new HashSet<object>());
			}

			var asset = (this.assetForPath[path] as T)!;

			if (!this.assetsForHolder.ContainsKey(holder))
				this.assetsForHolder.Add(holder, new HashSet<object>());

			if (this.holdersForAsset[asset].Contains(holder))
				return asset;

			this.holdersForAsset[asset].Add(holder);
			this.assetsForHolder[holder].Add(asset);

			return asset;
		}

		public void Unload(object holder)
		{
			foreach (var asset in this.assetsForHolder[holder])
				this.Unload(holder, asset);

			this.assetsForHolder.Remove(holder);
		}

		public void Unload(object holder, object asset)
		{
			this.holdersForAsset[asset].Remove(holder);

			if (this.holdersForAsset[asset].Any())
				return;

			this.holdersForAsset.Remove(asset);
			this.assetForPath.Remove(this.pathForAssets[asset]);
			this.pathForAssets.Remove(asset);

			if (asset is IDisposable disposable)
				disposable.Dispose();
		}
	}
}
