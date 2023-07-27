using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TxnLib.RmdEnums;

namespace TxnLib
{
    public static class RmdFunctions
    {
        public static Color[] TilePalette(Color[] Palette)
        {
            if (Palette.Length > 16 && Palette.Length < 256)
            {
                Color[] Pal = new Color[256];
                for (int i = 0; i < Palette.Length; i++)
                    Pal[i] = Palette[i];
                for (int i = 0; i < Pal.Length - Palette.Length; i++)
                    Pal[i] = Palette[^1];
                Palette = Pal;
            }
            else if (Palette.Length < 16)
            {
                Color[] Pal = new Color[16];
                for (int i = 0; i < Palette.Length; i++)
                    Pal[i] = Palette[i];
                for (int i = 0; i < Pal.Length - Palette.Length; i++)
                    Pal[i] = Color.FromArgb(0);
                Palette = Pal;
            }

            Color[] newPalette = new Color[Palette.Length];
            int newIndex = 0;
            int oldIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int x = 0; x < 8; x++)
                {
                    newPalette[newIndex++] = Palette[oldIndex++];
                }
                oldIndex += 8;
                for (int x = 0; x < 8; x++)
                {
                    newPalette[newIndex++] = Palette[oldIndex++];
                }
                oldIndex -= 16;
                for (int x = 0; x < 8; x++)
                {
                    newPalette[newIndex++] = Palette[oldIndex++];
                }
                oldIndex += 8;
                for (int x = 0; x < 8; x++)
                {
                    newPalette[newIndex++] = Palette[oldIndex++];
                }
            }
            return newPalette;
        }
        public static byte[] Unswizzle4Bpp(BinaryReader reader, int Width, int Height)
        {
            byte[] imageData = reader.ReadBytes(Width * Height / 2);

            byte[] unswizzledData = new byte[Width * Height];

            for (int by = 0; by < Height / 2; by++)
            {
                for (int bx = 0; bx < Width / 16; bx++)
                {
                    int blockY = (by / 2) * 4 + (by % 2);
                    int blockX = bx * 16;
                    int blockI = bx * 32 + by * Math.Max(Width, 32) * 2;

                    for (int j = 0; j < 8; j++)
                    {
                        int x = (blockX + (4 + j) % 8) ^ ((by & 2) << 1);
                        int x2 = (blockX + 8 + (4 + j) % 8) ^ ((by & 2) << 1);
                        int y = blockY;
                        int y2 = blockY + 2;
                        int i1 = blockI + (j * 4) % 32;
                        int i2 = blockI + (j * 4 + 17) % 32;
                        int i3 = blockI + (j * 4 + 2) % 32;
                        int i4 = blockI + (j * 4 + 19) % 32;

                        unswizzledData[y2 * Width + x] = (byte)ReadNibble(imageData, i1);
                        unswizzledData[y * Width + x] = (byte)ReadNibble(imageData, i2);
                        unswizzledData[y2 * Width + x2] = (byte)ReadNibble(imageData, i3);
                        unswizzledData[y * Width + x2] = (byte)ReadNibble(imageData, i4);
                    }
                }
            }

            return unswizzledData;
        }
        public static byte[] Unswizzle8Bpp(BinaryReader reader, int width, int height)
        {
            byte[] IndicesArray = reader.ReadBytes(width * height);
            byte[] newIndicesArray = new byte[IndicesArray.Length];

            for (int posY = 0; posY < height; posY++)
            {
                for (int posX = 0; posX < width; posX++)
                {
                    int blockLocation = (posY & (~0xF)) * width + (posX & (~0xF)) * 2;
                    int swapSelector = (((posY + 2) >> 2) & 0x1) * 4;
                    int positionY = (((posY & (~3)) >> 1) + (posY & 1)) & 0x7;
                    int columnLocation = positionY * width * 2 + ((posX + swapSelector) & 0x7) * 4;
                    int byteNumber = ((posY >> 1) & 1) + ((posX >> 2) & 2);

                    newIndicesArray[posY * width + posX] = IndicesArray[blockLocation + columnLocation + byteNumber];
                }
            }
            return newIndicesArray;
        }
        public static void Swizzle4Bpp(BinaryWriter writer, byte[] unswizzledData, int Width, int Height)
        {
            for (int by = 0; by < Height / 4; by++)
            {
                for (int bx = 0; bx < Width / 16; bx++)
                {
                    int blockY = (by / 2) * 4 + (by % 2);
                    int blockX = bx * 16;

                    for (int j = 0; j < 8; j++)
                    {
                        int x = (blockX + (4 + j) % 8) ^ ((by & 2) << 1);
                        int x2 = (blockX + 8 + (4 + j) % 8) ^ ((by & 2) << 1);
                        int y = blockY + 0;
                        int y2 = blockY + 2;
                        byte dataByte = (byte)((unswizzledData[y2 * Width + x] & 0x0F) | ((unswizzledData[y * Width + x] & 0x0F) << 4));
                        writer.Write(dataByte);

                        dataByte = (byte)((unswizzledData[y2 * Width + x2] & 0x0F) | ((unswizzledData[y * Width + x2] & 0x0F) << 4));
                        writer.Write(dataByte);
                    }
                }
            }
        }
        public static void Swizzle8Bpp(BinaryWriter writer, byte[] indicesArray, int width, int height)
        {
            byte[] newIndicesArray = new byte[indicesArray.Length];

            for (int posY = 0; posY < height; posY++)
            {
                for (int posX = 0; posX < width; posX++)
                {
                    byte pixelValue = indicesArray[(posY * width + posX)];

                    int blockLocation = (posY & (~0xF)) * width + (posX & (~0xF)) * 2;
                    int swapSelector = (((posY + 2) >> 2) & 0x1) * 4;
                    int positionY = (((posY & (~3)) >> 1) + (posY & 1)) & 0x7;
                    int columnLocation = positionY * width * 2 + ((posX + swapSelector) & 0x7) * 4;

                    int byteNumber = ((posY >> 1) & 1) + ((posX >> 2) & 2);

                    newIndicesArray[blockLocation + columnLocation + byteNumber] = pixelValue;
                }
            }

            writer.Write(newIndicesArray);
        }
        public static byte[] Swizzle(byte[] indicesArray, int width, int height)
        {
            byte[] newIndicesArray = new byte[indicesArray.Length];

            for (int posY = 0; posY < height; posY++)
            {
                for (int posX = 0; posX < width; posX++)
                {
                    byte pixelValue = indicesArray[(posY * width + posX)];

                    int blockLocation = (posY & (~0xF)) * width + (posX & (~0xF)) * 2;
                    int swapSelector = (((posY + 2) >> 2) & 0x1) * 4;
                    int positionY = (((posY & (~3)) >> 1) + (posY & 1)) & 0x7;
                    int columnLocation = positionY * width * 2 + ((posX + swapSelector) & 0x7) * 4;

                    int byteNumber = ((posY >> 1) & 1) + ((posX >> 2) & 2);

                    newIndicesArray[blockLocation + columnLocation + byteNumber] = pixelValue;
                }
            }
            return newIndicesArray;
        }
        public static byte[] Unswizzle(byte[] IndicesArray, int width, int height)
        {
            byte[] newIndicesArray = new byte[IndicesArray.Length];

            for (int posY = 0; posY < height; posY++)
            {
                for (int posX = 0; posX < width; posX++)
                {
                    int blockLocation = (posY & (~0xF)) * width + (posX & (~0xF)) * 2;
                    int swapSelector = (((posY + 2) >> 2) & 0x1) * 4;
                    int positionY = (((posY & (~3)) >> 1) + (posY & 1)) & 0x7;
                    int columnLocation = positionY * width * 2 + ((posX + swapSelector) & 0x7) * 4;
                    int byteNumber = ((posY >> 1) & 1) + ((posX >> 2) & 2);

                    newIndicesArray[posY * width + posX] = IndicesArray[blockLocation + columnLocation + byteNumber];
                }
            }
            return newIndicesArray;
        }
        private static int ReadNibble(byte[] data, int index)
        {
            int byteIndex = index / 2;
            bool bitShift = (index % 2) == 0;

            return (data[byteIndex] >> (bitShift ? 4 : 0)) & 0xF;
        }
        public static string ReadRwString(this BinaryReader reader)
        {
            RwString reString = (RwString)RmdChunk.Read(reader);
            return reString.Value;
        }
        public static void WriteRwString(this BinaryWriter writer, string Inputs)
        {
            RwString wrString = new(Inputs);
            RmdChunk.Write(wrString, writer);
        }
        public static int GetDepth(PS2PixelFormat format)
        {
            return format switch
            {
                PS2PixelFormat.PSMTC32 or PS2PixelFormat.PSMTC24 or PS2PixelFormat.PSMZ32 or PS2PixelFormat.PSMZ24 => 32,
                PS2PixelFormat.PSMTC16 or PS2PixelFormat.PSMTC16S or PS2PixelFormat.PSMZ16 or PS2PixelFormat.PSMZ16S => 16,
                PS2PixelFormat.PSMT8 or PS2PixelFormat.PSMT8H => 8,
                PS2PixelFormat.PSMT4 or PS2PixelFormat.PSMT4HL or PS2PixelFormat.PSMT4HH => 4,
                _ => throw new Exception($"Unknown PS2PixelFormat: {format}"),
            };
        }
        public static RwRasterFormat GetRasterFormat(int depth)
        {
            RwRasterFormat rasterFormat = RwRasterFormat.Format8888 | RwRasterFormat.LockPixels | RwRasterFormat.HasHeaders;

            if (depth == 8)
            {
                rasterFormat |= RwRasterFormat.Pal8;
                //rasterFormat |= RwRasterFormat.Swizzled;
            }
            else if (depth == 4)
            {
                rasterFormat |= RwRasterFormat.Pal4;
                //rasterFormat |= RwRasterFormat.Swizzled;
            }

            return rasterFormat;
        }
        public static ulong GetBits(ulong value, int dataSize, int dataIndex)
        {
            ulong BitsToReturn = ulong.MaxValue >> (64 - dataSize);
            return (value & (BitsToReturn << dataIndex)) >> dataIndex;
        }
        public static void SetBits(ref ulong value, ulong valueToSet, int dataSize, int dataIndex)
        {
            ulong BitsToSet = ulong.MaxValue >> (64 - dataSize);
            value &= ~(BitsToSet << dataIndex);
            value |= (valueToSet & BitsToSet) << dataIndex;
        }
    }
}
