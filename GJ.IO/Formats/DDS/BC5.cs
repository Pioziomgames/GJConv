using System.Drawing;

namespace DDSLib
{
    public static partial class DXT
    {
        public static Color[] ReadBC5Data(BinaryReader reader, uint width, uint height)
        {
            uint blockCountX = (width + 3) / 4;
            uint blockCountY = (height + 3) / 4;

            Color[] colors = new Color[width * height];

            for (int blockY = 0; blockY < blockCountY; blockY++)
            {
                for (int blockX = 0; blockX < blockCountX; blockX++)
                {
                    byte[] rData = reader.ReadBytes(8);
                    byte[] gData = reader.ReadBytes(8);
                    DecompressBC4Block(rData, blockX * 4, blockY * 4, width, height, colors, 1);
                    DecompressBC4Block(gData, blockX * 4, blockY * 4, width, height, colors, 2);
                }
            }

            return colors;
        }
    }
}
