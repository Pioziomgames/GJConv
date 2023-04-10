using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimLib
{
    public class GimEnums
    {
        public enum GimType
        {
            Generic  = 0,
            MipMap   = 1,
            MipMap2  = 2,
            Sequence = 3,
        }
        public enum GimOrder
        {
            Normal   = 0,
            PSPImage = 1,
        }
        public enum GimFormat
        {
            RGBA5650 = 0,
            RGBA5551 = 1,
            RGBA4444 = 2,
            RGBA8888 = 3,
            Index4   = 4,
            Index8   = 5,
            Index16  = 6,
            Index32  = 7,
            DXT1     = 8,
            DXT3     = 9,
            DXT5     = 10,
            DXT1EXT = 264,
            DXT3EXT = 265,
            DXT5EXT = 266,
        }
        public enum GimChunkType
        {
            GimBlock    = 0x01,
            GimFile     = 0x02,
            GimPicture  = 0x03,
            GimImage    = 0x04,
            GimPalette  = 0x05,
            GimSequence = 0x06,
            GimFileInfo = 0xff,
        }

        public struct GimChunkHeader
        {
            public long Start;
            public GimChunkType Type;
            public ushort ArgsOffs;
            public uint NextOffs;
            public uint ChildOffs;
            public uint DataOffs;
            public GimChunkHeader(BinaryReader reader)
            {
                Start = reader.BaseStream.Position;
                Type = (GimChunkType)reader.ReadUInt16();
                ArgsOffs = reader.ReadUInt16();
                NextOffs = reader.ReadUInt32();

                ChildOffs = reader.ReadUInt32();
                DataOffs = reader.ReadUInt32();


            }
        }
    }
}
