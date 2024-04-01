using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgaLib
{
    public enum TgaFormat : byte
    {
        None         = 0x00,
        Indexed      = 0x01,
        RGB          = 0x02,
        GrayScale    = 0x03,
        //run-length encoded
        RLEIndexed   = 0x09,
        RLERGB       = 0x0A,
        RLEGrayScale = 0x0B,
        // Huffman Delta run-length encoded
        HDRLEIndexed = 0x20,
        HDRLERGB     = 0x21,
    }
    public class TgaHeader
    {
        public byte UserDataSize { get { return (byte)UserData.Length; } }
        public bool UsesPalette;
        public TgaFormat ImageFormat;
        public ushort PaletteStartIndex;
        public ushort PaletteSize;
        public byte PaletteDepth;
        public ushort Xorigin;
        public ushort Yorigin;
        public ushort Width;
        public ushort Height;
        public byte BitsPerPixel;
        public byte AlphaDepth;
        public bool FlipHorizontal;
        public bool FlipVertical;
        public byte[] UserData;
        public TgaHeader()
        {
            UsesPalette = false;
            ImageFormat = TgaFormat.None;
            PaletteSize = 0;
            PaletteDepth = 0;
            Xorigin = 0;
            Yorigin = 0;
            Width = 0;
            Height = 0;
            BitsPerPixel = 0;
            AlphaDepth = 0;
            FlipHorizontal = false;
            FlipVertical = false;
            UserData = Array.Empty<byte>();
        }
        public TgaHeader(BinaryReader reader)
        {
            Read(reader);
        }
        void Read(BinaryReader reader)
        {
            byte userDataSize = reader.ReadByte();
            UsesPalette = reader.ReadBoolean();
            ImageFormat = (TgaFormat)reader.ReadByte();
            PaletteStartIndex = reader.ReadUInt16();
            PaletteSize = reader.ReadUInt16();
            PaletteDepth = reader.ReadByte();
            Xorigin = reader.ReadUInt16();
            Yorigin = reader.ReadUInt16();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            BitsPerPixel = reader.ReadByte();
            byte descryptor = reader.ReadByte();
            AlphaDepth = (byte)(descryptor >> 4);
            FlipHorizontal = ((descryptor >> 4) & 0b00000001) == 0b00000001;
            FlipVertical = ((descryptor >> 4) & 0b00000010) == 0b00000010;
            UserData = reader.ReadBytes(userDataSize);
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(UserDataSize);
            writer.Write(UsesPalette);
            writer.Write((byte)ImageFormat);
            writer.Write(PaletteStartIndex);
            writer.Write(PaletteSize);
            writer.Write(PaletteDepth);
            writer.Write(Xorigin);
            writer.Write(Yorigin);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(BitsPerPixel);
            byte descryptor = (byte)((AlphaDepth << 4) | ((FlipHorizontal ? 1 : 0) << 1) | (FlipVertical ? 1 : 0));
            writer.Write(descryptor);
            writer.Write(UserData);
        }
    }
}
