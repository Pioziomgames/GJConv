using System.Drawing;

namespace DDSLib
{
    public static partial class DXT
    {
        public static Color[] ReadSonyDXT3Data(BinaryReader reader, int width, int height, int pitch)
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
                            int c2 = reader.ReadInt32();
                            int c3 = reader.ReadInt32();
                            writer.Write(c2);
                            writer.Write(c3);
                            writer.Write(c1);
                            writer.Write(c0);
                        }
                        reader.BaseStream.Position = yStart + pitch;
                    }
                }

                using (BinaryReader reader2 = new BinaryReader(new MemoryStream(memoryStream.ToArray())))
                {
                    return ReadDXT3Data(reader2, (uint)width, (uint)height);
                }
            }
        }
        public static Color[] ReadDXT3Data(BinaryReader reader, uint width, uint height)
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
                    DecompressDXT3Alpha(alphaData, blockX * 4, blockY * 4, width, height, colors);
                }
            }

            return colors;
        }
        private static void DecompressDXT3Alpha(byte[] blockData, int startX, int startY, uint width, uint height, Color[] colors)
        {
            for (int i = 0; i < 16; i++)
            {
                byte alphaValue = (byte)((blockData[i / 2] >> (i % 2) * 4) & 0x0F);
                byte scaledAlpha = (byte)(alphaValue * 17);

                int x = startX + i % 4;
                int y = startY + i / 4;

                if (x < width && y < height)
                    colors[y * width + x] = Color.FromArgb(scaledAlpha, colors[y * width + x]);
            }
        }
    }
}
