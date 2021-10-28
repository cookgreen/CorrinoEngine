namespace CorrinoEngine.Assets
{
	using FileSystem;
	using LibEmperor;
	using OpenTK.Graphics.OpenGL4;
	using System;

	public class Texture : IDisposable
	{
		public readonly int Id;

		public Texture(AssetManager assetManager, IReadableFileSystem fileSystem, string path)
		{
			this.Id = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, this.Id);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (uint) TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (uint) TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint) TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint) TextureMagFilter.Linear);

			var tga = new Tga(fileSystem.Read(path)!);

			GL.TexImage2D(
				TextureTarget.Texture2D,
				0,
				PixelInternalFormat.Rgba8,
				tga.Width,
				tga.Height,
				0,
				PixelFormat.Rgba,
				PixelType.UnsignedByte,
				tga.Pixels
			);
		}

		public void Dispose()
		{
			GL.DeleteTexture(this.Id);
			GC.SuppressFinalize(this);
		}
	}
}
