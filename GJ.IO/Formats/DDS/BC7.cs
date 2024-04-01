using System.Drawing;

namespace DDSLib
{
    public static partial class DXT
    {
        public static Color[] ReadBC7Data(BinaryReader reader, uint width, uint height)
        {
            uint blockCountX = (width + 3) / 4;
            uint blockCountY = (height + 3) / 4;

            Color[] colors = new Color[width * height];
            int i = 0;
            for (int blockY = 0; blockY < blockCountY; blockY++)
            {
                for (int blockX = 0; blockX < blockCountX; blockX++)
                {
                    byte[] blockData = reader.ReadBytes(16);

                    DecompressBC7Block(blockData, blockX * 4, blockY * 4, width, height, colors);

                    i++;
                }
            }

            return colors;
        }
        public static int ReadBits(byte[] bytes, int offset, int bitCount)
        {
            int byteIndex = offset / 8;
            int bitOffset = offset % 8;
            int result = 0;
            int bitsLeft = bitCount;

            while (bitsLeft > 0)
            {
                int bitsToRead = Math.Min(bitsLeft, 8 - bitOffset);
                int mask = (1 << bitsToRead) - 1;
                int value = bytes[byteIndex] >> bitOffset & mask;
                result |= value << (bitCount - bitsLeft);
                bitsLeft -= bitsToRead;
                bitOffset = 0;
                byteIndex++;
            }

            return result;
        }
        static int[] aWeight2 = new int[] { 0, 21, 43, 64 };
        static int[] aWeight3 = new int[] { 0, 9, 18, 27, 37, 46, 55, 64 };
        static int[] aWeight4 = new int[] { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };

