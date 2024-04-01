using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tim2Lib.Tim2Enums;
using GJ.IO;
using System.Drawing.Imaging;
using System.Drawing;

namespace Tim2Lib
{
    public class Tim2File : Texture
    {
        public const uint MAGIC = 0x324D4954;
        public byte Version;
        public Tim2Alignment Alignment;
        public ushort PictureCount { get => (ushort)Pictures.Count; }
        public List<Tim2Picture> Pictures;
        public Tim2File(byte version, Tim2Alignment alignment, List<Tim2Picture> pictures)
        {
            Version = version;
            Alignment = alignment;
            Pictures = pictures;
        }
        public Tim2File(string Path) : base(Path) { }
        public Tim2File(byte[] Data) : base(Data) { }
        public Tim2File(BinaryReader reader) : base(reader) { }
        internal override void Read(BinaryReader reader)
        {
            if (reader.ReadUInt32() != MAGIC)
                throw new Exception("Not a proper TIM2 file");
            Version = reader.ReadByte();
            Alignment = (Tim2Alignment)reader.ReadByte();
            ushort count = reader.ReadUInt16();

            if (Alignment == Tim2Alignment.Align128)
                reader.BaseStream.Position += 120;
            else
                reader.BaseStream.Position += 8;

            if (Alignment > 0)
                reader.BaseStream.Position += 112;

            Pictures = new();
            for (int i = 0; i < count; i++)
                Pictures.Add(new Tim2Picture(reader, Alignment));
        }
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(MAGIC);
            writer.Write(Version);
            writer.Write((byte)Alignment);
            writer.Write((ushort)Pictures.Count);
            if(Alignment == Tim2Alignment.Align128)
                writer.BaseStream.Position += 120;
            else
                writer.BaseStream.Position += 8;

            if (Alignment > 0)
                writer.BaseStream.Position += 112;

            for (int i = 0; i < PictureCount; i++)
                Pictures[i].Write(writer, Alignment);
        }

        public override int GetWidth()
        {
            return Pictures[0].Header.Width;
        }

        public override int GetHeight()
        {
            return Pictures[0].Header.Height;
        }

        public override int GetMipMapCount()
        {
            return Pictures[0].Header.MipMapCount;
        }

        public override PixelFormat GetPixelFormat()
        {
            switch (Pictures[0].Header.PixelFormat)
            {
                case Tim2BPP.INDEX4:
                    return PixelFormat.Format4bppIndexed;
                case Tim2BPP.INDEX8:
                    return PixelFormat.Format8bppIndexed;
                case Tim2BPP.RGBA5551:
                    return PixelFormat.Format16bppArgb1555;
                case Tim2BPP.RGBA8880:
                    return PixelFormat.Format24bppRgb;
                default:
                    return PixelFormat.Format32bppArgb;
            }
        }

        public override Color[] GetPalette()
        {
            if (Pictures[0].Palette != null)
                return Pictures[0].Palette.Colors.ToArray();
            else
                return Array.Empty<Color>();
        }

        public override Color[] GetPixelData()
        {
            if (Pictures[0].Image.Pixels != null)
                return Pictures[0].Image.Pixels.ToArray();
            else
                return GetPixelDataFromIndexData();
        }

        public override byte[] GetIndexData()
        {
            if (Pictures[0].Image.PixelIndexes != null)
                return Pictures[0].Image.PixelIndexes.ToArray();
            else
                return Array.Empty<byte>();
        }
    }
}
