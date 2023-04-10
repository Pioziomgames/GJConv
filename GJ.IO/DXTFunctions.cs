using System;
using System.IO;
using System.Drawing;

namespace GJ.IO
{
    public partial class IOFunctions
    {
        private static Color ColorLerp(Color color1, Color color2, float t)
        {
            float r = color1.R + (color2.R - color1.R) * t;
            float g = color1.G + (color2.G - color1.G) * t;
            float b = color1.B + (color2.B - color1.B) * t;

            return Color.FromArgb((int)r, (int)g, (int)b);
        }
        static uint Extend(ushort c1)
        {
            uint r = (uint)((((c1 & 0xf800) * 0x21) << 3) & 0x00ff0000);
            uint g = (uint)((((c1 & 0x07e0) * 0x41) >> 1) & 0x0000ff00);
            uint b = (uint)(((c1 & 0x001f) * 0x21) >> 2);
            uint a = 0xff000000;
            return (r | g | b | a);
        }

        static uint Lerp2(int c1, int c2)
        {
            int r = 0x000000ff & (((0x000000ff & c1) + (0x000000ff & c2)) / 2);
            int g = 0x0000ff00 & (((0x0000ff00 & c1) + (0x0000ff00 & c2)) / 2);
            int b = 0x00ff0000 & (((0x00ff0000 & c1) + (0x00ff0000 & c2)) / 2);
            return (0xff000000 | (uint)r | (uint)g | (uint)b);
        }

        static uint Lerp3(int c1, int c2)
        {
            int r = 0x000000ff & (((0x000000ff & c1) * 2 + (0x000000ff & c2)) / 3);
            int g = 0x0000ff00 & (((0x0000ff00 & c1) * 2 + (0x0000ff00 & c2)) / 3);
            int b = 0x00ff0000 & (((0x00ff0000 & c1) * 2 + (0x00ff0000 & c2)) / 3);
            return (0xff000000 | (uint)r | (uint)g | (uint)b);
        }
        /*
        public static List<Color> ReadDXT1(BinaryReader reader, int width, int height)
        {
            List<Color> Output = new();
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    ushort c0 = reader.ReadUInt16();
                    ushort c1 = reader.ReadUInt16();
                    uint bitmask = reader.ReadUInt32();

                    var colors = new Color[4];

                    colors[0] = DecodeRGBA5650(c0);
                    colors[1] = DecodeRGBA5650(c1);
                    colors[2] = (c0 > c1) ? ColorLerp(colors[0], colors[1], 1 / 3f) : ColorLerp(colors[0], colors[1], 0.5f);
                    colors[3] = (c0 > c1) ? ColorLerp(colors[0], colors[1], 2 / 3f) : Color.FromArgb(0, 0, 0, 0);


                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int index = (int)((bitmask >> 2 * (4 * py + px)) & 3);

                            var color = colors[index];

                            if (px < width && py < height)
                                Output.Add(color);
                        }
                    }

                }
            }

            return Output;
        }
        */

