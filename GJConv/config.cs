using System.IO;
using static GJ.IO.BitMapMethods;

namespace GJConv
{
    public class Config
    {
        public bool Solidify;
        public bool SwapRedAndBlue;
        public bool ConvertToIndexed;
        public bool ConvertToFullColor;
        public short TmxUserId;
        public string TmxUserComment;
        public int TmxUserTextureId;
        public int TmxUserClutId;
        public bool TgaFlipHorizontal;
        public bool TgaFlipVertical;
        public ImgType DefaultOutputFormat;
        public void Default()
        {
            Solidify = true;
            SwapRedAndBlue = false;
            ConvertToIndexed = false;
            ConvertToFullColor = false;
            TmxUserId = 0;
            TmxUserComment = String.Empty;
            TmxUserTextureId = 0;
            TmxUserClutId = 0;
            TgaFlipHorizontal = false;
            TgaFlipVertical = false;
            DefaultOutputFormat = ImgType.tmx;
        }
        public Config()
        {
            Default();
        }
        public Config(string Path)
        {
            Default();
            string[] Lines = File.ReadAllLines(Path);

            for (int i = 0; i < Lines.Length; i++)
            {
                string line = Lines[i].Replace("\t", "");
                if (line.Contains("//"))
                    line = line[..line.IndexOf("//")];
                line = line.Replace(" ", "");
                line = line.ToLower();
                if (line.IndexOf('=') == -1)
                    continue;
                try
                {
                    switch (line.Substring(0, line.IndexOf('=')))
                    {
                        case "tmxuserid":
                            TmxUserId = ReadShort(line);
                            break;
                        case "tmxusertextureid":
                            TmxUserTextureId = ReadInt(line);
                            break;
                        case "tmxuserclutid":
                            TmxUserClutId = ReadInt(line);
                            break;
                        case "tmxusercomment":
                            TmxUserComment = ReadString(line);
                            if (TmxUserComment.Length > 28)
                            {
                                DetailedWarning(i, "TmxUserComment cannot be longer than 28");
                                TmxUserComment = TmxUserComment.Substring(0, 28);
                            }
                            break;
                        case "solidify":
                            Solidify = ReadOnOff(line, i);
                            break;
                        case "swapredandblue":
                            SwapRedAndBlue = ReadOnOff(line, i);
                            break;
                        case "converttoindexed":
                            ConvertToIndexed = ReadOnOff(line, i);
                            break;
                        case "converttofullcolor":
                            ConvertToFullColor = ReadOnOff(line, i);
                            break;
                        case "tgafliphorizontal":
                            TgaFlipHorizontal = ReadOnOff(line, i);
                            break;
                        case "tgaflipvertical":
                            TgaFlipVertical = ReadOnOff(line, i);
                            break;
                        case "defaultexportformat":
                            if (!Enum.TryParse(line.Substring(line.IndexOf('=') + 1), out ImgType OExt))
                                DetailedWarning(i, $"Unrecognized format: \"{line.Substring(line.IndexOf('=') + 1)}\"");
                            else
                                DefaultOutputFormat = OExt;
                            break;
                        default:
                            Console.WriteLine($"Config line {i+1}: Unrecognized option: \"{line.Substring(0, line.IndexOf('='))}\"");
                            break;
                    }
                }
                catch
                {
                    GenericWarning(i);
                }
            }
        }
        private short ReadShort(string line)
        {
            if (!short.TryParse(line.Substring(line.IndexOf('=') + 1), out short s))
                throw new Exception();
            else
                return s;
        }
        private int ReadInt(string line)
        {
            if (!int.TryParse(line.Substring(line.IndexOf('=') + 1), out int i))
                throw new Exception();
            else
                return i;
        }
        private string ReadString(string line)
        {
            int start = line.IndexOf('"') + 1;
            int size = line.IndexOf('"', start) - start;
            return line.Substring(start, size);
        }
        private bool ReadOnOff(string line, int i)
        {
            if (line[(line.IndexOf('=') + 1)..] == "on")
                return true;
            else if (line[(line.IndexOf('=') + 1)..] == "off")
                return false;
            else
            {
                Console.WriteLine($"Config line {i+1}: Unrecognized option!");
                throw new Exception();
            }
        }
        private void DetailedWarning(int line, string Warning)
        {
            Console.WriteLine($"Config line {line+1}: {Warning}!");
        }
        private void GenericWarning(int line)
        {
            Console.WriteLine($"Config line {line+1}: Improper formating!");
        }
        public static string DefaultConfig =
@"//================================
// GJConv configuration
//================================

//General
DefaultExportFormat = tmx // tmx tm2 gim tga png bmp jpg gif tif
Solidify = on // on off
SwapRedAndBlue = off // on off
ConvertToFullColor = off // on off
ConvertToIndexed = off // on off

//Tmx
TmxUserId = 0
TmxUserComment = """" // Cannot be longer than 28 characters
TmxUserTextureId = 0
TmxUserClutId = 0

//Tga
TgaFlipHorizontal = off // on off
TgaFlipVertical = off // on off";
    }
}
