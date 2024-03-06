using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tim2Lib.Tim2Enums;
using static GJ.IO.IOFunctions;

namespace Tim2Lib
{
    public struct PictureHeader
    {
        public uint TotalSize;
        public uint PaletteSize;
        public uint ImageSize;
        public ushort HeaderSize;
        public ushort PaletteColorsCount;
        public byte PictureFormat;
        public byte MipMapCount;
        public byte PaletteType;
        public Tim2BPP PixelFormat;
        public ushort Width;
        public ushort Height;
        public byte[] GsTex0;
        public byte[] GsTex1;
        public int GsRegs;
        public int GsTexClut;
        public byte[] UserData;
        public PictureHeader()
        {
            TotalSize = 0;
            PaletteSize = 0;
            ImageSize = 0;
            HeaderSize = 0;
            PaletteColorsCount = 0;
            PictureFormat = 0;
            MipMapCount = 1;
            PaletteType = 0;
            PixelFormat = Tim2BPP.RGBA8888;
            Width = 0;
            Height = 0;
            GsTex0 = new byte[8] {0,0,0,0,0,0,0,0};
            GsTex1 = GsTex0;
            GsRegs = 0;
            GsTexClut = 0;
            UserData = Array.Empty<byte>();
        }
        public PictureHeader(BinaryReader reader)
        {
            long Start = reader.BaseStream.Position;
            TotalSize = reader.ReadUInt32();
            PaletteSize = reader.ReadUInt32();
            ImageSize = reader.ReadUInt32();
            HeaderSize = reader.ReadUInt16();
            PaletteColorsCount = reader.ReadUInt16();
            PictureFormat = reader.ReadByte();
            MipMapCount = reader.ReadByte();
            PaletteType = reader.ReadByte();
            PixelFormat = (Tim2BPP)reader.ReadByte();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            GsTex0 = reader.ReadBytes(8);
            GsTex1 = reader.ReadBytes(8);
            GsRegs = reader.ReadInt32();
            GsTexClut = reader.ReadInt32();
            if (Start + HeaderSize > reader.BaseStream.Position)
                UserData = reader.ReadBytes((int)(Start + HeaderSize - reader.BaseStream.Position));
            else
                UserData = Array.Empty<byte>();
        }
    }

    public class Tim2Picture
    {
        public PictureHeader Header;
        public Tim2ImageData Image;
        public Tim2Palette? Palette;
        public bool UsesIndexes { get; private set; }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public void IndexedToFullColor()
        {
            if (UsesIndexes)
            {
                Image.IndexedToFullColor(Palette.Colors);
                Palette = null;
                UsesIndexes = false;
            }
        }
        public void FullColorToIndexed()
        {
            if (!UsesIndexes)
            {
                Palette = new(Image.FullColorToIndexed());
                UsesIndexes = true;
            }
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        public Tim2Picture(PictureHeader header, Tim2ImageData image, Tim2Palette palette)
        {
            UsesIndexes = true;
            Header = header;
            Image = image;
            Palette = palette;
        }
        public Tim2Picture(PictureHeader header, Tim2ImageData image)
        {
            UsesIndexes = false;
            Header = header;
            Image = image;
        }
        public Tim2Picture(BinaryReader reader, Tim2Alignment Alignment = Tim2Alignment.Align128)
        {
            Read(reader, Alignment);
        }
        public void Read(BinaryReader reader, Tim2Alignment Alignment = Tim2Alignment.Align128)
        {
            Header = new PictureHeader(reader);

            if (Alignment == Tim2Alignment.Align128)
                Align(reader, 128);

            Image = new Tim2ImageData(reader, Header.Height, Header.Width, Header.PixelFormat);

            if (Header.MipMapCount > 1) //Skip mip map data
            {
                int MipMapSize = 0;
                //Skip the first mip map since we already read that
                for (int i = 1; i < Header.MipMapCount; i++)
                {
                    int mipWidth = Math.Max(1, Header.Width >> i);
                    int mipHeight = Math.Max(1, Header.Height >> i);
                    MipMapSize += mipWidth * mipHeight;
                }
                switch (Header.PixelFormat)
                {
                    case Tim2BPP.RGBA8880:
                        MipMapSize *= 3;
                        break;
                    case Tim2BPP.RGBA5551:
                        MipMapSize *= 2;
                        break;
                    case Tim2BPP.INDEX4:
                        MipMapSize /= 2;
                        break;
                    case Tim2BPP.RGBA8888:
                        MipMapSize *= 4;
                        break;
                    //Tim2BPP.INDEX8 -- stays the same
                }
                reader.BaseStream.Position += MipMapSize;
            }

            if (Alignment == Tim2Alignment.Align128)
                Align(reader, 128);

            if (Header.PaletteSize > 0)
            {
                UsesIndexes = true;
                Palette = new Tim2Palette(reader, Header.PaletteColorsCount, Header.PaletteType);
            }
            else
                UsesIndexes = false;
        }
        public void Write(BinaryWriter writer, Tim2Alignment Alignment = Tim2Alignment.Align128)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            long Start = writer.BaseStream.Position;

            writer.Write(0); //TotalSize
            writer.Write(0); //PaletteSize
            writer.Write(0); //ImageSize
            writer.Write((ushort)0); //HeaderSize

            if (Palette == null) //Pallete Colors Count
                writer.Write((ushort)0);
            else if (Palette.Colors.Count <= 16)
                writer.Write((ushort)16);
            else if (Palette.Colors.Count <= 256)
                writer.Write((ushort)256);
            else
                writer.Write((ushort)Palette.Colors.Count);

            writer.Write(Header.PictureFormat);
            writer.Write(Header.MipMapCount);
            writer.Write(Header.PaletteType);
            writer.Write((byte)Header.PixelFormat);

            writer.Write(Header.Width);
            writer.Write(Header.Height);
            writer.Write(Header.GsTex0);
            writer.Write(Header.GsTex1);
            writer.Write(Header.GsRegs);
            writer.Write(Header.GsTexClut);
            if (Header.UserData.Length > 0)
                writer.Write(Header.UserData);
            if (Alignment == Tim2Alignment.Align128)
                Align(writer, 128);
            long ImageStart = writer.BaseStream.Position;
            Image.Write(writer, Header.PixelFormat);
            long ImageEnd = writer.BaseStream.Position;


            if (Alignment == Tim2Alignment.Align128)
                Align(writer, 128);
            //writer.BaseStream.Position += 76; // padding
            //writer.Write(0); //write - to ensure that the padding data gets written to the file

            long PaletteStart = writer.BaseStream.Position;
            if ((byte)Header.PixelFormat > 3)
                Palette.Write(writer, Header.PaletteType);
            long End = writer.BaseStream.Position;

            writer.BaseStream.Position = Start;
            writer.Write((uint)(End - Start));
            writer.Write((uint)(End - PaletteStart));
            writer.Write((uint)(ImageEnd - ImageStart));
            writer.Write((ushort)(ImageStart - Start));

            writer.BaseStream.Position = End;
            Align(writer, 128);

#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
    }
    
}
