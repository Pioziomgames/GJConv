using System.Drawing;
using System.Numerics;

namespace DDSLib
{
    public static partial class DXT
    {
        public static Color[] ReadBC6UData(BinaryReader reader, uint width, uint height)
        {
            return ReadBC6Data(reader, width, height, false);
        }
        public static Color[] ReadBC6SData(BinaryReader reader, uint width, uint height)
        {
            return ReadBC6Data(reader, width, height, true);
        }
        public static Color[] ReadBC6Data(BinaryReader reader, uint width, uint height, bool signed)
        {
            uint blockCountX = (width + 3) / 4;
            uint blockCountY = (height + 3) / 4;

            Color[] colors = new Color[width * height];

            for (int blockY = 0; blockY < blockCountY; blockY++)
            {
                for (int blockX = 0; blockX < blockCountX; blockX++)
                {
                    byte[] blockData = reader.ReadBytes(16);
                    DecompressBC6Block(blockData, blockX * 4, blockY * 4, width, height, colors, signed);
                }
            }

            return colors;
        }
        public static void DecompressBC6Block(byte[] blockData, int startX, int startY, uint width, uint height, Color[] colors, bool signed)
        {
            int mode = ReadBits(blockData, 0, 2);
            if (mode > 1)
                mode |= (ReadBits(blockData, 2, 3) << 2);

            mode = mode switch
            {
                0b00 => 1,
                0b01 => 2,
                0b00010 => 3,
                0b00110 => 4,
                0b01010 => 5,
                0b01110 => 6,
                0b10010 => 7,
                0b10110 => 8,
                0b11010 => 9,
                0b11110 => 10,
                0b00011 => 11,
                0b00111 => 12,
                0b01011 => 13,
                0b01111 => 14,
                _ => 0,
            };

            if (mode == 0)
                return; //Colors should already be set to 0

            (int, int, int)[] endPoints = new (int, int, int)[mode < 11 ? 4 : 2];
            int endPointBits = mode switch
            {
                1 => 10,
                2 => 7,
                6 => 9,
                7 => 8,
                8 => 8,
                9 => 8,
                10 => 6,
                11 => 10,
                13 => 12,
                14 => 16,
                _ => 11,
            };
            int deltaBits = mode switch
            {
                2 => 6,
                3 => 4,
                4 => 4,
                5 => 4,
                12 => 9,
                13 => 8,
                14 => 4,
                _ => 5,
            };

            endPoints[0] = GetColor0(blockData, mode, endPointBits);
            endPoints[1] = GetColor1(blockData, mode, deltaBits);

            if (mode != 11 && mode != 10)
            {
                endPoints[1] = SignExtend(endPoints[1], endPointBits);
                endPoints[1] = (
                    (endPoints[0].Item1 + endPoints[1].Item1) & ((1 << endPointBits) - 1),
                    (endPoints[0].Item2 + endPoints[1].Item2) & ((1 << endPointBits) - 1),
                    (endPoints[0].Item3 + endPoints[1].Item3) & ((1 << endPointBits) - 1));
            }

            if (signed)
            {
                endPoints[0] = SignExtend(endPoints[0], endPointBits);
                endPoints[0] = SignExtend(endPoints[1], endPointBits);
            }

            if (mode < 11)
            {
                endPoints[2] = GetColor2(blockData, mode, deltaBits);
                endPoints[3] = GetColor3(blockData, mode, deltaBits);
                if (mode != 11 && mode != 10)
                {
                    endPoints[2] = (
                        SignExtend(endPoints[2].Item1, mode == 3 || mode == 7 ? deltaBits + 1 : deltaBits),
                        SignExtend(endPoints[2].Item2, mode == 4 || mode == 8 ? deltaBits + 1 : deltaBits),
                        SignExtend(endPoints[2].Item3, mode == 5 || mode == 9 ? deltaBits + 1 : deltaBits));
                    endPoints[2] = (
                        (endPoints[0].Item1 + endPoints[2].Item1) & ((1 << endPointBits) - 1),
                        (endPoints[0].Item2 + endPoints[2].Item2) & ((1 << endPointBits) - 1),
                        (endPoints[0].Item3 + endPoints[2].Item3) & ((1 << endPointBits) - 1));

                    endPoints[3] = (
                        SignExtend(endPoints[3].Item1, mode == 3 || mode == 7 ? deltaBits + 1 : deltaBits),
                        SignExtend(endPoints[3].Item2, mode == 4 || mode == 8 ? deltaBits + 1 : deltaBits),
                        SignExtend(endPoints[3].Item3, mode == 5 || mode == 9 ? deltaBits + 1 : deltaBits));
                    endPoints[3] = (
                        (endPoints[0].Item1 + endPoints[3].Item1) & ((1 << endPointBits) - 1),
                        (endPoints[0].Item2 + endPoints[3].Item2) & ((1 << endPointBits) - 1),
                        (endPoints[0].Item3 + endPoints[3].Item3) & ((1 << endPointBits) - 1));
                }

                if (signed)
                {
                    endPoints[2] = SignExtend(endPoints[2], endPointBits);
                    endPoints[3] = SignExtend(endPoints[3], endPointBits);
                }
            }

            int numSubsets = 1;
            int partition = 0;
            if (mode < 11)
            {
                numSubsets = 2;
                partition = ReadBits(blockData, 77, 5);
            }

            for (int i = 0; i < endPoints.Length; i++)
                endPoints[i] = UnQuantize(endPoints[i], endPointBits, signed);

            int ind = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 4 - 1; j >= 0; j--)
                {
                    int subsetIndex = numSubsets == 2 ? BC6Subsets2PartitionTable[partition][ind] : 0;

                    (int, int, int) start = endPoints[subsetIndex * 2];
                    (int, int, int) end = endPoints[subsetIndex * 2 + 1];

                    int indexBits = mode < 11 ? 3 : 4;

                    int indexOffset = (ind == 0) ? 0
                        : indexBits * ind - (numSubsets == 2 && ind <= BC6Subsets2AnchorIndices[partition] ? 1 : 2);

                    int indexBitCount = ind == 0 || (numSubsets == 2 && ind == BC6Subsets2AnchorIndices[partition])
                        ? indexBits - 1 : indexBits;

                    int colorIndex = ReadBits(blockData, indexOffset + mode < 11 ? 82 : 65, indexBitCount);

                    if (startY + i <= height && startX + (3 - j) <= width)
                    {
                        colors[(startY + i) * width + (startX + (3 - j))] =
                            ExtendToColor(InterpolateColor(start, end, colorIndex, mode < 11 ? 3 : 4), signed);
                    }
                    ind++;
                }
            }
        }

        private static readonly int[][] BC6Subsets2PartitionTable = new int[32][]{
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
            new[] {0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0}
        };
        private static readonly int[] BC6Subsets2AnchorIndices = {
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 2, 8, 2, 2, 8, 8, 15,
            2, 8, 2, 2, 8, 8, 2, 2,
            15, 15, 6, 8, 2, 8, 15, 15,
            2, 8, 2, 2, 2, 15, 15, 6,
            6, 2, 6, 8, 15, 15, 2, 2,
            15, 15, 15, 15, 15, 2, 2, 15
        };

        private static Vector3 WhitePoint = new Vector3(0.3127f, 0.3290f, 1.0f);
        private static Color ExtendToColor((int, int, int) input, bool signed)
        {
            Vector3 hdrColor = new Vector3(
                (float)ExtendInt(input.Item1, signed),
                (float)ExtendInt(input.Item2, signed),
                (float)ExtendInt(input.Item3, signed));

            float maxLuminance = Math.Max(hdrColor.X, Math.Max(hdrColor.Y, hdrColor.Z));

            Vector3 ldrColor = hdrColor * (Vector3.One + hdrColor / (WhitePoint * WhitePoint)) / (Vector3.One + hdrColor);

            return Color.FromArgb((byte)(ldrColor.X * 255f), (byte)(ldrColor.Y * 255f), (byte)(ldrColor.Z * 255f));
        }
        public static Half ExtendInt(int input, bool signed)
        {
            if (signed)
            {
                int component = (input < 0) ? -(((-input) * 31) >> 5) : (input * 31) >> 5;
                if (component < 0)
                    return BitConverter.ToHalf(BitConverter.GetBytes((ushort)(-component) | 0x8000));
                else
                    return BitConverter.ToHalf(BitConverter.GetBytes((ushort)component));
            }
            else
            {
                int component = (input * 31) >> 6;
                return BitConverter.ToHalf(BitConverter.GetBytes((ushort)component));
            }
        }
        private static (int, int, int) InterpolateColor((int, int, int) start, (int, int, int) end, int colorIndex, int bitCount)
        {
            return (
                InterpolateInt(start.Item1, end.Item1, colorIndex, bitCount),
                InterpolateInt(start.Item2, end.Item2, colorIndex, bitCount),
                InterpolateInt(start.Item3, end.Item3, colorIndex, bitCount));
        }
        static int InterpolateInt(int s, int e, int index, int numBits)
        {
            if (numBits == 0)
                return s;

            int[] aWeight = numBits switch
            {
                2 => aWeight2,
                3 => aWeight3,
                _ => aWeight4,
            };

            return ((64 - aWeight[index]) * s + aWeight[index] * e + 32) >> 6;
        }
        private static (int, int, int) GetColor3(byte[] blockData, int mode, int deltaBits)
        {
            int r = ReadBits(blockData, 71, Math.Min(5, mode == 3 || mode == 7 ? deltaBits + 1 : deltaBits));
            int g = ReadBits(blockData, 51, 4);
            int b = 0;


            if (mode == 1 || (mode > 2 && mode < 10))
            {
                if (mode == 8)
                    b = ReadBits(blockData, 13, 1);
                else if (mode == 4)
                    b = ReadBits(blockData, 69, 1);
                else
                    b = ReadBits(blockData, 50, 1);

                if (mode == 9)
                    b |= ReadBits(blockData, 13, 1) << 1;
                else if (mode == 5)
                    b |= ReadBits(blockData, 69, 1) << 1;
                else
                    b |= ReadBits(blockData, 60, 1) << 1;

                if (mode != 7)
                {
                    b |= ReadBits(blockData, 70, 1) << 2;
                    b |= ReadBits(blockData, 76, 1) << 3;
                    if (mode == 1)
                        b |= ReadBits(blockData, 4, 1) << 4;
                    else if (mode == 5)
                        b |= ReadBits(blockData, 75, 1) << 4;
                }

            }

            if (mode == 2 || mode == 7)
            {
                r |= ReadBits(blockData, 76, 1) << 5;
                if (mode == 2)
                    g |= ReadBits(blockData, 3, 2) << 4;
            }
            if (mode == 2 || mode == 7 || mode == 10)
            {
                if (mode != 7)
                    b = ReadBits(blockData, 12, 2);

                b |= ReadBits(blockData, 23, 1) << 2;
                if (mode == 7)
                    b |= ReadBits(blockData, 33, 1) << 3;
                else
                    b |= ReadBits(blockData, 32, 1) << 3;
            }

            if (mode == 1 || mode == 4 || mode == 6 || mode == 8 || mode == 9)
                g |= ReadBits(blockData, 40, 1) << 4;

            if (mode == 2 || (mode > 5 && mode < 11))
            {
                b |= ReadBits(blockData, 34, 1) << 4;
                if (mode == 2 || mode == 9 || mode == 10)
                    b |= ReadBits(blockData, 33, 1) << 5;
            }

            return (r, g, b);
        }
        private static (int, int, int) GetColor2(byte[] blockData, int mode, int deltaBits)
        {
            int r = ReadBits(blockData, 65, Math.Min(5, mode == 3 || mode == 7 ? deltaBits + 1 : deltaBits));
            int g = ReadBits(blockData, 41, 4);
            int b = ReadBits(blockData, 61, 4);

            if (mode == 1)
            {
                g |= ReadBits(blockData, 2, 1) << 4;
                b |= ReadBits(blockData, 3, 1) << 4;
            }
            else if (mode == 4)
                g |= ReadBits(blockData, 75, 1) << 4;
            else if (mode == 5)
                b |= ReadBits(blockData, 40, 1) << 4;
            else if (mode == 2 || (mode > 5 && mode < 11))
            {
                g = ReadBits(blockData, 24, 1) << 4;
                b = ReadBits(blockData, 14, 1) << 4;
            }
            if (mode == 2 || mode == 7)
            {
                r |= ReadBits(blockData, 70, 1) << 5;
            }
            if (mode == 2 || mode == 10)
            {
                b |= ReadBits(blockData, 22, 1) << 5;
                if (mode == 2)
                    g |= ReadBits(blockData, 2, 1) << 5;
                else
                    g |= ReadBits(blockData, 21, 1) << 5;
            }
            else if (mode == 8)
                g = ReadBits(blockData, 23, 1) << 5;
            else if (mode == 9)
                b |= ReadBits(blockData, 23, 1) << 5;

            return (r,g,b);
        }
        private static (int, int, int) GetColor1(byte[] blockData, int mode, int deltaBits)
        {
            int r;
            int g;
            int b;

            if (mode != 11 && mode != 10)
            {
                r = ReadBits(blockData, 35, Math.Min(5, mode == 3 || mode == 7 ? deltaBits + 1 : deltaBits));
                g = ReadBits(blockData, 45, Math.Min(5, mode == 4 || mode == 8 ? deltaBits + 1 : deltaBits));
                b = ReadBits(blockData, 55, Math.Min(5, mode == 5 || mode == 9 ? deltaBits + 1 : deltaBits));
            }
            else
            {
                int s = mode == 11 ? 10 : 6;
                r = ReadBits(blockData, 35, s);
                g = ReadBits(blockData, 45, s);
                b = ReadBits(blockData, 55, s);
            }

            if (mode == 2 || mode == 7)
                r |= ReadBits(blockData, 40, 1) << 5;
            if (mode == 2 || mode == 8)
                g |= ReadBits(blockData, 50, 1) << 5;
            if (mode == 2 || mode == 9)
                b |= ReadBits(blockData, 60, 1) << 5;

            if (mode == 12 || mode == 13)
            {
                int s = mode == 13 ? 3 : 4;
                r |= ReadBits(blockData, 40, s) << 5;
                g |= ReadBits(blockData, 50, s) << 5;
                b |= ReadBits(blockData, 60, s) << 5;
            }

            return (r, g, b);
        }
        private static (int, int, int) GetColor0(byte[] blockData, int mode, int endPointBits)
        {
            int r = ReadBits(blockData, 5, Math.Min(10, endPointBits));
            int g = ReadBits(blockData, 15, Math.Min(10, endPointBits));
            int b = ReadBits(blockData, 25, Math.Min(10, endPointBits));

            if (mode > 2 && mode < 6)
            {
                r |= (ReadBits(blockData, mode == 3 ? 40 : 39, 1) << 10);
                g |= (ReadBits(blockData, mode == 4 ? 50 : 49, 1) << 10);
                b |= (ReadBits(blockData, mode == 5 ? 60 : 59, 1) << 10);
            }
            else if (mode > 11)
            {
                r |= ReadBits(blockData, 5, 10);
                g |= ReadBits(blockData, 15, 10);
                b |= ReadBits(blockData, 25, 10);

                for (int i = 0; i < (mode == 14 ? 6 : (mode == 13 ? 2 : 1)); i++)
                {
                    r |= (ReadBits(blockData, 44 - i, 1) << 10 + i);
                    g |= (ReadBits(blockData, 54 - i, 1) << 10 + i);
                    b |= (ReadBits(blockData, 64 - i, 1) << 10 + i);
                }
            }
            return (r, g, b);
        }
        private static (int, int, int) UnQuantize((int, int, int) i, int endpointBits, bool signed)
        {
            return (
                UnQuantize(i.Item1, endpointBits, signed),
                UnQuantize(i.Item2, endpointBits, signed),
                UnQuantize(i.Item3, endpointBits, signed));
        }
        private static int UnQuantize(int component, int endpointBits, bool signed)
        {
            if (component == 0)
                return 0;

            if (signed)
            {
                if (endpointBits >= 16)
                    return component;
                bool sign = false;

                if (component < 0)
                {
                    sign = true;
                    component = -component;
                }

                int res = (component >= ((1 << (endpointBits - 1)) - 1)) ? 0x7fff
                    : ((component << 15) + 0x4000) >> (endpointBits - 1);

                return sign ? -res : res;
            }
            else
            {
                if (endpointBits >= 15)
                    return component;
                else if (component == ((1 << endpointBits) - 1))
                    return 0xffff;
                else
                    return ((component << 15) + 0x4000) >> (endpointBits - 1);
            }
        }
        private static (int, int, int) SignExtend((int, int, int) input, int bits)
        {
            return (
                SignExtend(input.Item1, bits),
                SignExtend(input.Item2, bits),
                SignExtend(input.Item3, bits));
        }
        private static int SignExtend(int input, int bits)
        {
            int signMask = 1 << (bits - 1);
            int numberMask = signMask - 1;
            if ((input & signMask) != 0)
            {
                return (~numberMask) | (input & numberMask);
            }
            else
                return (input & numberMask);
        }
    }
}
