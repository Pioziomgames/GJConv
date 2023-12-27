using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDSLib;
using static DDSLib.DDSEnums;
using static DDSLib.DXT;
using GJ.IO;

namespace DDSLib
{
    public class DDSFile : Texture
    {
        public DDSHeader DDSHeader;
        public DXT10Header? DXT10Header;
        public Color[] Pixels;
        public DDSFile(string Path) : base(Path) { }
        public DDSFile(byte[] Data) : base(Data) { }
        public DDSFile(BinaryReader reader) : base(reader) { }
        public DDSFile(DDSHeader header, Color[] pixels)
        {
            DDSHeader = header;
            Pixels = pixels;
        }
        public DDSFile(DDSHeader header, Color[] pixels, DXT10Header header10)
        {
            DDSHeader = header;
            Pixels = pixels;
            DXT10Header = header10;
        }
        internal int CountBits(uint value)
        {
            int count = 0;
            while ((value & 1) == 0)
            {
                count++;
                value >>= 1;
            }
            return count;
        }
        public void ReadPixelData(BinaryReader reader)
        {
            Pixels = new Color[DDSHeader.Width * DDSHeader.Height];
            switch (DDSHeader.PixelFormat.FourCC)
            {
                case 0:
                {
                    uint RBitMask = DDSHeader.PixelFormat.RBitMask == 0 ? 0xff : DDSHeader.PixelFormat.RBitMask;
                    uint GBitMask = DDSHeader.PixelFormat.GBitMask == 0 ? 0xff00 : DDSHeader.PixelFormat.GBitMask;
                    uint BBitMask = DDSHeader.PixelFormat.BBitMask == 0 ? 0xff0000 : DDSHeader.PixelFormat.BBitMask;
                    uint ABitMask = DDSHeader.PixelFormat.ABitMask == 0 ? 0xff000000 : DDSHeader.PixelFormat.ABitMask;
                    for (int i = 0; i < DDSHeader.Width * DDSHeader.Height; i++)
                    {
                        uint Pixel = reader.ReadUInt32();
                        byte R = (byte)((Pixel & RBitMask) >> CountBits(RBitMask));
                        byte G = (byte)((Pixel & GBitMask) >> CountBits(GBitMask));
                        byte B = (byte)((Pixel & BBitMask) >> CountBits(BBitMask));
                        byte A = (byte)((Pixel & ABitMask) >> CountBits(ABitMask));
                        Pixels[i] = Color.FromArgb(A, R, G, B);
                    }
                    break;
                }
                case DDSFourCC.DXT1:
                {
                    Pixels = ReadDXT1Data(reader, DDSHeader.Width, DDSHeader.Height);
                    break;
                }
                case DDSFourCC.DXT3:
                {
                    Pixels = ReadDXT3Data(reader, DDSHeader.Width, DDSHeader.Height);
                    break;
                }
                case DDSFourCC.DXT5:
                {
                    Pixels = ReadDXT5Data(reader, DDSHeader.Width, DDSHeader.Height);
                    break;
                }
                /*case DDSFourCC.DX10:
                {
                    Pixels = ReadBC7Data(reader, DDSHeader.Width, DDSHeader.Height);
                    break;
                }*/
                default:
                    throw new Exception($"Unimplemented FourCC: {DDSHeader.PixelFormat.FourCC}");
            }
        }
        public void WritePixelData(BinaryWriter writer)
        {
            switch (DDSHeader.PixelFormat.FourCC)
            {
                case 0:
                {
                    uint RBitMask = DDSHeader.PixelFormat.RBitMask == 0 ? 0xff : DDSHeader.PixelFormat.RBitMask;
                    uint GBitMask = DDSHeader.PixelFormat.GBitMask == 0 ? 0xff00 : DDSHeader.PixelFormat.GBitMask;
                    uint BBitMask = DDSHeader.PixelFormat.BBitMask == 0 ? 0xff0000 : DDSHeader.PixelFormat.BBitMask;
                    uint ABitMask = DDSHeader.PixelFormat.ABitMask == 0 ? 0xff000000 : DDSHeader.PixelFormat.ABitMask;
                    for (int i = 0; i < DDSHeader.Width * DDSHeader.Height; i++)
                    {
                        uint Pixel = (uint)(((Pixels[i].R << CountBits(RBitMask)) & RBitMask) |
                            ((Pixels[i].G << CountBits(GBitMask)) & GBitMask) |
                            ((Pixels[i].B << CountBits(BBitMask)) & BBitMask) |
                            ((Pixels[i].A << CountBits(ABitMask)) & ABitMask));
                        writer.Write(Pixel);
                    }
                    break;
                }
                case DDSFourCC.DXT1:
                {
                    WriteDXT1Data(writer, Pixels, (int)DDSHeader.Width, (int)DDSHeader.Height);
                    break;
                }
                default:
                    throw new Exception($"Unimplemented FourCC: {DDSHeader.PixelFormat.FourCC}");
            }
        }
        internal override void Read(BinaryReader reader)
        {
            uint MAGIC = reader.ReadUInt32();
            if (MAGIC != 0x20534444)
                throw new Exception("Not a proper DDS File");
            DDSHeader = new DDSHeader(reader);
            if (DDSHeader.PixelFormat.FourCC == DDSFourCC.DX10)
                DXT10Header = new DXT10Header(reader);
            ReadPixelData(reader);
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.Write(0x20534444);
            DDSHeader.Write(writer);
            DXT10Header?.Write(writer);
            WritePixelData(writer);
        }
    }
    public class DDSHeader
    {
        public DDSFlags Flags;
        public uint Height;
        public uint Width;
        public uint PitchOrLinearSize;
        public uint Depth;
        public uint MipMapCount;
        public byte[] ReservedData;
        public DDSPixelFormat PixelFormat;
        public DDSCaps Caps1;
        public DDSCaps2 Caps2;
        public uint Caps3;
        public uint Caps4;
        public uint Reserved;
        public DDSHeader(uint height, uint width, DDSFlags flags, DDSPixelFormat pixelFormat)
        {
            Height = height;
            Width = width;
            Flags = flags;
            PixelFormat = pixelFormat;
            ReservedData = new byte[44];
        }
        public DDSHeader(BinaryReader reader)
        {
            uint size = reader.ReadUInt32();
            long end = reader.BaseStream.Position + size - 4;
            Flags = (DDSFlags)reader.ReadUInt32();
            Height = reader.ReadUInt32();
            Width = reader.ReadUInt32();
            PitchOrLinearSize = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            MipMapCount = reader.ReadUInt32();
            ReservedData = reader.ReadBytes(44);
            PixelFormat = new DDSPixelFormat(reader);
            Caps1 = (DDSCaps)reader.ReadUInt32();
            Caps2 = (DDSCaps2)reader.ReadUInt32();
            Caps3 = reader.ReadUInt32();
            Caps4 = reader.ReadUInt32();
            Reserved = reader.ReadUInt32();
            reader.BaseStream.Position = end;
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(124);
            writer.Write((uint)Flags);
            writer.Write(Height);
            writer.Write(Width);
            writer.Write(PitchOrLinearSize);
            writer.Write(Depth);
            writer.Write(MipMapCount);
            long reserveEnd = writer.BaseStream.Position + 44;
            writer.Write(ReservedData);
            writer.BaseStream.Position = reserveEnd;
            PixelFormat.Write(writer);
            writer.Write((uint)Caps1);
            writer.Write((uint)Caps2);
            writer.Write(Caps3);
            writer.Write(Caps4);
            writer.Write(Reserved);
        }
    }
    public class DDSPixelFormat
    {
        public DDSPixelFormatFlags Flags;
        public DDSFourCC FourCC;
        public uint RGBBitCount;
        public uint RBitMask;
        public uint GBitMask;
        public uint BBitMask;
        public uint ABitMask;

