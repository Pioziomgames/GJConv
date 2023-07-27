using System.IO;
using System.Drawing;

namespace TgaLib
{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    public class TgaImageData
    {
        public Color[][]? Pixels { get; private set; }
        public byte[][]? PixelIndexes { get; private set; }
        public TgaImageData(Color[][] pixels)
        {
            SetPixels(pixels);
        }
        public TgaImageData(Color[] pixels, ushort Height, ushort Width)
        {
            SetAllPixels(pixels, Height, Width);
        }
        public TgaImageData(byte[] indexes, ushort Height, ushort Width)
        {
            SetAllIndexes(indexes, Height, Width);
        }
        public TgaImageData(byte[][] indexes)
        {
            SetIndexes(indexes);
        }
        public void SetPixels(Color[][] pixels)
        {
            Pixels = pixels;
            PixelIndexes = null;
        }
        public void SetIndexes(byte[][] indexes)
        {
            PixelIndexes = indexes;
            Pixels = null;
        }
        public void SetAllPixels(Color[] NewPixels, ushort Height, ushort Width)
        {
            Color[][] colorArray = new Color[Height][];
            for (int i = 0; i < Height; i++)
            {
                colorArray[i] = new Color[Width];
                for (int j = 0; j < Width; j++)
                {
                    colorArray[i][j] = NewPixels[(Height - i - 1) * Width + j];
                }
            }
            Pixels = colorArray;
            PixelIndexes = null;
        }
        public void SetAllIndexes(byte[] NewIndexes, ushort Height, ushort Width)
        {
            byte[][] indexArray = new byte[Height][];
            for (int i = 0; i < Height; i++)
            {
                indexArray[i] = new byte[Width];
                for (int j = 0; j < Width; j++)
                {
                    indexArray[i][j] = NewIndexes[(Height - i - 1) * Width + j];
                }
            }
            Pixels = null;
            PixelIndexes = indexArray;
        }
        public TgaImageData(BinaryReader reader, TgaHeader Header)
        {
            if (Header.ImageFormat.HasFlag(TgaFormat.Indexed))
                PixelIndexes = new byte[Header.Height][];
            else
                Pixels = new Color[Header.Height][];

            if ((int)Header.ImageFormat >= 0x19)
                throw new NotImplementedException("Huffman Delta run-length encoded data is not supported");

            if (((int)Header.ImageFormat & 0x8) == 0x8)
            {
                for (int y = 0; y < Header.Height; y++)
                {
                    if (Header.ImageFormat.HasFlag(TgaFormat.Indexed))
                        PixelIndexes[y] = new byte[Header.Width];
                    else
                        Pixels[y] = new Color[Header.Width];
                    for (int x = 0; x < Header.Width;)
                    {
                        byte header = reader.ReadByte();
                        if (header < 128)
                        {
                            header++;
                            for (int i = 0; i < header; i++)
                            {
                                if (Header.BitsPerPixel == 32)
                                {
                                    byte B = reader.ReadByte();
                                    byte G = reader.ReadByte();
                                    byte R = reader.ReadByte();
                                    byte A = reader.ReadByte();

                                    Pixels[y][x++] = Color.FromArgb(A, R, G, B);
                                }
                                else if (Header.BitsPerPixel == 24)
                                {
                                    byte B = reader.ReadByte();
                                    byte G = reader.ReadByte();
                                    byte R = reader.ReadByte();

                                    Pixels[y][x++] = Color.FromArgb(R, G, B);
                                }
                                else if (Header.BitsPerPixel == 16)
                                {
                                    ushort pixel = reader.ReadUInt16();
                                    byte B = (byte)(((pixel & 0x001F) << 3) | ((pixel & 0x001F) >> 2));
                                    byte G = (byte)(((pixel & 0x03E0) >> 2) | ((pixel & 0x03E0) >> 7));
                                    byte R = (byte)(((pixel & 0x7C00) >> 7) | ((pixel & 0x7C00) >> 12));
                                    byte A = (byte)(pixel & 0x8000);

                                    if (A != 0)
                                    {
                                        A = 255;
                                    }

                                    Pixels[y][x++] = Color.FromArgb(A, R, G, B);
                                }
                                else if (Header.BitsPerPixel == 15)
                                {
                                    ushort pixel = reader.ReadUInt16();
                                    byte B = (byte)(((pixel & 0x001F) << 3) | ((pixel & 0x001F) >> 2));
                                    byte G = (byte)(((pixel & 0x03E0) >> 2) | ((pixel & 0x03E0) >> 7));
                                    byte R = (byte)(((pixel & 0x7C00) >> 7) | ((pixel & 0x7C00) >> 12));

                                    Pixels[y][x++] = Color.FromArgb(R, G, B);
                                }
                                else if (Header.ImageFormat.HasFlag(TgaFormat.GrayScale))
                                {
                                    byte gray = reader.ReadByte();
                                    Pixels[y][x++] = Color.FromArgb(gray);
                                }
                                else if (Header.BitsPerPixel == 8)
                                {
                                    byte index = reader.ReadByte();
                                    PixelIndexes[y][x++] = index;
                                }
                            }
                        }
                        else
                        {
                            header -= 127;

                            if (Header.BitsPerPixel == 32)
                            {
                                byte B = reader.ReadByte();
                                byte G = reader.ReadByte();
                                byte R = reader.ReadByte();
                                byte A = reader.ReadByte();

                                Color color = Color.FromArgb(A, R, G, B);

                                for (int i = 0; i < header; i++)
                                {
                                    Pixels[y][x++] = color;
                                }
                            }
                            else if (Header.BitsPerPixel == 24)
                            {
                                byte B = reader.ReadByte();
                                byte G = reader.ReadByte();
                                byte R = reader.ReadByte();

                                Color color = Color.FromArgb(R, G, B);

                                for (int i = 0; i < header; i++)
                                {
                                    Pixels[y][x++] = color;
                                }
                            }
                            else if (Header.BitsPerPixel == 16)
                            {
                                ushort pixel = reader.ReadUInt16();
                                byte B = (byte)(((pixel & 0x001F) << 3) | ((pixel & 0x001F) >> 2));
                                byte G = (byte)(((pixel & 0x03E0) >> 2) | ((pixel & 0x03E0) >> 7));
                                byte R = (byte)(((pixel & 0x7C00) >> 7) | ((pixel & 0x7C00) >> 12));
                                byte A = (pixel & 0x8000) != 0 ? (byte)255 : (byte)0;

                                Color color = Color.FromArgb(A, R, G, B);

                                for (int i = 0; i < header; i++)
                                {
                                    Pixels[y][x++] = color;
                                }
                            }
                            else if (Header.BitsPerPixel == 15)
                            {
                                ushort pixel = reader.ReadUInt16();
                                byte B = (byte)(((pixel & 0x001F) << 3) | ((pixel & 0x001F) >> 2));
                                byte G = (byte)(((pixel & 0x03E0) >> 2) | ((pixel & 0x03E0) >> 7));
                                byte R = (byte)(((pixel & 0x7C00) >> 7) | ((pixel & 0x7C00) >> 12));

                                Color color = Color.FromArgb(R, G, B);

                                for (int i = 0; i < header; i++)
                                {
                                    Pixels[y][x++] = color;
                                }
                            }
                            else if (Header.ImageFormat.HasFlag(TgaFormat.GrayScale))
                            {
                                byte gray = reader.ReadByte();
                                Color color = Color.FromArgb(gray, gray, gray);
                                for (int i = 0; i < header; i++)
                                {
                                    Pixels[y][x++] = color;
                                }
                            }
                            else if (Header.BitsPerPixel == 8)
                            {
                                byte index = reader.ReadByte();

                                for (int i = 0; i < header; i++)
                                {
                                    PixelIndexes[y][x++] = index;
                                }
                            }
                        }
                    }
                }
                
            }
            else
            {
                switch(Header.BitsPerPixel)
                {
                    case 8:
                        for (int y = 0; y < Header.Height; y++)
                        {
                            if (Header.ImageFormat.HasFlag(TgaFormat.GrayScale))
                                Pixels[y] = new Color[Header.Width];
                            else
                                PixelIndexes[y] = new byte[Header.Width];

                            for (int x = 0; x < Header.Width; x++)
                            {
                                byte pixel = reader.ReadByte();
                                if (Header.ImageFormat.HasFlag(TgaFormat.GrayScale))
                                    Pixels[y][x] = Color.FromArgb(pixel, pixel, pixel);
                                else
                                    PixelIndexes[y][x] = pixel;
                            }
                        }
                        break;
                    case 15:
                        for (int y = 0; y < Header.Height; y++)
                        {
                            Pixels[y] = new Color[Header.Width];
                            for (int x = 0; x < Header.Width; x++)
                            { 
                                ushort pixel = reader.ReadUInt16();
                                int R = (pixel & 0x7C00) >> 10;
                                int G = (pixel & 0x03E0) >> 5;
                                int B = (pixel & 0x001F);
                                R <<= 3;
                                G <<= 3;
                                B <<= 3;
                                Pixels[y][x] = Color.FromArgb(R, G, B);
                            }
                        }
                        break;
                    case 16:
                        for (int y = 0; y < Header.Height; y++)
                        {
                            Pixels[y] = new Color[Header.Width];
                            for (int x = 0; x < Header.Width; x++)
                            {
                                ushort pixel = reader.ReadUInt16();
                                int R = (pixel & 0x7C00) >> 10;
                                int G = (pixel & 0x03E0) >> 5;
                                int B = (pixel & 0x001F);
                                int A = (pixel & 0x8000) != 0 ? 255 : 0;
                                R <<= 3;
                                G <<= 3;
                                B <<= 3;
                                Pixels[y][x] = Color.FromArgb(A, R, G, B);
                            }
                        }
                        break;
                    case 24:
                        for (int y = 0; y < Header.Height; y++)
                        {
                            Pixels[y] = new Color[Header.Width];
                            for (int x = 0; x < Header.Width; x++)
                            {
                                byte B = reader.ReadByte();
                                byte G = reader.ReadByte();
                                byte R = reader.ReadByte();
                                Pixels[y][x] = Color.FromArgb(R, G, B);
                            }
                        }
                        break;
                    case 32:
                        for (int y = 0; y < Header.Height; y++)
                        {
                            Pixels[y] = new Color[Header.Width];
                            for (int x = 0; x < Header.Width; x++)
                            {
                                byte B = reader.ReadByte();
                                byte G = reader.ReadByte();
                                byte R = reader.ReadByte();
                                byte A = reader.ReadByte();
                                Pixels[y][x] = Color.FromArgb(A,R, G, B);
                            }
                        }
                        break;
                }
            }
        }
        public Color[] AllPixels(bool flipHorizontal = false, bool flipVertical = false)
        {
            if (Pixels == null)
                throw new Exception("Image is indexed!");
            int height = Pixels.Length;
            int width = Pixels[0].Length;
            Color[] allPixels = new Color[height * width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = flipVertical ? (i * width + j) : ((height - i - 1) * width + j);
                    index = flipHorizontal ? ((i + 1) * width - j - 1) : index;
                    allPixels[index] = Pixels[i][j];
                }
            }
            return allPixels;
        }
        public byte[] AllIndexes(bool flipHorizontal = false, bool flipVertical = false)
        {
            if (PixelIndexes == null)
                throw new Exception("Image is not indexed!");
            int height = PixelIndexes.Length;
            int width = PixelIndexes[0].Length;
            byte[] allIndexes = new byte[height * width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = flipVertical ? (i * width + j) : ((height - i - 1) * width + j);
                    index = flipHorizontal ? ((i + 1) * width - j - 1) : index;
                    allIndexes[index] = PixelIndexes[i][j];
                }
            }
            return allIndexes;
        }
        public void Write(BinaryWriter writer, TgaHeader Header)
        {
            if ((int)Header.ImageFormat >= 0x19)
                throw new NotImplementedException("Huffman Delta run-length encoded data is not supported");
            else if ((int)Header.ImageFormat >= 0x08)
                throw new NotImplementedException("Run-length encoded exporting is not supported");

            for (int y = 0; y < Header.Height; y++)
            {
                for (int x = 0; x < Header.Width; x++)
                {
                    switch (Header.BitsPerPixel)
                    {
                        case 8:
                            {
                                if (Header.ImageFormat.HasFlag(TgaFormat.GrayScale))
                                {
                                    byte gray = (byte)Math.Round((Pixels[y][x].R + Pixels[y][x].G + Pixels[y][x].B) / 3.0);
                                    writer.Write(gray);
                                }
                                else
                                {
                                    writer.Write(PixelIndexes[y][x]);
                                }
                                break;
                            }
                        case 15:
                            {
                                int R = Pixels[y][x].R >> 3;
                                int G = Pixels[y][x].G >> 3;
                                int B = Pixels[y][x].B >> 3;
                                ushort pixel = (ushort)((R << 10) | (G << 5) | B);
                                writer.Write(pixel);
                                break;
                            }
                        case 16:
                            {
                                int A = Pixels[y][x].A >= 128 ? 0x8000 : 0;
                                int R = Pixels[y][x].R >> 3;
                                int G = Pixels[y][x].G >> 3;
                                int B = Pixels[y][x].B >> 3;
                                ushort pixel = (ushort)(A | (R << 10) | (G << 5) | B);
                                writer.Write(pixel);
                                break;
                            }
                        case 24:
                            {
                                writer.Write(Pixels[y][x].B);
                                writer.Write(Pixels[y][x].G);
                                writer.Write(Pixels[y][x].R);
                                break;
                            }
                        case 32:
                            {
                                writer.Write(Pixels[y][x].B);
                                writer.Write(Pixels[y][x].G);
                                writer.Write(Pixels[y][x].R);
                                writer.Write(Pixels[y][x].A);
                                break;
                            }
                    }
                }
            }
        }
    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
}
