using CorrinoEngine.Assets;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CorrinoEngine.Maps
{
    public class MapTerrainMaterialData : IDisposable
    {
        public int GroundColorTexture { get; private set; }
        public int GroundLightTexture { get; private set; }
        public Vector3 LightDirection { get; set; } = new Vector3(-0.25f, 1f, 0.35f);
        public Vector3 AmbientTint { get; set; } = Vector3.One;

        public bool HasGroundColor => GroundColorTexture != 0;
        public bool HasGroundLight => GroundLightTexture != 0;

        public static MapTerrainMaterialData Load(AssetManager assetManager, GameMap map, MapLightingData lightingData)
        {
            MapTerrainMaterialData data = new MapTerrainMaterialData();
            if (lightingData != null)
            {
                data.LightDirection = lightingData.Direction.LengthSquared > 0 ? lightingData.Direction.Normalized() : data.LightDirection;
                if (lightingData.Colors.Count > 0)
                {
                    Color first = lightingData.Colors[0];
                    data.AmbientTint = new Vector3(first.R / 255f, first.G / 255f, first.B / 255f);
                }
            }

            data.GroundColorTexture = LoadGroundColorTexture(assetManager, map);
            data.GroundLightTexture = LoadGroundLightTexture(assetManager, map);
            return data;
        }

        public void Dispose()
        {
            if (GroundColorTexture != 0)
                GL.DeleteTexture(GroundColorTexture);
            if (GroundLightTexture != 0)
                GL.DeleteTexture(GroundLightTexture);
        }

        private static int LoadGroundColorTexture(AssetManager assetManager, GameMap map)
        {
            string path = map?.Metadata?.GroundColor;
            if (string.IsNullOrWhiteSpace(path))
                return 0;

            using Stream stream = assetManager.Read(path);
            if (stream == null)
                return 0;

            using MemoryStream memory = new MemoryStream();
            stream.CopyTo(memory);
            byte[] bytes = memory.ToArray();
            if (bytes.Length != 2048 * 2048 + 8)
                return 0;

            byte[] src = bytes.Skip(8).ToArray();
            byte[] linear = new byte[src.Length];
            int dstOffset = 0;
            for (int blockY = 0; blockY < 512; blockY++)
            {
                int tileRow = blockY >> 6;
                int rowInTile = blockY & 63;
                for (int tileCol = 0; tileCol < 8; tileCol++)
                {
                    int srcOffset = ((tileRow * 8 + tileCol) * 4096 + rowInTile * 64) * 16;
                    System.Buffer.BlockCopy(src, srcOffset, linear, dstOffset, 1024);
                    dstOffset += 1024;
                }
            }

            return CreateCompressedTexture(linear, InternalFormat.CompressedRgbaS3tcDxt3Ext, 2048, 2048);
        }

        private static int LoadGroundLightTexture(AssetManager assetManager, GameMap map)
        {
            string path = map?.Metadata?.GroundLight;
            if (string.IsNullOrWhiteSpace(path))
                return 0;

            using Stream stream = assetManager.Read(path);
            if (stream == null)
                return 0;

            using MemoryStream memory = new MemoryStream();
            stream.CopyTo(memory);
            byte[] bytes = memory.ToArray();
            if (bytes.Length != 2048 * 2048)
                return 0;

            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, 2048, 2048, 0, PixelFormat.Red, PixelType.UnsignedByte, bytes);
            return texture;
        }

        private static int CreateCompressedTexture(byte[] compressedBytes, InternalFormat format, int width, int height)
        {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, format, width, height, 0, compressedBytes.Length, compressedBytes);
            return texture;
        }
    }
}
