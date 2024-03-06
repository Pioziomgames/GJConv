using static TxnLib.RmdEnums;
using static TxnLib.RmdFunctions;
using static GJ.IO.IOFunctions;
using System.Drawing;

namespace TxnLib
{
    public class RwTextureNative : RmdChunk
    {
        public string TextureName;
        public string MaskName;
        public RwPlatformId Platform;
        public uint Flags;
        public RasterInfoStruct RasterInfo;
        public RasterDataStruct RasterData;
        public RmdChunk Extension;
        public RwTextureNative(string textureName, RwPlatformId platform, uint flags, RasterInfoStruct rasterInfo, RasterDataStruct rasterData, string maskName = "", RmdChunk? extension = null, int version = 469893175)
            : base(RmdChunkType.TextureNative, version)
        {
            TextureName = textureName;
            MaskName = maskName;
            Platform = platform;
            Flags = flags;
            RasterInfo = rasterInfo;
            RasterData = rasterData;
            if (extension != null)
                Extension = extension;
            else
                Extension = new RmdBinaryChunk(RmdChunkType.Extension, version);
        }
        public RwTextureNative(RmdChunkType type, uint size, int version, BinaryReader reader)
           : base(type, size, version, reader)
        {
        }
        protected override void ReadChunkData(BinaryReader reader, uint Size)
        {
            reader.BaseStream.Position += 12;

            Platform = (RwPlatformId)reader.ReadUInt32();
            Flags = reader.ReadUInt32();
            TextureName = reader.ReadRwString();
            MaskName = reader.ReadRwString();

            reader.BaseStream.Position += 12;

            RasterInfo = (RasterInfoStruct)RmdChunk.ReadStruct(reader, Type);
            RasterData = (RasterDataStruct)RmdChunk.ReadStruct(reader, Type, RasterInfo);

            Extension = RmdChunk.Read(reader);
        }
        protected override void WriteChunkData(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(8);
            writer.Write(469893175);

            writer.Write((uint)Platform);
            writer.Write(Flags);
            writer.WriteRwString(TextureName);
            writer.WriteRwString(MaskName);

            writer.Write(1);
            writer.Write(0); //DummySize for now
            writer.Write(469893175);
            long RasterStart = writer.BaseStream.Position;

            RmdChunk.Write(RasterInfo, writer);
            RmdChunk.Write(RasterData, writer, RasterInfo);

            long ExtensionStart = writer.BaseStream.Position;

            writer.BaseStream.Position = RasterStart - 8;
            writer.Write((uint)(ExtensionStart - RasterStart)); //Go back and fix the size
            writer.BaseStream.Position = ExtensionStart;

            RmdChunk.Write(Extension, writer);
        }
    }
    public class RasterDataStruct : RmdChunk
    {
        public PS2ImageHeader? ImageHeader;
        public Color[] ImageData;
        public byte[] ImageIndexData;
        public PS2ImageHeader? PaletteHeader;
        public Color[] PaletteData;
        public byte[] MipMapData;
        private RasterDataStruct(RasterInfoStruct Info, Color[]? imageData = null, byte[]? imageIndexData = null, Color[]? paletteData = null)
        : base(RmdChunkType.Struct, 469893175)
        {
            bool HasHeaders = Info.Format.HasFlag(RwRasterFormat.HasHeaders);
            
            if (imageData != null)
                ImageData = imageData;
            else if (imageIndexData != null && paletteData != null)
            {
                ImageIndexData = imageIndexData;
                PaletteData = paletteData;
            }
            else
                throw new Exception("No Image Data was provided");

            if (HasHeaders)
                GenerateHeaders(Info);
        }
        public RasterDataStruct(RasterInfoStruct Info, Color[] imageData)
           : this(Info, imageData, null, null)
        {
        }
        public RasterDataStruct(RasterInfoStruct Info, byte[] imageIndexData, Color[] paletteData)
            : this(Info, null, imageIndexData, paletteData)
        {
        }
        public RasterDataStruct(RmdChunkType type, uint size, int version, BinaryReader reader, object? data)
           : base(type, size, version, reader, data)
        {
        }
        public void GenerateHeaders(RasterInfoStruct Info)
        {
            int TotalSize = Info.Width * Info.Height;
            ImageHeader = new();
            ImageHeader.TrxRegReg.TransmissionWidth = (ulong)Info.Width;
            ImageHeader.TrxRegReg.TransmissionHeight = (ulong)Info.Height;
            if (Info.Depth <= 8)
            {
                ImageHeader.TrxRegReg.TransmissionWidth /= 2;
                ImageHeader.TrxRegReg.TransmissionHeight /= 2;
            }
            ImageHeader.GifTag2.RepeatCount = (ImageHeader.TrxRegReg.TransmissionWidth * ImageHeader.TrxRegReg.TransmissionHeight) / 4;

            if (Info.Depth <= 8)
            {
                PaletteHeader = new();
                PaletteHeader.GifTag2.RepeatCount = Info.Depth == 4 ? (ulong)6 : (ulong)64;
                PaletteHeader.TrxRegReg.TransmissionWidth = Info.Depth == 4 ? (ulong)8 : (ulong)16;
                PaletteHeader.TrxRegReg.TransmissionHeight = Info.Depth == 4 ? (ulong)3 : (ulong)16;
                if (Info.Depth == 8)
                {
                    //None of this makes any sense
                    if (TotalSize < 8192)
                    {
                        PaletteHeader.TrxPosReg.DestinationRectangleX = 48;
                        PaletteHeader.TrxPosReg.DestinationRectangleY = 16;
                    }
                    else if (TotalSize == 8192 && Info.Width != 64)
                    {
                        PaletteHeader.TrxPosReg.DestinationRectangleX = 0;
                        PaletteHeader.TrxPosReg.DestinationRectangleY = 32;
                    }
                    else if (TotalSize < 16384)
                    {
                        PaletteHeader.TrxPosReg.DestinationRectangleX = 48;
                        PaletteHeader.TrxPosReg.DestinationRectangleY = 48;
                    }
                    else if (TotalSize == 32768)
                    {
                        if (Info.Width > Info.Height)
                        {
                            PaletteHeader.TrxPosReg.DestinationRectangleX = 0;
                            PaletteHeader.TrxPosReg.DestinationRectangleY = 0;
                        }
                        else
                        {
                            PaletteHeader.TrxPosReg.DestinationRectangleX = 0;
                            PaletteHeader.TrxPosReg.DestinationRectangleY = 128;
                        }
                    }
                    else if (TotalSize < 65536)
                    {
                        PaletteHeader.TrxPosReg.DestinationRectangleX = 0;
                        PaletteHeader.TrxPosReg.DestinationRectangleY = 64;
                    }
                    else
                    {
                        PaletteHeader.TrxPosReg.DestinationRectangleX = 0;
                        PaletteHeader.TrxPosReg.DestinationRectangleY = 0;
                    }

                }
            }
        }
        protected override void ReadChunkData(BinaryReader reader, uint Size, object? data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            RasterInfoStruct Info = (RasterInfoStruct)data;

            bool HasHeaders = Info.Format.HasFlag(RwRasterFormat.HasHeaders);
            long ImageStart = reader.BaseStream.Position;
            if (HasHeaders)
                ImageHeader = new(reader);

            if (Info.Depth == 8)
            {
                ImageIndexData = reader.ReadBytes(Info.Width * Info.Height);
            }
            else if (Info.Depth == 4)
            {
                ImageIndexData = new byte[Info.Width * Info.Height];
                int index = 0;
                int RowByteCount = (int)Math.Ceiling((double)(Info.Width / 2.0));
                for (int row = 0; row < Info.Height; row++)
                {
                    byte[] rowData = reader.ReadBytes(RowByteCount);
                    for (int col = 0; col < Info.Width; col += 2)
                    {
                        byte indexByte = rowData[col / 2];
                        byte index1 = (byte)(indexByte & 0x0F);
                        byte index2 = (byte)((indexByte >> 4) & 0x0F);
                        ImageIndexData[index] = index1;
                        ImageIndexData[index + 1] = index2;
                        index += 2;
                    }
                }
            }
            else
            {
                ImageData = new Color[Info.Width * Info.Height];
                for (int i = 0; i < Info.Width * Info.Height; i++)
                {
                    if (Info.Depth == 8)
                        ImageIndexData[i] = reader.ReadByte();
                    else if (Info.Depth == 32)
                        ImageData[i] = ReadRGBA8888(reader);
                    else if (Info.Depth == 16)
                        ImageData[i] = ReadRGBA5551(reader);
                }
            }

            int MipMapDataSize = (int)(Info.TexelDataLength + ImageStart - reader.BaseStream.Position);

            if (MipMapDataSize > 0)
                MipMapData = reader.ReadBytes(MipMapDataSize);
            else
                MipMapData = Array.Empty<byte>();

            reader.BaseStream.Position = Info.TexelDataLength + ImageStart;
            if (Info.Depth <= 8)
            {
                ImageIndexData = Unswizzle(ImageIndexData, Info.Width, Info.Height);
                long PaletteStart = reader.BaseStream.Position;
                if (HasHeaders)
                    PaletteHeader = new(reader);

                int ColorCount = Info.Depth == 4 ? 16 : 256;
                PaletteData = new Color[ColorCount];
                for (int i = 0; i < ColorCount; i++)
                {
                    if (GetDepth(Info.Tex0Reg.PalettePixelFormat) == 32)
                        PaletteData[i] = ReadPSMCT32(reader);
                    else
                        PaletteData[i] = ReadPSMCT16(reader);
                }

                if (Info.Depth == 8)
                {
                    if (!Info.Tex0Reg.DisablePaletteSwizling)
                        PaletteData = TilePalette(PaletteData);
                }

                //Skipping needed here due to padding
                reader.BaseStream.Position = Info.PaletteDataLength + PaletteStart;
            }
        }
        protected override void WriteChunkData(BinaryWriter writer, object? data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            RasterInfoStruct Info = (RasterInfoStruct)data;

            bool HasHeaders = Info.Format.HasFlag(RwRasterFormat.HasHeaders);
            if (HasHeaders && ImageHeader == null || HasHeaders && Info.Depth <= 8 && PaletteHeader == null)
                throw new Exception("Cannot write PS2Image Headers because they are null");

            long ImageStart = writer.BaseStream.Position;
            if (HasHeaders)
            {
                if (ImageHeader == null)
                    throw new Exception("Attempted to write a non existing ImageHeader");
                ImageHeader.Write(writer);
            }

            if (Info.Depth <= 8)
            {
                byte[] swizzled = Swizzle(ImageIndexData, Info.Width, Info.Height);
                if (Info.Depth == 8)
                    writer.Write(swizzled);
                else
                {
                    for (int l = 0; l < swizzled.Length; l += 2)
                    {
                        byte indexByte = (byte)((swizzled[l+1] << 4) | swizzled[l]);
                        writer.Write(indexByte);
                    }
                }

                if (MipMapData != null && MipMapData.Length > 0)
                {
                    writer.Write(MipMapData);
                }

                writer.BaseStream.Position = Info.TexelDataLength + ImageStart;

                long PaletteStart = writer.BaseStream.Position;

                if (HasHeaders)
                {
                    if (PaletteHeader == null)
                        throw new Exception("Attempted to write a non existing PaletteHeader");
                    PaletteHeader.Write(writer);
                }

                Color[] P = PaletteData;
                if (Info.Depth == 8)
                {
                    if (!Info.Tex0Reg.DisablePaletteSwizling)
                        P = TilePalette(PaletteData);
                }

                for (int i = 0; i < P.Length; i++)
                {
                    if (GetDepth(Info.Tex0Reg.PalettePixelFormat) == 32)
                        WritePSMCT32(writer, P[i]);
                    else
                        WritePSMCT16(writer, P[i]);
                }
                writer.BaseStream.Position = Info.PaletteDataLength + PaletteStart;
            }
            else
            {
                for (int i = 0; i < Info.Width * Info.Height; i++)
                {
                    if (Info.Depth == 32)
                        WritePSMCT32(writer, ImageData[i]);
                    else if (Info.Depth == 16)
                        WritePSMCT16(writer, ImageData[i]);
                }
                writer.BaseStream.Position = Info.TexelDataLength + ImageStart;
            }
        }
    }
    public class PS2ImageHeader
    {
        public GifTag GifTag0;
        public GifTag GifTag1;
        public TrxPosRegister TrxPosReg;
        public ulong TrxPosAddress;
        public TrxRegRegister TrxRegReg;
        public ulong TrxRegAddress;
        public TransmissionDirection TrxDirReg;
        public ulong TrxDirAddress;
        public GifTag GifTag2;
        public GifTag GifTag3;

