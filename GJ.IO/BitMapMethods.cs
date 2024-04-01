using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using GimLib;
using Tim2Lib;
using TmxLib;
using TxnLib;
using TgaLib;
using CtxrLib;
using DDSLib;
using TasofroLib;
using GimLib.Chunks;
using static GimLib.GimEnums;
using static Tim2Lib.Tim2Enums;
using static TmxLib.TmxEnums;
using static TxnLib.RmdEnums;
using static DDSLib.DDSEnums;
using ImageProcessor.Imaging.Quantizers;
using ImageProcessor.Imaging;

namespace GJ.IO
{
    public class BitMapMethods
    {
#pragma warning disable CA1069 // Enums values should not be duplicated
        public enum ImgType
        {
            png,
            tm2,
            gim,
            tmx,
            jpeg,
            jpg = 4,
            jfif = 4,
            bmp,
            gif,
            tif,
            tiff = 7,
            tga,
            txn,
            rwtex = 9,
            dds,
            ctxr,
            txtr = 11,
            tfbm,
            tfpa,
        }
#pragma warning restore CA1069 // Enums values should not be duplicated
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public static ImgType CheckFormat(BinaryReader reader)
        {
            uint MAGIC = reader.ReadUInt32();
            reader.BaseStream.Position = 0;
            if (MAGIC == CtxrFile.MAGIC)
                return ImgType.ctxr;
            else if (MAGIC == DDSFile.MAGIC)
                return ImgType.dds;
            else if (MAGIC == GimFile.MAGIC)
                return ImgType.gim;
            else if (MAGIC == TFBMFile.MAGIC)
                return ImgType.tfbm;
            else if (MAGIC == TFPAFile.MAGIC)
                return ImgType.tfpa;
            else if (MAGIC == Tim2File.MAGIC)
                return ImgType.tm2;
            else if (MAGIC == (uint)RmdChunkType.TextureNative)
                return ImgType.txn;

            reader.BaseStream.Position = 8;
            MAGIC = reader.ReadUInt32();
            reader.BaseStream.Position = 0;
            if (MAGIC == TmxFile.MAGIC)
                return ImgType.tmx;

            reader.BaseStream.Position = 2;
            byte tgaImageType = reader.ReadByte();
            reader.BaseStream.Position = 16;
            byte tgaBpp = reader.ReadByte();
            reader.BaseStream.Position = 0;
            if (Enum.IsDefined(typeof(TgaFormat), tgaImageType) &&
                (tgaBpp == 32 || tgaBpp == 24 || tgaBpp == 16 || tgaBpp == 8))
                return ImgType.tga;

            return ImgType.png;
        }
        public static Bitmap[] ImportBitmaps(string path, ImgType Type)
        {
            return Type switch
            {
                ImgType.ctxr => GetCtxrBitmaps(path),
                _ => new Bitmap[] { ImportBitmap(path, Type) },
            };
        }
        public static Bitmap ImportBitmap(string path, ImgType Type)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                return ConvertToBitmap(reader, Type);
        }
        public static void ExportBitmap(string path, Bitmap Image, ImgType Type, short TmxUserId = 0, string TmxUserComment = "", int TmxUserTextureId = 0, int TmxUserClutId = 0, bool TgaFlipHorizontal = false, bool TgaFlipVertical = false, bool GimPSPOrder = false, int MipMapCount = 1)
        {
            switch (Type)
            {
                case ImgType.gif: Image.Save(path, ImageFormat.Gif); break;
                case ImgType.bmp: Image.Save(path, ImageFormat.Bmp); break;
                case ImgType.tif: Image.Save(path, ImageFormat.Tiff); break;
                case ImgType.jpg: Image.Save(path, ImageFormat.Jpeg); break;
                case ImgType.tm2: Tim2FromBitmap(Image).Save(path); break;
                case ImgType.gim:
                    GimFile gim = GimFromBitmap(Image);
                    if (GimPSPOrder)
                    {
                        GimImage img = (GimImage)gim.FileChunk.GatherChildrenOfType(GimChunkType.GimImage)[0];
                        img.ImgInfo.Order = GimOrder.PSPImage;
                    }
                    gim.Save(path);
                    break;
                case ImgType.tmx:
                    TmxFile tmx = TmxFromBitmap(Image);
                    tmx.UserId = TmxUserId;
                    tmx.Picture.Header.UserComment = TmxUserComment;
                    tmx.Picture.Header.UserClutId = TmxUserClutId;
                    tmx.Picture.Header.UserTextureId = TmxUserTextureId;
                    tmx.Save(path);
                    break;
                case ImgType.tga:
                    TgaFile tga = TgaFromBitmap(Image);
                    tga.Header.FlipHorizontal = TgaFlipHorizontal;
                    tga.Header.FlipVertical = TgaFlipVertical;
                    tga.Save(path);
                    break;
                case ImgType.txn:
                    RwTextureNative txn = TxnFromBitmap(Image, Path.GetFileNameWithoutExtension(path));
                    using (BinaryWriter writer = new(File.Create(path)))
                    {
                        RmdChunk.Write(txn, writer);
                        writer.Flush();
                        writer.Close();
                    }
                    break;
                case ImgType.ctxr: CtxrFromBitmap(Image, 7).Save(path); break;
                case ImgType.dds: DdsFromBitmap(Image).Save(path); break;
                default: Image.Save(path,ImageFormat.Png);break;
            }
        }
        public static Bitmap ImportBitmap(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                return ImportBitmap(reader);
            }
        }
        public static Bitmap ImportBitmap(BinaryReader reader)
        {
            ImgType type = CheckFormat(reader);
            return ConvertToBitmap(reader, type);
        }
        public static Bitmap ImportBitmap(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return ImportBitmap(reader);
                }
            }
        }
        public static byte[] GetPixelIndices(Bitmap image)
        {
            BitmapData ImageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

            byte[] indices = new byte[ImageData.Height * ImageData.Width];

            unsafe
            {
                byte* p = (byte*)ImageData.Scan0;
                Parallel.For(0, ImageData.Height, y =>
                {
                    for (int x = 0; x < ImageData.Width; x++)
                    {
                        int offset = y * ImageData.Stride + x;
                        indices[x + y * ImageData.Width] = (p[offset]);
                    }
                });
            }
            image.UnlockBits(ImageData);

            return indices;
        }
        public static Color[] GetPaletteData(Bitmap image, int paletteColorLimit)
        {
            Color[] palette = new Color[paletteColorLimit];

            for (int i = 0; i < image.Palette.Entries.Length; i++)
            {
                if (i == paletteColorLimit)
                    break;

                palette[i] = image.Palette.Entries[i];
            }

            return palette;
        }
        public static Color[] GetPixels(Bitmap image)
        {
            Color[] Colors = new Color[image.Width * image.Height];
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            int pixelSize = Image.GetPixelFormatSize(image.PixelFormat) / 8;
            IntPtr pointer = data.Scan0;
            int Width = image.Width;
            Color[] Palette = image.Palette.Entries;
            if (image.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                Parallel.For(0, image.Height, y =>
                 {
                     for (int x = 0; x < Width; x++)
                     {
                         int offset = y * data.Stride + x;
                         byte index = Marshal.ReadByte(pointer + offset);
                         Colors[y * Width + x] = Palette[index];
                     }
                 });
            }
            else if (image.PixelFormat == PixelFormat.Format4bppIndexed)
            {
                Parallel.For(0, image.Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = y * data.Stride + x / 2;
                        byte b = Marshal.ReadByte(pointer + offset);
                        int index;
                        if (x % 2 == 0)
                            index = b & 0x0F;
                        else
                            index = (b & 0xF0) >> 4;
                        Colors[y * Width + x] = Palette[index];
                    }
                });
            }
            else if (image.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                Parallel.For(0, image.Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = y * data.Stride + x / 8;
                        byte b = Marshal.ReadByte(pointer + offset);
                        int index = (b >> (7 - (x % 8))) & 1;
                        Colors[y * Width + x] = image.Palette.Entries[index];
                    }
                });
            }
            else if (image.PixelFormat == PixelFormat.Format24bppRgb)
            {
                byte[] pixelData = new byte[image.Height * data.Stride];
                Marshal.Copy(pointer, pixelData, 0, pixelData.Length);
                Parallel.For(0, image.Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = y * data.Stride + x * 3;
                        Color color = Color.FromArgb(pixelData[offset + 2], pixelData[offset + 1], pixelData[offset]);
                        Colors[y * Width + x] = color;
                    }
                });
            }
            else if (image.PixelFormat == PixelFormat.Format16bppRgb565)
            {
                Parallel.For(0, image.Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = y * data.Stride + x * 2;
                        ushort pixelValue = (ushort)Marshal.ReadInt16(pointer + offset);
                        int r = (pixelValue >> 11) & 0x1F;
                        int g = (pixelValue >> 5) & 0x3F;
                        int b = pixelValue & 0x1F;
                        r = (r << 3) | (r >> 2);
                        g = (g << 2) | (g >> 4);
                        b = (b << 3) | (b >> 2);
                        Color pixelColor = Color.FromArgb(r, g, b);
                        Colors[y * Width + x] = pixelColor;
                    }
                });
            }
            else if (image.PixelFormat == PixelFormat.Format16bppArgb1555)
            {
                Parallel.For(0, image.Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = y * data.Stride + x * 2;
                        ushort pixelValue = (ushort)Marshal.ReadInt16(pointer + offset);
                        int a = (pixelValue >> 15) & 0x01;
                        int r = (pixelValue >> 10) & 0x1F;
                        int g = (pixelValue >> 5) & 0x1F;
                        int b = pixelValue & 0x1F;
                        r = (r << 3) | (r >> 2);
                        g = (g << 3) | (g >> 2);
                        b = (b << 3) | (b >> 2);
                        int aScaled = a * 255;
                        Color pixelColor = Color.FromArgb(aScaled, r, g, b);
                        Colors[y * Width + x] = pixelColor;
                    }
                });
            }
            else if (image.PixelFormat == PixelFormat.Format16bppRgb555)
            {
                Parallel.For(0, image.Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = y * data.Stride + x * 2;
                        ushort pixelValue = (ushort)Marshal.ReadInt16(pointer + offset);
                        int r = (pixelValue >> 10) & 0x1F;
                        int g = (pixelValue >> 5) & 0x1F;
                        int b = pixelValue & 0x1F;
                        r = (r << 3) | (r >> 2);
                        g = (g << 3) | (g >> 2);
                        b = (b << 3) | (b >> 2);
                        Color pixelColor = Color.FromArgb(r, g, b);
                        Colors[y * Width + x] = pixelColor;
                    }
                });
            }
            else if (image.PixelFormat == PixelFormat.Format16bppGrayScale)
            {
                Parallel.For(0, image.Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = y * data.Stride + x * 2;
                        ushort pixelValue = (ushort)Marshal.ReadInt16(pointer + offset);
                        Color pixelColor = Color.FromArgb(pixelValue, pixelValue, pixelValue);
                        Colors[y * Width + x] = pixelColor;
                    }
                });
            }
            else
            {
                Parallel.For(0, image.Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = y * data.Stride + x * pixelSize;
                        byte[] pixel = new byte[pixelSize];
                        Marshal.Copy(pointer + offset, pixel, 0, pixelSize);
                        Color color = Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
                        Colors[y * Width + x] = color;
                    }
                });
            }

            image.UnlockBits(data);
            return Colors;
        }
        public static CtxrFile CtxrFromBitmap(Bitmap image, int MipMapCount = 1)
        {
            Color[] pixels = GetPixels(image);
            CtxrFile output =
                MipMapCount > 1 ? new CtxrFile((ushort)image.Width, (ushort)image.Height,
                    GenerateMipMaps(pixels, image.Width, image.Height, MipMapCount))
                    : new CtxrFile((ushort)image.Width, (ushort)image.Height, pixels);
            return output;
        }
        public static TgaFile TgaFromBitmap(Bitmap image)
        {
            TgaHeader Header = new();
            Header.Width = (ushort)image.Width;
            Header.Height = (ushort)image.Width;

            switch (image.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                    Header.BitsPerPixel = 8;
                    Header.PaletteDepth = 32;
                    Header.AlphaDepth = 0;
                    Header.ImageFormat = TgaFormat.Indexed;
                    Header.UsesPalette = true;
                    break;
                case PixelFormat.Format16bppGrayScale:
                    Header.BitsPerPixel = 8;
                    Header.ImageFormat = TgaFormat.GrayScale;
                    Header.AlphaDepth = 0;
                    break;
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    Header.BitsPerPixel = 15;
                    Header.ImageFormat = TgaFormat.RGB;
                    Header.AlphaDepth = 0;
                    break;
                case PixelFormat.Format16bppArgb1555:
                    Header.BitsPerPixel = 16;
                    Header.ImageFormat = TgaFormat.RGB;
                    Header.AlphaDepth = 1;
                    break;
                case PixelFormat.Format24bppRgb:
                    Header.BitsPerPixel = 24;
                    Header.ImageFormat = TgaFormat.RGB;
                    Header.AlphaDepth = 0;
                    break;
                default:
                    Header.BitsPerPixel = 32;
                    Header.AlphaDepth = 8;
                    Header.ImageFormat = TgaFormat.RGB;
                    break;
            }
            Color[] Pixels = GetPixels(image).ToArray();
            TgaFile Output = new(Header, Pixels);

            if (Header.UsesPalette)
                Output.FullColorToIndexed();

            return Output;
        }
        public static Tim2File Tim2FromBitmap(Bitmap image)
        {
            List<Tim2Picture> Pictures = new();

            PictureHeader Header = new();
            Header.Width = (ushort)image.Width;
            Header.Height = (ushort)image.Height;

            Header.PixelFormat = image.PixelFormat switch
            {
                PixelFormat.Format24bppRgb => Tim2BPP.RGBA8880,
                PixelFormat.Format16bppArgb1555 => Tim2BPP.RGBA5551,
                PixelFormat.Format8bppIndexed => Tim2BPP.INDEX8,
                PixelFormat.Format4bppIndexed => Tim2BPP.INDEX4,
                _ => Tim2BPP.RGBA8888,
            };
            Color[] Pixels = GetPixels(image);
            Tim2ImageData ImageData = new(Pixels);
            Tim2Picture OutputPic = new(Header, ImageData);
            if ((image.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
            {
                OutputPic.Header.PaletteType = ((byte)Tim2BPP.RGBA8888) | 128;
                OutputPic.FullColorToIndexed();
                if (OutputPic.Palette.Colors.Count <= 16)
                    OutputPic.Header.PixelFormat = Tim2BPP.INDEX4;
                else if (OutputPic.Palette.Colors.Count <= 256)
                    OutputPic.Header.PixelFormat = Tim2BPP.INDEX8;
            }

            Pictures.Add(OutputPic);

            Tim2File Output = new(4, Tim2Alignment.Align16, Pictures);
            return Output;
        }
        public static RwTextureNative TxnFromBitmap(Bitmap image, string TextureName = "")
        {
            PS2PixelFormat Format = PS2PixelFormat.PSMTC32;
            switch (image.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                    Format = PS2PixelFormat.PSMT4;
                    break;
                case PixelFormat.Format8bppIndexed:
                    Format = PS2PixelFormat.PSMT8;
                    break;
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppRgb555:
                    Format = PS2PixelFormat.PSMTC16;
                    break;
            }
            RasterInfoStruct RasterInfo = new(image.Width, image.Height, Format);
            RasterDataStruct RasterData;
            if (Format == PS2PixelFormat.PSMT4 || Format == PS2PixelFormat.PSMT8)
            {
                image = LimitColors(image);
                Color[] Palette = GetPaletteData(image, Format == PS2PixelFormat.PSMT4 ? 16 : 256);
                byte[] Indexes = GetPixelIndices(image);
                RasterData = new(RasterInfo, Indexes, Palette );
            }
            else
                RasterData = new(RasterInfo, GetPixels(image));

            RwTextureNative TXN = new(TextureName, RwPlatformId.PS2, 4354, RasterInfo, RasterData);
            return TXN;
        }
        public static Bitmap[] GetCtxrBitmaps(string path)
        {
            CtxrFile InCtxr = new(path);
            return GetCtxrBitmaps(InCtxr);
        }
        public static Bitmap GetCtxrBitmap(string path)
        {
            CtxrFile InCtxr = new(path);
            return GetCtxrBitmaps(InCtxr)[0];
        }
        public static Bitmap GetCtxrBitmap(BinaryReader reader)
        {
            CtxrFile InCtxr = new(reader);
            return GetCtxrBitmaps(InCtxr)[0];
        }
        public static Bitmap GetTfbmBitmap(BinaryReader reader)
        {
            TFBMFile InTfbm = new(reader);
            return GetTfbmBitmap(InTfbm);
        }
        public static Bitmap GetTxnBitmap(string path)
        {
            using (BinaryReader reader = new(File.OpenRead(path)))
            {
                RwTextureNative TXN = (RwTextureNative)RmdChunk.Read(reader);
                return GetTxnBitmap(TXN);
            }
        }
        public static Bitmap GetTxnBitmap(BinaryReader reader)
        {
            RwTextureNative TXN = (RwTextureNative)RmdChunk.Read(reader);
            return GetTxnBitmap(TXN);
        }
        public static Bitmap GetTim2Bitmap(string path)
        {
            Tim2File InTm2 = new(path);
            return GetTim2Bitmap(InTm2);
        }
        public static Bitmap GetTim2Bitmap(BinaryReader reader)
        {
            Tim2File InTm2 = new(reader);
            return GetTim2Bitmap(InTm2);
        }
        public static Bitmap GetTgaBitmap(string path)
        {
            TgaFile InTga = new(path);
            return GetTgaBitmap(InTga);
        }
        public static Bitmap GetTgaBitmap(BinaryReader reader)
        {
            TgaFile InTga = new(reader);
            return GetTgaBitmap(InTga);
        }
        public static Bitmap GetTim2Bitmap(byte[] Data)
        {
            Tim2File InTm2 = new(Data);
            return GetTim2Bitmap(InTm2);
        }
        public static Bitmap GetTgaBitmap(byte[] Data)
        {
            TgaFile InTga = new(Data);
            return GetTgaBitmap(InTga);
        }
        public static Bitmap GetCtxrBitmap(CtxrFile InCtxr)
        {
            return GetCtxrBitmaps(InCtxr)[0];
        }
        public static Bitmap ConvertToBitmap(BinaryReader reader, ImgType format)
        {
            switch(format)
            {
                case ImgType.txn:
                    return GetTxnBitmap(reader);
                case ImgType.tiff:
                case ImgType.bmp:
                case ImgType.gif:
                case ImgType.jpg:
                case ImgType.png:
                    return new Bitmap(reader.BaseStream);
                default:
                {
                    Texture tex;
                    switch (format)
                    {
                        case ImgType.tm2: tex = new Tim2File(reader); break;
                        case ImgType.tmx: tex = new TmxFile(reader); break;
                        case ImgType.tga: tex = new TgaFile(reader); break;
                        case ImgType.ctxr: tex = new CtxrFile(reader); break;
                        case ImgType.tfbm: tex = new TFBMFile(reader); break;
                        case ImgType.tfpa: tex = new TFPAFile(reader); break;
                        case ImgType.dds: tex = new DDSFile(reader); break;
                        case ImgType.gim: tex = new GimFile(reader); break;
                        default: throw new Exception("Unimplemented format: " + format);
                    }
                    return ConvertToBitmap(tex);
                }
            }
        }
        public static Bitmap ConvertToBitmap(Texture tex)
        {
            Bitmap image;
            switch(tex.GetPixelFormat())
            {
                case PixelFormat.Format4bppIndexed:
                {
                    image = new(tex.GetWidth(), tex.GetHeight(), PixelFormat.Format4bppIndexed);
                    BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                    byte[] indexes = tex.GetIndexData().Cast<byte>().ToArray();
                    Color[] pal = tex.GetPixelData();
                    int byteCount = (int)Math.Ceiling(indexes.Length / 2.0);
                    byte[] pixels = new byte[byteCount];

                    for (int i = 0; i < indexes.Length; i++)
                    {
                        byte colorIndex = indexes[i];
                        int byteIndex = i / 2;
                        int shift = (i % 2 == 0) ? 4 : 0;
                        pixels[byteIndex] |= (byte)(colorIndex << shift);
                    }

                    Marshal.Copy(pixels.ToArray(), 0, data.Scan0, pixels.Length);
                    ColorPalette palette = image.Palette;
                    for (int i = 0; i < pal.Length; i++)
                        palette.Entries[i] = pal[i];
                    image.Palette = palette;
                    image.UnlockBits(data);
                    break;
                }
                case PixelFormat.Format8bppIndexed:
                {
                    image = new(tex.GetWidth(), tex.GetHeight(), PixelFormat.Format8bppIndexed);
                    BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                    byte[] indexes = tex.GetIndexData().Cast<byte>().ToArray();
                    Marshal.Copy(indexes, 0, data.Scan0, indexes.Length);
                    ColorPalette palette = image.Palette;
                    Color[] pal = tex.GetPalette();
                    for (int i = 0; i < pal.Length; i++)
                        palette.Entries[i] = pal[i];
                    image.Palette = palette;
                    image.UnlockBits(data);
                    break;
                }
                case PixelFormat.Format16bppGrayScale:
                { //Ignoring the bpp of this format because why would you use 16 bits for gray scale
                    image = new(tex.GetWidth(), tex.GetHeight(), PixelFormat.Format8bppIndexed);
                    BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                    Color[] colors = tex.GetPixelData();
                    byte[] indexes = new byte[colors.Length];
                    for (int i = 0; i < colors.Length; i++)
                        indexes[i] = colors[i].R;

                    Marshal.Copy(indexes, 0, data.Scan0, colors.Length);
                    ColorPalette palette = image.Palette;
                    for (int i = 0; i < 256; i++)
                        palette.Entries[i] = Color.FromArgb(i, i, i);
                    image.Palette = palette;
                    image.UnlockBits(data);
                    break;
                }
                case PixelFormat.Format16bppArgb1555:
                {
                    image = new(tex.GetWidth(), tex.GetHeight(), PixelFormat.Format16bppArgb1555);
                    Color[] colors = tex.GetPixelData();
                    ushort[] pixels = new ushort[colors.Length];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        ushort alpha = 0x8000;
                        if (colors[i].A < 10)
                            alpha = 0;
                        pixels[i] = (ushort)(alpha | ((colors[i].R >> 3) << 10) | ((colors[i].G >> 3) << 5) | (colors[i].B >> 3));
                    }
                    BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                    byte[] BytePixels = new byte[pixels.Length * 2];
                    Buffer.BlockCopy(pixels, 0, BytePixels, 0, BytePixels.Length);
                    Marshal.Copy(BytePixels, 0, data.Scan0, pixels.Length * 2);
                    image.UnlockBits(data);
                    break;
                }
                case PixelFormat.Format16bppRgb565:
                {
                    image = new(tex.GetWidth(), tex.GetHeight(), PixelFormat.Format16bppRgb565);
                    Color[] colors = tex.GetPixelData();
                    ushort[] pixels = new ushort[colors.Length];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        ushort R = (byte)(((float)colors[i].R / 255) * 31);
                        ushort G = (byte)(((float)colors[i].G / 255) * 63);
                        ushort B = (byte)(((float)colors[i].B / 255) * 31);

                        int RGB = 0;
                        RGB |= R;
                        RGB |= (G << 5);
                        RGB |= (B << 11);
                        pixels[i] = (ushort)RGB;
                    }
                    BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                    byte[] BytePixels = new byte[pixels.Length * 2];
                    Buffer.BlockCopy(pixels, 0, BytePixels, 0, BytePixels.Length);
                    Marshal.Copy(BytePixels, 0, data.Scan0, pixels.Length * 2);
                    image.UnlockBits(data);
                    break;
                }
                case PixelFormat.Format24bppRgb:
                {
                    image = new(tex.GetWidth(), tex.GetHeight(), PixelFormat.Format24bppRgb);
                    Color[] colors = tex.GetPixelData();
                    byte[] pixels = new byte[colors.Length * 3];
                    for (int i = 0; i < colors.Length; i++)
                    {
                        int offset = i * 3;
                        pixels[offset] = colors[i].B;
                        pixels[offset + 1] = colors[i].G;
                        pixels[offset + 2] = colors[i].R;
                    }
                    BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                    Marshal.Copy(pixels, 0, data.Scan0, pixels.Length * 2);
                    image.UnlockBits(data);
                    break;
                }
                default:
                {
                    image = new(tex.GetWidth(), tex.GetHeight(), PixelFormat.Format32bppArgb);
                    Color[] colors = tex.GetPixelData();
                    byte[] pixels = new byte[colors.Length * 4];
                    for (int i = 0; i < colors.Length; i++)
                    {
                        int offset = i * 4;
                        pixels[offset] = colors[i].B;
                        pixels[offset + 1] = colors[i].G;
                        pixels[offset + 2] = colors[i].R;
                        pixels[offset + 3] = colors[i].A;
                    }
                    BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                    Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                    image.UnlockBits(data);
                    break;
                }
            }
            return image;
        }
        public static Bitmap[] GetCtxrBitmaps(CtxrFile InCtxr)
        {
            Bitmap[] Output = new Bitmap[InCtxr.Header.MipMapCount];
            for (int m = 0; m < InCtxr.Header.MipMapCount; m++)
            {
                int MipWidth = InCtxr.Header.Width >> m;
                int MipHeight = InCtxr.Header.Height >> m;
                Bitmap image = new(MipWidth, MipHeight, PixelFormat.Format32bppArgb);
                Color[] colors = InCtxr.ImageData[m];
                byte[] pixels = new byte[colors.Length * 4];
                for (int i = 0; i < colors.Length; i++)
                {
                    int offset = i * 4;
                    pixels[offset] = colors[i].B;
                    pixels[offset + 1] = colors[i].G;
                    pixels[offset + 2] = colors[i].R;
                    pixels[offset + 3] = colors[i].A;
                }

                BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                image.UnlockBits(data);
                Output[m] = image;
            }
            return Output;
        }
        public static Bitmap GetTfbmBitmap(TFBMFile InTFBM)
        {
            if (InTFBM.Header.BPP == 8)
            {
                Bitmap image = new(InTFBM.Header.Width, InTFBM.Header.Height, PixelFormat.Format8bppIndexed);
                BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                byte[] indexes = InTFBM.PixelIndexes;
                Marshal.Copy(indexes, 0, data.Scan0, indexes.Length);
                ColorPalette palette = image.Palette;
                for (int i = 0; i < InTFBM.Palette.Length; i++)
                    palette.Entries[i] = InTFBM.Palette[i];
                image.Palette = palette;
                image.UnlockBits(data);
                return image;
            }
            else if (InTFBM.Header.BPP == 24)
            {
                Bitmap image = new(InTFBM.Header.Width, InTFBM.Header.Height, PixelFormat.Format24bppRgb);
                Color[] colors = InTFBM.Pixels;
                byte[] pixels = new byte[colors.Length * 3];
                for (int i = 0; i < colors.Length; i++)
                {
                    int offset = i * 3;
                    pixels[offset] = colors[i].B;
                    pixels[offset + 1] = colors[i].G;
                    pixels[offset + 2] = colors[i].R;
                }
                BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length * 2);
                image.UnlockBits(data);
                return image;
            }
            else
            {
                Bitmap image = new(InTFBM.Header.Width, InTFBM.Header.Height, PixelFormat.Format32bppArgb);
                Color[] colors = InTFBM.Pixels;
                byte[] pixels = new byte[colors.Length * 4];
                for (int i = 0; i < colors.Length; i++)
                {
                    int offset = i * 4;
                    pixels[offset] = colors[i].B;
                    pixels[offset + 1] = colors[i].G;
                    pixels[offset + 2] = colors[i].R;
                    pixels[offset + 3] = colors[i].A;
                }

                BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                image.UnlockBits(data);
                return image;
            }
            
        }
        public static Bitmap GetTxnBitmap(RwTextureNative Txn)
        {
            switch (Txn.RasterInfo.Depth)
            {
                case 32:
                    {
                        Bitmap image = new(Txn.RasterInfo.Width, Txn.RasterInfo.Height, PixelFormat.Format32bppArgb);
                        Color[] colors = Txn.RasterData.ImageData;
                        byte[] pixels = new byte[colors.Length * 4];
                        for (int i = 0; i < colors.Length; i++)
                        {
                            int offset = i * 4;
                            pixels[offset] = colors[i].B;
                            pixels[offset + 1] = colors[i].G;
                            pixels[offset + 2] = colors[i].R;
                            pixels[offset + 3] = colors[i].A;
                        }

                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                        image.UnlockBits(data);
                        return image;
                    }
                case 24:
                    {
                        Bitmap image = new(Txn.RasterInfo.Width, Txn.RasterInfo.Height, PixelFormat.Format24bppRgb);
                        Color[] colors = Txn.RasterData.ImageData;
                        byte[] pixels = new byte[colors.Length * 3];
                        for (int i = 0; i < colors.Length; i++)
                        {
                            int offset = i * 3;
                            pixels[offset] = colors[i].B;
                            pixels[offset + 1] = colors[i].G;
                            pixels[offset + 2] = colors[i].R;
                        }
                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        Marshal.Copy(pixels, 0, data.Scan0, pixels.Length * 2);
                        image.UnlockBits(data);
                        return image;
                    }
                case 16:
                case 15:
                    {
                        Bitmap image = new(Txn.RasterInfo.Width, Txn.RasterInfo.Height, PixelFormat.Format16bppArgb1555);
                        Color[] colors = Txn.RasterData.ImageData;
                        ushort[] pixels = new ushort[colors.Length];
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            ushort alpha = 0x8000;
                            if (colors[i].A < 10)
                                alpha = 0;
                            pixels[i] = (ushort)(alpha | ((colors[i].R >> 3) << 10) | ((colors[i].G >> 3) << 5) | (colors[i].B >> 3));
                        }
                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        byte[] BytePixels = new byte[pixels.Length * 2];
                        Buffer.BlockCopy(pixels, 0, BytePixels, 0, BytePixels.Length);
                        Marshal.Copy(BytePixels, 0, data.Scan0, pixels.Length * 2);
                        image.UnlockBits(data);
                        return image;
                    }
                case 8:
                    {
                        Bitmap image = new(Txn.RasterInfo.Width, Txn.RasterInfo.Height, PixelFormat.Format8bppIndexed);
                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        byte[] indexes = Txn.RasterData.ImageIndexData;
                        Marshal.Copy(indexes, 0, data.Scan0, indexes.Length);
                        ColorPalette palette = image.Palette;
                        for (int i = 0; i < Txn.RasterData.PaletteData.Length; i++)
                            palette.Entries[i] = Txn.RasterData.PaletteData[i];
                        image.Palette = palette;
                        image.UnlockBits(data);
                        return image;
                    }
                case 4:
                    {
                        Bitmap image = new(Txn.RasterInfo.Width, Txn.RasterInfo.Height, PixelFormat.Format4bppIndexed);
                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        int byteCount = (int)Math.Ceiling(Txn.RasterData.ImageIndexData.Length / 2.0);
                        byte[] pixels = new byte[byteCount];
                        for (int i = 0; i < Txn.RasterData.ImageIndexData.Length; i++)
                        {
                            byte colorIndex = Txn.RasterData.ImageIndexData[i];
                            int byteIndex = i / 2;
                            int shift = (i % 2 == 0) ? 0 : 4;
                            pixels[byteIndex] |= (byte)(colorIndex << shift);
                        }

                        Marshal.Copy(pixels.ToArray(), 0, data.Scan0, pixels.Length);
                        ColorPalette palette = image.Palette;
                        for (int i = 0; i < Txn.RasterData.PaletteData.Length; i++)
                            palette.Entries[i] = Txn.RasterData.PaletteData[i];
                        image.Palette = palette;
                        image.UnlockBits(data);
                        return image;
                    }
                default: throw new Exception("Unimplemented RWPixelFormat");
            }
        }
        public static Bitmap GetTgaBitmap(TgaFile InTga)
        {
            switch (InTga.Header.BitsPerPixel)
            {
                case 32:
                    {
                        Bitmap image = new(InTga.Header.Width, InTga.Header.Height, PixelFormat.Format32bppArgb);
                        Color[] colors = InTga.GetAllPixels();
                        byte[] pixels = new byte[colors.Length * 4];
                        for (int i = 0; i < colors.Length; i++)
                        {
                            int offset = i * 4;
                            pixels[offset] = colors[i].B;
                            pixels[offset + 1] = colors[i].G;
                            pixels[offset + 2] = colors[i].R;
                            pixels[offset + 3] = colors[i].A;
                        }

                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                        image.UnlockBits(data);
                        return image;
                    }
                case 28:
                    {
                        Bitmap image = new(InTga.Header.Width, InTga.Header.Height, PixelFormat.Format24bppRgb);
                        Color[] colors = InTga.GetAllPixels();
                        byte[] pixels = new byte[colors.Length * 3];
                        for (int i = 0; i < colors.Length; i++)
                        {
                            int offset = i * 3;
                            pixels[offset] = colors[i].B;
                            pixels[offset + 1] = colors[i].G;
                            pixels[offset + 2] = colors[i].R;
                        }
                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        Marshal.Copy(pixels, 0, data.Scan0, pixels.Length * 2);
                        image.UnlockBits(data);
                        return image;
                    }
                case 16:
                case 15:
                    {
                        Bitmap image = new(InTga.Header.Width, InTga.Header.Height, PixelFormat.Format16bppArgb1555);
                        Color[] colors = InTga.GetAllPixels();
                        ushort[] pixels = new ushort[colors.Length];
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            ushort alpha = 0x8000;
                            if (colors[i].A < 128 && InTga.Header.BitsPerPixel == 16)
                                alpha = 0;
                            pixels[i] = (ushort)(alpha | ((colors[i].R >> 3) << 10) | ((colors[i].G >> 3) << 5) | (colors[i].B >> 3));
                        }
                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        byte[] BytePixels = new byte[pixels.Length * 2];
                        Buffer.BlockCopy(pixels, 0, BytePixels, 0, BytePixels.Length);
                        Marshal.Copy(BytePixels, 0, data.Scan0, pixels.Length * 2);
                        image.UnlockBits(data);
                        return image;
                    }
                case 8:
                    {
                        if (InTga.Header.ImageFormat.HasFlag(TgaFormat.GrayScale))
                        {
                            Bitmap image = new(InTga.Header.Width, InTga.Header.Height, PixelFormat.Format8bppIndexed);
                            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                            Color[] colors = InTga.GetAllPixels();
                            byte[] indexes = new byte[colors.Length];
                            for (int i = 0; i < colors.Length; i++)
                                indexes[i] = colors[i].R;

                            Marshal.Copy(indexes, 0, data.Scan0, colors.Length);
                            ColorPalette palette = image.Palette;
                            for (int i = 0; i < 256; i++)
                                palette.Entries[i] = Color.FromArgb(i, i, i);
                            image.Palette = palette;
                            image.UnlockBits(data);
                            return image;
                        }
                        else
                        {
                            Bitmap image = new(InTga.Header.Width, InTga.Header.Height, PixelFormat.Format8bppIndexed);
                            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                            byte[] indexes = InTga.GetAllIndexes();
                            Marshal.Copy(indexes, 0, data.Scan0, indexes.Length);
                            ColorPalette palette = image.Palette;
                            for (int i = 0; i < InTga.Palette.Colors.Length; i++)
                                palette.Entries[i] = InTga.Palette.Colors[i];
                            image.Palette = palette;
                            image.UnlockBits(data);
                            return image;
                        }
                    }
                default: throw new Exception("Unimplemented TGA PixelFormat");
            }
        }
        public static Bitmap GetTim2Bitmap(Tim2File InTm2)
        {
            switch (InTm2.Pictures[0].Header.PixelFormat)
            {
                case Tim2BPP.RGBA5551:
                    {
                        Bitmap image = new(InTm2.Pictures[0].Header.Width, InTm2.Pictures[0].Header.Height, PixelFormat.Format16bppArgb1555);

                        ushort[] pixels = new ushort[InTm2.Pictures[0].Image.Pixels.Count];
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            Color color = InTm2.Pictures[0].Image.Pixels[i];
                            ushort alpha = (ushort)(color.A >= 128 ? 0x8000 : 0x0);
                            pixels[i] = (ushort)(alpha | ((color.R >> 3) << 10) | ((color.G >> 3) << 5) | (color.B >> 3));
                        }
                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        byte[] BytePixels = new byte[pixels.Length * 2];
                        Buffer.BlockCopy(pixels, 0, BytePixels, 0, BytePixels.Length);
                        Marshal.Copy(BytePixels, 0, data.Scan0, pixels.Length * 2);
                        image.UnlockBits(data);
                        return image;
                    }
                case Tim2BPP.RGBA8880:
                    {
                        Bitmap image = new(InTm2.Pictures[0].Header.Width, InTm2.Pictures[0].Header.Height, PixelFormat.Format24bppRgb);

                        byte[] pixels = new byte[InTm2.Pictures[0].Image.Pixels.Count * 3];
                        for (int i = 0; i < InTm2.Pictures[0].Image.Pixels.Count; i++)
                        {
                            Color color = InTm2.Pictures[0].Image.Pixels[i];
                            int offset = i * 3;
                            pixels[offset] = color.B;
                            pixels[offset + 1] = color.G;
                            pixels[offset + 2] = color.R;
                        }

                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                        image.UnlockBits(data);
                        return image;
                    }
                case Tim2BPP.RGBA8888:
                    {
                        Bitmap image = new(InTm2.Pictures[0].Header.Width, InTm2.Pictures[0].Header.Height, PixelFormat.Format32bppArgb);

                        byte[] pixels = new byte[InTm2.Pictures[0].Image.Pixels.Count * 4];
                        for (int i = 0; i < InTm2.Pictures[0].Image.Pixels.Count; i++)
                        {
                            Color color = InTm2.Pictures[0].Image.Pixels[i];
                            int offset = i * 4;
                            pixels[offset] = color.B;
                            pixels[offset + 1] = color.G;
                            pixels[offset + 2] = color.R;
                            pixels[offset + 3] = color.A;
                        }

                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                        image.UnlockBits(data);
                        return image;
                    }
                case Tim2BPP.INDEX8:
                    {
                        Bitmap image = new(InTm2.Pictures[0].Header.Width, InTm2.Pictures[0].Header.Height, PixelFormat.Format8bppIndexed);
                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        Marshal.Copy(InTm2.Pictures[0].Image.PixelIndexes.ToArray(), 0, data.Scan0, InTm2.Pictures[0].Image.PixelIndexes.Count);
                        ColorPalette palette = image.Palette;
                        for (int i = 0; i < InTm2.Pictures[0].Palette.Colors.Count; i++)
                            palette.Entries[i] = InTm2.Pictures[0].Palette.Colors[i];
                        image.Palette = palette;
                        image.UnlockBits(data);
                        return image;
                    }
                case Tim2BPP.INDEX4:
                    {
                        Bitmap image = new(InTm2.Pictures[0].Header.Width, InTm2.Pictures[0].Header.Height, PixelFormat.Format4bppIndexed);
                        BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        int byteCount = (int)Math.Ceiling(InTm2.Pictures[0].Image.PixelIndexes.Count / 2.0);
                        byte[] pixels = new byte[byteCount];

                        for (int i = 0; i < InTm2.Pictures[0].Image.PixelIndexes.Count; i++)
                        {
                            byte colorIndex = InTm2.Pictures[0].Image.PixelIndexes[i];
                            int byteIndex = i / 2;
                            int shift = (i % 2 == 0) ? 4 : 0;
                            pixels[byteIndex] |= (byte)(colorIndex << shift);
                        }

                        Marshal.Copy(pixels.ToArray(), 0, data.Scan0, pixels.Length);
                        ColorPalette palette = image.Palette;
                        for (int i = 0; i < InTm2.Pictures[0].Palette.Colors.Count; i++)
                            palette.Entries[i] = InTm2.Pictures[0].Palette.Colors[i];
                        image.Palette = palette;
                        image.UnlockBits(data);
                        return image;
                    }
                default: throw new Exception("Unimplemented TM2 PixelFormat");
            }
        }
        public static Bitmap GetGimBitmap(string path)
        {
            GimFile InGim = new(path);
            return GetGimBitmap(InGim);
        }
        public static Bitmap GetGimBitmap(BinaryReader reader)
        {
            GimFile InGim = new(reader);
            return GetGimBitmap(InGim);
        }
        public static Bitmap GetGimBitmap(byte[] Data)
        {
            GimFile InGim = new(Data);
            return GetGimBitmap(InGim);
        }
        public static Bitmap GetGimBitmap(GimFile InGim)
        {
            List<GimChunk> Pics = InGim.FileChunk.GatherChildrenOfType(GimChunkType.GimPicture, false);
            if (Pics.Count == 0)
                throw new Exception("No Image data found in gim file");
            GimChunk Picture = Pics[0];
            List<GimChunk> Imgs = Picture.GatherChildrenOfType(GimChunkType.GimImage, false);
            if (Imgs.Count == 0)
                throw new Exception("No Image data found in gim file");
            GimImage Image = (GimImage)Imgs[0];
            if ((int)Image.ImgInfo.Format > 3 && (int)Image.ImgInfo.Format < 8)
            {
                List<GimChunk> Pals = Picture.GatherChildrenOfType(GimChunkType.GimPalette, false);
                if (Pals.Count == 0)
                    throw new Exception("ImageData uses indexed colors but no palette data was found");
                GimPalette Palette = (GimPalette)Pals[0];
                List<Color> Colors = Palette.Levels[0].Frames[0].Pixels.ToList();

                switch (Image.ImgInfo.Format)
                {
                    case GimFormat.Index8:
                        {
                            Bitmap image = new(Image.ImgInfo.Width, Image.ImgInfo.Height, PixelFormat.Format8bppIndexed);
                            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                            byte[] indexes = new byte[Image.Levels[0].Frames[0].PixelsIndex.Length];
                            for (int i = 0; i < indexes.Length; i++)
                                indexes[i] = (byte)Image.Levels[0].Frames[0].PixelsIndex[i];
                            Marshal.Copy(indexes, 0, data.Scan0, indexes.Length);
                            ColorPalette palette = image.Palette;
                            for (int i = 0; i < palette.Entries.Length; i++)
                                palette.Entries[i] = Colors[i];
                            image.Palette = palette;
                            image.UnlockBits(data);
                            return image;
                        }
                    case GimFormat.Index4:
                        {
                            Bitmap image = new(Image.ImgInfo.Width, Image.ImgInfo.Height, PixelFormat.Format4bppIndexed);
                            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                            int byteCount = (int)Math.Ceiling(Image.Levels[0].Frames[0].PixelsIndex.Length / 2.0);
                            byte[] pixels = new byte[byteCount];

                            for (int i = 0; i < Image.Levels[0].Frames[0].PixelsIndex.Length; i++)
                            {
                                byte colorIndex = (byte)Image.Levels[0].Frames[0].PixelsIndex[i];
                                int byteIndex = i / 2;
                                int shift = (i % 2 == 0) ? 4 : 0;
                                pixels[byteIndex] |= (byte)(colorIndex << shift);
                            }

                            Marshal.Copy(pixels.ToArray(), 0, data.Scan0, pixels.Length);
                            ColorPalette palette = image.Palette;
                            for (int i = 0; i < palette.Entries.Length; i++)
                                palette.Entries[i] = Colors[i];
                            image.Palette = palette;
                            image.UnlockBits(data);
                            return image;
                        }
                    case GimFormat.Index16:
                    case GimFormat.Index32:
                        {
                            Bitmap image = new(Image.ImgInfo.Width, Image.ImgInfo.Height, PixelFormat.Format32bppArgb);
                            uint[] Indexes = Image.Levels[0].Frames[0].PixelsIndex;
                            byte[] pixels = new byte[Indexes.Length * 4];
                            for (int i = 0; i < pixels.Length; i++)
                            {
                                Color color = Colors[(int)Indexes[i]];
                                int offset = i * 4;
                                pixels[offset] = color.B;
                                pixels[offset + 1] = color.G;
                                pixels[offset + 2] = color.R;
                                pixels[offset + 3] = color.A;
                            }
                            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length * 2);
                            image.UnlockBits(data);
                            return image;
                        }
                    default: throw new Exception($"Unimplemented Gim Format: {Image.ImgInfo.Format}");
                }
            }
            else
            {
                switch (Image.ImgInfo.Format)
                {
                    case GimFormat.DXT1:
                    case GimFormat.DXT1EXT:
                    case GimFormat.DXT3:
                    case GimFormat.DXT3EXT:
                    case GimFormat.DXT5:
                    case GimFormat.DXT5EXT:
                    case GimFormat.RGBA4444:
                    case GimFormat.RGBA8888:
                        {
                            Bitmap image = new(Image.ImgInfo.Width, Image.ImgInfo.Height, PixelFormat.Format32bppArgb);
                            Color[] Colors = Image.Levels[0].Frames[0].Pixels;
                            byte[] pixels = new byte[Colors.Length * 4];
                            for (int i = 0; i < Colors.Length; i++)
                            {
                                Color color = Colors[i];
                                int offset = i * 4;
                                pixels[offset] = color.B;
                                pixels[offset + 1] = color.G;
                                pixels[offset + 2] = color.R;
                                pixels[offset + 3] = color.A;
                            }
                            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                            image.UnlockBits(data);
                            return image;
                        }
                    case GimFormat.RGBA5551:
                        {
                            Bitmap image = new(Image.ImgInfo.Width, Image.ImgInfo.Height, PixelFormat.Format16bppArgb1555);
                            Color[] Colors = Image.Levels[0].Frames[0].Pixels;
                            ushort[] pixels = new ushort[Colors.Length];
                            for (int i = 0; i < Colors.Length; i++)
                            {
                                Color color = Colors[i];
                                ushort alpha = (ushort)(color.A >= 128 ? 0x8000 : 0x0);
                                pixels[i] = (ushort)(alpha | ((color.R >> 3) << 10) | ((color.G >> 3) << 5) | (color.B >> 3));
                            }
                            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                            byte[] BytePixels = new byte[pixels.Length * 2];
                            Buffer.BlockCopy(pixels, 0, BytePixels, 0, BytePixels.Length);
                            Marshal.Copy(BytePixels, 0, data.Scan0, pixels.Length * 2);
                            image.UnlockBits(data);
                            return image;
                        }
                    case GimFormat.RGBA5650:
                        {
                            Bitmap image = new(Image.ImgInfo.Width, Image.ImgInfo.Height, PixelFormat.Format16bppRgb565);
                            Color[] Colors = Image.Levels[0].Frames[0].Pixels;
                            ushort[] pixels = new ushort[Colors.Length];
                            for (int i = 0; i < Colors.Length; i++)
                            {
                                Color color = Colors[i];
                                pixels[i] = (ushort)(((color.R >> 3) << 11) | ((color.G >> 2) << 5) | (color.B >> 3));
                            }
                            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
                            byte[] BytePixels = new byte[pixels.Length * 2];
                            Buffer.BlockCopy(pixels, 0, BytePixels, 0, BytePixels.Length);
                            Marshal.Copy(BytePixels, 0, data.Scan0, pixels.Length * 2);
                            image.UnlockBits(data);
                            return image;
                        }
                    default: throw new Exception($"Unimplemented Gim Format: {Image.ImgInfo.Format}");
                }
            }
        }
        public static Bitmap GetDdsBitmap(string path)
        {
            DDSFile InDds = new(path);
            return GetDdsBitmap(InDds);
        }
        public static Bitmap GetDdsBitmap(BinaryReader reader)
        {
            DDSFile InDds = new(reader);
            return GetDdsBitmap(InDds);
        }
        public static Bitmap GetTmxBitmap(string path)
        {
            TmxFile InTmx = new(path);
            return GetTmxBitmap(InTmx);
        }
        public static Bitmap GetTmxBitmap(BinaryReader reader)
        {
            TmxFile InTmx = new(reader);
            return GetTmxBitmap(InTmx);
        }
        public static Bitmap GetDdsBitmap(DDSFile InDds)
        {
            PixelFormat Format = PixelFormat.Format32bppArgb;
            Bitmap image = new((int)InDds.DDSHeader.Width, (int)InDds.DDSHeader.Height, Format);
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
            if (Format == PixelFormat.Format32bppArgb)
            {
                byte[] pixels = new byte[InDds.Pixels.Length * 4];
                for (int i = 0; i < InDds.Pixels.Length; i++)
                {
                    Color color = InDds.Pixels[i];
                    int offset = i * 4;
                    pixels[offset] = color.B;
                    pixels[offset + 1] = color.G;
                    pixels[offset + 2] = color.R;
                    pixels[offset + 3] = color.A;
                }
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            }
            image.UnlockBits(data);
            return image;
        }
        public static Bitmap GetTmxBitmap(TmxFile InTmx)
        {
            PixelFormat Format = PixelFormat.Format32bppArgb;
            switch (InTmx.Picture.Header.PixelFormat)
            {
                case TmxPixelFormat.PSMTC24:
                    Format = PixelFormat.Format24bppRgb; break;
                case TmxPixelFormat.PSMT4HL:
                case TmxPixelFormat.PSMT4:
                case TmxPixelFormat.PSMT4HH:
                    Format = PixelFormat.Format4bppIndexed; break;
                case TmxPixelFormat.PSMT8:
                case TmxPixelFormat.PSMT8H:
                    Format = PixelFormat.Format8bppIndexed; break;
            }
            Bitmap image = new(InTmx.Picture.Header.Width, InTmx.Picture.Header.Height, Format);
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, image.PixelFormat);
            if (InTmx.Picture.Header.PaletteCount > 0)
            {
                if (Format == PixelFormat.Format8bppIndexed)
                {
                    byte[] indexes = InTmx.Picture.Image.PixelIndexes.ToArray();
                    Marshal.Copy(indexes, 0, data.Scan0, indexes.Length);
                }
                else
                {
                    int byteCount = (int)Math.Ceiling(InTmx.Picture.Image.PixelIndexes.Count / 2.0);
                    byte[] pixels = new byte[byteCount];

                    for (int i = 0; i < InTmx.Picture.Image.PixelIndexes.Count; i++)
                    {
                        byte colorIndex = InTmx.Picture.Image.PixelIndexes[i];
                        int byteIndex = i / 2;
                        int shift = (i % 2 == 0) ? 4 : 0;
                        pixels[byteIndex] |= (byte)(colorIndex << shift);
                    }
                    Marshal.Copy(pixels.ToArray(), 0, data.Scan0, pixels.Length);
                }

                ColorPalette palette = image.Palette;
                for (int i = 0; i < InTmx.Picture.Palette.Colors.Count; i++)
                    palette.Entries[i] = InTmx.Picture.Palette.Colors[i];
                image.Palette = palette;
            }
            else
            {
                if (Format == PixelFormat.Format32bppArgb)
                {
                    byte[] pixels = new byte[InTmx.Picture.Image.Pixels.Count * 4];
                    for (int i = 0; i < InTmx.Picture.Image.Pixels.Count; i++)
                    {
                        Color color = InTmx.Picture.Image.Pixels[i];
                        int offset = i * 4;
                        pixels[offset] = color.B;
                        pixels[offset + 1] = color.G;
                        pixels[offset + 2] = color.R;
                        pixels[offset + 3] = color.A;
                    }
                    Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                }
                else
                {
                    byte[] pixels = new byte[InTmx.Picture.Image.Pixels.Count * 3];
                    for (int i = 0; i < InTmx.Picture.Image.Pixels.Count; i++)
                    {
                        Color color = InTmx.Picture.Image.Pixels[i];
                        int offset = i * 3;
                        pixels[offset] = color.B;
                        pixels[offset + 1] = color.G;
                        pixels[offset + 2] = color.R;
                    }
                    Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                }
            }
            image.UnlockBits(data);
            return image;
        }
        public static DDSFile DdsFromBitmap(Bitmap image)
        {
            DDSFlags flags = DDSFlags.UseCaps | DDSFlags.UseWidth | DDSFlags.UseHeight | DDSFlags.UsePixelFormat;
            DDSPixelFormat pixelFormat = new DDSPixelFormat(DDSPixelFormatFlags.HasRGB | DDSPixelFormatFlags.HasAlphaPixels);
            DDSHeader header = new((uint)image.Height, (uint)image.Width, flags, pixelFormat);
            header.Caps1 = DDSCaps.Texture;
            header.Depth = 1;
            header.MipMapCount = 1;
            Color[] pixels = GetPixels(image);
            DDSFile Out = new DDSFile(header, pixels);
            return Out;
            /*
            DDSFlags flags = DDSFlags.UseCaps | DDSFlags.UseWidth | DDSFlags.UseHeight | DDSFlags.UsePixelFormat;
            DDSPixelFormat pixelFormat = new DDSPixelFormat(DDSPixelFormatFlags.HasFourCC, DDSFourCC.DXT1);
            DDSHeader header = new((uint)image.Height, (uint)image.Width, flags, pixelFormat);
            header.Caps1 = DDSCaps.Texture;
            Color[] pixels = GetPixels(image);
            DDSFile Out = new DDSFile(header, pixels);
            return Out;
            */
        }
        public static TmxFile TmxFromBitmap(Bitmap image)
        {
            TmxPictureHeader Header = new();
            Header.Width = (ushort)image.Width;
            Header.Height = (ushort)image.Height;

            bool Index = false;
            if ((image.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
                Index = true;
            else if (image.PixelFormat == PixelFormat.Format24bppRgb)
                Header.PixelFormat = TmxPixelFormat.PSMTC24;

            Color[] Pixels = GetPixels(image);

            TmxImage Image = new(Pixels);

            TmxPicture Pic = new(Header, Image);

            if (Index)
                Pic.FullColorToIndexed();

            TmxFile Out = new(Pic, 0, 2);
            return Out;
        }
        public static GimFile GimFromBitmap(Bitmap image)
        {
            GimBinaryChunk FileChunk = new(Array.Empty<byte>(), GimChunkType.GimFile, new List<GimChunk>(), null);

            GimBinaryChunk PictureChunk = new(Array.Empty<byte>(), GimChunkType.GimPicture, new List<GimChunk>(), null);

            ImageInfo ImgInfo = new();
            ImgInfo.Width = (ushort)image.Width;
            ImgInfo.Height = (ushort)image.Height;
            ImgInfo.FrameCount = 1;

            bool Index = false;
            if ((image.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
                Index = true;
            else if (image.PixelFormat == PixelFormat.Format16bppRgb555 || image.PixelFormat == PixelFormat.Format16bppArgb1555)
                ImgInfo.Format = GimFormat.RGBA5551;
            else if (image.PixelFormat == PixelFormat.Format16bppRgb565)
                ImgInfo.Format = GimFormat.RGBA5650;
            else
                ImgInfo.Format = GimFormat.RGBA8888;

            Level newLevel = new();
            newLevel.Frames = new();

            Color[] Pixels = GetPixels(image);

            Frame GimFrame = new();
            GimFrame.Pixels = Pixels;
            newLevel.Frames.Add(GimFrame);

            List<Level> NewLevels = new();
            NewLevels.Add(newLevel);
            GimImage NewImage = new(ImgInfo, NewLevels, PictureChunk);

            if (Index)
            {
                GimPalette pal = NewImage.FullColorToIndexed();
                PictureChunk.Children.Add(pal);
            }

            PictureChunk.Children.Add(NewImage);
            FileChunk.Children.Add(PictureChunk);
            GimFile File = new(FileChunk);
            return File;
        }
        public static Bitmap ForcePowerOfTwo(Bitmap bitmap, bool Linear)
        {
            int NewWidth = GetClosestPowerOfTwo(bitmap.Width);
            int NewHeight = GetClosestPowerOfTwo(bitmap.Height);

            if (NewHeight == bitmap.Height && NewWidth == bitmap.Width)
            {
                return bitmap;
            }
            return ResizeImage(bitmap, new Size(NewWidth, NewHeight), Linear);
        }
        public static Bitmap ResizeImage(Bitmap bitmap, Size size, bool Linear)
        {
            if (bitmap.Width == size.Width && bitmap.Height == size.Height)
                return bitmap;
            Resizer resizer = new(size);
            return resizer.ResizeImage(bitmap, !Linear);
        }
        public static int GetClosestPowerOfTwo(int number, bool OnlyScaleUp = false, bool OnlyScaleDown = false)
        {
            bool negative = false;
            if (number < 0)
            {
                number = -number;
                negative = true;
            }

            int LowPowerOfTwo = 1;
            while (LowPowerOfTwo <= number)
            {
                LowPowerOfTwo <<= 1;
            }
            LowPowerOfTwo >>= 1;

            int HighPowerOfTwo = 1;
            while (HighPowerOfTwo < number)
            {
                HighPowerOfTwo <<= 1;
            }
            if (OnlyScaleUp)
                return HighPowerOfTwo;
            if (OnlyScaleDown)
                return LowPowerOfTwo;
            if (number - LowPowerOfTwo <= HighPowerOfTwo - number)
            {
                number = LowPowerOfTwo;
            }
            else
            {
                number = HighPowerOfTwo;
            }
            if (negative)
                number = -number;
            return number;
        }
        public static Bitmap LimitColors(Bitmap bitmap)
        {
            OctreeQuantizer quantizer = new();

            return quantizer.Quantize(bitmap);
        }
        public static Bitmap Solidify(Bitmap bitmap)
        {
            Bitmap solidifiedBitmap = new(bitmap.Width, bitmap.Height);
            int solidifyDistance = 2; //Number of transparent pixels around the non transparent pixels to solidify
            int bitmapWidth = bitmap.Width;
            int bitmapHeight = bitmap.Height;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData solidifiedBitmapData = solidifiedBitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            byte[] bitmapPixels = new byte[bitmapData.Stride * bitmapHeight];
            Marshal.Copy(bitmapData.Scan0, bitmapPixels, 0, bitmapPixels.Length);

            byte[] solidifiedBitmapPixels = new byte[solidifiedBitmapData.Stride * bitmapHeight];

            Parallel.For(0, bitmapWidth, x =>
            {
                int xMin = Math.Max(x - solidifyDistance, 0);
                int xMax = Math.Min(x + solidifyDistance, bitmapWidth - 1);

                for (int y = 0; y < bitmapHeight; y++)
                {
                    int yMin = Math.Max(y - solidifyDistance, 0);
                    int yMax = Math.Min(y + solidifyDistance, bitmapHeight - 1);

                    int index = y * bitmapData.Stride + x * 4;
                    byte alpha = bitmapPixels[index + 3];

                    if (alpha == 0)
                    {
                        double totalRed = 0.0;
                        double totalGreen = 0.0;
                        double totalBlue = 0.0;
                        int count = 0;

                        for (int i = xMin; i <= xMax; i++)
                        {
                            for (int j = yMin; j <= yMax; j++)
                            {
                                int nearbyIndex = j * bitmapData.Stride + i * 4;
                                byte nearbyAlpha = bitmapPixels[nearbyIndex + 3];

                                if (nearbyAlpha != 0)
                                {
                                    totalRed += bitmapPixels[nearbyIndex + 2];
                                    totalGreen += bitmapPixels[nearbyIndex + 1];
                                    totalBlue += bitmapPixels[nearbyIndex];
                                    count++;
                                }
                            }
                        }

                        if (count > 0)
                        {
                            byte red = (byte)(totalRed / count);
                            byte green = (byte)(totalGreen / count);
                            byte blue = (byte)(totalBlue / count);

                            int solidifiedIndex = y * solidifiedBitmapData.Stride + x * 4;
                            solidifiedBitmapPixels[solidifiedIndex + 3] = alpha;
                            solidifiedBitmapPixels[solidifiedIndex + 2] = red;
                            solidifiedBitmapPixels[solidifiedIndex + 1] = green;
                            solidifiedBitmapPixels[solidifiedIndex] = blue;
                        }
                    }
                    else
                    {
                        int solidifiedIndex = y * solidifiedBitmapData.Stride + x * 4;
                        solidifiedBitmapPixels[solidifiedIndex + 3] = alpha;
                        solidifiedBitmapPixels[solidifiedIndex + 2] = bitmapPixels[index + 2];
                        solidifiedBitmapPixels[solidifiedIndex + 1] = bitmapPixels[index + 1];
                        solidifiedBitmapPixels[solidifiedIndex] = bitmapPixels[index];
                    }
                }
            });

            Marshal.Copy(solidifiedBitmapPixels, 0, solidifiedBitmapData.Scan0, solidifiedBitmapPixels.Length);
            solidifiedBitmap.UnlockBits(solidifiedBitmapData);

            return solidifiedBitmap;
        }
        public static Color[][] GenerateMipMaps(Color[] colors, int width, int height, int mipMapCount)
        {
            Color[][] mipMaps = new Color[mipMapCount][];

            mipMaps[0] = colors;

            for (int i = 1; i < mipMapCount; i++)
            {
                int newWidth = width >> i;
                int newHeight = height >> i;

                mipMaps[i] = GenerateMipMapLevel(mipMaps[i - 1], newWidth, newHeight);
            }

            return mipMaps;
        }

        private static Color[] GenerateMipMapLevel(Color[] previousMipMap, int width, int height)
        {
            Color GetPixel(Color[] colors, int width, int height, int x, int y)
            {
                x = Math.Max(0, Math.Min(x, width - 1));
                y = Math.Max(0, Math.Min(y, height - 1));
                return colors[y * width + x];
            }

            Color[] mipMap = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color averageColor = AverageColor(
                        GetPixel(previousMipMap, width, height, x * 2, y * 2),
                        GetPixel(previousMipMap, width, height, x * 2 + 1, y * 2),
                        GetPixel(previousMipMap, width, height, x * 2, y * 2 + 1),
                        GetPixel(previousMipMap, width, height, x * 2 + 1, y * 2 + 1));
                    mipMap[y * width + x] = averageColor;
                }
            }

            return mipMap;
        }
        private static Color AverageColor(params Color[] colors)
        {
            float totalR = 0f, totalG = 0f, totalB = 0f, totalA = 0f;

            foreach (Color color in colors)
            {
                totalR += color.R;
                totalG += color.G;
                totalB += color.B;
                totalA += color.A;
            }

            int count = colors.Length;
            return Color.FromArgb((byte)(totalA / count), (byte)(totalR / count),
                (byte)(totalG / count), (byte)(totalB / count));
        }
    }
}
