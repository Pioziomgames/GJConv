using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tim2Lib.Tim2Enums;
using GJ.IO;

namespace Tim2Lib
{
    public class Tim2File : Texture
    {
        public const uint MAGIC = 0x324D4954;
        public byte Version;
        public Tim2Alignment Alignment;
        public ushort PictureCount { get => (ushort)Pictures.Count; }
        public List<Tim2Picture> Pictures;
        public Tim2File(byte version, Tim2Alignment alignment, List<Tim2Picture> pictures)
        {
            Version = version;
            Alignment = alignment;
            Pictures = pictures;
        }
        public Tim2File(string Path) : base(Path) { }
        public Tim2File(byte[] Data) : base(Data) { }
        public Tim2File(BinaryReader reader) : base(reader) { }
        internal override void Read(BinaryReader reader)
        {
            if (reader.ReadUInt32() != MAGIC)
                throw new Exception("Not a proper TIM2 file");
            Version = reader.ReadByte();
            Alignment = (Tim2Alignment)reader.ReadByte();
            ushort count = reader.ReadUInt16();

            if (Alignment == Tim2Alignment.Align128)
                reader.BaseStream.Position += 120;
            else
                reader.BaseStream.Position += 8;

            if (Alignment > 0)
                reader.BaseStream.Position += 112;

            Pictures = new();
            for (int i = 0; i < count; i++)
                Pictures.Add(new Tim2Picture(reader, Alignment));
        }
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(MAGIC);
            writer.Write(Version);
            writer.Write((byte)Alignment);
            writer.Write((ushort)Pictures.Count);
            if(Alignment == Tim2Alignment.Align128)
                writer.BaseStream.Position += 120;
            else
                writer.BaseStream.Position += 8;

            if (Alignment > 0)
                writer.BaseStream.Position += 112;

            for (int i = 0; i < PictureCount; i++)
                Pictures[i].Write(writer, Alignment);
        }
    }
}
