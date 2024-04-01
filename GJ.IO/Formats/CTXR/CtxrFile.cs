using GJ.IO;
using System.Drawing;
using static System.Buffers.Binary.BinaryPrimitives;
using static GJ.IO.IOFunctions;
using System.Drawing.Imaging;

namespace CtxrLib
{
    public class CtxrFile : Texture
    {
        public const uint MAGIC = 0x52545854;
        public CtxrHeader Header;
        public Color[][] ImageData;
        public CtxrFile(ushort width, ushort height, Color colorMin, Color colorMax, Color[] Pixels)
        {
            Header = new(width, height);
            ImageData = new Color[1][] { Pixels };
            Header.ColorMin = colorMin;
            Header.ColorMax = colorMax;
        }
        public CtxrFile(ushort width, ushort height, Color[] Pixels)
        {
            Header = new(width, height, Pixels);
            ImageData = new Color[1][] { Pixels };
        }
        public CtxrFile(ushort width, ushort height, Color[][] Pixels)
        {
            Header = new(width, height, Pixels[0]);
            ImageData = Pixels;
            Header.MipMapCount = (ushort)Pixels.Length;
        }
        public CtxrFile(CtxrHeader header, Color[] Pixels)
        {
            Header = header;
            ImageData = new Color[1][] { Pixels };
        }
        public CtxrFile(string Path) : base(Path) { }
        public CtxrFile(byte[] Data) : base(Data) { }
        public CtxrFile(BinaryReader reader) : base(reader) { }
        internal override void Read(BinaryReader reader)
        {
            if (reader.ReadUInt32() != MAGIC)
                throw new Exception("Not a proper CTXR/TXTR File");
            Header = new(reader);

            ImageData = new Color[Header.MipMapCount][];
            for (int j = 0; j < Header.MipMapCount; j++)
            {
                uint ChunkSize = ReverseEndianness(reader.ReadUInt32());
                ImageData[j] = new Color[ChunkSize / 4];
                for (int i = 0; i < ImageData[j].Length; i++)
                {
                    ImageData[j][i] = HalfAlphaToFull(ReadBGRA8888(reader));
                }
                Align(reader, 32);
            }
        }
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(MAGIC);
            Header.Write(writer);
            for (int j = 0; j < Header.MipMapCount; j++)
            {
                writer.Write(ReverseEndianness((uint)ImageData[j].Length * 4));
                for (int i = 0; i < ImageData[j].Length; i++)
                {
                    WriteBGRA8888(writer, FullAlphaToHalf(ImageData[j][i]));
                }
                Align(writer, 32);
            }
        }

        public override int GetWidth()
        {
            return Header.Width;
        }

        public override int GetHeight()
        {
            return Header.Height;
        }

        public override int GetMipMapCount()
        {
            return Header.MipMapCount;
        }

        public override PixelFormat GetPixelFormat()
        {
            return PixelFormat.Format32bppArgb;
        }

        public override Color[] GetPalette()
        {
            return Array.Empty<Color>();
        }

        public override Color[] GetPixelData()
        {
            return ImageData[0];
        }

        public override byte[] GetIndexData()
        {
            return Array.Empty<byte>();
        }
    }
}