        static readonly int[][] BC7Subsets2PartitionTable = {
            new[] {0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
            new[] {0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1},
            new[] {0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1},
            new[] {0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1},
            new[] {0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1},
            new[] {0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1},
            new[] {0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1},
            new[] {0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1},
            new[] {0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1},
            new[] {0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1},
            new[] {0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0},
            new[] {0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0},
            new[] {0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0},
            new[] {0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1},
            new[] {0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0},
            new[] {0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0},
            new[] {0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0},
            new[] {0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0},
            new[] {0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0},
            new[] {0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0},
            new[] {0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0},
            new[] {0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0},
            new[] {0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1},
            new[] {0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1},
            new[] {0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0},
            new[] {0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0},
            new[] {0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0},
            new[] {0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0},
            new[] {0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1},
            new[] {0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1},
            new[] {0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0},
            new[] {0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0},
            new[] {0, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0},
            new[] {0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0},
            new[] {0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0},
            new[] {0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1},
            new[] {0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1},
            new[] {0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0},
            new[] {0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0},
            new[] {0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0},
            new[] {0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0},
            new[] {0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0},
            new[] {0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1},
            new[] {0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1},
            new[] {0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0},
            new[] {0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0},
            new[] {0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 1},
            new[] {0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1},
            new[] {0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new[] {0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1},
            new[] {0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
            new[] {0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0},
            new[] {0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0},
            new[] {0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1}
        };
        static readonly int[][] BC7Subsets3PartitionTable = {
            new[] {0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 1, 2, 2, 2, 2},
            new[] {0, 0, 0, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 2, 1},
            new[] {0, 0, 0, 0, 2, 0, 0, 1, 2, 2, 1, 1, 2, 2, 1, 1},
            new[] {0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 1, 0, 1, 1, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2},
            new[] {0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 2, 2},
            new[] {0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1},
            new[] {0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2},
            new[] {0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2},
            new[] {0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2},
            new[] {0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2},
            new[] {0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2},
            new[] {0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2},
            new[] {0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2, 1, 2, 2, 2},
            new[] {0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0, 2, 2, 2, 0},
            new[] {0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2},
            new[] {0, 1, 1, 1, 0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0},
            new[] {0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2},
            new[] {0, 0, 2, 2, 0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1},
            new[] {0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2},
            new[] {0, 0, 0, 1, 0, 0, 0, 1, 2, 2, 2, 1, 2, 2, 2, 1},
            new[] {0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2},
            new[] {0, 0, 0, 0, 1, 1, 0, 0, 2, 2, 1, 0, 2, 2, 1, 0},
            new[] {0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1, 0, 0, 0, 0},
            new[] {0, 0, 1, 2, 0, 0, 1, 2, 1, 1, 2, 2, 2, 2, 2, 2},
            new[] {0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1, 0, 1, 1, 0},
            new[] {0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1},
            new[] {0, 0, 2, 2, 1, 1, 0, 2, 1, 1, 0, 2, 0, 0, 2, 2},
            new[] {0, 1, 1, 0, 0, 1, 1, 0, 2, 0, 0, 2, 2, 2, 2, 2},
            new[] {0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1},
            new[] {0, 0, 0, 0, 2, 0, 0, 0, 2, 2, 1, 1, 2, 2, 2, 1},
            new[] {0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 2, 2, 2},
            new[] {0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 2, 0, 0, 1, 1},
            new[] {0, 0, 1, 1, 0, 0, 1, 2, 0, 0, 2, 2, 0, 2, 2, 2},
            new[] {0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0},
            new[] {0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0},
            new[] {0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0},
            new[] {0, 1, 2, 0, 2, 0, 1, 2, 1, 2, 0, 1, 0, 1, 2, 0},
            new[] {0, 0, 1, 1, 2, 2, 0, 0, 1, 1, 2, 2, 0, 0, 1, 1},
            new[] {0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0, 1, 1},
            new[] {0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1},
            new[] {0, 0, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2, 1, 1, 2, 2},
            new[] {0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 1, 1},
            new[] {0, 2, 2, 0, 1, 2, 2, 1, 0, 2, 2, 0, 1, 2, 2, 1},
            new[] {0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 0, 1},
            new[] {0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1},
            new[] {0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2},
            new[] {0, 2, 2, 2, 0, 1, 1, 1, 0, 2, 2, 2, 0, 1, 1, 1},
            new[] {0, 0, 0, 2, 1, 1, 1, 2, 0, 0, 0, 2, 1, 1, 1, 2},
            new[] {0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2},
            new[] {0, 2, 2, 2, 0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2},
            new[] {0, 0, 0, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2},
            new[] {0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2},
            new[] {0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2, 2, 2, 2, 2},
            new[] {0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2},
            new[] {0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2},
            new[] {0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 1},
            new[] {0, 2, 2, 2, 1, 2, 2, 2, 0, 2, 2, 2, 1, 2, 2, 2},
            new[] {0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
            new[] {0, 1, 1, 1, 2, 0, 1, 1, 2, 2, 0, 1, 2, 2, 2, 0},
        };
        static readonly int[] BC7Subsets2AnchorIndices = {
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 2, 8, 2, 2, 8, 8, 15,
            2, 8, 2, 2, 8, 8, 2, 2,
            15, 15, 6, 8, 2, 8, 15, 15,
            2, 8, 2, 2, 2, 15, 15, 6,
            6, 2, 6, 8, 15, 15, 2, 2,
            15, 15, 15, 15, 15, 2, 2, 15
        };
        static readonly int[] BC7Subsets3AnchorIndices2 = {
            3, 3, 15, 15, 8, 3, 15, 15,
            8, 8, 6, 6, 6, 5, 3, 3,
            3, 3, 8, 15, 3, 3, 6, 10,
            5, 8, 8, 6, 8, 5, 15, 15,
            8, 15, 3, 5, 6, 10, 8, 15,
            15, 3, 15, 5, 15, 15, 15, 15,
            3, 15, 5, 5, 5, 8, 5, 10,
            5, 10, 8, 13, 15, 12, 3, 3
        };
        static readonly int[] BC7Subsets3AnchorIndices3 = {
            15, 8, 8, 3, 15, 15, 3, 8,
            15, 15, 15, 15, 15, 15, 15, 8,
            15, 8, 15, 3, 15, 8, 15, 8,
            3, 15, 6, 10, 15, 15, 10, 8,
            15, 3, 15, 10, 10, 8, 9, 10,
            6, 15, 8, 15, 3, 6, 6, 8,
            15, 3, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 3, 15, 15, 8
        };
        public static void DecompressBC7Block(byte[] blockData, int startX, int startY, uint width, uint height, Color[] colors)
        {
            byte mode = ExtractMode(blockData[0]);

            if (mode == 8)
                return; //Colors should already be set to 0

            int numSubsets = 1;
            if (mode == 0 || mode == 2)
                numSubsets = 3;
            else if (mode == 1 || mode == 3 || mode == 7)
                numSubsets = 2;
            
            int partitionIndex = 0;
            int rotationBits = 0;
            if (mode != 4 && mode != 5 && mode != 6)
            {
                partitionIndex = ReadBits(blockData, mode+1, mode == 0 ? 4 : 6);
            }
            else if (mode != 6)
                rotationBits = ReadBits(blockData, mode+1, 2);

            int type4IndexMode = 0;
            if (mode == 4)
                type4IndexMode = ReadBits(blockData, 7, 1);

            Color[] endPoints = new Color[numSubsets*2];

            int endPointStart = mode switch
            {
                0 => 5,
                1 => 8,
                2 => 9,
                3 => 10,
                6 => 7,
                7 => 14,
                _ => 8,
            };
            int endPointSize = mode switch
            {
                0 => 4,
                1 => 6,
                2 => 5,
                4 => 5,
                7 => 5,
                _ => 7,
            };
            int nChn = (mode > 3) ? 4 : 3;
            bool bigA = mode == 4 || mode == 5;
            for (int i = 0; i < endPoints.Length; i++)
            {
                int off = endPointStart + endPointSize * i;
                int r = ReadBits(blockData, off, endPointSize);
                int g = ReadBits(blockData, off + (endPointSize * endPoints.Length), endPointSize);
                int b = ReadBits(blockData, off + (endPointSize * endPoints.Length) * 2, endPointSize);

                int a = 255;
                if (bigA)
                    off = endPointStart + (endPointSize + 1) * i;
                if (nChn == 4)
                    a = ReadBits(blockData, off + (endPointSize * endPoints.Length) * 3,
                     endPointSize + (bigA ? 1 : 0));

                endPoints[i] = Color.FromArgb(a, r, g, b);
            }



            if (mode != 2 && mode != 4 && mode != 5)
            {
                for (int i = 0; i < endPoints.Length; i++)
                    endPoints[i] = Color.FromArgb(
                        endPoints[i].A << (mode > 3 ? 1 : 0),
                        endPoints[i].R << 1,
                        endPoints[i].G << 1,
                        endPoints[i].B << 1);
            }

            byte[] pBits = ExtractPBits(blockData, mode);

            if (mode == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    endPoints[i] = Color.FromArgb(
                    endPoints[i].R | pBits[i > 1 ? 1 : 0],
                    endPoints[i].G | pBits[i > 1 ? 1 : 0],
                    endPoints[i].B | pBits[i > 1 ? 1 : 0]);
                }
            }
            else if (pBits.Length > 0)
            {
                for (int i = 0; i < endPoints.Length; i++)
                    endPoints[i] = Color.FromArgb(
                    endPoints[i].A | pBits[i],
                    endPoints[i].R | pBits[i],
                    endPoints[i].G | pBits[i],
                    endPoints[i].B | pBits[i]);
            }

            int cSize = mode switch
            {
                1 => 7,
                3 => 8,
                5 => 7,
                6 => 8,
                7 => 6,
                _ => 5,
            };

            int aSize = mode switch
            {
                4 => 6,
                5 => 8,
                6 => 8,
                7 => 6,
                _ => 0,
            };

            for (int i = 0; i < endPoints.Length; i++)
            {
                int r = endPoints[i].R << (8 - cSize);
                int g = endPoints[i].G << (8 - cSize);
                int b = endPoints[i].B << (8 - cSize);
                int a = endPoints[i].A << (8 - aSize);

                endPoints[i] = Color.FromArgb(
                    mode > 3 ? a | (a >> aSize) : 255,
                    r | (r >> cSize),
                    g | (g >> cSize),
                    b | (b >> cSize));
            }

            int ind = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 4 - 1; j >= 0; j--)
                {
                    int subsetIndex = numSubsets switch
                    {
                        2 => BC7Subsets2PartitionTable[partitionIndex][ind],
                        3 => BC7Subsets3PartitionTable[partitionIndex][ind],
                        _ => 0,
                    };

                    Color start = endPoints[2 * subsetIndex];
                    Color end = endPoints[2 * subsetIndex + 1];

                    int colorSize = mode switch
                    {
                        0 => 3,
                        1 => 3,
                        4 when type4IndexMode == 1 => 3,
                        4 when type4IndexMode == 0 => 2,
                        6 => 4,
                        _ => 2,
                    };
                    int alphaSize = mode switch
                    {
                        4 when type4IndexMode == 0 => 3,
                        4 when type4IndexMode == 1 => 2,
                        5 => 2,
                        6 => 4,
                        7 => 2,
                        _ => 0,
                    };

                    int colorIndex = GetColorIndex(blockData, mode, numSubsets, partitionIndex, colorSize, ind);
                    int alphaIndex = GetAlphaIndex(blockData, mode, numSubsets, partitionIndex, alphaSize, ind);

                    if (startY + i <= height && startX + (3 - j) <= width)
                    {
                        colors[(startY + i) * width + (startX + (3 - j))] =
                            InterpolateColor(start, end, colorIndex, alphaIndex, colorSize, alphaSize, rotationBits);
                    }
                    ind++;
                }
            }
        }
        static int GetColorIndex(byte[] blockData, int mode, int numSubsets, int partition, int numBits, int index)
        {
            int indexOffset = GetIndexOffset(numSubsets, partition, numBits, index);
            int indexBitCount = GetIndexBitCount(numSubsets, partition, numBits, index);
            int indexStart = GetIndexStart(mode, numBits);
            return ReadBits(blockData, indexStart + indexOffset, indexBitCount);
        }
        static int GetAlphaIndex(byte[] blockData, int mode, int numSubsets, int partition, int numBits, int index)
        {
            if (numBits == 0)
                return 0;
            int indexOffset = GetIndexOffset(numSubsets, partition, numBits, index);
            int indexBitCount = GetIndexBitCount(numSubsets, partition, numBits, index);
            int indexStart = GetIndexStart(mode, numBits, true);
            return ReadBits(blockData, indexStart + indexOffset, indexBitCount);
        }
        static int GetIndexStart(int mode, int numBits, bool alpha = false)
        {
            switch (mode)
            {
                case 1: return 82;
                case 2: return 99;
                case 3: return 98;
                case 4: return numBits == 2 ? 50 : 81;
                case 5: return alpha ? 97 : 66;
                case 6: return 65;
                case 7: return 98;
                default: return 83;
            }
        }
        static int GetIndexBitCount(int numSubsets, int partition, int numBits, int index)
        {
            if (index == 0)
                return numBits-1;
            if (numSubsets == 2 && BC7Subsets2AnchorIndices[partition] == index)
                return numBits - 1;
            else if (numSubsets == 3 && (index == BC7Subsets3AnchorIndices2[partition]
                || index == BC7Subsets3AnchorIndices3[partition]))
                return numBits - 1;
            return numBits;
        }
        static int GetIndexOffset(int numSubsets, int partition, int numBits, int index)
        {
            if (index == 0)
                return 0;
            if (numSubsets == 1)
                return numBits * index - 1;
            else if (numSubsets == 2)
            {
                if (index <= BC7Subsets2AnchorIndices[partition])
                    return numBits * index - 1;
                else
                    return numBits * index - 2;
            }
            else
            {
                int anch2 = BC7Subsets3AnchorIndices2[partition];
                int anch3 = BC7Subsets3AnchorIndices3[partition];

                if (index <= anch2 && index <= anch3)
                    return numBits * index - 1;
                else if (index > anch2 && index > anch3)
                    return numBits * index - 3;
                else
                    return numBits * index - 2;
            }
        }
        static Color InterpolateColor(Color start, Color end, int colorIndex, int alphaIndex, int colorNumBits, int alphaNumBits, int rotationBits)
        {
            byte r = InterpolateByte(start.R, end.R, colorIndex, colorNumBits);
            byte g = InterpolateByte(start.G, end.G, colorIndex, colorNumBits);
            byte b = InterpolateByte(start.B, end.B, colorIndex, colorNumBits);
            byte a = InterpolateByte(start.A, end.A, alphaIndex, alphaNumBits);
            switch (rotationBits)
            {
                case 1: (a, r) = (r, a); break;
                case 2: (a, g) = (g, a); break;
                case 3: (a, b) = (b, a); break;
            }
            return Color.FromArgb(a, r, g, b);
        }
        static byte InterpolateByte(byte s, byte e, int index, int numBits)
        {
            if (numBits == 0)
                return s;

            int[] aWeight = numBits switch
            {
                2 => aWeight2,
                3 => aWeight3,
                _ => aWeight4,
            };

            return (byte)(((64 - aWeight[index]) * s + aWeight[index] * e + 32) >> 6);
        }
        static byte ExtractMode(byte input)
        {
            if (input == 0)
                return 8;

            byte count = 0;
            while ((input & 1) == 0)
            {
                count++;
                input >>= 1;
            }
            return count;
        }
        static byte[] ExtractPBits(byte[] blockData, int mode)
        {
            int start = 0;
            int count = 0;
            if (mode == 0)
            {
                start = 77;
                count = 6;
            }
            else if (mode == 1)
            {
                start = 80;
                count = 2;
            }
            else if (mode == 3)
            {
                start = 94;
                count = 4;
            }
            else if (mode == 6)
            {
                start = 63;
                count = 2;
            }
            else if (mode == 7)
            {
                start = 94;
                count = 4;
            }    

            if (count == 0)
                return Array.Empty<byte>();

            byte[] output = new byte[6];
            for (int i = 0; i < count; i++)
                output[i] = (byte)ReadBits(blockData, start+i, 1);
            return output;
        }
    }
}