        public PS2ImageHeader()
        {
            GifTag0 = new();
            GifTag0.RepeatCount = 3;
            GifTag0.RegisterDescriptorCount = 1;
            GifTag1 = new();
            GifTag1.RepeatCount = 14;
            TrxPosReg = new();
            TrxPosAddress = 81;
            TrxRegReg = new();
            TrxRegAddress = 82;
            TrxDirReg = TransmissionDirection.HostToLocal;
            TrxDirAddress = 83;
            GifTag2 = new();
            GifTag2.Mode = GifMode.Image;
            GifTag3 = new();
        }
        public PS2ImageHeader(BinaryReader reader)
        {
            GifTag0 = new(reader);
            GifTag1 = new(reader);
            TrxPosReg = new(reader);
            TrxPosAddress = reader.ReadUInt64();
            TrxRegReg = new(reader);
            TrxRegAddress = reader.ReadUInt64();
            TrxDirReg = (TransmissionDirection)reader.ReadUInt64();
            TrxDirAddress = reader.ReadUInt64();
            GifTag2 = new(reader);
            GifTag3 = new(reader);
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(GifTag0.Value);
            writer.Write(GifTag1.Value);
            writer.Write(TrxPosReg.Value);
            writer.Write(TrxPosAddress);
            writer.Write(TrxRegReg.Value);
            writer.Write(TrxRegAddress);
            writer.Write((ulong)TrxDirReg);
            writer.Write(TrxDirAddress);
            writer.Write(GifTag2.Value);
            writer.Write(GifTag3.Value);
        }
    }
    public class TrxPosRegister
    {
        public ulong Value;

