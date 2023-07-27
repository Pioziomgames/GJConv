using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TxnLib.RmdEnums;

namespace TxnLib
{
    public abstract class RmdChunk
    {
        public RmdChunkType Type { get; set; }
        public int Version { get; set; }
        public RmdChunk(RmdChunkType type, int version)
        {
            Type = type;
            Version = version;
        }
        public RmdChunk(RmdChunkType type, uint size, int version, BinaryReader reader)
            : this(type, version)
        {
            long start = reader.BaseStream.Position;
            ReadChunkData(reader, size);
            reader.BaseStream.Position = start + size;
        }
        public RmdChunk(RmdChunkType type, uint size, int version, BinaryReader reader, object? data)
            : this(type, version)
        {
            long start = reader.BaseStream.Position;
            ReadChunkData(reader, size, data);
            reader.BaseStream.Position = start + size;
        }
        public static void Write(RmdChunk chunk, BinaryWriter writer)
        {
            writer.Write((int)chunk.Type);
            writer.Write(0);
            writer.Write(chunk.Version);
            long start = writer.BaseStream.Position;
            chunk.WriteChunkData(writer);
            uint size = (uint)(writer.BaseStream.Position - start);
            writer.BaseStream.Position = start - 8;
            writer.Write(size);
            writer.BaseStream.Position = start + size;
        }
        public static void Write(RmdChunk chunk, BinaryWriter writer, object? data)
        {
            writer.Write((int)chunk.Type);
            writer.Write(0);
            writer.Write(chunk.Version);
            long start = writer.BaseStream.Position;
            chunk.WriteChunkData(writer, data);
            uint size = (uint)(writer.BaseStream.Position - start);
            writer.BaseStream.Position = start - 8;
            writer.Write(size);
            writer.BaseStream.Position = start + size;
        }
        public static RmdChunk Read(BinaryReader reader)
        {
            RmdChunkType type = (RmdChunkType)reader.ReadInt32();
            uint size = reader.ReadUInt32();
            int version = reader.ReadInt32();

            return (type) switch
            {
                RmdChunkType.String => new RwString(type, size, version, reader),
                RmdChunkType.TextureNative => new RwTextureNative(type, size, version, reader),
                RmdChunkType.TextureDictionary => new RwTextureDictionary(type, size, version, reader),
                _ => new RmdBinaryChunk(type, size, version, reader),
            };
        }
        public static RmdChunk ReadStruct(BinaryReader reader, RmdChunkType ParentType, object? data = null)
        {
            RmdChunkType type = (RmdChunkType)reader.ReadInt32();
            uint size = reader.ReadUInt32();
            int version = reader.ReadInt32();

            switch (ParentType)
            {
                case RmdChunkType.TextureNative:
                    {
                        if (data == null)
                            return new RasterInfoStruct(type, size, version, reader);
                        else
                            return new RasterDataStruct(type, size, version, reader, (RasterInfoStruct)data);
                    }
                default: return new RmdBinaryChunk(type, size, version, reader);
            }
        }
        protected virtual void ReadChunkData(BinaryReader reader, uint ChunkSize) { }
        protected virtual void ReadChunkData(BinaryReader reader, uint ChunkSize, object? data) { }
        protected virtual void WriteChunkData(BinaryWriter writer) { }
        protected virtual void WriteChunkData(BinaryWriter writer, object? data = null) { }
        protected virtual void WriteChunkData() { }
    }
    public class RmdBinaryChunk : RmdChunk
    {
        public byte[] Data;
        public RmdBinaryChunk(RmdChunkType type, int version)
            : base(type, version)
        {
            Data = Array.Empty<byte>();
        }
        public RmdBinaryChunk(RmdChunkType type, int version, byte[] data)
            : base(type, version)
        {
            Data = data;
        }
        public RmdBinaryChunk(RmdChunkType type, uint size, int version, BinaryReader reader)
           : base(type, size, version, reader)
        {
        }
        protected override void WriteChunkData(BinaryWriter writer)
        {
            if (Data != null)
                writer.Write(Data);
        }
        protected override void ReadChunkData(BinaryReader reader, uint Size)
        {
            Data = reader.ReadBytes((int)Size);
        }
    }
}
