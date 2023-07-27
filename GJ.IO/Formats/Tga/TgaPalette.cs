using System.IO;
using System.Drawing;

namespace TgaLib
{
    public class TgaPalette
    {
        public Color[] Colors;
        public TgaPalette(Color[] colors)
        {
            Colors = colors;
        }
        public TgaPalette(BinaryReader reader, TgaHeader Header)
        {
            Colors = new Color[Header.PaletteSize];
            switch (Header.PaletteDepth)
            {
                case 15:
                    for (int i = 0; i < Header.PaletteSize; i++)
                    {
                        ushort pixel = reader.ReadUInt16();
                        int R = (pixel & 0x7C00) >> 10;
                        int G = (pixel & 0x03E0) >> 5;
                        int B = (pixel & 0x001F);
                        R <<= 3;
                        G <<= 3;
                        B <<= 3;
                        Colors[i] = Color.FromArgb(R, G, B);
                    }
                    break;
                case 16:
                    for (int i = 0; i < Header.PaletteSize; i++)
                    {
                        ushort pixel = reader.ReadUInt16();
                        int R = (pixel & 0x7C00) >> 10;
                        int G = (pixel & 0x03E0) >> 5;
                        int B = (pixel & 0x001F);
                        int A = (pixel & 0x8000) != 0 ? 255 : 0;
                        R <<= 3;
                        G <<= 3;
                        B <<= 3;
                        Colors[i] = Color.FromArgb(A,R,G,B);
                    }
                    break;
                case 24:
                    for (int i = 0; i < Header.PaletteSize; i++)
                    {
                        byte B = reader.ReadByte();
                        byte G = reader.ReadByte();
                        byte R = reader.ReadByte();
                        Colors[i] = Color.FromArgb(R,G,B);
                    }
                    break;
                case 32:
                    for (int i = 0; i < Header.PaletteSize; i++)
                    {
                        byte B = reader.ReadByte();
                        byte G = reader.ReadByte();
                        byte R = reader.ReadByte();
                        byte A = reader.ReadByte();
                        Colors[i] = Color.FromArgb(A,R,G,B);
                    }
                    break;
            }
        }
        public void Write(BinaryWriter writer, TgaHeader Header)
        {
            switch (Header.PaletteDepth)
            {
                case 15:
                    for (int i = 0; i < Header.PaletteSize; i++)
                    {
                        int R = Colors[i].R >> 3;
                        int G = Colors[i].G >> 3;
                        int B = Colors[i].B >> 3;
                        ushort pixel = (ushort)((R << 10) | (G << 5) | B);
                        writer.Write(pixel);
                    }
                    break;
                case 16:
                    for (int i = 0; i < Header.PaletteSize; i++)
                    {
                        int A = Colors[i].A >= 128 ? 0x8000 : 0;
                        int R = Colors[i].R >> 3;
                        int G = Colors[i].G >> 3;
                        int B = Colors[i].B >> 3;
                        ushort pixel = (ushort)(A | (R << 10) | (G << 5) | B);
                        writer.Write(pixel);
                    }
                    break;
                case 24:
                    for (int i = 0; i < Header.PaletteSize; i++)
                    {
                        writer.Write(Colors[i].B);
                        writer.Write(Colors[i].G);
                        writer.Write(Colors[i].R);
                    }
                    break;
                case 32:
                    for (int i = 0; i < Header.PaletteSize; i++)
                    {
                        writer.Write(Colors[i].B);
                        writer.Write(Colors[i].G);
                        writer.Write(Colors[i].R);
                        writer.Write(Colors[i].A);
                    }
                    break;
            }
        }
    }
}
