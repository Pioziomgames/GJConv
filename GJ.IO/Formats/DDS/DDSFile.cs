using System.Drawing;
using System.Drawing.Imaging;
using GJ.IO;
using static DDSLib.DDSEnums;
using static DDSLib.DXT;

namespace DDSLib
{
    public class DDSFile : Texture
    {
        public const uint MAGIC = 0x20534444;
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

                    int RShift = CountBits(RBitMask);
                    int GShift = CountBits(GBitMask);
                    int BShift = CountBits(BBitMask);
                    int AShift = CountBits(ABitMask);
                    for (int i = 0; i < DDSHeader.Width * DDSHeader.Height; i++)
                    {
                        uint Pixel = reader.ReadUInt32();
                        byte R = (byte)((Pixel & RBitMask) >> RShift);
                        byte G = (byte)((Pixel & GBitMask) >> GShift);
                        byte B = (byte)((Pixel & BBitMask) >> BShift);
                        byte A = (byte)((Pixel & ABitMask) >> AShift);
                        Pixels[i] = Color.FromArgb(A, R, G, B);
                    }
                    break;
                }
                case DDSFourCC.DXT1:
                case DDSFourCC.BC1:
                case DDSFourCC.BC1U:
                    Pixels = ReadDXT1Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                case DDSFourCC.DXT3:
                case DDSFourCC.BC2:
                case DDSFourCC.BC2U:
                    Pixels = ReadDXT3Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                case DDSFourCC.DXT5:
                case DDSFourCC.BC3:
                case DDSFourCC.BC3U:
                    Pixels = ReadDXT5Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                case DDSFourCC.ATI1:
                case DDSFourCC.BC4:
                case DDSFourCC.BC4U:
                    Pixels = ReadBC4Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                case DDSFourCC.ATI2:
                case DDSFourCC.BC5:
                case DDSFourCC.BC5U:
                    Pixels = ReadBC5Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                case DDSFourCC.BC7:
                case DDSFourCC.BC7U:
                    ReadBC7Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                case DDSFourCC.DX10:
                {
                    switch (DXT10Header?.DxgiFormat)
                    {
                        case DXGIFormat.BC1_TYPELESS:
                        case DXGIFormat.BC1_UNORM:
                        case DXGIFormat.BC1_UNORM_SRGB:
                            Pixels = ReadDXT1Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                        case DXGIFormat.BC2_TYPELESS:
                        case DXGIFormat.BC2_UNORM:
                        case DXGIFormat.BC2_UNORM_SRGB:
                            Pixels = ReadDXT3Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                        case DXGIFormat.BC3_TYPELESS:
                        case DXGIFormat.BC3_UNORM:
                        case DXGIFormat.BC3_UNORM_SRGB:
                            Pixels = ReadDXT5Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                        case DXGIFormat.BC4_TYPELESS:
                        case DXGIFormat.BC4_UNORM:
                        case DXGIFormat.BC4_SNORM:
                            Pixels = ReadBC4Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                        case DXGIFormat.BC5_TYPELESS:
                        case DXGIFormat.BC5_UNORM:
                        case DXGIFormat.BC5_SNORM:
                            Pixels = ReadBC5Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                        /*case DXGIFormat.BC6H_SF16:
                            Pixels = ReadBC6Data(reader, DDSHeader.Width, DDSHeader.Height, true); break;
                        case DXGIFormat.BC6H_UF16:
                        case DXGIFormat.BC6H_TYPELESS:
                            Pixels = ReadBC6Data(reader, DDSHeader.Width, DDSHeader.Height, false); break;*/
                        case DXGIFormat.BC7_TYPELESS:
                        case DXGIFormat.BC7_UNORM:
                        case DXGIFormat.BC7_UNORM_SRGB:
                            Pixels = ReadBC7Data(reader, DDSHeader.Width, DDSHeader.Height); break;
                        default:
                            throw new Exception($"Unimplemented Dxgi format: {DXT10Header?.DxgiFormat}");
                    }
                    break;
                }
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
            if (reader.ReadUInt32() != MAGIC)
                throw new Exception("Not a proper DDS file");
            DDSHeader = new DDSHeader(reader);
            if (DDSHeader.PixelFormat.FourCC == DDSFourCC.DX10)
                DXT10Header = new DXT10Header(reader);
            ReadPixelData(reader);
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.Write(MAGIC);
            DDSHeader.Write(writer);
            DXT10Header?.Write(writer);
            WritePixelData(writer);
        }

        public override int GetWidth()
        {
            return (int)DDSHeader.Width;
        }

        public override int GetHeight()
        {
            return (int)DDSHeader.Height;
        }

        public override int GetMipMapCount()
        {
            return (int)DDSHeader.MipMapCount;
        }

        public override PixelFormat GetPixelFormat()
        {
            /*if (DXT10Header != null)
            {
                switch(DXT10Header.DxgiFormat)
                {
                    default:
                        return PixelFormat.Format32bppArgb;
                }
            }
            else
            {*/
                return PixelFormat.Format32bppArgb;
            //}
        }

        public override Color[] GetPalette()
        {
            return Array.Empty<Color>();
        }

        public override Color[] GetPixelData()
        {
            return Pixels;
        }

        public override byte[] GetIndexData()
        {
            return Array.Empty<byte>();
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
