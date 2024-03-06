using GJ.IO;
using System.Drawing;
using static System.Buffers.Binary.BinaryPrimitives;
using static GJ.IO.IOFunctions;

namespace CtxrLib
{
    public class CtxrFile : Texture
    {
        public const uint MAGIC = 0x52545854;
        public CtxrHeader Header;
        public Color[] ImageData;
        public CtxrFile(ushort width, ushort height, Color least, Color most, Color[] Pixels)
        {
            Header = new(width, height);
            ImageData = Pixels;
        }
        public CtxrFile(ushort width, ushort height, Color[] Pixels)
        {
            Header = new(width, height, Pixels);
            ImageData = Pixels;
        }
        public CtxrFile(CtxrHeader header, Color[] Pixels)
        {
            Header = header;
            ImageData = Pixels;
        }
        public CtxrFile(string Path) : base(Path) { }
        public CtxrFile(byte[] Data) : base(Data) { }
        public CtxrFile(BinaryReader reader) : base(reader) { }
        internal override void Read(BinaryReader reader)
        {
            if (reader.ReadUInt32() != MAGIC)
                throw new Exception("Not a proper CTXR/TXTR File");
            Header = new(reader);

            uint ChunkSize = ReverseEndianness(reader.ReadUInt32());
            if (Header.Width * Header.Height != ChunkSize / 4)
                throw new Exception("Image data size doesn't match the expected size (unimplemented pixel format?)");
            ImageData = new Color[ChunkSize / 4];
            for (int i = 0; i < ImageData.Length; i++)
            {
                ImageData[i] = HalfAlphaToFull(ReadBGRA8888(reader));
            }
            Align(reader, 32);
        }
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(MAGIC);
            Header.Write(writer);
            writer.Write(ReverseEndianness((uint)ImageData.Length * 4));
            for (int i = 0; i < ImageData.Length; i++)
            {
                WriteBGRA8888(writer, FullAlphaToHalf(ImageData[i]));
            }
            Align(writer, 32);
        }
    }
}