        public DDSPixelFormat(DDSPixelFormatFlags flags, DDSFourCC fourCC)
        {
            Flags = flags;
            FourCC = fourCC;
        }
        public DDSPixelFormat(DDSPixelFormatFlags flags)
        {
            Flags = flags;
            RGBBitCount = 32;
            RBitMask = 0xff;
            GBitMask = 0xff00;
            BBitMask = 0xff0000;
            ABitMask = 0xff000000;
        }
        public DDSPixelFormat(BinaryReader reader)
        {
            uint size = reader.ReadUInt32();
            long end = reader.BaseStream.Position + size-4;

            Flags = (DDSPixelFormatFlags)reader.ReadUInt32();
            FourCC = (DDSFourCC)reader.ReadUInt32();
            RGBBitCount = reader.ReadUInt32();
            RBitMask = reader.ReadUInt32();
            GBitMask = reader.ReadUInt32();
            BBitMask = reader.ReadUInt32();
            ABitMask = reader.ReadUInt32();

            reader.BaseStream.Position = end;
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(32);
            writer.Write((uint)Flags);
            writer.Write((uint)FourCC);
            writer.Write(RGBBitCount);
            writer.Write(RBitMask);
            writer.Write(GBitMask);
            writer.Write(BBitMask);
            writer.Write(ABitMask);
        }
    }
    public class DXT10Header
    {
        public DXGIFormat DxgiFormat;
        public D3D10ResourceDimention ResourceDimention;
        public uint MiscFlags;
        public uint ArraySize;
        public uint MiscFlags2;
        public DXT10Header(BinaryReader reader)
        {
            DxgiFormat = (DXGIFormat)reader.ReadUInt32();
            ResourceDimention = (D3D10ResourceDimention)reader.ReadUInt32();
            MiscFlags = reader.ReadUInt32();
            ArraySize = reader.ReadUInt32();
            MiscFlags2 = reader.ReadUInt32();
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write((uint)DxgiFormat);
            writer.Write((uint)ResourceDimention);
            writer.Write(MiscFlags);
            writer.Write(ArraySize);
            writer.Write(MiscFlags2);
        }
    }
}
