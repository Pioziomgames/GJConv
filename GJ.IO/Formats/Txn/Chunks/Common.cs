using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GJ.IO.IOFunctions;
using static TxnLib.RmdEnums;

namespace TxnLib
{
    public class RwString : RmdChunk
    {
        public string Value;
        public RwString(string value, int version = 469893175)
            : base(RmdChunkType.String, version)
        {
            Value = value;
        }
        public RwString(RmdChunkType type, uint size, int version, BinaryReader reader)
           : base(type, size, version, reader)
        {
        }
        protected override void ReadChunkData(BinaryReader reader, uint Size)
        {
            Value = new string(reader.ReadChars((int)Size));
            Value = Value.TrimEnd();
        }
        protected override void WriteChunkData(BinaryWriter writer)
        {
            for (int i = 0; i < Value.Length; i++)
                writer.Write(Value[i]);
            int padding = Align(Value.Length+1,4) - Value.Length;

            writer.Seek(padding, SeekOrigin.Current);
        }
    }
}
