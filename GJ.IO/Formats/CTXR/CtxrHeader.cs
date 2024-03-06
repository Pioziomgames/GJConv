using System.Drawing;
using static System.Buffers.Binary.BinaryPrimitives;
using static GJ.IO.IOFunctions;

namespace CtxrLib
{
    public class CtxrHeader
    {
        //Values are in big endian
        public uint Version;
        public ushort Width;
        public ushort Height;
        public ushort Field01; //Always 1/256?
        public ushort Field02;
        public ushort Field03;
        public byte Field04; //Always 1?

        public ushort Field05;
        public ushort Field06;
        public Color ColorMin;
        public Color ColorMax;
        public ushort Field09; //Always FFFF
        public ushort Field10;
        public ushort Field11;
        public ushort MipMapCount;
        //+ 88 bytes padding?
        public CtxrHeader()
        {
            DefaultValues();
        }
        public CtxrHeader(ushort width, ushort height, Color[] colors)
        {
            DefaultValues();
            Width = width;
            Height = height;
            CalculateColorBounds(colors);
        }
        public CtxrHeader(ushort width, ushort height, Color colorMin, Color colorMax)
        {
            DefaultValues();
            Width = width;
            Height = height;
            ColorMin = colorMin;
            ColorMin = colorMax;
        }
        public CtxrHeader(ushort width, ushort height)
        {
            DefaultValues();
            Width = width;
            Height = height;
        }
        private void DefaultValues()
        {
            Version = 7;
            Field01 = 256;
            Field04 = 1;

            ColorMin = Color.FromArgb(0);
            ColorMax = Color.White;
            Field09 = 0xFFFF;
            MipMapCount = 1;
        }
        public void CalculateColorBounds(Color[] ImageData)
        {
            byte lr = 255;
            byte lg = 255;
            byte lb = 255;
            byte la = 255;

            byte mr = 0;
            byte mg = 0;
            byte mb = 0;
            byte ma = 0;

            for (int i = 0; i < ImageData.Length; i++)
            {
                Color c = ImageData[i];
                if (c.R < lr)
                    lr = c.B;
                if (c.R > mr)
                    mr = c.R;

                if (c.G < lg)
                    lg = c.G;
                if (c.G > mg)
                    mg = c.G;

                if (c.B < lb)
                    lb = c.B;
                if (c.B > mb)
                    mb = c.B;

                if (c.A < la)
                    la = c.A;
                if (c.A > ma)
                    ma = c.A;
            }
            ColorMin = Color.FromArgb(la, lr, lg, lb);
            ColorMax = Color.FromArgb(ma, mr, mg, mb);
        }
        public CtxrHeader(BinaryReader reader)
        {
            Read(reader);
        }
        void Read(BinaryReader reader)
        {
            Version = ReverseEndianness(reader.ReadUInt32());
            Width = ReverseEndianness(reader.ReadUInt16());
            Height = ReverseEndianness(reader.ReadUInt16());
            Field01 = reader.ReadUInt16();
            Field02 = reader.ReadUInt16();
            Field03 = reader.ReadUInt16();
            Field04 = reader.ReadByte();
            Field05 = reader.ReadUInt16();
            Field06 = reader.ReadUInt16();
            ColorMin = HalfAlphaToFull(ReadRGBA8888(reader));
            ColorMax = HalfAlphaToFull(ReadRGBA8888(reader));
            Field09 = reader.ReadUInt16();
            Field10 = reader.ReadUInt16();
            Field11 = reader.ReadUInt16();
            MipMapCount = ReverseEndianness(reader.ReadUInt16());
            Align(reader, 128);
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(ReverseEndianness(Version));
            writer.Write(ReverseEndianness(Width));
            writer.Write(ReverseEndianness(Height));
            writer.Write(Field01);
            writer.Write(Field02);
            writer.Write(Field03);
            writer.Write(Field04);
            writer.Write(Field05);
            writer.Write(Field06);
            WriteRGBA8888(writer,FullAlphaToHalf(ColorMin));
            WriteRGBA8888(writer, FullAlphaToHalf(ColorMax));
            writer.Write(Field09);
            writer.Write(Field10);
            writer.Write(Field11);
            writer.Write(ReverseEndianness(MipMapCount));
            Align(writer, 128);
        }
    }
}
