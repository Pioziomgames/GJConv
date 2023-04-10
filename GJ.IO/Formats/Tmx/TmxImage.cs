using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TmxLib.TmxEnums;
using static GJ.IO.IOFunctions;

namespace TmxLib
{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    public class TmxImage
    {
        public List<Color>? Pixels;
        public List<byte>? PixelIndexes;

        public void IndexedToFullColor(List<Color> Palette)
        {
            Pixels = new();
            if (PixelIndexes != null)
            {
                for (int i = 0; i < PixelIndexes.Count; i++)
                    Pixels.Add(Palette[PixelIndexes[i]]);
                PixelIndexes = null;
            }
        }
        public List<Color> FullColorToIndexed()
        {
            List<Color> Colors = new();
            List<byte> Indexes = new();

            for (int i = 0; i < Pixels.Count; i++)
            {
                if (!Colors.Contains(Pixels[i]))
                    Colors.Add(Pixels[i]);
                Indexes.Add((byte)Colors.IndexOf(Pixels[i]));
            }
            
            Pixels = null;
            PixelIndexes = Indexes;
            return Colors;
        }
        public TmxImage(List<Color> pixels)
        {
            Pixels = pixels;
        }
        public TmxImage(Color[] pixels)
        {
            Pixels = pixels.ToList();
        }
        public TmxImage(List<byte> pixelIndexes)
        {
            PixelIndexes = pixelIndexes;
        }
        public TmxImage(BinaryReader reader, TmxPixelFormat PixelFormat, ushort Width, ushort Height)
        {
            if ((byte)PixelFormat > 0x0A)
            {
                PixelIndexes = new();
                if (PixelFormat == TmxPixelFormat.PSMT8 || PixelFormat == TmxPixelFormat.PSMT8H)
                {
                    for (int i = 0; i < Height; i++)
                    {
                        for (int j = 0; j < Width; j++)
                            PixelIndexes.Add(reader.ReadByte());
                    }
                }
                else if (PixelFormat == TmxPixelFormat.PSMT4 || PixelFormat == TmxPixelFormat.PSMT4HL || PixelFormat == TmxPixelFormat.PSMT4HH)
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
                    throw new Exception("Unimplemented PixelFormat");
            }
            else
            {
                Pixels = new();
                for (int i = 0; i < Width * Height; i++)
                {
                    Color NC;
                    if (PixelFormat == TmxPixelFormat.PSMTC32)
                    {
                        NC = ReadRGBA8888(reader);
                        NC = Color.FromArgb(HalfByteToFull(NC.A), NC.R, NC.G, NC.B);
                    }
                    else if (PixelFormat == TmxPixelFormat.PSMTC24)
                        NC = ReadRGBA8880(reader);
                    else if (PixelFormat == TmxPixelFormat.PSMTC16 || PixelFormat == TmxPixelFormat.PSMTC16S)
                    {
                        NC = ReadRGBA5551(reader);
                        NC = Color.FromArgb(255, NC.R, NC.G, NC.B);
                    }
                    else
                        throw new Exception("Unimplemented PixelFormat");

                    Pixels.Add(NC);
                }
            }

        }
        public void Write(BinaryWriter writer, TmxPixelFormat PixelFormat)
        {
            if ((byte)PixelFormat > 0x0A)
            {
                if (PixelFormat == TmxPixelFormat.PSMT8 || PixelFormat == TmxPixelFormat.PSMT8H)
                    for (int i = 0; i < PixelIndexes.Count; i++)
                        writer.Write(PixelIndexes[i]);
                else if (PixelFormat == TmxPixelFormat.PSMT4 || PixelFormat == TmxPixelFormat.PSMT4HL || PixelFormat == TmxPixelFormat.PSMT4HH)
                {
                    for (int i = 0; i < PixelIndexes.Count; i += 2)
                    {
                        byte indexByte = (byte)((PixelIndexes[i] << 4) | PixelIndexes[i + 1]);
                        writer.Write(indexByte);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Pixels.Count; i++)
                {
                    if (PixelFormat == TmxPixelFormat.PSMTC32)
                    {
                        Color NC = Color.FromArgb(FullByteToHalf(Pixels[i].A), Pixels[i].R, Pixels[i].G, Pixels[i].B);
                        WriteRGBA8888(writer, NC);
                    }
                    else if (PixelFormat == TmxPixelFormat.PSMTC24)
                        WriteRGBA8880(writer, Pixels[i]);
                    else if (PixelFormat == TmxPixelFormat.PSMTC16 || PixelFormat == TmxPixelFormat.PSMTC16S)
                    {
                        Color NC = Color.FromArgb(0, Pixels[i].R, Pixels[i].G, Pixels[i].B);
                        WriteRGBA5551(writer, NC);
                    }
                        
                    else
                        throw new Exception("Unimplemented PixelFormat");
                }
            }
        }
    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
}
