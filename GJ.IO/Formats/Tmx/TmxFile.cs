using GJ.IO;

namespace TmxLib
{
    public class TmxFile : Texture
    {
        public short Flag;
        public short UserId;
        public TmxPicture Picture;
        public TmxFile(TmxPicture picture, short userId = 0, short flag = 2)
        {
            Flag = flag;
            UserId = userId;
            Picture = picture;
        }
        public TmxFile(string Path) : base(Path) { }
        public TmxFile(byte[] Data) : base(Data) { }
        public TmxFile(BinaryReader reader) : base(reader) { }
        internal override void Read(BinaryReader reader)
        {
            Flag = reader.ReadInt16();
            UserId = reader.ReadInt16();
            reader.ReadInt32(); //FileSize

            int MAGIC = reader.ReadInt32();
            if (MAGIC != 0x30584D54)
                throw new Exception("Not a proper TMX file");

            reader.BaseStream.Position += 4;

            Picture = new(reader);

        }
        internal override void Write(BinaryWriter writer)
        {
            long Start = writer.BaseStream.Position;
            writer.Write(Flag);
            writer.Write(UserId);
            writer.Write(0); //FileSize
            writer.Write(0x30584D54);
            writer.Write(0); //reserved

            Picture.Write(writer);
            long End = writer.BaseStream.Position;

            writer.BaseStream.Position = Start + 4;
            writer.Write((int)(End - Start));
        }
    }
}