using GJ.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace TgaLib
{
    public class TgaFile : Texture
    {
        public TgaHeader Header;
        public TgaPalette? Palette;
        public TgaImageData ImageData;
        public TgaFile(TgaHeader header, Color[] Pixels)
        {
            Header = header;
            ImageData = new(Pixels, Header.Height, Header.Width);
        }
        public TgaFile(TgaHeader header, Color[][] Pixels)
        {
            Header = header;
            ImageData = new(Pixels);
        }
        public TgaFile(TgaHeader header, byte[] Indexes, Color[] Colors)
        {
            Header = header;
            ImageData = new(Indexes, Header.Height, Header.Width);
            Palette = new TgaPalette(Colors);
        }
        public TgaFile(TgaHeader header, byte[][] Indexes, Color[] Colors)
        {
            Header = header;
            ImageData = new(Indexes);
            Palette = new TgaPalette(Colors);
        }
        public TgaFile(string Path) : base(Path) { }
        public TgaFile(byte[] Data) : base(Data) { }
        public TgaFile(BinaryReader reader) : base(reader) { }
        
        public Color[] GetAllPixels()
        {
            return ImageData.AllPixels(Header.FlipHorizontal, Header.FlipVertical);
        }
        public byte[] GetAllIndexes()
        {
            return ImageData.AllIndexes(Header.FlipHorizontal, Header.FlipVertical);
        }
        public void IndexedToFullColor()
        {
            if (ImageData.PixelIndexes == null || Palette == null)
                return;
            Color[][] Pixels = new Color[Header.Height][];
            for (int y = 0; y < ImageData.PixelIndexes.Length; y++)
            {
                Pixels[y] = new Color[Header.Width];
                for (int x = 0; x < ImageData.PixelIndexes.Length; x++)
                {
                    Pixels[y][x] = Palette.Colors[ImageData.PixelIndexes[y][x]];
                }
            }
            ImageData.SetPixels(Pixels);
            Palette = null;
        }
        public void FullColorToIndexed()
        {
            if (ImageData.Pixels == null)
                return;
            List<Color> Colors = new();
            byte[][] Indexes = new byte[Header.Height][];

            for (int y = 0; y < Header.Height; y++)
            {
                Indexes[y] = new byte[Header.Width];
                for (int x = 0; x < Header.Width; x++)
                {
                    if (!Colors.Contains(ImageData.Pixels[y][x]))
                        Colors.Add(ImageData.Pixels[y][x]);
                    Indexes[y][x] = ((byte)Colors.IndexOf(ImageData.Pixels[y][x]));
                }
            }
            Palette = new(Colors.ToArray());
            Header.PaletteSize = (ushort)Colors.Count;
            ImageData.SetIndexes(Indexes);
        }
        internal override void Read(BinaryReader reader)
        {
            Header = new(reader);
            if (Header.UsesPalette)
                Palette = new(reader, Header);
            else
                Palette = null;

            ImageData = new(reader, Header);
        }
        internal override void Write(BinaryWriter writer)
        {
            Header.Write(writer);
            if (Palette != null)
                Palette.Write(writer, Header);
            ImageData.Write(writer, Header);
        }

        public override int GetWidth()
        {
            return Header.Width;
        }

        public override int GetHeight()
        {
            return Header.Height;
        }

        public override int GetMipMapCount()
        {
            return 1;
        }

        public override PixelFormat GetPixelFormat()
        {
            switch (Header.ImageFormat)
            {
                case TgaFormat.GrayScale:
                case TgaFormat.RLEGrayScale:
                    return PixelFormat.Format16bppGrayScale;
                case TgaFormat.Indexed:
                case TgaFormat.RLEIndexed:
                case TgaFormat.HDRLEIndexed:
                    return PixelFormat.Format8bppIndexed;
                default:
                    return PixelFormat.Format32bppArgb;
            }
        }

        public override Color[] GetPalette()
        {
            if (Palette != null)
                return Palette.Colors;
            else
                return Array.Empty<Color>();
        }

        public override Color[] GetPixelData()
        {
            if (ImageData.Pixels != null)
                return GetAllPixels();
            else
                return GetPixelDataFromIndexData();
        }

        public override byte[] GetIndexData()
        {
            if (ImageData.PixelIndexes != null)
                return GetAllIndexes();
            else
                return Array.Empty<byte>();
        }
    }
}