        public TrxPosRegister(ulong value)
        {
            Value = value;
        }
        public TrxPosRegister(BinaryReader reader)
        {
            Value = reader.ReadUInt64();
        }
        public TrxPosRegister()
        {
            Value = 0;
        }
        public ulong SourceRectangleX
        {
            get { return GetBits(Value, 11, 0); }
            set { SetBits(ref Value, value, 11, 0); }
        }

        public ulong SourceRectangleY
        {
            get { return GetBits(Value, 11, 16); }
            set { SetBits(ref Value, value, 11, 16); }
        }

        public ulong DestinationRectangleX
        {
            get { return GetBits(Value, 11, 32); }
            set { SetBits(ref Value, value, 11, 32); }
        }

        public ulong DestinationRectangleY
        {
            get { return GetBits(Value, 11, 48); }
            set { SetBits(ref Value, value, 11, 48); }
        }

        public TransmissionOrder PixelTransmissionOrder
        {
            get { return (TransmissionOrder)GetBits(Value, 2, 59); }
            set { SetBits(ref Value, (ulong)value, 2, 59); }
        }
    }
    public class TrxRegRegister
    {
        public ulong Value;

        public TrxRegRegister(ulong value)
        {
            Value = value;
        }
        public TrxRegRegister(BinaryReader reader)
        {
            Value = reader.ReadUInt64();
        }
        public TrxRegRegister()
        {
            Value = 0;
        }
        public TrxRegRegister(ulong transmissionWidth, ulong transmissionHeight)
        {
            Value = 0;
            TransmissionWidth = transmissionWidth;
            TransmissionHeight = transmissionHeight;
        }
        public ulong TransmissionWidth
        {
            get { return GetBits(Value, 32, 0); }
            set { SetBits(ref Value, value, 32, 0); }
        }
        public ulong TransmissionHeight
        {
            get { return GetBits(Value, 32, 32); }
            set { SetBits(ref Value, value, 32, 32); }
        }
    }
    public class GifTag
    {
        public ulong Value;
        public GifTag(ulong value)
        {
            Value = value;
        }
        public GifTag(BinaryReader reader)
        {
            Value = reader.ReadUInt64();
        }
        public GifTag()
        {
            Value = 0;
        }
        public ulong RepeatCount
        {
            get { return GetBits(Value, 15, 0); }
            set { SetBits(ref Value, value, 15, 0); }
        }
        public bool NoFollowingPrimitive
        {
            get { return (GetBits(Value, 1, 15) == 1); }
            set { SetBits(ref Value, value ? (ulong)1 : (ulong)0, 1, 15); }
        }
        public bool UsePrimitiveData
        {
            get { return (GetBits(Value, 1, 46) == 1); }
            set { SetBits(ref Value, value ? (ulong)1 : (ulong)0, 1, 46); }
        }
        public ulong PrimitiveData
        {
            get { return GetBits(Value, 11, 47); }
            set { SetBits(ref Value, value, 11, 47); }
        }
        public GifMode Mode
        {
            get { return (GifMode)GetBits(Value, 2, 58); }
            set { SetBits(ref Value, (ulong)value, 2, 58); }
        }
        public ulong RegisterDescriptorCount
        {
            get { return GetBits(Value, 4, 60); }
            set { SetBits(ref Value, value, 4, 60); }
        }
    }
    public class RasterInfoStruct : RmdChunk
    {
        public int Width;
        public int Height;
        public int Depth;
        public RwRasterFormat Format;
        public Tex0Register Tex0Reg;
        public Tex1Register Tex1Reg;
        public MipRegister Mip1Reg;
        public MipRegister Mip2Reg;
        public uint TexelDataLength;
        public uint PaletteDataLength;
        public uint GPUAlignedLength;
        public uint SkyMipMapValue;
        public RasterInfoStruct(int width, int height, PS2PixelFormat format, int version = 469893175)
            :base (RmdChunkType.Struct, version)
        {
            Width = width;
            Height = height;
            Depth = GetDepth(format);
            Format = GetRasterFormat(Depth);
            Tex0Reg = new Tex0Register(width, height, format);
            Tex1Reg = new Tex1Register();

            int TextureSize = Width * Height;
            if (TextureSize < 16384)
            {
                Tex1Reg.MaxMipLevel = 7;
                Tex1Reg.MipMinFilter = PS2FilterMode.None;

                if (TextureSize > 4096)
                    Tex1Reg.MipMaxFilter = PS2FilterMode.Nearest;

            }
            else
            {
                Tex1Reg.MipMaxFilter = PS2FilterMode.Linear; 
                Tex1Reg.MipMinFilter = PS2FilterMode.None;
            }

            Mip1Reg = new MipRegister();
            Mip2Reg = new MipRegister();


            SkyMipMapValue = 4032;
            CalculateSizes();
        }
        public void CalculateSizes()
        {
            int TextureSize = Width * Height;
            TexelDataLength = (uint)(TextureSize * (Depth / 8));
            if (Format.HasFlag(RwRasterFormat.HasHeaders))
                TexelDataLength += 0x50; //HeaderSize
            if (Format.HasFlag(RwRasterFormat.MipMap))
            { 
                for (int i = 1; i < (int)Tex1Reg.MaxMipLevel; i++)
                {
                    TexelDataLength += (uint)((Width >> i) * (Height >> i) * (Depth / 8));
                }
            }
            PaletteDataLength = 0;
            GPUAlignedLength = (uint)((Width / 2) * (Height / 2));
            if (Depth <= 8)
            {
                PaletteDataLength = (uint)(Depth == 4 ? 16 : 256 * 4);

                GPUAlignedLength += PaletteDataLength / 4;
                if (Format.HasFlag(RwRasterFormat.HasHeaders))
                    PaletteDataLength += 0x50; //HeaderSize
            }
            GPUAlignedLength = Align(GPUAlignedLength, 2048);
        }
        public RasterInfoStruct(RmdChunkType type, uint size, int version, BinaryReader reader)
           : base(type, size, version, reader)
        {
        }
        protected override void ReadChunkData(BinaryReader reader, uint Size)
        {
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Depth = reader.ReadInt32();
            Format = (RwRasterFormat)reader.ReadUInt32();
            Tex0Reg = new(reader);
            Tex1Reg = new(reader);
            Mip1Reg = new(reader);
            Mip2Reg = new(reader);
            TexelDataLength = reader.ReadUInt32();
            PaletteDataLength = reader.ReadUInt32();
            GPUAlignedLength = reader.ReadUInt32();
            SkyMipMapValue = reader.ReadUInt32();
        }
        protected override void WriteChunkData(BinaryWriter writer)
        {
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(Depth);
            writer.Write((uint)Format);
            writer.Write(Tex0Reg.Value);
            writer.Write(Tex1Reg.Value);
            writer.Write(Mip1Reg.Value);
            writer.Write(Mip2Reg.Value);
            writer.Write(TexelDataLength);
            writer.Write(PaletteDataLength);
            writer.Write(GPUAlignedLength);
            writer.Write(SkyMipMapValue);
        }
        
    }
    public class Tex0Register
    {
        public ulong Value;

