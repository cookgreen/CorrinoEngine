using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;

namespace CorrinoEngine.Maps
{
    public class MapXbf
    {
        private const uint TlvTagPrefix = 0xA0000000;
        private const int MeshPrefixBytes = 8;

        public const int TagZonesOrMaterials = 0x01;
        public const int TagMapSize = 0x02;
        public const int TagTiles = 0x03;
        public const int TagSpice = 0x04;
        public const int TagGameElements = 0x05;
        public const int TagBuildings = 0x07;
        public const int TagSpiceMound = 0x09;
        public const int TagUnknown0A = 0x0A;
        public const int TagBuildTimestamp = 0x0B;

        public int Version { get; private set; }
        public int MetaEnd { get; private set; }
        public Vector2i MapSize { get; private set; }
        public int BuildTimestamp { get; private set; }
        public bool HasBuildTimestamp { get; private set; }
        public Vector2i ChecksumValues { get; private set; }
        public bool HasChecksumValues { get; private set; }
        public byte[] TileGrid { get; private set; } = Array.Empty<byte>();
        public byte[] SpiceGrid { get; private set; } = Array.Empty<byte>();
        public List<MapXbfBuilding> Buildings { get; } = new();
        public List<Vector2i> SpiceMounds { get; } = new();
        public List<MapXbfTlvRecord> TlvRecords { get; } = new();
        public byte[] RawBytes { get; private set; } = Array.Empty<byte>();
        public int MeshOffset => MetaEnd + MeshPrefixBytes;

        public static MapXbf Load(Stream stream)
        {
            using MemoryStream memory = new MemoryStream();
            if (stream.CanSeek)
                stream.Position = 0;
            stream.CopyTo(memory);
            return Parse(memory.ToArray());
        }

        public static MapXbf Parse(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 16)
                throw new InvalidDataException("Map XBF file is too small.");

            MapXbf map = new MapXbf
            {
                RawBytes = bytes
            };

            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            map.Version = reader.ReadInt32();
            if (map.Version != 1)
                throw new InvalidDataException($"Unsupported Map XBF version {map.Version}.");

            map.MetaEnd = reader.ReadInt32();
            if (map.MetaEnd < 8 || map.MetaEnd + MeshPrefixBytes + 4 > bytes.Length)
                throw new InvalidDataException($"Invalid TLV meta_end {map.MetaEnd} for {bytes.Length} byte file.");

            map.ParseTlv(bytes);
            return map;
        }

        public bool HasSizedTileGrid()
        {
            return MapSize.X > 0 && MapSize.Y > 0 && TileGrid.Length == MapSize.X * MapSize.Y;
        }

        public bool HasSizedSpiceGrid()
        {
            return MapSize.X > 0 && MapSize.Y > 0 && SpiceGrid.Length == MapSize.X * MapSize.Y;
        }

        public int TileAt(int x, int y)
        {
            if (!HasSizedTileGrid() || x < 0 || y < 0 || x >= MapSize.X || y >= MapSize.Y)
                return -1;
            return TileGrid[y * MapSize.X + x];
        }

        public int SpiceAt(int x, int y)
        {
            if (!HasSizedSpiceGrid() || x < 0 || y < 0 || x >= MapSize.X || y >= MapSize.Y)
                return -1;
            return SpiceGrid[y * MapSize.X + x];
        }

        private void ParseTlv(byte[] bytes)
        {
            int offset = 8;
            while (offset < MetaEnd)
            {
                if (offset + 8 > MetaEnd)
                    throw new InvalidDataException($"Truncated TLV record header at offset {offset}.");

                uint tag = ReadU32(bytes, offset);
                int length = (int)ReadU32(bytes, offset + 4);
                int payloadOffset = offset + 8;
                int payloadEnd = payloadOffset + length;
                if ((tag & 0xFFFFFF00) != TlvTagPrefix)
                    throw new InvalidDataException($"Invalid TLV tag 0x{tag:X8} at offset {offset}.");
                if (payloadEnd > MetaEnd)
                    throw new InvalidDataException($"TLV tag 0x{tag & 0xFF:X2} overruns meta section.");

                int tagId = (int)(tag & 0xFF);
                TlvRecords.Add(new MapXbfTlvRecord(offset, tagId, length, payloadOffset));
                ParseTlvRecord(tagId, bytes, payloadOffset, length);
                offset = payloadEnd;
            }

            if (MapSize.X > 0 && MapSize.Y > 0)
            {
                if (TileGrid.Length > 0 && TileGrid.Length != MapSize.X * MapSize.Y)
                    throw new InvalidDataException("Map XBF Tiles length does not match MapSize.");
                if (SpiceGrid.Length > 0 && SpiceGrid.Length != MapSize.X * MapSize.Y)
                    throw new InvalidDataException("Map XBF Spice length does not match MapSize.");
            }
        }

