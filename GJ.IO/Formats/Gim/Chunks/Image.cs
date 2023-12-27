using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using static GJ.IO.IOFunctions;
using static GimLib.GimFunctions;
using static GimLib.GimEnums;
using static DDSLib.DXT;

namespace GimLib.Chunks
{
    public struct ImageInfo
    {
        public ImageInfo()
        {
            HeaderSize = 48;
            Unused = 0;
            Format = GimFormat.RGBA8888;
            Order = GimOrder.Normal;
            Width = 0;
            Height = 0;
            BitsPerPixel = 32;
            PitchAlign = 16;
            HeightAlign = 1;
            DimCount = 2;
            Reserved = 0;
            Reserved2 = 0;
            OffsetsOffs = 0;
            ImagesOffs = 0;
            TotalSize = 0;
            PlaneMask = 0;
            LevelType = GimType.MipMap;
            LevelCount = 1;
            FrameType = GimType.Sequence;
            FrameCount = 1;
        }
        public ushort HeaderSize;
        public ushort Unused { get; }
        public GimFormat Format;
        public GimOrder Order;
        public ushort Width;
        public ushort Height;
        public ushort BitsPerPixel;
        public ushort PitchAlign;
        public ushort HeightAlign;
        public ushort DimCount;
        public ushort Reserved { get; }
        public ushort Reserved2 { get; }
        public uint OffsetsOffs { get; }
        public uint ImagesOffs { get; }
        public uint TotalSize { get; }
        public uint PlaneMask;
        public GimType LevelType;
        public ushort LevelCount;
        public GimType FrameType;
        public ushort FrameCount;

        public ImageInfo(BinaryReader reader)
        {
            HeaderSize = reader.ReadUInt16();
            Unused = reader.ReadUInt16();
            Format = (GimFormat)reader.ReadUInt16();
            Order = (GimOrder)reader.ReadUInt16();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            BitsPerPixel = reader.ReadUInt16();
            PitchAlign = reader.ReadUInt16();
            HeightAlign = reader.ReadUInt16();
            DimCount = reader.ReadUInt16();
            Reserved = reader.ReadUInt16();
            Reserved2 = reader.ReadUInt16();
            OffsetsOffs = reader.ReadUInt32();
            ImagesOffs = reader.ReadUInt32();
            TotalSize = reader.ReadUInt32();
            PlaneMask = reader.ReadUInt32();
            LevelType = (GimType)reader.ReadUInt16();
            LevelCount = reader.ReadUInt16();
            FrameType = (GimType)reader.ReadUInt16();
            FrameCount = reader.ReadUInt16();
        }
    }
    public struct Frame
    {
        public Color[] Pixels;
        public uint[] PixelsIndex;
    }
    public struct Level
    {
        public List<Frame> Frames;
    }
    public class GimImage : GimChunk
    {
        public GimPalette FullColorToIndexed()
        {
            ImageInfo Header = ImgInfo;
            List<Color> Colors = new();
            for (int i = 0; i < Levels.Count; i++)
            {
                for (int j = 0; j < Levels[i].Frames.Count; j++)
                {
                    uint[] PixelsIndex = new uint[ImgInfo.Height * ImgInfo.Width];
                    for (int k = 0; k < Levels[i].Frames[j].Pixels.Length; k++)
                    {
                        if (!Colors.Contains(Levels[i].Frames[j].Pixels[k]))
                            Colors.Add(Levels[i].Frames[j].Pixels[k]);

                        PixelsIndex[k] = (uint)Colors.IndexOf(Levels[i].Frames[j].Pixels[k]);
                    }
                    Frame NewFrame = new();
                    NewFrame.PixelsIndex = PixelsIndex;
                    Levels[i].Frames[j] = NewFrame;
                }
            }
            if (Colors.Count <= 16)
            {
                ImgInfo.BitsPerPixel = 4;
                ImgInfo.Format = GimFormat.Index4;
            }
            else if (Colors.Count <= byte.MaxValue + 1)
            {
                ImgInfo.BitsPerPixel = 8;
                ImgInfo.Format = GimFormat.Index8;
            }
            else if (Colors.Count <= ushort.MaxValue + 1)
            {
                ImgInfo.BitsPerPixel = 16;
                ImgInfo.Format = GimFormat.Index16;
            }
            else if (Colors.Count <= int.MaxValue)
            {
                ImgInfo.BitsPerPixel = 32;
                ImgInfo.Format = GimFormat.Index32;
            }
            else
            {
                throw new Exception("The image has too many colors to be indexed");
                //throw new Exception("Indexes higher than 16 bits are not implemented");
            }
            
            Header.Height = 1;
            Header.Width = (ushort)Colors.Count;
            Header.LevelType = GimType.MipMap2; //Palettes use MipMap2
            Frame palFrame = new();
            palFrame.Pixels = Colors.ToArray();
            Level palLevel = new();
            palLevel.Frames = new();
            palLevel.Frames.Add(palFrame);
            List<Level> palLevels = new();
            palLevels.Add(palLevel);
            GimPalette NewPal = new(Header,palLevels, null);
            return NewPal;
        }
        public void IndexedToFullColor(GimPalette Palette)
        {
            for (int i = 0; i < Levels.Count; i++)
            {
                for (int j = 0; j < Levels[i].Frames.Count; j++)
                {
                    Color[] Pix = new Color[Levels[i].Frames[j].PixelsIndex.Length];
                    for (int k = 0; k < Levels[i].Frames[j].PixelsIndex.Length; k++)
                    {
                        Pix[k] = Palette.Levels[0].Frames[0].Pixels[k];
                    }
                    Frame NFrame = new();
                    NFrame.Pixels = Pix;
                    Levels[i].Frames[j] = NFrame;
                }
            }
            ImgInfo.Format = GimFormat.RGBA8888;
            ImgInfo.BitsPerPixel = 32;
        }
        public ImageInfo ImgInfo;
        public List<Level> Levels = new();