        public Tex0Register(ulong value)
        {
            Value = value;
        }
        public Tex0Register(BinaryReader reader)
        {
            Value = reader.ReadUInt64();
        }
        public Tex0Register(int width, int height, PS2PixelFormat format)
        {
            Value = 0;
            if (format != PS2PixelFormat.PSMZ32 && format != PS2PixelFormat.PSMTC24)
                HasAlpha = true;

            PaletteBufferLoad = PaletteBufferLoadMode.Load;
            
            TextureBufferWidth = CalculateBufferWidth(width, GetDepth(format));
            TexturePixelFormat = format;
            TextureWidth = (ulong)Math.Log2(width);
            TextureHeight = (ulong)Math.Log2(height);
        }
        public static ulong CalculateBufferWidth(int width, int depth)
        {
            ulong bufferWidth = (ulong)(width / (depth * 8));

            if (bufferWidth < 1)
                bufferWidth = 1;
            else if (bufferWidth == 1)
                bufferWidth = 2;
            else
                bufferWidth = (ulong)Math.Ceiling((double)bufferWidth);

            return bufferWidth;
        }
        
        public ulong TextureBasePointer
        {
            get { return GetBits(Value, 14, 0); }
            set { SetBits(ref Value, value, 14, 0); }
        }
        public ulong TextureBufferWidth
        {
            get { return GetBits(Value, 6, 14); }
            set { SetBits(ref Value, value, 6, 14); }
        }
        public PS2PixelFormat TexturePixelFormat
        {
            get { return (PS2PixelFormat)GetBits(Value, 6, 20); }
            set { SetBits(ref Value, (ulong)value, 6, 20); }
        }
        public ulong TextureWidth
        {
            get { return GetBits(Value, 4, 26); }
            set { SetBits(ref Value, value, 4, 26); }
        }
        public ulong TextureHeight
        {
            get { return GetBits(Value, 4, 30); }
            set { SetBits(ref Value, value, 4, 30); }
        }
        public bool HasAlpha
        {
            get { return (GetBits(Value, 1, 34) == 1); }
            set { SetBits(ref Value, value ? (ulong)1 : (ulong)0, 1, 34); }
        }
        public PS2TextureFunction TextureFunction
        {
            get { return (PS2TextureFunction)GetBits(Value, 2, 35); }
            set { SetBits(ref Value, (ulong)value, 2, 35); }
        }
        public ulong PaletteBufferBasePointer
        {
            get { return GetBits(Value, 14, 37); }
            set { SetBits(ref Value, value, 14, 37); }
        }

