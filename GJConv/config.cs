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
        public bool TmxUseFilenameForUserComment;
        public int TmxUserTextureId;
        public int TmxUserClutId;
        public bool TgaFlipHorizontal;
        public bool TgaFlipVertical;
        public bool LinearScaling;
        public bool ForcePowerOfTwo;
        public bool GimPSPOrder;
        public int ForceWidth;
        public int ForceHeight;
        public ImgType DefaultOutputFormat;
        public void Default()
        {
            Solidify = true;
            SwapRedAndBlue = false;
            ConvertToIndexed = false;
            ConvertToFullColor = false;
            LinearScaling = true;
            ForcePowerOfTwo = false;
            TmxUserId = 0;
            TmxUserComment = String.Empty;
            TmxUserTextureId = 0;
            TmxUserClutId = 0;
            ForceWidth = 0;
            ForceHeight = 0;
            TmxUseFilenameForUserComment = true;
            TgaFlipHorizontal = false;
            TgaFlipVertical = false;
            GimPSPOrder = false;
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
                if (!line.Contains('='))
                    continue;
                try
                {
                    switch (line[..line.IndexOf('=')])
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
                        case "forcewidth":
                            ForceWidth = ReadInt(line);
                            break;
                        case "forceheight":
                            ForceHeight = ReadInt(line);
                            break;
                        case "gimpsporder":
                            GimPSPOrder = ReadOnOff(line, i);
                            break;
                        case "tmxusercomment":
                            TmxUserComment = ReadString(line);
                            if (TmxUserComment.Length > 28)
                            {
                                DetailedWarning(i, "TmxUserComment cannot be longer than 28");
                                TmxUserComment = TmxUserComment[..28];
                            }
                            break;
                        case "tmxusefilenameforusercomment":
                            TmxUseFilenameForUserComment = ReadOnOff(line, i);
                            break;
                        case "solidify":
                            Solidify = ReadOnOff(line, i);
                            break;
                        case "linearscaling":
                            LinearScaling = ReadOnOff(line, i);
                            break;
                        case "forcepoweroftwo":
                            ForcePowerOfTwo = ReadOnOff(line, i);
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
                            if (!Enum.TryParse(line[(line.IndexOf('=') + 1)..], out ImgType OExt))
                                DetailedWarning(i, $"Unrecognized format: \"{line[(line.IndexOf('=') + 1)..]}\"");
                            else
                                DefaultOutputFormat = OExt;
                            break;
                        default:
                            Console.WriteLine($"Config line {i+1}: Unrecognized option: \"{line[..line.IndexOf('=')]}\"");
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
            if (!short.TryParse(line[(line.IndexOf('=') + 1)..], out short s))
                throw new Exception();
            else
                return s;
        }
        private int ReadInt(string line)
        {
            if (!int.TryParse(line[(line.IndexOf('=') + 1)..], out int i))
                throw new Exception();
            else
                return i;
        }
        private static string ReadString(string line)
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
        private static void DetailedWarning(int line, string Warning)
        {
            Console.WriteLine($"Config line {line+1}: {Warning}!");
        }
        private static void GenericWarning(int line)
        {
            Console.WriteLine($"Config line {line+1}: Improper formating!");
        }
        public readonly static string DefaultConfig =
@"//================================
// GJConv configuration
//================================

//General
DefaultExportFormat = tmx // tmx tm2 gim tga png bmp jpg gif tif txn
Solidify = on // on off
SwapRedAndBlue = off // on off
ConvertToFullColor = off // on off
ConvertToIndexed = off // on off
LinearScaling = on // on off
ForcePowerOfTwo = off // on off

//Tmx
TmxUserId = 0
TmxUserComment = """" // Cannot be longer than 28 characters
TmxUseFilenameForUserComment = on // on off
//(while this is on the value of TmxUserComment is ignored)
TmxUserTextureId = 0
TmxUserClutId = 0

//Gim
GimPSPOrder = off // on off

//Tga
TgaFlipHorizontal = off // on off
TgaFlipVertical = off // on off";
    }
}
