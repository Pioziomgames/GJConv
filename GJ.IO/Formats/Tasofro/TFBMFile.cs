using System.Drawing;
using System.IO.Compression;
using System.Drawing.Imaging;
using GJ.IO;
using static GJ.IO.IOFunctions;

namespace TasofroLib
{
    public class TFBMFile : Texture
    {
        public const uint MAGIC = 0x4D424654;
        public TFBMHeader Header;
        public Color[]? Pixels;
        public byte[]? PixelIndexes;
        public Color[]? Palette;
        public TFBMFile(string Path)
            : base(Path)
        {

        }
        public TFBMFile(BinaryReader reader)
            : base(reader)
        {

        }
        public TFBMFile(byte[] Data)
            : base(Data)
        {

        }
        internal override void Read(BinaryReader reader)
        {
            uint magic = reader.ReadUInt32();
            if (magic != MAGIC)
                throw new Exception("Not a proper TFBM file");
            Header = new TFBMHeader(reader);
            MemoryStream decompressedStream = new MemoryStream();
            using (ZLibStream decompressionStream = new ZLibStream(
                reader.BaseStream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(decompressedStream);
            }
            using (BinaryReader dReader = new BinaryReader(decompressedStream))
            {
                dReader.BaseStream.Position = 0;
                int size = Header.Width * Header.Height;
                if (Header.BPP == 8)
                {
                    Palette = new Color[256];
                    Palette[0] = Color.FromArgb(0);
                    for (int i = 0; i < 255; i++)
                        Palette[i + 1] = Color.FromArgb(i, i, i); //The palette data is stored in a different file
                    PixelIndexes = new byte[size];
                    for (int i = 0; i < size; i++)
                        PixelIndexes[i] = dReader.ReadByte();
                }
                else
                {
                    Pixels = new Color[size];
                    if (Header.BPP == 32)
                    {
                        for (int i = 0; i < size; i++)
                            Pixels[i] = ReadBGRA8888(dReader);
                    }
                    else if (Header.BPP == 24)
                    {
                        for (int i = 0; i < size; i++)
                            Pixels[i] = ReadBGRA8880(dReader);
                    }
                    else
                        throw new Exception("Unimplemented TFBM BPP: " + Header.BPP);
                }
            }
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.Write(MAGIC);
            Header.Write(writer);
            MemoryStream uncompressedStream = new MemoryStream();
            using (BinaryWriter uWriter = new BinaryWriter(uncompressedStream))
            {
                int size = Header.Width * Header.Height;
                if (Header.BPP == 8 && PixelIndexes != null)
                {
                    for (int i = 0; i < size; i++)
                        uWriter.Write(PixelIndexes[i]);
                }
                else
                {
                    if (Header.BPP == 32 && Pixels != null)
                    {
                        for (int i = 0; i < size; i++)
                            WriteBGRA8888(uWriter, Pixels[i]);
                    }
                    else if (Header.BPP == 24 && Pixels != null)
                    {
                        for (int i = 0; i < size; i++)
                            WriteBGRA8880(uWriter, Pixels[i]);
                    }
                    else
                        throw new Exception("Unimplemented TFBM BPP: " + Header.BPP);
                }
            }
            uncompressedStream.Position = 0;
            using (ZLibStream compressionStream = new ZLibStream(
                uncompressedStream, CompressionMode.Compress))
            {
                compressionStream.CopyTo(writer.BaseStream);
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

        public override byte[] GetIndexData()
        {
            if (PixelIndexes != null)
                return PixelIndexes;
            else
                return Array.Empty<byte>();
        }

        public override int GetMipMapCount()
        {
            return 1;
        }

        public override Color[] GetPalette()
        {
            if (Palette != null)
                return Palette;
            else
                return Array.Empty<Color>();
        }

        public override Color[] GetPixelData()
        {
            if (Pixels != null)
                return Pixels;
            else
                return GetPixelDataFromIndexData();
        }

        public override PixelFormat GetPixelFormat()
        {
            switch(Header.BPP)
            {
                case 8:
                    return PixelFormat.Format8bppIndexed;
                case 24:
                    return PixelFormat.Format24bppRgb;
                case 32:
                    return PixelFormat.Format32bppArgb;
                default:
                    return PixelFormat.Format32bppArgb;
            }
        }


    }
    public class TFBMHeader
    {
        public byte Version;
        public byte BPP;
        public int Width;
        public int Height;
        public int PaddingWidth;
        public int CompressedSize;

        public TFBMHeader(BinaryReader reader)
        {
            Read(reader);
        }
        void Read(BinaryReader reader)
        {
            Version = reader.ReadByte();
            BPP = reader.ReadByte();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            PaddingWidth = reader.ReadInt32();
            CompressedSize = reader.ReadInt32();
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(BPP);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(PaddingWidth);
            writer.Write(CompressedSize);

        }
    }
}