        public PS2PixelFormat PalettePixelFormat
        {
            get { return (PS2PixelFormat)GetBits(Value, 4, 51); }
            set { SetBits(ref Value, (ulong)value, 4, 51); }
        }

        public bool DisablePaletteSwizling
        {
            get { return (GetBits(Value, 1, 55) == 1); }
            set { SetBits(ref Value, value ? (ulong)1 : (ulong)0, 1, 55); }
        }
        public ulong PaletteEntryOffset
        {
            get { return GetBits(Value, 5, 56); }
            set { SetBits(ref Value, value, 5, 56); }
        }
        public PaletteBufferLoadMode PaletteBufferLoad
        {
            get { return (PaletteBufferLoadMode)GetBits(Value, 3, 61); }
            set { SetBits(ref Value, (ulong)value, 3, 61); }
        }
    }
    public class Tex1Register
    {
        public ulong Value;

        public Tex1Register(ulong value)
        {
            Value = value;
        }
        public Tex1Register(BinaryReader reader)
        {
            Value = reader.ReadUInt64();
        }
        public Tex1Register()
        {
            Value = 0;
            MipMinFilter = PS2FilterMode.Nearest;
        }
        public bool UseFixedLodValue
        {
            get { return (GetBits(Value, 1, 0) == 1); }
            set { SetBits(ref Value, value ? (ulong)1 : (ulong)0, 1, 0); }
        }
        public ulong MaxMipLevel
        {
            get { return GetBits(Value, 3, 2); }
            set { SetBits(ref Value, value, 3, 2); }
        }
        public PS2FilterMode MipMaxFilter
        {
            get { return (PS2FilterMode)GetBits(Value, 2, 5); }
            set { SetBits(ref Value, (ulong)value, 2, 5); }
        }
        public PS2FilterMode MipMinFilter
        {
            get { return (PS2FilterMode)GetBits(Value, 2, 7); }
            set { SetBits(ref Value, (ulong)value, 2, 7); }
        }
        public bool UseAutoBaseAdress
        {
            get { return (GetBits(Value, 1, 9) == 1); }
            set { SetBits(ref Value, value ? (ulong)1 : (ulong)0, 1, 9); }
        }
        public ulong MipL
        {
            get { return GetBits(Value, 2, 19); }
            set { SetBits(ref Value, value, 2, 19); }
        }
        public ulong MipK
        {
            get { return GetBits(Value, 12, 32); }
            set { SetBits(ref Value, value, 12, 32); }
        }
    }
    public class MipRegister
    {
        public ulong Value;

