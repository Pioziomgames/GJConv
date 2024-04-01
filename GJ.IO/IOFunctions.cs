using System;
using System.Drawing;

namespace GJ.IO
{
    public partial class IOFunctions
    {
        public static void Seek(BinaryWriter writer, int Offset) //Makes sure that padding is written even if it's the end of the file
        {
            if (Offset < 0)
                writer.BaseStream.Position -= Offset;
            else
                for (int i = 0; i < Offset; i++)
                    writer.Write((byte)0);
        }
        public static void Align(BinaryReader reader, int alignment)
        {
            reader.BaseStream.Seek(Align(reader.BaseStream.Position, alignment), SeekOrigin.Begin);
        }
        public static void Align(BinaryWriter writer, int alignment)
        {
            long oldh = writer.BaseStream.Position;
            long newh = Align(writer.BaseStream.Position, alignment);

            for (int i = 0; i < newh - oldh; i++)
                writer.Write((byte)0);
        }
        public static int Align(int value, int alignment)
        {
            return (value + (alignment - 1) & ~(alignment - 1));
        }
        public static long Align(long value, int alignment)
        {
            return (value + (alignment - 1) & ~(alignment - 1));
        }
        public static uint Align(uint value, int alignment)
        {
            return (uint)(value + (alignment - 1) & ~(alignment - 1));
        }

        public static void WriteRGBA5650(BinaryWriter writer, Color VColor)
        {
            ushort R = (byte)(((float)VColor.R / 255) * 31);
            ushort G = (byte)(((float)VColor.G / 255) * 63);
            ushort B = (byte)(((float)VColor.B / 255) * 31);

            int RGB = 0;
            RGB |= R;
            RGB |= (G << 5);
            RGB |= (B << 11);
            writer.Write((ushort)RGB);
        }
        public static void WriteRGBA5551(BinaryWriter writer, Color VColor)
        {
            ushort R = (byte)(((float)VColor.R / 255) * 31);
            ushort G = (byte)(((float)VColor.G / 255) * 31);
            ushort B = (byte)(((float)VColor.B / 255) * 31);
            byte A = 0;
            if (VColor.A > 0)
                A = 1;

            int RGBA = 0;
            RGBA |= R;
            RGBA |= (G << 5);
            RGBA |= (B << 10);
            RGBA |= (A << 15);
            writer.Write((ushort)RGBA);
        }
        public static void WriteRGBA4444(BinaryWriter writer, Color VColor)
        {
            ushort R = (byte)(((float)VColor.R / 255) * 15);
            ushort G = (byte)(((float)VColor.G / 255) * 15);
            ushort B = (byte)(((float)VColor.B / 255) * 15);
            ushort A = (byte)(((float)VColor.A / 255) * 15);

            int RGBA = 0;

            RGBA |= R;
            RGBA |= (G << 4);
            RGBA |= (B << 8);
            RGBA |= (A << 12);
            writer.Write((ushort)RGBA);
        }
        public static void WriteBGRA8888(BinaryWriter writer, Color VColor)
        {
            writer.Write(VColor.B);
            writer.Write(VColor.G);
            writer.Write(VColor.R);
            writer.Write(VColor.A);
        }
        public static void WriteRGBA8888(BinaryWriter writer, Color VColor)
        {
            writer.Write(VColor.R);
            writer.Write(VColor.G);
            writer.Write(VColor.B);
            writer.Write(VColor.A);
        }
        public static void WriteRGBA8880(BinaryWriter writer, Color VColor)
        {
            writer.Write(VColor.R);
            writer.Write(VColor.G);
            writer.Write(VColor.B);
        }
        public static void WriteBGRA8880(BinaryWriter writer, Color VColor)
        {
            writer.Write(VColor.B);
            writer.Write(VColor.G);
            writer.Write(VColor.R);
        }
        public static void WritePSMCT32(BinaryWriter writer, Color VColor)
        {
            writer.Write((uint)(VColor.R | (VColor.G << 8) | (VColor.B << 16) | (FullByteToHalf(VColor.A) << 24)));
        }

