using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxnLib
{
    public class RmdEnums
    {
        public enum RmdChunkType : uint
        {
            Struct            = 0x00000001,
            String            = 0x00000002,
            Extension         = 0x00000003,
            TextureNative     = 0x00000015,
            TextureDictionary = 0x00000016,
        }
        public enum RwDevice : ushort
        {
            Default = 0x0,
            D3D8 = 0x1,
            D3D9 = 0x2,
            PS2 = 0x6,
            Xbox = 0x8
        }
        public enum PS2PixelFormat
        {
            PSMTC32 = 0x00,
            PSMTC24 = 0x01,
            PSMTC16 = 0x02,
            PSMTC16S = 0x0A,
            PSMT8 = 0x13,
            PSMT4 = 0x14,
            PSMT8H = 0x1B,
            PSMT4HL = 0x24,
            PSMT4HH = 0x2C,
            PSMZ32 = 0x30,
            PSMZ24 = 0x31,
            PSMZ16 = 0x32,
            PSMZ16S = 0x3A
        }
        public enum PS2TextureFunction
        {
            Modulate = 0x00,
            Decal = 0x01,
            Highlight = 0x02,
            Highlight2 = 0x03
        }
        public enum PaletteBufferLoadMode
        {
            TempBufferNotChanged = 0,
            Load = 1,
            LoadCopyCbp0 = 2,
            LoadCopyCbp1 = 3,
            LoadCopyCbp0Neq = 4,
            LoadCopyCbp1Neq = 5
        }
        public enum GifMode
        {
            Packed = 0,
            Reglist = 1,
            Image = 2,
            Disable = 3
        }
        public enum TransmissionDirection : ulong
        {
            HostToLocal = 0x00,
            LocalToHost = 0x01,
            LocalToLocal = 0x02,
            Deactivated = 0x03
        }
        public enum TransmissionOrder
        {
            UpLtoLoR = 0x00,
            LoLtoUpR = 0x01,
            UpRtoLoL = 0x02,
            LoRtoUpL = 0x03
        }
        public enum RwPlatformId : uint
        {
            Xbox = 0x05,
            D3D8 = 0x08,
            D3D9 = 0x09,
            PS2  = 0x325350,
        }

        [Flags]
        public enum RwRasterFormat : uint
        {
            LockPixels = 0x00004,
            Format1555 = 0x00100,
            Format565 = 0x00200, 
            Format4444 = 0x00300,
            FormatLum8 = 0x00400,
            Format8888 = 0x00500, 
            Format888 = 0x00600,
            Format555 = 0x00A00,
            AutoMipMap = 0x01000,
            Pal8 = 0x02000,
            Pal4 = 0x04000,
            MipMap = 0x08000,
            Swizzled = 0x10000,
            HasHeaders = 0x20000
        }
        public enum PS2FilterMode
        {
            None = 0x00,
            Nearest = 0x01,
            Linear = 0x02,
            MipNearest = 0x03,
            MipLinear = 0x04,
            LinearMipNearest = 0x05,
            LinearMipLinear = 0x06
        }
    }
}
