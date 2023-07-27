using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxnLib
{
    public class RmdFile
    {
        public List<RmdChunk> Chunks;

        public RmdFile(string Path)
        {
            using (BinaryReader reader = new(File.OpenRead(Path)))
                Read(reader);
        }
        public RmdFile(byte[] data)
        {
            using (MemoryStream memoryStream = new(data))
            {
                using (BinaryReader reader = new(memoryStream))
                    Read(reader);
            }
        }
        public RmdFile(BinaryReader reader, uint Size = 0)
        {
            Read(reader, Size);
        }
        public RmdFile(List<RmdChunk> chunks)
        {
            Chunks = chunks;
        }
        public RmdFile(RmdChunk[] chunks)
        {
            Chunks = chunks.ToList();
        }

        private void Read(BinaryReader reader, uint Size = 0)
        {
            Chunks = new List<RmdChunk>();
            long Start = reader.BaseStream.Position;
            if (Size == 0 || Size > reader.BaseStream.Length - Start)
            {
                Size = (uint)reader.BaseStream.Length - (uint)Start;
            }

            while (reader.BaseStream.Position < Size)
                Chunks.Add(RmdChunk.Read(reader));
            reader.BaseStream.Position = Start + Size;
        }
        public void Save(string Path)
        {
            using (BinaryWriter reader = new(File.OpenWrite(Path)))
            {
                Save(reader);
                reader.Flush();
                reader.Close();
            }
        }
        public void Save(BinaryWriter writer)
        {
            for (int i = 0; i < Chunks.Count; i++)
                RmdChunk.Write(Chunks[i], writer);
        }
        public byte[] Save()
        {
            using (MemoryStream ms = new())
            {
                using (BinaryWriter writer = new(ms))
                {
                    Save(writer);
                }
                return ms.ToArray();
            }
        }
    }
}