        private static Color InterpolateColor(Color c1, Color c2, int factor)
        {
            // Calculate the interpolated color components
            int r = c1.R + (factor * (c2.R - c1.R) / 3);
            int g = c1.G + (factor * (c2.G - c1.G) / 3);
            int b = c1.B + (factor * (c2.B - c1.B) / 3);

            // Clamp the color components to the valid range
            r = Clamp(r, 0, 255);
            g = Clamp(g, 0, 255);
            b = Clamp(b, 0, 255);

            // Return the interpolated color
            return Color.FromArgb(r, g, b);
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
        public static List<Color> ReadDXT1(BinaryReader reader, int width, int height)
        {
            long start = reader.BaseStream.Position;
            int pitch = 16 * 8 - 1;
            pitch = ((16 * width + pitch) & ~pitch ) / 8;


            // Create the list to store the pixels
            List<Color> pixels = new List<Color>();

            // Iterate over the blocks
            for (int y = 0; y < height; y += 4)
            {
                int dh = height - y;
                if (dh >= 4) dh = 4;

                for (int x = 0; x < width; x += 4)
                {
                    int dw = width - x;
                    if (dw >= 4) dw = 4;

                    // Read the color block

                    reader.BaseStream.Seek(start + x, SeekOrigin.Begin);
                    Color[] colorBlock = ReadColorBlockDXT1(reader, dw, dh);

                    // Add the colors to the list in the correct order
                    for (int j = 0; j < dh; j++)
                    {
                        for (int i = 0; i < dw; i++)
                        {
                            pixels.Add(colorBlock[j * 4 + i]);
                        }
                    }
                }

                // Skip the unused bytes at the end of the scanline
                reader.BaseStream.Seek(start + pitch * 4, SeekOrigin.Begin);
                start = reader.BaseStream.Position;
            }

            // Return the list of pixels
            return pixels;
        }

        private static Color[] ReadColorBlockDXT1(BinaryReader reader, int width, int height)
        {
            // Read the color block
            ushort color0 = reader.ReadUInt16();
            ushort color1 = reader.ReadUInt16();
            uint bits = reader.ReadUInt32();

            // Create the array to store the colors
            Color[] colors = new Color[width * height];

            int ic0 = (int)Extend(color0);
            int ic1 = (int)Extend(color1);
            colors[0] = Color.FromArgb(ic0);
            colors[1] = Color.FromArgb(ic1);

            if (ic0 > ic1)
            {
                colors[2] = Color.FromArgb((int)Lerp3(ic0, ic1));
                colors[3] = Color.FromArgb((int)Lerp3(ic1, ic0));
            }
            else
            {
                colors[2] = Color.FromArgb((int)Lerp2(ic0, ic1));
                colors[3] = Color.FromArgb(0, 0, 0, 0);
            }

            // Decode the bits
            for (int i = 0; i < width * height; i++)
            {
                // Extract the color index from the bits
                int index = (int)((bits >> (i * 2)) & 0x3);

                // Choose the color based on the index
                colors[i] = colors[index];
            }

            // Return the array of colors
            return colors;
        }

        public static List<Color> ReadDXT3(BinaryReader reader, int width, int height)
        {
            var colors = new List<Color>();

            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    // Read 16 2-bit color indices
                    ushort c0 = reader.ReadUInt16();
                    ushort c1 = reader.ReadUInt16();

                    // Extract the color values
                    byte r0 = (byte)((c0 >> 11) & 0x1F);
                    byte g0 = (byte)((c0 >> 5) & 0x3F);
                    byte b0 = (byte)(c0 & 0x1F);
                    byte r1 = (byte)((c1 >> 11) & 0x1F);
                    byte g1 = (byte)((c1 >> 5) & 0x3F);
                    byte b1 = (byte)(c1 & 0x1F);

                    // Scale the color values to 8 bits
                    r0 = (byte)((r0 << 3) | (r0 >> 2));
                    g0 = (byte)((g0 << 2) | (g0 >> 4));
                    b0 = (byte)((b0 << 3) | (b0 >> 2));
                    r1 = (byte)((r1 << 3) | (r1 >> 2));
                    g1 = (byte)((g1 << 2) | (g1 >> 4));
                    b1 = (byte)((b1 << 3) | (b1 >> 2));

                    // Read the alpha values
                    byte[] alpha = new byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        alpha[i] = reader.ReadByte();
                    }

                    // Decode the 4x4 block of texel data
                    for (int j = 0; j < 4; j++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            // Read the next 2-bit color index
                            int c = (j * 4) + i;
                            int index = (c / 2) % 4;
                            int shift = (c % 2) * 4;
                            int idx = (alpha[index] >> shift) & 0xF;

                            // Interpolate the color based on the index
                            byte r, g, b;
                            if (idx == 0)
                            {
                                r = r0;
                                g = g0;
                                b = b0;
                            }
                            else if (idx == 1)
                            {
                                r = r1;
                                g = g1;
                                b = b1;
                            }
                            else if (idx == 2)
                            {
                                r = (byte)((2 * r0 + r1) / 3);
                                g = (byte)((2 * g0 + g1) / 3);
                                b = (byte)((2 * b0 + b1) / 3);
                            }
                            else if (idx == 3)
                            {
                                r = (byte)((r0 + 2 * r1) / 3);
                                g = (byte)((g0 + 2 * g1) / 3);
                                b = (byte)((b0 + 2 * b1) / 3);
                            }
                            else
                            {
                                // Interpolate the remaining colors
                                r = (byte)((idx * r0 + (8 - idx) * r1) / 8);
                                g = (byte)((idx * g0 + (8 - idx) * g1) / 8);
                                b = (byte)((idx * b0 + (8 - idx) * b1) / 8);
                            }

                            // Read the alpha value
                            int a = alpha[c / 2] & 0xF;
                            a = (a << 4) | a;

                            // Add the texel to the list
                            colors.Add(Color.FromArgb(a, r, g, b));
                        }
                    }
                }
            }

            return colors;
        }

        public static List<Color> ReadDXT5(BinaryReader reader, int width, int height)
        {
            var colors = new List<Color>();

            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    // Read the two alpha values
                    byte alpha0 = reader.ReadByte();
                    byte alpha1 = reader.ReadByte();

                    // Read the alpha indices
                    byte[] alphaIndices = reader.ReadBytes(6);

                    // Read 16 2-bit color indices
                    ushort c0 = reader.ReadUInt16();
                    ushort c1 = reader.ReadUInt16();

                    // Extract the color values
                    byte r0 = (byte)((c0 >> 11) & 0x1F);
                    byte g0 = (byte)((c0 >> 5) & 0x3F);
                    byte b0 = (byte)(c0 & 0x1F);
                    byte r1 = (byte)((c1 >> 11) & 0x1F);
                    byte g1 = (byte)((c1 >> 5) & 0x3F);
                    byte b1 = (byte)(c1 & 0x1F);

                    // Scale the color values to 8 bits
                    r0 = (byte)((r0 << 3) | (r0 >> 2));
                    g0 = (byte)((g0 << 2) | (g0 >> 4));
                    b0 = (byte)((b0 << 3) | (b0 >> 2));
                    r1 = (byte)((r1 << 3) | (r1 >> 2));
                    g1 = (byte)((g1 << 2) | (g1 >> 4));
                    b1 = (byte)((b1 << 3) | (b1 >> 2));

                    // Decode the 4x4 block of texel data
                    for (int j = 0; j < 4; j++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            // Read the next 3-bit alpha index
                            int c = (j * 4) + i;
                            int index = c / 2;
                            int shift = (c % 2) * 3;
                            int idx = (alphaIndices[index] >> shift) & 0x7;

                            // Interpolate the alpha value based on the index
                            int a;
                            if (idx == 0)
                            {
                                a = alpha0;
                            }
                            else if (idx == 1)
                            {
                                a = alpha1;
                            }
                            else if (alpha0 > alpha1)
                            {
                                a = (byte)(((8 - idx) * alpha0 + (idx - 1) * alpha1) / 7);
                            }
                            else if (idx == 6)
                            {
                                a = 0;
                            }
                            else if (idx == 7)
                            {
                                a = 255;
                            }
                            else
                            {
                                // Interpolate the remaining alpha values
                                a = (byte)(((6 - idx) * alpha0 + (idx - 1) * alpha1) / 5);
                            }

                            // Read the next 2-bit color index
                            index = (c / 2) % 3;
                            shift = (c % 2) * 4;
                            idx = (alphaIndices[index + 3] >> shift) & 0xF;

                            // Interpolate the color based on the index
                            byte r, g, b;
                            if (idx == 0)
                            {
                                r = r0;
                                g = g0;
                                b = b0;
                            }
                            else if (idx == 1)
                            {
                                r = r1;
                                g = g1;
                                b = b1;
                            }
                            else if (idx == 2)
                            {
                                r = (byte)((2 * r0 + r1) / 3);
                                g = (byte)((2 * g0 + g1) / 3);
                                b = (byte)((2 * b0 + b1) / 3);
                            }
                            else if (idx == 3)
                            {
                                r = (byte)((r0 + 2 * r1) / 3);
                                g = (byte)((g0 + 2 * g1) / 3);
                                b = (byte)((b0 + 2 * b1) / 3);
                            }
                            else
                            {
                                // Interpolate the remaining colors
                                r = (byte)((idx * r0 + (16 - idx) * r1) / 16);
                                g = (byte)((idx * g0 + (16 - idx) * g1) / 16);
                                b = (byte)((idx * b0 + (16 - idx) * b1) / 16);
                            }

                            // Add the texel to the list
                            colors.Add(Color.FromArgb(a, r, g, b));
                        }
                    }
                }
            }

            return colors;
        }

    }
}
