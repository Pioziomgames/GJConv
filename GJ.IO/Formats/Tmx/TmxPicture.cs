using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TmxLib.TmxEnums;

namespace TmxLib
{
    public class TmxPictureHeader
    {
        public byte PaletteCount;
        public TmxPixelFormat PaletteFormat;
        public ushort Width;
        public ushort Height;
        public TmxPixelFormat PixelFormat;
        public byte MipMapCount;
        public byte MipK;
        public byte MipL;
        public TmxWrapMode TexWrapMode;
        public int UserTextureId;
        public int UserClutId;
        string userComment;
        public string UserComment {
        get => userComment;

        set
            {
                if (value.Length > 28)
                    throw new Exception("UserComment cannot be longer than 28 characters");
                userComment = value;
            }
        }
        
        public TmxPictureHeader()
        {
            PaletteCount = 0;
            PaletteFormat = TmxPixelFormat.PSMTC32;
            Width = 0;
            Height = 0;
            PixelFormat = TmxPixelFormat.PSMTC32;
            MipMapCount = 0;
            MipK = 0;
            MipL = 0;
            TexWrapMode = TmxWrapMode.Off;
            UserTextureId = 0;
            UserClutId = 0;
            UserComment = String.Empty;
        }
        public TmxPictureHeader(BinaryReader reader)
        {
            Read(reader);
        }
        void Read(BinaryReader reader)
        {
            PaletteCount = reader.ReadByte();
            PaletteFormat = (TmxPixelFormat)reader.ReadByte();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            PixelFormat = (TmxPixelFormat)reader.ReadByte();
            MipMapCount = reader.ReadByte();
            MipK = reader.ReadByte();
            MipL = reader.ReadByte();
            reader.BaseStream.Position++;
            TexWrapMode = (TmxWrapMode)reader.ReadByte();
            UserTextureId = reader.ReadInt32();
            UserClutId = reader.ReadInt32();
            userComment = new string(reader.ReadChars(28)).Replace("\0","");
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(PaletteCount);
            writer.Write((byte)PaletteFormat);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write((byte)PixelFormat);
            writer.Write(MipMapCount);
            writer.Write(MipK);
            writer.Write(MipL);
            writer.BaseStream.Position++;
            writer.Write((byte)TexWrapMode);
            writer.Write(UserTextureId);
            writer.Write(UserClutId);
            writer.Write(UserComment.ToArray());
            for (int i = 0; i < 28 - UserComment.Length; i++)
                writer.Write((byte)0);
        }
    }
    public class TmxPicture
    {
        public TmxPictureHeader Header;
        public TmxPalette? Palette;
        public TmxImage Image;

        public void FullColorToIndexed()
        {
            if (Palette == null)
            {
                Palette = new(Image.FullColorToIndexed());
                Header.PaletteFormat = Header.PixelFormat;
                Header.PaletteCount = 1;

                if (Palette.Colors.Count <= 16)
                    Header.PixelFormat = TmxPixelFormat.PSMT4;
                else if (Palette.Colors.Count > 256)
                    throw new Exception("Palettes with over 256 colors are not supported");
                else
                    Header.PixelFormat = TmxPixelFormat.PSMT8;
            }
        }
        public void IndexedToFullColor()
        {
            if (Palette != null)
            {
                Header.PaletteCount = 0;
                Header.PixelFormat = Header.PaletteFormat;
                Header.PaletteFormat = TmxPixelFormat.PSMTC32;
                Image.IndexedToFullColor(Palette.Colors);
                Palette = null;
            }
        }
        public TmxPicture(BinaryReader reader)
        {
            Read(reader);
        }
        public TmxPicture(TmxPictureHeader header, TmxImage image, TmxPalette palette)
        {
            Palette = palette;
            Header = header;
            Image = image;
        }
        public TmxPicture(TmxPictureHeader header, TmxImage image)
        {
            Header = header;
            Image = image;
        }
        void Read(BinaryReader reader)
        {
            Header = new TmxPictureHeader(reader);

            if (Header.PaletteCount > 0)
                Palette = new(reader, Header.PixelFormat, Header.PaletteFormat);

            Image = new(reader, Header.PixelFormat, Header.Width, Header.Height);
        }
        public void Write(BinaryWriter writer)
        {
            if (Palette != null)
            {
                Header.PaletteCount = 1;
            }
            Header.Write(writer);

            if (Palette != null)
                Palette.Write(writer, Header.PaletteFormat);

            Image.Write(writer, Header.PixelFormat);
        }
    }
}
