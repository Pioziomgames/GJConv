using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GimLib.Chunks;
using GJ.IO;
using static GimLib.GimEnums;

namespace GimLib
{
    public class GimFile : Texture
    {
        public const uint MAGIC = 776423757;
        public int Version;
        public int Style;
        public int Option;
        public GimChunk FileChunk;

        public GimFile(GimChunk fileChunk, int version = 825110576, int style = 5264208, int option = 0)
        {
            Version = version;
            Style = style;
            Option = option;
            FileChunk = fileChunk;
        }
        public GimFile(string Path) : base(Path) { }
        public GimFile(byte[] Data) : base(Data) { }
        public GimFile(BinaryReader reader) : base(reader) { }
        internal override void Read(BinaryReader reader)
        {
            int sig = reader.ReadInt32();
            if (sig != MAGIC)
                throw new Exception("Not a proper Gim File");

            Version = reader.ReadInt32();
            Style = reader.ReadInt32();
            Option = reader.ReadInt32();
            FileChunk = GimChunk.Read(null, reader);
        }
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(MAGIC);
            writer.Write(Version);
            writer.Write(Style);
            writer.Write(Option);

            GimChunk.Write(FileChunk, writer);
        }

        public override int GetWidth()
        {
            List<GimChunk> Pics = FileChunk.GatherChildrenOfType(GimChunkType.GimPicture, false);
            if (Pics.Count == 0)
                return 0;
            GimChunk Picture = Pics[0];
            List<GimChunk> Imgs = Picture.GatherChildrenOfType(GimChunkType.GimImage, false);
            if (Imgs.Count == 0)
                return 0;
            GimImage Image = (GimImage)Imgs[0];
            return Image.ImgInfo.Width;
        }

        public override int GetHeight()
        {
            List<GimChunk> Pics = FileChunk.GatherChildrenOfType(GimChunkType.GimPicture, false);
            if (Pics.Count == 0)
                return 0;
            GimChunk Picture = Pics[0];
            List<GimChunk> Imgs = Picture.GatherChildrenOfType(GimChunkType.GimImage, false);
            if (Imgs.Count == 0)
                return 0;
            GimImage Image = (GimImage)Imgs[0];
            return Image.ImgInfo.Height;
        }

        public override int GetMipMapCount()
        {
            List<GimChunk> Pics = FileChunk.GatherChildrenOfType(GimChunkType.GimPicture, false);
            if (Pics.Count == 0)
                return 0;
            GimChunk Picture = Pics[0];
            List<GimChunk> Imgs = Picture.GatherChildrenOfType(GimChunkType.GimImage, false);
            if (Imgs.Count == 0)
                return 0;
            GimImage Image = (GimImage)Imgs[0];
            if (Image.ImgInfo.LevelType == GimType.MipMap || Image.ImgInfo.LevelType == GimType.MipMap2)
                return Image.ImgInfo.LevelCount;
            return Image.ImgInfo.FrameType == GimType.MipMap || Image.ImgInfo.FrameType == GimType.MipMap2 ? Image.ImgInfo.FrameCount : 1;
        }

        public override PixelFormat GetPixelFormat()
        {
            List<GimChunk> Pics = FileChunk.GatherChildrenOfType(GimChunkType.GimPicture, false);
            if (Pics.Count == 0)
                return 0;
            GimChunk Picture = Pics[0];
            List<GimChunk> Imgs = Picture.GatherChildrenOfType(GimChunkType.GimImage, false);
            if (Imgs.Count == 0)
                return 0;
            GimImage Image = (GimImage)Imgs[0];
            switch (Image.ImgInfo.Format)
            {
                case GimFormat.Index4:
                    return PixelFormat.Format4bppIndexed;
                case GimFormat.Index8:
                    return PixelFormat.Format8bppIndexed;
                case GimFormat.RGBA5551:
                    return PixelFormat.Format16bppArgb1555;
                case GimFormat.RGBA5650:
                    return PixelFormat.Format16bppRgb565;
                default:
                    return PixelFormat.Format32bppArgb;
            }
        }

        public override Color[] GetPalette()
        {
            List<GimChunk> Pics = FileChunk.GatherChildrenOfType(GimChunkType.GimPicture, false);
            if (Pics.Count == 0)
                return Array.Empty<Color>();
            GimChunk Picture = Pics[0];
            List<GimChunk> Imgs = Picture.GatherChildrenOfType(GimChunkType.GimImage, false);
            if (Imgs.Count == 0)
                return Array.Empty<Color>();
            List<GimChunk> Pals = Picture.GatherChildrenOfType(GimChunkType.GimPalette, false);
            if (Pals.Count == 0)
                return Array.Empty<Color>();
            GimPalette Palette = (GimPalette)Pals[0];
            return Palette.Levels[0].Frames[0].Pixels;
        }

        public override Color[] GetPixelData()
        {
            List<GimChunk> Pics = FileChunk.GatherChildrenOfType(GimChunkType.GimPicture, false);
            if (Pics.Count == 0)
                return Array.Empty<Color>();
            GimChunk Picture = Pics[0];
            List<GimChunk> Imgs = Picture.GatherChildrenOfType(GimChunkType.GimImage, false);
            if (Imgs.Count == 0)
                return Array.Empty<Color>();
            GimImage Image = (GimImage)Imgs[0];
            if (Image.ImgInfo.BitsPerPixel <= 8)
                return GetPixelDataFromIndexData();
            return Image.Levels[0].Frames[0].Pixels;
        }

        public override byte[] GetIndexData()
        {
            List<GimChunk> Pics = FileChunk.GatherChildrenOfType(GimChunkType.GimPicture, false);
            if (Pics.Count == 0)
                return Array.Empty<byte>();
            GimChunk Picture = Pics[0];
            List<GimChunk> Imgs = Picture.GatherChildrenOfType(GimChunkType.GimImage, false);
            if (Imgs.Count == 0)
                return Array.Empty<byte>();
            GimImage Image = (GimImage)Imgs[0];
            return Image.Levels[0].Frames[0].PixelsIndex.Select(x => (byte)x).ToArray();
        }
    }
}
