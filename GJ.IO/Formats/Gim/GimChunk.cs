using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GimLib.Chunks;
using static GimLib.GimEnums;
using static GimLib.GimFunctions;
using static GJ.IO.IOFunctions;

namespace GimLib
{
    public abstract class GimChunk
    {
        public GimChunkType Type;
        public List<GimChunk> Children = new();
        public GimChunk? Parent;

        public GimChunk()
        {
        }
        public GimChunk(GimChunkType type, GimChunk? parent)
        {
            Type = type;
            Parent = parent;
            Children = new List<GimChunk>();
        }
        public List<GimChunk> GatherChildrenOfType(GimChunkType type, bool searchChildrenOfChildren = true)
        {
            List<GimChunk> output = new();

            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].Type == type)
                    output.Add(Children[i]);
                else if (searchChildrenOfChildren)
                    output.AddRange(Children[i].GatherChildrenOfType(type, true));
            }
            return output;
        }
        public GimChunk(GimChunk? parent, ref GimChunkHeader Header, BinaryReader reader)
            : this(Header.Type, parent)
        {
            ReadData(ref Header, reader);
            Align(reader, 4);
            ReadChildren(ref Header, reader);
        }

        public static GimChunk Read(GimChunk? Parent, BinaryReader reader)
        {
            GimChunkHeader Header = new(reader);

            return (Header.Type) switch
            {
                GimChunkType.GimImage => new GimImage(Parent, ref Header, reader),
                GimChunkType.GimPalette => new GimPalette(Parent, ref Header, reader),
                GimChunkType.GimFileInfo => new GimFileInfo(Parent, ref Header, reader),
                _ => new GimBinaryChunk(Parent, ref Header, reader),
            };
        }

        protected virtual void ReadData(ref GimChunkHeader Header, BinaryReader reader) { }
        protected virtual void ReadChildren(ref GimChunkHeader Header, BinaryReader reader)
        {
            var end = Header.Start + Header.NextOffs - 7; // smallest possible chunk is 8 bytes
            while (reader.BaseStream.Position < end)
                Children.Add(GimChunk.Read(this, reader));
            Align(reader, 4);
        }
        public static void Write(GimChunk chunk, BinaryWriter writer)
        {
            long start = (int)writer.BaseStream.Position;
            long end;

            // Skip header values
            writer.BaseStream.Seek(16, SeekOrigin.Current);

            // Write body
            var argsOffs = writer.BaseStream.Position - start;

            var dataOffs = writer.BaseStream.Position - start;
            chunk.WriteData(writer);
            Align(writer, 4);

            var childOffs = writer.BaseStream.Position - start;
            chunk.WriteChildren(writer);
            Align(writer, 4);

            end = writer.BaseStream.Position;
            var nextOffs = end - start;

            // Go back and write header values
            writer.BaseStream.Seek(start, SeekOrigin.Begin);
            writer.Write((ushort)chunk.Type);
            writer.Write((ushort)argsOffs);
            writer.Write((uint)nextOffs);
            writer.Write((uint)childOffs);
            writer.Write((uint)dataOffs);

            // Return to end
            writer.BaseStream.Seek(end, SeekOrigin.Begin);
        }
        protected virtual void WriteData(BinaryWriter writer) { }
        protected virtual void WriteChildren(BinaryWriter writer)
        {
            for (int i = 0; i < Children.Count; i++)
                GimChunk.Write(Children[i], writer);
        }
    }

    public class GimBinaryChunk : GimChunk
    {
        public byte[] Data;

        public GimBinaryChunk(byte[] data, GimChunkType ChunkType, List<GimChunk> children, GimChunk? parent = null)
        {
            Type = ChunkType;
            Children = children;
            Parent = parent;
            Data = data;
        }
        public GimBinaryChunk(GimChunk? parent, ref GimChunkHeader Header, BinaryReader reader)
            : base(parent, ref Header, reader)
        {
        }
        protected override void ReadData(ref GimChunkHeader Header, BinaryReader reader)
        {
            int dataSize = (int)Header.ChildOffs - (int)Header.DataOffs;
            if (dataSize > 0)
                Data = reader.ReadBytes(dataSize);
        }
        protected override void WriteData(BinaryWriter writer)
        {
            if (Data != null)
                writer.Write(Data);
        }
    }
}
