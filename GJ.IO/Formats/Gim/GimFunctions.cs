using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GimLib.GimEnums;

namespace GimLib
{
    public class GimFunctions
    {
        public static void WriteString(BinaryWriter writer, string Input)
        {
            for (int i = 0; i < Input.Length; i++)
                writer.Write(Input[i]);
            writer.Write((byte)0);
        }
        public static string ReadString(BinaryReader reader)
        {
            string Output = "";
            byte b = 1;
            while (b != 0)
            {
                b = reader.ReadByte();
                if (b != 0)
                    Output += Encoding.UTF8.GetString(new byte[] { b });
            }
            return Output;
        }
        
    }
}
