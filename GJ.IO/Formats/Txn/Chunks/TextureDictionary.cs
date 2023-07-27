using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TxnLib.RmdEnums;

namespace TxnLib
{
    public class RwTextureDictionary : RmdChunk
    {
        public ushort TextureCount { get { return (ushort)Textures.Count; } }
        public RwDevice Device;
        public List<RwTextureNative> Textures;
        public RmdChunk Extension;
        public RwTextureDictionary(List<RwTextureNative> textures, RwDevice device, RmdChunk? extension = null, int version = 469893175)
            : base(RmdChunkType.TextureDictionary, version)
        {
            Textures = textures;
            Device = device;
            if (extension == null)
                Extension = new RmdBinaryChunk(RmdChunkType.Extension, 469893175);
            else
                Extension = extension;
        }
        public RwTextureDictionary(RmdChunkType type, uint size, int version, BinaryReader reader)
           : base(type, size, version, reader)
        {
        }
        protected override void ReadChunkData(BinaryReader reader, uint Size)
        {
            reader.BaseStream.Position += 12;
            Textures = new();
            ushort textureCount = reader.ReadUInt16();
            Device = (RwDevice)reader.ReadUInt16();

            for (int i = 0; i < textureCount; i++)
                Textures.Add((RwTextureNative)RmdChunk.Read(reader));

            Extension = RmdChunk.Read(reader);
        }
        protected override void WriteChunkData(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(4);
            writer.Write(469893175);
            writer.Write(TextureCount);
            writer.Write((ushort)Device);
            for (int i = 0;i < Textures.Count;i++)
                RmdChunk.Write(Textures[i], writer);

            RmdChunk.Write(Extension, writer);
        }
    }
}
