using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GJ.IO;

namespace GimLib
{
    public class GimFile : Texture
    {
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
            if (sig != 776423757)
                throw new Exception("Not a proper Gim File");

            Version = reader.ReadInt32();
            Style = reader.ReadInt32();
            Option = reader.ReadInt32();
            FileChunk = GimChunk.Read(null, reader);
        }
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(776423757);
            writer.Write(Version);
            writer.Write(Style);
            writer.Write(Option);

            GimChunk.Write(FileChunk, writer);
        }
    }
}
