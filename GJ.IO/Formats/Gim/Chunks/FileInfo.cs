using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GimLib.GimFunctions;
using static GimLib.GimEnums;

namespace GimLib.Chunks
{
    public class GimFileInfo : GimChunk
    {
        public string ProjectName;
        public string UserName;
        public string SavedDate;
        public string Originator;
        public GimFileInfo(string projectName, string userName, string savedData, string originator, GimChunk? parent)
        {
            ProjectName = projectName;
            UserName = userName;
            SavedDate = savedData;
            Originator = originator;
            Parent = parent;
            Type = GimChunkType.GimFileInfo;
        }
        public GimFileInfo(GimChunk? parent, ref GimEnums.GimChunkHeader Header, BinaryReader reader)
            : base(parent, ref Header, reader)
        {
        }

        protected override void ReadData(ref GimEnums.GimChunkHeader Header, BinaryReader reader)
        {
            ProjectName = ReadString(reader);
            UserName = ReadString(reader);
            SavedDate = ReadString(reader);
            Originator = ReadString(reader);
        }
        protected override void WriteData(BinaryWriter writer)
        {
            WriteString(writer, ProjectName);
            WriteString(writer, UserName);
            WriteString(writer, SavedDate);
            WriteString(writer, Originator);
        }
    }
}
