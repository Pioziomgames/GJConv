using GJ.IO;
using static TmxLib.TmxEnums;
using System.Drawing;
using System.Drawing.Imaging;

namespace TmxLib
{
    public class TmxFile : Texture
    {
        public const uint MAGIC = 0x30584D54;
        public short Flag;
        public short UserId;
        public TmxPicture Picture;
        public TmxFile(TmxPicture picture, short userId = 0, short flag = 2)
        {
            Flag = flag;
            UserId = userId;
            Picture = picture;
        }
        public TmxFile(string Path) : base(Path) { }
        public TmxFile(byte[] Data) : base(Data) { }
        public TmxFile(BinaryReader reader) : base(reader) { }
        internal override void Read(BinaryReader reader)
        {
            Flag = reader.ReadInt16();
            UserId = reader.ReadInt16();
            reader.ReadInt32(); //FileSize

            if (reader.ReadInt32() != MAGIC)
                throw new Exception("Not a proper TMX file");

            reader.BaseStream.Position += 4;

            Picture = new(reader);

        }
        internal override void Write(BinaryWriter writer)
        {
            long Start = writer.BaseStream.Position;
            writer.Write(Flag);
            writer.Write(UserId);
            writer.Write(0); //FileSize
            writer.Write(MAGIC);
            writer.Write(0); //reserved

            Picture.Write(writer);
            long End = writer.BaseStream.Position;

            writer.BaseStream.Position = Start + 4;
            writer.Write((int)(End - Start));
        }

        public override int GetWidth()
        {
            return Picture.Header.Width;
        }

        public override int GetHeight()
        {
            return Picture.Header.Height;
        }

        public override int GetMipMapCount()
        {
            return Picture.Header.MipMapCount;
        }

        public override PixelFormat GetPixelFormat()
        {
            switch(Picture.Header.PixelFormat)
            {
                case TmxPixelFormat.PSMT4:
                case TmxPixelFormat.PSMT4HL:
                case TmxPixelFormat.PSMT4HH:
                    return PixelFormat.Format4bppIndexed;
                case TmxPixelFormat.PSMT8:
                case TmxPixelFormat.PSMT8H:
                    return PixelFormat.Format8bppIndexed;
                case TmxPixelFormat.PSMTC16:
                case TmxPixelFormat.PSMTC16S:
                    return PixelFormat.Format16bppArgb1555;
                case TmxPixelFormat.PSMTC24:
                    return PixelFormat.Format24bppRgb;
                default:
                    return PixelFormat.Format32bppArgb;
            }
        }

        public override Color[] GetPalette()
        {
            if (Picture.Palette != null)
                return Picture.Palette.Colors.ToArray();
            else
                return Array.Empty<Color>();
        }

        public override Color[] GetPixelData()
        {
            if (Picture.Image.Pixels != null)
                return Picture.Image.Pixels.ToArray();
            else
                return GetPixelDataFromIndexData();
        }

        public override byte[] GetIndexData()
        {
            if (Picture.Image.PixelIndexes != null)
                return Picture.Image.PixelIndexes.Cast<byte>().ToArray();
            else
                return Array.Empty<byte>();
        }
    }
}