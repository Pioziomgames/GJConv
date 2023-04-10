using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TmxLib.TmxEnums;
using static GJ.IO.IOFunctions;

namespace TmxLib
{
    public class TmxPalette
    {
        public List<Color> Colors;
        public TmxPalette(BinaryReader reader, TmxPixelFormat PixelFormat, TmxPixelFormat PaletteFormat)
        {
            Colors = new List<Color>();
            int ColorCount = 16;
            if (PixelFormat == TmxPixelFormat.PSMT8 || PixelFormat == TmxPixelFormat.PSMT8H)
                ColorCount = 256;

            for (int i = 0; i < ColorCount; i++)
            {
                Color NC;
                if (PaletteFormat == TmxPixelFormat.PSMTC32)
                {
                    NC = ReadRGBA8888(reader);
                    NC = Color.FromArgb(HalfByteToFull(NC.A), NC.R, NC.G, NC.B);
                }
                else if (PaletteFormat == TmxPixelFormat.PSMTC24)
                {
                    NC = ReadRGBA8880(reader);
                }
                else if (PaletteFormat == TmxPixelFormat.PSMTC16 || PaletteFormat == TmxPixelFormat.PSMTC16S)
                {
                    NC = ReadRGBA5551(reader);
                    NC = Color.FromArgb(255, NC.R, NC.G, NC.B);
                }
                else
                    throw new Exception("Unimplemented PaletteFormat");

                Colors.Add(NC);
            }
            Colors = SortedColors();
        }
        public TmxPalette(List<Color> colors)
        {
            Colors = colors;
        }
        public void Write(BinaryWriter writer, TmxPixelFormat PaletteFormat)
        {
            List<Color> NewColors = Colors;
            if (Colors.Count < 16)
                while (NewColors.Count < 16)
                    NewColors.Add(Color.Transparent);
            else if (Colors.Count < 256)
                while (NewColors.Count < 256)
                    NewColors.Add(Color.Transparent);

            NewColors = SortedColors();
            for (int i = 0; i < NewColors.Count;i++)
            {
                if (PaletteFormat == TmxPixelFormat.PSMTC32)
                {
                    Color NC = Color.FromArgb(FullByteToHalf(NewColors[i].A), NewColors[i].R, NewColors[i].G, NewColors[i].B);
                    WriteRGBA8888(writer, NC);
                }
                else if (PaletteFormat == TmxPixelFormat.PSMTC24)
                {
                    WriteRGBA8880(writer, NewColors[i]);
                }
                else if (PaletteFormat == TmxPixelFormat.PSMTC16 || PaletteFormat == TmxPixelFormat.PSMTC16S)
                {
                    Color NC = Color.FromArgb(1, NewColors[i].R, NewColors[i].G, NewColors[i].B);
                    WriteRGBA5551(writer, NC);
                }
                else
                    throw new Exception("Unimplemented PaletteFormat");
            }
        }
        public List<Color> SortedColors()
        {
            if (Colors.Count != 256)
                return Colors;
            List<Color> NewColors = Colors;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Color tmp = NewColors[8 + j + i * 32];
                    NewColors[8 + j + i * 32] = NewColors[16 + j + i * 32];
                    NewColors[16 + j + i * 32] = tmp;
                }
            }
            return Colors;
        }
    }
}
