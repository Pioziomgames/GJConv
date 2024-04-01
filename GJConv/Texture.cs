using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GJ.IO
{
    public abstract class Texture
    {
        public Texture()
        {

        }
        public Texture(string Path)
        {
            using (BinaryReader reader = new(File.OpenRead(Path)))
                Read(reader);
        }
        public Texture(byte[] Data)
        {
            Read(new BinaryReader(new MemoryStream(Data)));
        }
        public Texture(BinaryReader reader)
        {
            Read(reader);
        }
        public byte[] Save()
        {
            MemoryStream ms = new();
            using (BinaryWriter bw = new(ms))
                Save(bw);
            return ms.ToArray();
        }
        public void Save(string Path)
        {
            using (BinaryWriter writer = new(File.Create(Path)))
            {
                Write(writer);
                writer.Flush();
                writer.Close();
            }
        }
        public void Save(BinaryWriter writer)
        {
            Write(writer);
        }
        public abstract int GetWidth();
        public abstract int GetHeight();
        public abstract int GetMipMapCount();
        internal abstract void Read(BinaryReader reader);
        internal abstract void Write(BinaryWriter writer);
    }
}