        public GimImage(ImageInfo imgInfo, List<Level> levels, GimChunk? parent = null)
        {
            ImgInfo = imgInfo;
            Levels = levels;
            Type = GimChunkType.GimImage;
            Parent = parent;
        }
        public GimImage(GimChunk? parent, ref GimChunkHeader Header, BinaryReader reader) : base(parent, ref Header, reader)
        {
        }

        private Color ReadColor(BinaryReader reader)
        {
            switch (ImgInfo.Format)
            {
                case GimFormat.RGBA5650:
                    {
                        return ReadRGBA5650(reader);
                    }
                case GimFormat.RGBA5551:
                    {
                        return ReadRGBA5551(reader);
                    }
                case GimFormat.RGBA4444:
                    {
                        return ReadRGBA4444(reader);
                    }
                case GimFormat.RGBA8888:
                    {
                        return ReadRGBA8888(reader);
                    }
                default: throw new Exception($"Unimplemented GIM Color Format: {ImgInfo.Format}");
            }
        }
        private void WriteColor(Color VColor, BinaryWriter writer)
        {
            switch (ImgInfo.Format)
            {
                case GimFormat.RGBA5650:
                    {
                        WriteRGBA5650(writer,VColor);
                        break;
                    }
                case GimFormat.RGBA5551:
                    {
                        WriteRGBA5551(writer,VColor);
                        break;
                    }
                case GimFormat.RGBA4444:
                    {
                        WriteRGBA4444(writer, VColor);
                        break;
                    }
                case GimFormat.RGBA8888:
                    {
                        WriteRGBA8888(writer, VColor);
                        break;
                    }
                default:
                    throw new Exception("Unimplemented Color Format");
            }
        }
        private void WriteIndex(uint Index, BinaryWriter writer)
        {
            switch (ImgInfo.Format)
            {
                case GimFormat.Index8:
                    {
                        writer.Write((byte)Index);
                        break;
                    }
                case GimFormat.Index16:
                    {
                        writer.Write((ushort)Index);
                        break;
                    }
                case GimFormat.Index32:
                    {
                        writer.Write(Index);
                        break;
                    }
            }
        }
        protected override void ReadData(ref GimChunkHeader Header, BinaryReader reader)
        {
            long Start = reader.BaseStream.Position;
            ImgInfo = new(reader);
            reader.BaseStream.Seek(Start+ImgInfo.OffsetsOffs, SeekOrigin.Begin);

            for (int i = 0; i < ImgInfo.LevelCount; i++)
            {
                uint Offset = reader.ReadUInt32();
                long Cur = reader.BaseStream.Position;

                reader.BaseStream.Seek(Offset + Start, SeekOrigin.Begin);
                Level NewLevel = new();
                NewLevel.Frames = new();

                for (int j = 0; j < ImgInfo.FrameCount; j++)
                {
                    Frame NewFrame = new();

                    if ((int)ImgInfo.Format > 7)
                    {
                        int pitch = (ImgInfo.BitsPerPixel * ImgInfo.Width + 7) / 8;
                        pitch = (pitch + ImgInfo.PitchAlign - 1) / ImgInfo.PitchAlign * ImgInfo.PitchAlign;
                        if (ImgInfo.Format == GimFormat.DXT1 || ImgInfo.Format == GimFormat.DXT1EXT)
                            NewFrame.Pixels = ReadSonyDXT1Data(reader, ImgInfo.Width, ImgInfo.Height,pitch);
                        else if (ImgInfo.Format == GimFormat.DXT3 || ImgInfo.Format == GimFormat.DXT3EXT)
                            NewFrame.Pixels = ReadSonyDXT3Data(reader, ImgInfo.Width, ImgInfo.Height, pitch);
                        else if (ImgInfo.Format == GimFormat.DXT5 || ImgInfo.Format == GimFormat.DXT5EXT)
                            NewFrame.Pixels = ReadSonyDXT5Data(reader, ImgInfo.Width, ImgInfo.Height, pitch);
                        else
                            throw new Exception($"Unsupported GIM color format: {ImgInfo.Format}");
                    }
                    else if ((int)ImgInfo.Format > 3)
                    {
                        NewFrame.PixelsIndex = new uint[ImgInfo.Width * ImgInfo.Height];

                        switch (ImgInfo.Format)
                        {
                            case GimFormat.Index4:
                                {
                                    if (ImgInfo.Order == GimOrder.PSPImage)
                                    {
                                        //A block has 16 bytes of width and 8 pixels of height
                                        int blockWidth = 32; // Width of each block in pixels
                                        int blockHeight = 8; // Height of each block in pixels

                                        for (int blockRow = 0; blockRow < ImgInfo.Height / blockHeight; blockRow++)
                                        {
                                            for (int blockCol = 0; blockCol < ImgInfo.Width / blockWidth; blockCol++)
                                            {
                                                for (int pixelRow = 0; pixelRow < blockHeight; pixelRow++)
                                                {
                                                    for (int pixelCol = 0; pixelCol < blockWidth; pixelCol += 2)
                                                    {
                                                        int x = blockCol * blockWidth + pixelCol;
                                                        int y = blockRow * blockHeight + pixelRow;

                                                        int packedIndexes = reader.ReadByte();
                                                        NewFrame.PixelsIndex[x + y * ImgInfo.Width] = (uint)packedIndexes & 0x0F;
                                                        NewFrame.PixelsIndex[x + 1 + y * ImgInfo.Width] = (uint)(packedIndexes >> 4) & 0x0F;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int index = 0;
                                        int RowByteCount = (int)Math.Ceiling((double)(ImgInfo.Width / 2.0));
                                        for (int row = 0; row < ImgInfo.Height; row++)
                                        {
                                            byte[] rowData = reader.ReadBytes(RowByteCount);
                                            for (int col = 0; col < ImgInfo.Width; col += 2)
                                            {
                                                byte indexByte = rowData[col / 2];
                                                uint index2 = (uint)((indexByte >> 4) & 0x0F);
                                                uint index1 = (uint)(indexByte & 0x0F);
                                                NewFrame.PixelsIndex[index] = index1;
                                                NewFrame.PixelsIndex[index + 1] = index2;
                                                index += 2;
                                            }
                                        }
                                    }
                                    break;
                                }
                            case GimFormat.Index8:
                                {
                                    if (ImgInfo.Order == GimOrder.PSPImage)
                                    {
                                        //A block has 16 bytes of width and 8 pixels of height
                                        int blockWidth = 16; // Width of each block in pixels
                                        int blockHeight = 8; // Height of each block in pixels

                                        for (int blockRow = 0; blockRow < ImgInfo.Height / blockHeight; blockRow++)
                                        {
                                            for (int blockCol = 0; blockCol < ImgInfo.Width / blockWidth; blockCol++)
                                            {
                                                for (int pixelRow = 0; pixelRow < blockHeight; pixelRow++)
                                                {
                                                    for (int pixelCol = 0; pixelCol < blockWidth; pixelCol++)
                                                    {
                                                        int x = blockCol * blockWidth + pixelCol;
                                                        int y = blockRow * blockHeight + pixelRow;
                                                        NewFrame.PixelsIndex[x + y * ImgInfo.Width] = reader.ReadByte();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int index = 0;
                                        for (int k = 0; k < ImgInfo.Height; k++)
                                        {
                                            for (int l = 0; l < ImgInfo.Width; l++)
                                            {
                                                NewFrame.PixelsIndex[index] = reader.ReadByte();
                                                index++;
                                            }
                                        }
                                    }
                                    break;
                                }
                            case GimFormat.Index16:
                                {
                                    if (ImgInfo.Order == GimOrder.PSPImage)
                                    {
                                        //A block has 16 bytes of width and 8 pixels of height
                                        int blockWidth = 8; // Width of each block in pixels
                                        int blockHeight = 8; // Height of each block in pixels

                                        for (int blockRow = 0; blockRow < ImgInfo.Height / blockHeight; blockRow++)
                                        {
                                            for (int blockCol = 0; blockCol < ImgInfo.Width / blockWidth; blockCol++)
                                            {
                                                for (int pixelRow = 0; pixelRow < blockHeight; pixelRow++)
                                                {
                                                    for (int pixelCol = 0; pixelCol < blockWidth; pixelCol++)
                                                    {
                                                        int x = blockCol * blockWidth + pixelCol;
                                                        int y = blockRow * blockHeight + pixelRow;
                                                        NewFrame.PixelsIndex[x + y * ImgInfo.Width] = reader.ReadUInt16();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int index = 0;
                                        for (int k = 0; k < ImgInfo.Height; k++)
                                        {
                                            for (int l = 0; l < ImgInfo.Width; l++)
                                            {
                                                NewFrame.PixelsIndex[index] = reader.ReadUInt16();
                                                index++;
                                            }
                                        }
                                    }
                                    break;
                                }
                            case GimFormat.Index32:
                                {
                                    if (ImgInfo.Order == GimOrder.PSPImage)
                                    {
                                        //A block has 16 bytes of width and 8 pixels of height
                                        int blockWidth = 4; // Width of each block in pixels
                                        int blockHeight = 8; // Height of each block in pixels

                                        for (int blockRow = 0; blockRow < ImgInfo.Height / blockHeight; blockRow++)
                                        {
                                            for (int blockCol = 0; blockCol < ImgInfo.Width / blockWidth; blockCol++)
                                            {
                                                for (int pixelRow = 0; pixelRow < blockHeight; pixelRow++)
                                                {
                                                    for (int pixelCol = 0; pixelCol < blockWidth; pixelCol++)
                                                    {
                                                        int x = blockCol * blockWidth + pixelCol;
                                                        int y = blockRow * blockHeight + pixelRow;
                                                        NewFrame.PixelsIndex[x + y * ImgInfo.Width] = reader.ReadUInt32();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int index = 0;
                                        for (int k = 0; k < ImgInfo.Height; k++)
                                        {
                                            for (int l = 0; l < ImgInfo.Width; l++)
                                            {
                                                NewFrame.PixelsIndex[index] = reader.ReadUInt32();
                                                index++;
                                            }
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                    else
                    {
                        NewFrame.Pixels = new Color[ImgInfo.Width * ImgInfo.Height];
                        if (ImgInfo.Order == GimOrder.PSPImage) //PSP "Faster" Image order
                        {
                            //A block has 16 bytes of width and 8 pixels of height
                            int blockWidth = 8; // Width of each block in pixels
                            // 16 / 2 (All non indexed formats besides 8888 are 2 bytes long)
                            if (ImgInfo.Format == GimFormat.RGBA8888)
                                blockWidth = 4; // 16 / 4 (8888 is 4 bytes long)

                            int blockHeight = 8; // Height of each block in pixels

                            for (int blockRow = 0; blockRow < ImgInfo.Height / blockHeight; blockRow++)
                            {
                                for (int blockCol = 0; blockCol < ImgInfo.Width / blockWidth; blockCol++)
                                {
                                    for (int pixelRow = 0; pixelRow < blockHeight; pixelRow++)
                                    {
                                        for (int pixelCol = 0; pixelCol < blockWidth; pixelCol++)
                                        {
                                            int x = blockCol * blockWidth + pixelCol;
                                            int y = blockRow * blockHeight + pixelRow;
                                            NewFrame.Pixels[x + y * ImgInfo.Width] = ReadColor(reader);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int index = 0;
                            for (int k = 0; k < ImgInfo.Height; k++)
                            {
                                for (int l = 0; l < ImgInfo.Width; l++)
                                {
                                    NewFrame.Pixels[index] = ReadColor(reader);
                                    index++;
                                }
                            }
                        }
                    }

                    NewLevel.Frames.Add(NewFrame);
                }
                Levels.Add(NewLevel);
                reader.BaseStream.Seek(Cur, SeekOrigin.Begin);
            }
            reader.BaseStream.Seek(Start + ImgInfo.TotalSize, SeekOrigin.Begin);
        }
        protected override void WriteData(BinaryWriter writer)
        {
            long Start = writer.BaseStream.Position;
            writer.Write(ImgInfo.HeaderSize);
            writer.Write((ushort)0); //Unused
            writer.Write((ushort)ImgInfo.Format);
            writer.Write((ushort)ImgInfo.Order);
            writer.Write(ImgInfo.Width);
            writer.Write(ImgInfo.Height);
            writer.Write(ImgInfo.BitsPerPixel);
            writer.Write(ImgInfo.PitchAlign);
            writer.Write(ImgInfo.HeightAlign);
            writer.Write(ImgInfo.DimCount);
            writer.Write((ushort)0); //Reserved
            writer.Write((ushort)0); //Reserved2

            //Leave those 3 values as dummies and change them later
            long vals = writer.BaseStream.Position;
            writer.Write(0); //OffsetsOffs
            writer.Write(0); //ImagesOffs
            writer.Write(0); //TotalSize


            writer.Write(ImgInfo.PlaneMask);
            writer.Write((ushort)ImgInfo.LevelType);
            writer.Write((ushort)Levels.Count);
            writer.Write((ushort)ImgInfo.FrameType);
            writer.Write((ushort)Levels[0].Frames.Count);

            Align(writer, 16);
            long OffStart = writer.BaseStream.Position;
            for (int i = 0; i < Levels.Count; i++) //Write Dummy Offsets
                for (int j = 0; j < Levels[i].Frames.Count; j++)
                    writer.Write(1);

            Align(writer, 16);
            long ImageStart = writer.BaseStream.Position;
            int imgCount = 0;
            for (int i = 0; i < Levels.Count; i++) //Write the image data and offsets
                for (int j = 0; j < Levels[i].Frames.Count; j++)
                {
                    long cur = writer.BaseStream.Position;
                    writer.Seek((int)OffStart + imgCount, SeekOrigin.Begin);
                    writer.Write((int)(cur - Start));
                    writer.Seek((int)cur, SeekOrigin.Begin);
                    imgCount +=4;

                    int blockWidth = 128 / ImgInfo.BitsPerPixel;
                    int blockHeight = 8;

                    if ((int)ImgInfo.Format > 3)
                    {
                        if (ImgInfo.Order == GimOrder.PSPImage)
                        {
                            if (ImgInfo.Format == GimFormat.Index4)
                            {
                                for (int blockRow = 0; blockRow < ImgInfo.Height / blockHeight; blockRow++)
                                {
                                    for (int blockCol = 0; blockCol < ImgInfo.Width / blockWidth; blockCol++)
                                    {
                                        for (int pixelRow = 0; pixelRow < blockHeight; pixelRow++)
                                        {
                                            for (int pixelCol = 0; pixelCol < blockWidth; pixelCol++)
                                            {
                                                int x = blockCol * blockWidth + pixelCol;
                                                int y = blockRow * blockHeight + pixelRow;
                                                byte indexByte = (byte)((Levels[i].Frames[j].PixelsIndex[x + y * ImgInfo.Width] << 4) | Levels[i].Frames[j].PixelsIndex[x + 1 + y * ImgInfo.Width]);
                                                writer.Write(indexByte);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int blockRow = 0; blockRow < ImgInfo.Height / blockHeight; blockRow++)
                                {
                                    for (int blockCol = 0; blockCol < ImgInfo.Width / blockWidth; blockCol++)
                                    {
                                        for (int pixelRow = 0; pixelRow < blockHeight; pixelRow++)
                                        {
                                            for (int pixelCol = 0; pixelCol < blockWidth; pixelCol++)
                                            {
                                                int x = blockCol * blockWidth + pixelCol;
                                                int y = blockRow * blockHeight + pixelRow;
                                                WriteIndex(Levels[i].Frames[j].PixelsIndex[x + y * ImgInfo.Width], writer);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (ImgInfo.Format == GimFormat.Index4)
                            {
                                for (int l = 0; l < Levels[i].Frames[j].PixelsIndex.Length; l += 2)
                                {
                                    byte indexByte = (byte)((Levels[i].Frames[j].PixelsIndex[l] << 4) | Levels[i].Frames[j].PixelsIndex[l + 1]);
                                    writer.Write(indexByte);
                                }
                            }
                            else
                            {
                                for (int l = 0; l < Levels[i].Frames[j].PixelsIndex.Length; l++)
                                    WriteIndex(Levels[i].Frames[j].PixelsIndex[l], writer);
                            }
                        }


                    }
                    else
                    {
                        if (ImgInfo.Order == GimOrder.PSPImage)
                        {
                            int index = 0;
                            for (int blockRow = 0; blockRow < ImgInfo.Height / blockHeight; blockRow++)
                            {
                                for (int blockCol = 0; blockCol < ImgInfo.Width / blockWidth; blockCol++)
                                {
                                    for (int pixelRow = 0; pixelRow < blockHeight; pixelRow++)
                                    {
                                        for (int pixelCol = 0; pixelCol < blockWidth; pixelCol++)
                                        {
                                            int x = blockCol * blockWidth + pixelCol;
                                            int y = blockRow * blockHeight + pixelRow;
                                            WriteColor(Levels[i].Frames[j].Pixels[x + y * ImgInfo.Width], writer);
                                            index++;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int n = 0; n < Levels[i].Frames[j].Pixels.Length; n++)
                            {
                                WriteColor(Levels[i].Frames[j].Pixels[n], writer);
                            }
                        }
                    }

                }
            Align(writer, 16);
            long End = writer.BaseStream.Position;

            writer.BaseStream.Seek(vals, SeekOrigin.Begin); // Go back and write offsets
            writer.Write((int)(OffStart - Start));
            writer.Write((int)(ImageStart - Start));
            writer.Write((int)(End - Start));
            
            writer.BaseStream.Seek(End, SeekOrigin.Begin);
        }
    }
    public class GimPalette : GimChunk
    {
        public ImageInfo ImgInfo { get; set; }
        public List<Level> Levels = new();

        public GimPalette(ImageInfo imgInfo, List<Level> levels, GimChunk? parent = null)
        {
            Children = new();
            ImgInfo = imgInfo;
            Levels = levels;
            Type = GimChunkType.GimPalette;
            Parent = parent;
        }
        public GimPalette(GimChunk? parent, ref GimChunkHeader Header, BinaryReader reader) : base(parent, ref Header, reader)
        {
        }

        private Color ReadColor(BinaryReader reader)
        {
            switch (ImgInfo.Format)
            {
                case GimFormat.RGBA5650:
                    {
                        return ReadRGBA5650(reader);
                    }
                case GimFormat.RGBA5551:
                    {
                        return ReadRGBA5551(reader);
                    }
                case GimFormat.RGBA4444:
                    {
                        return ReadRGBA4444(reader);
                    }
                case GimFormat.RGBA8888:
                    {
                        return ReadRGBA8888(reader);
                    }
                default: throw new Exception("Unimplemented Color Format");
            }
        }
        private void WriteColor(Color VColor, BinaryWriter writer)
        {
            switch (ImgInfo.Format)
            {
                case GimFormat.RGBA5650:
                    {
                        WriteRGBA5650(writer, VColor);
                        break;
                    }
                case GimFormat.RGBA5551:
                    {
                        WriteRGBA5551(writer, VColor);
                        break;
                    }
                case GimFormat.RGBA4444:
                    {
                        WriteRGBA4444(writer, VColor);
                        break;
                    }
                case GimFormat.RGBA8888:
                    {
                        WriteRGBA8888(writer, VColor);
                        break;
                    }
                default:
                    throw new Exception("Unimplemented Color Format");
            }
        }
        protected override void ReadData(ref GimChunkHeader Header, BinaryReader reader)
        {
            long Start = reader.BaseStream.Position;
            ImgInfo = new(reader);
            reader.BaseStream.Seek(Start + ImgInfo.OffsetsOffs, SeekOrigin.Begin);

            for (int i = 0; i < ImgInfo.LevelCount; i++)
            {
                uint Offset = reader.ReadUInt32();
                long Cur = reader.BaseStream.Position;

                reader.BaseStream.Seek(Offset + Start, SeekOrigin.Begin);
                Level NewLevel = new();
                NewLevel.Frames = new();
                for (int j = 0; j < ImgInfo.FrameCount; j++)
                {
                    Frame NewFrame = new();
                    if ((int)ImgInfo.Format > 3)
                    {
                        throw new Exception("Indexed Colors not allowed on Palletes");
                    }
                    else
                    {
                        List<Color> Pixs = new();
                        for (int k = 0; k < ImgInfo.Height; k++)
                        {
                            for (int l = 0; l < ImgInfo.Width; l++)
                                Pixs.Add(ReadColor(reader));
                        }
                        NewFrame.Pixels = Pixs.ToArray();
                    }
                    NewLevel.Frames.Add(NewFrame);
                }
                Levels.Add(NewLevel);
                reader.BaseStream.Seek(Cur, SeekOrigin.Begin);
            }
            reader.BaseStream.Seek(Start + ImgInfo.TotalSize, SeekOrigin.Begin);
        }
        protected override void WriteData(BinaryWriter writer)
        {
            long Start = writer.BaseStream.Position;
            writer.Write(ImgInfo.HeaderSize);
            writer.Write((ushort)0); //Unused
            writer.Write((ushort)ImgInfo.Format);
            writer.Write((ushort)ImgInfo.Order);
            writer.Write((ushort)Align(ImgInfo.Width, 8));//Padding needs to be counted as a part of the palette here
            //Otherwise the width is considered as "illegal"
            writer.Write(ImgInfo.Height);
            writer.Write(ImgInfo.BitsPerPixel);
            writer.Write(ImgInfo.PitchAlign);
            writer.Write(ImgInfo.HeightAlign);
            writer.Write(ImgInfo.DimCount);
            writer.Write((ushort)0); //Reserved
            writer.Write((ushort)0); //Reserved2

            //Leave those 3 values as dummies and change them later
            long vals = writer.BaseStream.Position;
            writer.Write(0); //OffsetsOffs
            writer.Write(0); //ImagesOffs
            writer.Write(0); //TotalSize


            writer.Write(ImgInfo.PlaneMask);
            writer.Write((ushort)ImgInfo.LevelType);
            writer.Write((ushort)Levels.Count);
            writer.Write((ushort)ImgInfo.FrameType);
            writer.Write((ushort)Levels[0].Frames.Count);

            Align(writer, 16);
            long OffStart = writer.BaseStream.Position;
            for (int i = 0; i < Levels.Count; i++) //Write Dummy Offsets
                for (int j = 0; j < Levels[i].Frames.Count; j++)
                    writer.Write(1);

            Align(writer, 16);
            long ImageStart = writer.BaseStream.Position;
            int imgCount = 0;
            for (int i = 0; i < Levels.Count; i++) //Write the image data
                for (int j = 0; j < Levels[i].Frames.Count; j++)
                {
                    long cur = writer.BaseStream.Position;
                    writer.Seek((int)OffStart + imgCount, SeekOrigin.Begin);
                    writer.Write((int)cur - Start);
                    writer.Seek((int)cur, SeekOrigin.Begin);
                    imgCount += 4;
                    for (int n = 0; n < Levels[i].Frames[j].Pixels.Length; n++)
                    {
                        WriteColor(Levels[i].Frames[j].Pixels[n], writer);
                    }
                }
            Align(writer, 16); //Add padding to the palette
            long End = writer.BaseStream.Position;

            writer.BaseStream.Seek(vals, SeekOrigin.Begin); // Go back and write offsets
            writer.Write((int)(OffStart - Start));
            writer.Write((int)(ImageStart - Start));
            writer.Write((int)(End - Start));

            writer.BaseStream.Seek(End, SeekOrigin.Begin);
        }
    }

}
