using System.Drawing;

namespace DDSLib
{
    public static partial class DXT
    {
        public static Color[] ReadBC4Data(BinaryReader reader, uint width, uint height)
        {
            uint blockCountX = (width + 3) / 4;
            uint blockCountY = (height + 3) / 4;

            Color[] colors = new Color[width * height];

            for (int blockY = 0; blockY < blockCountY; blockY++)
            {
                for (int blockX = 0; blockX < blockCountX; blockX++)
                {
                    byte[] blockData = reader.ReadBytes(8);
                    DecompressBC4Block(blockData, blockX * 4, blockY * 4, width, height, colors);
                }
            }

            return colors;
        }
        public static void DecompressBC4Block(byte[] blockData, int startX, int startY, uint width, uint height, Color[] colors, int channel = 0)
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
                {
                    if (channel == 4)
                        colors[y * width + x] = Color.FromArgb(alphaValue, colors[y * width + x]);
                    else if (channel == 3)
                        colors[y * width + x] = Color.FromArgb(colors[y * width + x].R, colors[y * width + x].G, alphaValue);
                    else if (channel == 2)
                        colors[y * width + x] = Color.FromArgb(colors[y * width + x].R, alphaValue, colors[y * width + x].B);
                    else if (channel == 1)
                        colors[y * width + x] = Color.FromArgb(alphaValue, colors[y * width + x].G, colors[y * width + x].B);
                    else
                        colors[y * width + x] = Color.FromArgb(alphaValue, alphaValue, alphaValue);
                }
            }
        }
    }
}