        public MipRegister(ulong value)
        {
            Value = value;
        }
        public MipRegister(BinaryReader reader)
        {
            Value = reader.ReadUInt64();
        }
        public MipRegister()
        {
            Value = 0;
            Mip1BufferWidth = 1;
            Mip2BufferWidth = 1;
            Mip3BufferWidth = 1;
        }
        public ulong Mip1BasePointer
        {
            get { return GetBits(Value, 14, 0); }
            set { SetBits(ref Value, value, 14, 0); }
        }

        public ulong Mip1BufferWidth
        {
            get { return GetBits(Value, 6, 14); }
            set { SetBits(ref Value, value, 6, 14); }
        }

        public ulong Mip2BasePointer
        {
            get { return GetBits(Value, 14, 20); }
            set { SetBits(ref Value, value, 14, 20); }
        }

        public ulong Mip2BufferWidth
        {
            get { return GetBits(Value, 6, 34); }
            set { SetBits(ref Value, value, 6, 34); }
        }

        public ulong Mip3BasePointer
        {
            get { return GetBits(Value, 14, 40); }
            set { SetBits(ref Value, value, 14, 40); }
        }

        public ulong Mip3BufferWidth
        {
            get { return GetBits(Value, 6, 54); }
            set { SetBits(ref Value, value, 6, 54); }
        }
    }
}
