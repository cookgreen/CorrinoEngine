using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace CorrinoEngine.Maps
{
    public class MapNavigationData
    {
        public const int NavSize = 256;
        public const int PassGround = 1;
        public const int PassInfantry = 2;
        public const int PassAir = 4;

        public Vector2i SourceGridSize { get; set; }
        public int[] TerrainType { get; private set; } = Array.Empty<int>();
        public int[] SourceTileX { get; private set; } = Array.Empty<int>();
        public int[] SourceTileY { get; private set; } = Array.Empty<int>();
        public byte[] SpiceValue { get; private set; } = Array.Empty<byte>();
        public int[] PassMask { get; private set; } = Array.Empty<int>();
        public float[] MovementCost { get; private set; } = Array.Empty<float>();
        public byte[] Buildable { get; private set; } = Array.Empty<byte>();

        public bool IsLoaded => TerrainType.Length == NavSize * NavSize;

        public static MapNavigationData Build(MapXbf mapXbf)
        {
            MapNavigationData nav = new MapNavigationData
            {
                SourceGridSize = mapXbf.MapSize,
                TerrainType = new int[NavSize * NavSize],
                SourceTileX = new int[NavSize * NavSize],
                SourceTileY = new int[NavSize * NavSize],
                SpiceValue = new byte[NavSize * NavSize],
                PassMask = new int[NavSize * NavSize],
                MovementCost = new float[NavSize * NavSize],
                Buildable = new byte[NavSize * NavSize]
            };

            for (int navY = 0; navY < NavSize; navY++)
            {
                for (int navX = 0; navX < NavSize; navX++)
                {
                    int i = navY * NavSize + navX;
                    int tileX = NavAxisToSourceTile(navX, mapXbf.MapSize.X);
                    int tileY = NavAxisToSourceTile(navY, mapXbf.MapSize.Y);
                    int typeId = mapXbf.TileAt(tileX, tileY);
                    nav.SourceTileX[i] = tileX;
                    nav.SourceTileY[i] = tileY;
                    nav.TerrainType[i] = typeId;
                    ApplyTerrainAttrs(nav.PassMask, nav.MovementCost, nav.Buildable, i, typeId);
                    int spice = mapXbf.SpiceAt(tileX, tileY);
                    nav.SpiceValue[i] = (byte)Math.Max(0, spice);
                }
            }

            return nav;
        }

        public bool IsBuildableCell(int navX, int navY)
        {
            if (!IsLoaded || navX < 0 || navY < 0 || navX >= NavSize || navY >= NavSize)
                return false;
            return Buildable[navY * NavSize + navX] != 0;
        }

        public static Vector2i WorldToNav(Vector2 world, Vector2 mapMin, Vector2 mapMax)
        {
            Vector2 size = mapMax - mapMin;
            if (size.X <= 0 || size.Y <= 0)
                return Vector2i.Zero;

            float x01 = Math.Clamp((world.X - mapMin.X) / size.X, 0f, 1f);
            float y01 = Math.Clamp((world.Y - mapMin.Y) / size.Y, 0f, 1f);
            return new Vector2i(
                Math.Clamp((int)MathF.Floor(x01 * (NavSize - 1)), 0, NavSize - 1),
                Math.Clamp((int)MathF.Floor(y01 * (NavSize - 1)), 0, NavSize - 1));
        }

        private static int NavAxisToSourceTile(int navCoord, int sourceSize)
        {
            return Math.Clamp((int)MathF.Floor((navCoord + 0.5f) / NavSize * sourceSize), 0, sourceSize - 1);
        }

        private static void ApplyTerrainAttrs(int[] passMask, float[] movementCost, byte[] buildable, int index, int typeId)
        {
            switch (MapTerrainRules.FromId(typeId))
            {
                case MapTerrainType.Sand:
                    passMask[index] = PassGround | PassAir;
                    movementCost[index] = 1.15f;
                    buildable[index] = 0;
                    break;
                case MapTerrainType.Rock:
                    passMask[index] = PassGround | PassAir;
                    movementCost[index] = 1.0f;
                    buildable[index] = 1;
                    break;
                case MapTerrainType.NonBuildRock:
                    passMask[index] = PassGround | PassAir;
                    movementCost[index] = 1.0f;
                    buildable[index] = 0;
                    break;
                case MapTerrainType.InfantryRock:
                    passMask[index] = PassInfantry | PassAir;
                    movementCost[index] = 1.2f;
                    buildable[index] = 0;
                    break;
                case MapTerrainType.Dustbowl:
                    passMask[index] = PassGround | PassAir;
                    movementCost[index] = 1.35f;
                    buildable[index] = 0;
                    break;
                case MapTerrainType.Ramp:
                    passMask[index] = PassGround | PassAir;
                    movementCost[index] = 1.25f;
                    buildable[index] = 0;
                    break;
                default:
                    passMask[index] = PassAir;
                    movementCost[index] = 1000000f;
                    buildable[index] = 0;
                    break;
            }
        }
    }
}
