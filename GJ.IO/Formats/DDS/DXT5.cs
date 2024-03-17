using System.Drawing;

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
                    DecompressBC4Block(alphaData, blockX * 4, blockY * 4, width, height, colors, 4);
                }
            }

            return colors;
        }
    }
}