        private void ParseTlvRecord(int tagId, byte[] bytes, int payloadOffset, int length)
        {
            switch (tagId)
            {
                case TagMapSize:
                    if (length == 8)
                        MapSize = new Vector2i(ReadI32(bytes, payloadOffset), ReadI32(bytes, payloadOffset + 4));
                    break;
                case TagTiles:
                    TileGrid = CopyBytes(bytes, payloadOffset, length);
                    break;
                case TagSpice:
                    SpiceGrid = CopyBytes(bytes, payloadOffset, length);
                    break;
                case TagSpiceMound:
                    ParseSpiceMounds(bytes, payloadOffset, length);
                    break;
                case TagBuildings:
                    ParseBuildings(bytes, payloadOffset, length);
                    break;
                case TagUnknown0A:
                    if (length == 8)
                    {
                        ChecksumValues = new Vector2i(ReadI32(bytes, payloadOffset), ReadI32(bytes, payloadOffset + 4));
                        HasChecksumValues = true;
                    }
                    break;
                case TagBuildTimestamp:
                    if (length == 4)
                    {
                        BuildTimestamp = ReadI32(bytes, payloadOffset);
                        HasBuildTimestamp = true;
                    }
                    break;
            }
        }

        private void ParseSpiceMounds(byte[] bytes, int payloadOffset, int length)
        {
            if (length % 8 != 0)
                return;

            for (int offset = payloadOffset; offset < payloadOffset + length; offset += 8)
            {
                SpiceMounds.Add(new Vector2i(ReadI32(bytes, offset), ReadI32(bytes, offset + 4)));
            }
        }

        private void ParseBuildings(byte[] bytes, int payloadOffset, int length)
        {
            if (length < 4)
                return;

            int payloadEnd = payloadOffset + length;
            int count = (int)ReadU32(bytes, payloadOffset);
            int cursor = payloadOffset + 4;
            for (int i = 0; i < count; i++)
            {
                int nameEnd = cursor;
                while (nameEnd < payloadEnd && bytes[nameEnd] != 0)
                    nameEnd++;
                if (nameEnd >= payloadEnd)
                    return;

                string name = System.Text.Encoding.ASCII.GetString(bytes, cursor, nameEnd - cursor);
                cursor = nameEnd + 1;
                if (cursor + 12 > payloadEnd)
                    return;

                Buildings.Add(new MapXbfBuilding
                {
                    Name = name,
                    X = ReadI16(bytes, cursor),
                    Owner = ReadI16(bytes, cursor + 2),
                    Y = ReadI16(bytes, cursor + 4),
                    Padding = ReadI16(bytes, cursor + 6),
                    Reserved = ReadI32(bytes, cursor + 8)
                });
                cursor += 12;
            }
        }

        private static byte[] CopyBytes(byte[] bytes, int offset, int length)
        {
            byte[] result = new byte[length];
            Buffer.BlockCopy(bytes, offset, result, 0, length);
            return result;
        }

        private static uint ReadU32(byte[] bytes, int offset)
        {
            return (uint)(bytes[offset]
                | (bytes[offset + 1] << 8)
                | (bytes[offset + 2] << 16)
                | (bytes[offset + 3] << 24));
        }

        private static int ReadI32(byte[] bytes, int offset)
        {
            return unchecked((int)ReadU32(bytes, offset));
        }

        private static short ReadI16(byte[] bytes, int offset)
        {
            return unchecked((short)(bytes[offset] | (bytes[offset + 1] << 8)));
        }
    }

    public record MapXbfTlvRecord(int Offset, int TagId, int Length, int PayloadOffset);

    public class MapXbfBuilding
    {
        public string Name { get; set; }
        public short X { get; set; }
        public short Owner { get; set; }
        public short Y { get; set; }
        public short Padding { get; set; }
        public int Reserved { get; set; }
    }
}
