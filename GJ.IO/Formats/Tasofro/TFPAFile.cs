using System.Drawing;
using System.IO.Compression;
using System.Drawing.Imaging;
using GJ.IO;
using static GJ.IO.IOFunctions;

namespace TasofroLib
{
    public class TFPAFile : Texture
    {
        public const uint MAGIC = 0x41504654;
        public byte[] PixelIndexes { get; private set; }
        public Color[] Palette;
        public TFPAFile(string Path)
            : base(Path)
        {

        }
        public TFPAFile(BinaryReader reader)
            : base(reader)
        {

        }
        public TFPAFile(byte[] Data)
            : base(Data)
        {

        }
        internal override void Read(BinaryReader reader)
        {
            uint magic = reader.ReadUInt32();
            if (magic != MAGIC)
                throw new Exception("Not a proper TFPA file");
            byte version = reader.ReadByte();
            int compressedSize = reader.ReadInt32();
            MemoryStream decompressedStream = new MemoryStream();
            using (ZLibStream decompressionStream = new ZLibStream(
                reader.BaseStream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(decompressedStream);
            }
            using (BinaryReader dReader = new BinaryReader(decompressedStream))
            {
                dReader.BaseStream.Position = 512;
                Palette = new Color[256];
                PixelIndexes = new byte[256];
                for (int i = 0; i < 256; i++) //This is just a palette format
                    PixelIndexes[i] = (byte)i;
                for (int i = 0; i < 256; i++)
                    Palette[i] = ReadBGRA8888(dReader);
            }
        }
        internal override void Write(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
        public override int GetWidth()
        {
            return 16;
        }
        public override int GetHeight()
        {
            return 16;
        }

        public override byte[] GetIndexData()
        {
            return PixelIndexes;
        }

        public override int GetMipMapCount()
        {
            return 1;
        }

        public override Color[] GetPalette()
        {
            return Palette;
        }

        public override Color[] GetPixelData()
        {
            return GetPixelDataFromIndexData();
        }

        public override PixelFormat GetPixelFormat()
        {
            return PixelFormat.Format8bppIndexed;
        }


    }
}
