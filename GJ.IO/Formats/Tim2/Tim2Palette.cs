using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GJ.IO.IOFunctions;

namespace Tim2Lib
{
    public class Tim2Palette
    {
        public List<Color> Colors;

        public Tim2Palette(List<Color> colors)
        {
            Colors = colors;
        }
        public Tim2Palette(BinaryReader reader, int Size, byte Type)
        {
            Read(reader, Size, Type);
        }
        public void Read(BinaryReader reader, int Size, byte Type)
        {
            Colors = new();
            byte CType = (byte)(7 & Type);

            for (int i = 0; i < Size; i++)
            {
                Color NC = new();
                if (CType == 1)
                    NC = ReadRGBA5551(reader);
                else if (CType == 2)
                    NC = ReadRGBA8880(reader);
                else if (CType == 3)
                    NC = ReadRGBA8888(reader);

                Colors.Add(Color.FromArgb(HalfByteToFull(NC.A), NC.R, NC.G, NC.B));
            }
            if (Size < 16)
            {
                if (CType == 3)
                    reader.BaseStream.Position += (16 - Size) * 4;
                else if (CType == 2)
                    reader.BaseStream.Position += (16 - Size) * 3;
                else
                    reader.BaseStream.Position += (16 - Size) * 2;
            }
            else if (Size < 256 && Size != 16)
            {
                if (CType == 3)
                    reader.BaseStream.Position += (256 - Size) * 4;
                if (CType == 2)
                    reader.BaseStream.Position += (256 - Size) * 3;
                else
                    reader.BaseStream.Position += (256 - Size) * 2;
            }

            if ((Type & 128) == 0)
                SortColors(Type);

        }
        public void SortColors(byte Type)
        {
            if (Colors.Count != 256)
                return;

            switch ((Tim2Enums.Tim2BPP)(Type & ~128))
            {
                case Tim2Enums.Tim2BPP.RGBA5551:
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                Color tmp = Colors[4 + j + i * 16];
                                Colors[4 + j + i * 16] = Colors[8 + j + i * 16];
                                Colors[8 + j + i * 16] = tmp;
                            }
                        }
                        break;
                    }
                case Tim2Enums.Tim2BPP.RGBA8888:
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                Color tmp = Colors[8 + j + i * 32];
                                Colors[8 + j + i * 32] = Colors[16 + j + i * 32];
                                Colors[16 + j + i * 32] = tmp;
                            }
                        }
                        break;
                    }
            }
        }
        public void Write(BinaryWriter writer, byte Type)
        {
            byte CType = (byte)(7 & Type);
            for (int i = 0; i < Colors.Count; i++)
            {
                Color NC = Color.FromArgb(FullByteToHalf(Colors[i].A), Colors[i].R, Colors[i].G, Colors[i].B);

                if (CType == 1)
                    WriteRGBA5551(writer,NC);
                else if (CType == 2)
                    WriteRGBA8880(writer, NC);
                else if (CType == 3)
                    WriteRGBA8888(writer, NC);
            }
            if (Colors.Count < 16)
            {
                if (CType == 3)
                    Seek(writer, (16 - Colors.Count) * 4);
                else if (CType == 1)
                    Seek(writer, (16 - Colors.Count) * 3);
                else
                    Seek(writer, (16 - Colors.Count) * 2);
            }
            else if (Colors.Count < 256 && Colors.Count != 16)
            {
                if (CType == 3)
                    Seek(writer,(256 - Colors.Count) * 4);
                else if (CType == 2)
                    Seek(writer, (256 - Colors.Count) * 3);
                else
                    Seek(writer, (256 - Colors.Count) * 2);
            }
        }
    }
}
