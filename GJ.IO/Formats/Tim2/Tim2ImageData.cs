using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tim2Lib.Tim2Enums;
using static GJ.IO.IOFunctions;

namespace Tim2Lib
{
    public class Tim2ImageData
    {
        public List<Color> Pixels;
        public List<byte> PixelIndexes;

        public void IndexedToFullColor(List<Color> Colors)
        {
            Pixels = new();
            for (int i = 0; i < PixelIndexes.Count; i++)
            {
                Pixels.Add(Colors[PixelIndexes[i]]);
            }
            PixelIndexes.Clear();
        }
        public List<Color> FullColorToIndexed()
        {
            List<Color> Colors = new();
            List<byte> Indexes = new();
            for (int i = 0; i < Pixels.Count;i++)
            {
                if (!Colors.Contains(Pixels[i]))
                    Colors.Add(Pixels[i]);
                Indexes.Add((byte)Colors.IndexOf(Pixels[i]));
            }
            Pixels.Clear();
            PixelIndexes = Indexes;
            return Colors;
        }
        public Tim2ImageData(BinaryReader reader, int Width, int Height, Tim2BPP BPP)
        {
            Read(reader, Width, Height, BPP);
        }
        public Tim2ImageData(List<Color> pixels)
        {
            Pixels = pixels;
            PixelIndexes = new();
        }
        public Tim2ImageData(Color[] pixels)
        {
            Pixels = pixels.ToList();
            PixelIndexes = new();
        }

        public Tim2ImageData(List<byte> pixelIndexes)
        {
            Pixels = new();
            PixelIndexes = pixelIndexes;
        }
        public void Read(BinaryReader reader, int Width, int Height, Tim2BPP BPP)
        {
            int Size = Width * Height;
            Pixels = new();
            PixelIndexes = new();
            if (BPP == Tim2BPP.INDEX8)
            {
                for (int i = 0; i < Size; i++)
                    PixelIndexes.Add(reader.ReadByte());
            }
            else if (BPP == Tim2BPP.INDEX4)
            {
                int RowByteCount = (int)Math.Ceiling((double)Width / 2);

                for (int row = 0; row < Height; row++)
                {
                    byte[] rowData = reader.ReadBytes(RowByteCount);
                    for (int col = 0; col < Width; col += 2)
                    {
                        byte indexByte = rowData[col / 2];
                        byte index2 = (byte)((indexByte >> 4) & 0x0F);
                        byte index1 = (byte)(indexByte & 0x0F);
                        PixelIndexes.Add(index1);
                        PixelIndexes.Add(index2);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Size; i++)
                {
                    Color NC;
                    if (BPP == Tim2BPP.RGBA8888)
                        NC = ReadRGBA8888(reader);
                    else if (BPP == Tim2BPP.RGBA8880)
                        NC = ReadRGBA8880(reader);
                    else if (BPP == Tim2BPP.RGBA5551)
                        NC = ReadRGBA5551(reader);
                    else throw new Exception("Unimplemented PixelFormat");

                    Pixels.Add(Color.FromArgb(HalfByteToFull(NC.A), NC.R, NC.G, NC.B));
                }
            }
        }
        public void Write(BinaryWriter writer, Tim2BPP BPP)
        {
            if (BPP == Tim2BPP.INDEX8)
                for (int i = 0; i < PixelIndexes.Count; i++)
                    writer.Write(PixelIndexes[i]);
            else if (BPP == Tim2BPP.INDEX4)
            {
                for (int i = 0; i < PixelIndexes.Count; i += 2)
                {
                    byte indexByte = (byte)((PixelIndexes[i] << 4) | PixelIndexes[i + 1]);
                    writer.Write(indexByte);
                }
            }
            else
            {
                for (int i = 0; i < Pixels.Count; i++)
                {
                    Color NC = Color.FromArgb(FullByteToHalf(Pixels[i].A), Pixels[i].R, Pixels[i].G, Pixels[i].B);
                    
                    if (BPP == Tim2BPP.RGBA8888)
                        WriteRGBA8888(writer, NC);
                    else if (BPP == Tim2BPP.RGBA8880)
                        WriteRGBA8880(writer, NC);
                    else if (BPP == Tim2BPP.RGBA5551)
                        WriteRGBA5551(writer, NC);
                }
            }
        }
    }
}
