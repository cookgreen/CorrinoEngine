using OpenTK.Mathematics;
using System;
using System.IO;

namespace CorrinoEngine.Maps
{
    public class MapHeightField
    {
        public const int Resolution = 256;

        public int[] Values { get; private set; } = Array.Empty<int>();
        public float HeightScale { get; set; } = 1f / 256f;

        public bool IsLoaded => Values.Length == Resolution * Resolution;

        public static MapHeightField Load(Stream stream)
        {
            using MemoryStream memory = new MemoryStream();
            if (stream.CanSeek)
                stream.Position = 0;
            stream.CopyTo(memory);
            byte[] bytes = memory.ToArray();
            if (bytes.Length != Resolution * Resolution * 2)
                throw new InvalidDataException($"CPF size mismatch: {bytes.Length}");

            MapHeightField field = new MapHeightField
            {
                Values = new int[Resolution * Resolution]
            };

            for (int i = 0; i < field.Values.Length; i++)
            {
                field.Values[i] = bytes[i * 2] | (bytes[i * 2 + 1] << 8);
            }

            return field;
        }

        public float SampleHeight01(float x01, float y01)
        {
            if (!IsLoaded)
                return 0f;

            float x = Math.Clamp(x01, 0f, 1f) * (Resolution - 1);
            float y = Math.Clamp(y01, 0f, 1f) * (Resolution - 1);
            int x0 = (int)MathF.Floor(x);
            int y0 = (int)MathF.Floor(y);
            int x1 = Math.Min(Resolution - 1, x0 + 1);
            int y1 = Math.Min(Resolution - 1, y0 + 1);
            float tx = x - x0;
            float ty = y - y0;

            float h00 = Values[y0 * Resolution + x0];
            float h10 = Values[y0 * Resolution + x1];
            float h01 = Values[y1 * Resolution + x0];
            float h11 = Values[y1 * Resolution + x1];
            float hx0 = MathHelper.Lerp(h00, h10, tx);
            float hx1 = MathHelper.Lerp(h01, h11, tx);
            return MathHelper.Lerp(hx0, hx1, ty) * HeightScale;
        }
    }
}