        public static void WritePSMCT16(BinaryWriter writer, Color VColor)
        {
            ushort colorData = (ushort)(((FullByteToHalf(VColor.A) >> 7) << 15) | ((VColor.B >> 3) << 10) | ((VColor.G >> 3) << 5) | (VColor.R >> 3));
            writer.Write(colorData);
        }
        public static Color ReadPSMCT32(BinaryReader reader)
        {
            uint color = reader.ReadUInt32();
            return Color.FromArgb(
                HalfByteToFull((byte)((color >> 24) & byte.MaxValue)),
                (byte)(color & byte.MaxValue),
                (byte)((color >> 8) & byte.MaxValue),
                (byte)((color >> 16) & byte.MaxValue));
        }
        public static Color ReadPSMCT16(BinaryReader reader)
        {
            ushort color = reader.ReadUInt16();
            return Color.FromArgb(
            byte.MaxValue,
            (byte)((color & 0x001F) << 3),
            (byte)(((color & 0x03E0) >> 5) << 3),
            (byte)(((color & 0x7C00) >> 10) << 3));
        }
        public static Color ReadRGBA5650(BinaryReader reader)
        {
            byte R = 0;
            byte G = 0;
            byte B = 0;
            byte A = 0xFF;

            ushort RGBA = reader.ReadUInt16();
            //Red
            if ((RGBA & 0x1) == 0x1)
                R += 0x8;
            if ((RGBA & 0x2) == 0x2)
                R += 0x10;
            if ((RGBA & 0x4) == 0x4)
                R += 0x21;
            if ((RGBA & 0x8) == 0x8)
                R += 0x42;
            if ((RGBA & 0x10) == 0x10)
                R += 0x84;

            //Green
            if ((RGBA & 0x20) == 0x20)
                G += 0x4;
            if ((RGBA & 0x40) == 0x40)
                G += 0x8;
            if ((RGBA & 0x80) == 0x80)
                G += 0x10;
            if ((RGBA & 0x100) == 0x100)
                G += 0x20;
            if ((RGBA & 0x200) == 0x200)
                G += 0x41;
            if ((RGBA & 0x400) == 0x400)
                G += 0x82;

            //Blue
            if ((RGBA & 0x800) == 0x800)
                B += 0x8;
            if ((RGBA & 0x1000) == 0x1000)
                B += 0x10;
            if ((RGBA & 0x2000) == 0x2000)
                B += 0x21;
            if ((RGBA & 0x4000) == 0x4000)
                B += 0x42;
            if ((RGBA & 0x8000) == 0x8000)
                B += 0x84;

            return Color.FromArgb(A, R, G, B);
        }
        public static Color ReadRGBA5551(BinaryReader reader)
        {
            byte R = 0;
            byte G = 0;
            byte B = 0;
            byte A = 0;

            ushort RGBA = reader.ReadUInt16();

            //Red
            if ((RGBA & 0x1) == 0x1)
                R += 0x8;
            if ((RGBA & 0x2) == 0x2)
                R += 0x10;
            if ((RGBA & 0x4) == 0x4)
                R += 0x21;
            if ((RGBA & 0x8) == 0x8)
                R += 0x42;
            if ((RGBA & 0x10) == 0x10)
                R += 0x84;

            //Green
            if ((RGBA & 0x20) == 0x20)
                G += 0x8;
            if ((RGBA & 0x40) == 0x40)
                G += 0x10;
            if ((RGBA & 0x80) == 0x80)
                G += 0x21;
            if ((RGBA & 0x100) == 0x100)
                G += 0x42;
            if ((RGBA & 0x200) == 0x200)
                G += 0x84;

            //Blue
            if ((RGBA & 0x400) == 0x400)
                B += 0x8;
            if ((RGBA & 0x800) == 0x800)
                B += 0x10;
            if ((RGBA & 0x1000) == 0x1000)
                B += 0x21;
            if ((RGBA & 0x2000) == 0x2000)
                B += 0x42;
            if ((RGBA & 0x4000) == 0x4000)
                B += 0x84;

            //Alpha
            if ((RGBA & 0x8000) == 0x8000)
                A += 0xFF;

            return Color.FromArgb(A, R, G, B);
        }
        public static Color ReadRGBA4444(BinaryReader reader)
        {
            byte R = 0;
            byte G = 0;
            byte B = 0;
            byte A = 0;
            ushort RnG = reader.ReadByte();
            ushort BnA = reader.ReadByte();
            //Red
            if ((RnG & 0x1) == 0x1)
                R += 0x11;
            if ((RnG & 0x2) == 0x2)
                R += 0x22;
            if ((RnG & 0x4) == 0x4)
                R += 0x44;
            if ((RnG & 0x8) == 0x8)
                R += 0x88;
            //Green
            if ((RnG & 0x10) == 0x10)
                G += 0x11;
            if ((RnG & 0x20) == 0x20)
                G += 0x22;
            if ((RnG & 0x40) == 0x40)
                G += 0x44;
            if ((RnG & 0x80) == 0x80)
                G += 0x88;
            //Blue
            if ((BnA & 0x1) == 0x1)
                B += 0x11;
            if ((BnA & 0x2) == 0x2)
                B += 0x22;
            if ((BnA & 0x4) == 0x4)
                B += 0x44;
            if ((BnA & 0x8) == 0x8)
                B += 0x88;
            //Alpha
            if ((BnA & 0x10) == 0x10)
                A += 0x11;
            if ((BnA & 0x20) == 0x20)
                A += 0x22;
            if ((BnA & 0x40) == 0x40)
                A += 0x44;
            if ((BnA & 0x80) == 0x80)
                A += 0x88;

            return Color.FromArgb(A, R, G, B);
        }
        public static Color ReadRGBA8888(BinaryReader reader)
        {
            byte R = reader.ReadByte();
            byte G = reader.ReadByte();
            byte B = reader.ReadByte();
            byte A = reader.ReadByte();

            return Color.FromArgb(A, R, G, B);
        }
        public static Color ReadBGRA8888(BinaryReader reader)
        {
            byte B = reader.ReadByte();
            byte G = reader.ReadByte();
            byte R = reader.ReadByte();
            byte A = reader.ReadByte();

            return Color.FromArgb(A, R, G, B);
        }
        public static Color ReadBGRA8880(BinaryReader reader)
        {
            byte B = reader.ReadByte();
            byte G = reader.ReadByte();
            byte R = reader.ReadByte();

            return Color.FromArgb(R, G, B);
        }
        public static Color ReadRGBA8880(BinaryReader reader)
        {
            byte R = reader.ReadByte();
            byte G = reader.ReadByte();
            byte B = reader.ReadByte();

            return Color.FromArgb(R, G, B);
        }
        public static byte HalfByteToFull(byte Input)
        {
            return (byte)Math.Min(Input / 128.0f * 255, 255);
        }
        public static byte FullByteToHalf(byte Input)
        {
            return (byte)(Input / 255.0f * 128);
        }
        public static Color HalfAlphaToFull(Color input)
        {
            return Color.FromArgb(HalfByteToFull(input.A), input);
        }
        public static Color FullAlphaToHalf(Color input)
        {
            return Color.FromArgb(FullByteToHalf(input.A), input);
        }
    }
}
