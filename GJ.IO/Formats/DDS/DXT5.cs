using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDSLib
{
    public static partial class DXT
    {
        public static Color[] ReadSonyDXT5Data(BinaryReader reader, int width, int height, int pitch)
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
                            ushort c0 = reader.ReadUInt16();
                            ushort c1 = reader.ReadUInt16();
                            ushort c2 = reader.ReadUInt16();
                            ushort c3 = reader.ReadUInt16();
                            ushort c4 = reader.ReadUInt16();
                            ushort c5 = reader.ReadUInt16();
                            ushort c6 = reader.ReadUInt16();
                            ushort c7 = reader.ReadUInt16();
                            writer.Write(c7);
                            writer.Write(c4);
                            writer.Write(c5);
                            writer.Write(c6);
                            writer.Write(c2);
                            writer.Write(c3);
                            writer.Write(c0);
                            writer.Write(c1);
                        }
                        reader.BaseStream.Position = yStart + pitch;
                    }
                }

                using (BinaryReader reader2 = new BinaryReader(new MemoryStream(memoryStream.ToArray())))
                {
                    return ReadDXT5Data(reader2, (uint)width, (uint)height);
                }
            }
        }
        public static Color[] ReadDXT5Data(BinaryReader reader, uint width, uint height)
        {
            uint blockCountX = (width + 3) / 4;
            uint blockCountY = (height + 3) / 4;

            Color[] colors = new Color[width * height];

            for (int blockY = 0; blockY < blockCountY; blockY++)
            {
                for (int blockX = 0; blockX < blockCountX; blockX++)
                {
                    byte[] alphaData = reader.ReadBytes(8);
                    byte[] blockData = reader.ReadBytes(8);

                    DecompressDXT1Block(blockData, blockX * 4, blockY * 4, width, height, colors);
                    DecompressDXT5Alpha(alphaData, blockX * 4, blockY * 4, width, height, colors);
                }
            }

            return colors;
        }
        private static void DecompressDXT5Alpha(byte[] blockData, int startX, int startY, uint width, uint height, Color[] colors)
        {
            byte alpha0 = blockData[0];
            byte alpha1 = blockData[1];

            byte[] alphaValues = new byte[8];
            alphaValues[0] = alpha0;
            alphaValues[1] = alpha1;

            if (alpha0 > alpha1)
                for (int i = 1; i < 7; i++)
                    alphaValues[i + 1] = (byte)(((7 - i) * alpha0 + i * alpha1) / 7);
            else
            { 
                for (int i = 1; i < 5; i++)
                    alphaValues[i + 1] = (byte)(((5 - i) * alpha0 + i * alpha1) / 5);

                alphaValues[6] = 0;
                alphaValues[7] = 255;
            }

            ulong alphaBits = BitConverter.ToUInt64(blockData.Skip(2).Concat(new byte[2]).ToArray(), 0);

            for (int alphaIndex = 0; alphaIndex < 16; alphaIndex++)
            {
                int alphaCodeIndex = 3 * alphaIndex;
                int alphaValueIndex = (int)((alphaBits >> alphaCodeIndex) & 0x07);

                byte alphaValue = alphaValues[alphaValueIndex];

                int x = startX + alphaIndex % 4;
                int y = startY + alphaIndex / 4;

                if (x < width && y < height)
                    colors[y * width + x] = Color.FromArgb(alphaValue, colors[y * width + x]);
            }
        }
    }
}
