using System;
using System.Drawing;

namespace DDSLib
{
    public static partial class DXT
    {
        public static Color[] ReadSonyDXT1Data(BinaryReader reader, int width, int height, int pitch)
        {
            pitch *= 4;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    for (int y = 0; y < height; y += 4)
                    {
                        long yStart = reader.BaseStream.Position;
                        for (int x = 0; x < width; x += 4)
                        {
                            int c0 = reader.ReadInt32();
                            int c1 = reader.ReadInt32();
                            writer.Write(c1);
                            writer.Write(c0);
                        }
                        reader.BaseStream.Position = yStart + pitch;
                    }
                }

                using (BinaryReader reader2 = new BinaryReader(new MemoryStream(memoryStream.ToArray())))
                {
                    return ReadDXT1Data(reader2, (uint)width, (uint)height);
                }
            }
        }
        public static Color[] ReadDXT1Data(BinaryReader reader, uint width, uint height)
        {
            uint blockCountX = (width + 3) / 4;
            uint blockCountY = (height + 3) / 4;

            Color[] colors = new Color[width * height];

            for (int blockY = 0; blockY < blockCountY; blockY++)
            {
                for (int blockX = 0; blockX < blockCountX; blockX++)
                {
                    byte[] blockData = reader.ReadBytes(8);

                    DecompressDXT1Block(blockData, blockX * 4, blockY * 4, width, height, colors);
                }
            }

            return colors;
        }
        public static void DecompressDXT1Block(byte[] blockData, int startX, int startY, uint width, uint height, Color[] colors)
        {
            ushort color0 = BitConverter.ToUInt16(blockData, 0);
            ushort color1 = BitConverter.ToUInt16(blockData, 2);

            Color[] colorPalette = DecodeColorPalette(color0, color1);

            uint indices = BitConverter.ToUInt32(blockData, 4);

            for (int y = 0; y < 4 && (startY + y) < height; y++)
            {
                for (int x = 0; x < 4 && (startX + x) < width; x++)
                {
                    int index = (int)((indices >> (2 * (y * 4 + x))) & 0x03);

                    colors[(startY + y) * width + (startX + x)] = colorPalette[index];
                }
            }
        }
        public static Color[] DecodeColorPalette(ushort color0, ushort color1)
        {
            Color[] palette = new Color[4];

            byte r0 = (byte)((color0 & 0xF800) >> 8);
            byte g0 = (byte)((color0 & 0x07E0) >> 3);
            byte b0 = (byte)((color0 & 0x001F) << 3);

            byte r1 = (byte)((color1 & 0xF800) >> 8);
            byte g1 = (byte)((color1 & 0x07E0) >> 3);
            byte b1 = (byte)((color1 & 0x001F) << 3);

            palette[0] = Color.FromArgb(255, r0, g0, b0);
            palette[1] = Color.FromArgb(255, r1, g1, b1);
            palette[2] = Color.FromArgb(255, (byte)((2 * r0 + r1) / 3),
                (byte)((2 * g0 + g1) / 3), (byte)((2 * b0 + b1) / 3));

            if (color0 > color1)
                palette[3] = Color.FromArgb(255, (byte)((r0 + 2 * r1) / 3), (byte)((g0 + 2 * g1) / 3), (byte)((b0 + 2 * b1) / 3));
            else
                palette[3] = Color.FromArgb(0);
            return palette;
        }
        public static void WriteDXT1Data(BinaryWriter writer, Color[] pixels, int width, int height)
        {
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    Color[] Block = CreateBlock(pixels, width, height, x, y);
                    writer.Write(CompressDXT1Block(Block));
                }
            }
        }
        static Color[] CreateBlock(Color[] texturePixels, int textureWidth, int textureHeight, int startX, int startY)
        {
            Color[] block = new Color[4 * 4];
            int blockIndex = 0;
            for (int offsetY = 0; offsetY < 4; offsetY++)
            {
                for (int offsetX = 0; offsetX < 4; offsetX++)
                {
                    int pixelX = startX + offsetX;
                    int pixelY = startY + offsetY;

                    if (pixelX < textureWidth && pixelY < textureHeight)
                       block[blockIndex++] = texturePixels[pixelY * textureWidth + pixelX];
                }
            }
            return block;
        }
        public static byte[] CompressDXT1Block(Color[] colors)
        {
            ushort[] rgb565Colors = new ushort[4];

            for (int i = 0; i < 4; i++)
            {
                Color color = colors[i];
                int r = color.R;
                int g = color.G;
                int b = color.B;

                rgb565Colors[i] = (ushort)(((r >> 3) << 11) | ((g >> 2) << 5) | (b >> 3));
            }

            ushort color0 = rgb565Colors[0];
            ushort color1 = rgb565Colors[1];

            byte[] block = new byte[8];
            block[0] = (byte)(color0 & 0xFF);
            block[1] = (byte)((color0 >> 8) & 0xFF);
            block[2] = (byte)(color1 & 0xFF);
            block[3] = (byte)((color1 >> 8) & 0xFF);

            ushort indices = 0;
            for (int i = 0; i < 16; i++)
            {
                Color color = colors[i];
                int r = color.R;
                int g = color.G;
                int b = color.B;

                ushort rgb565 = (ushort)(((r >> 3) << 11) | ((g >> 2) << 5) | (b >> 3));

                // Use a more accurate method to find the closest match
                int distance0 = Math.Abs(color0 - rgb565);
                int distance1 = Math.Abs(color1 - rgb565);

                if (distance0 < distance1)
                {
                    indices |= (ushort)(0 << (i * 2));
                }
                else
                {
                    indices |= (ushort)(1 << (i * 2));
                }
            }

            block[4] = (byte)(indices & 0xFF);
            block[5] = (byte)((indices >> 8) & 0xFF);

            return block;
        }
    }
}
