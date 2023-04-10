using System.IO;
using System.Drawing;
using static GJ.IO.BitMapMethods;
using System.Reflection;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GJConv
{
    internal class Program
    {
        static string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GJConv.cfg");
        static Config config;
        static void Main(string[] args)
        {
            if (File.Exists(ConfigPath))
                config = new Config(ConfigPath);
            else
            {
                config = new();
                Console.WriteLine("Config not found, creating a new one...\n");
                File.WriteAllText(ConfigPath, Config.DefaultConfig);
            }
            string InputPath = string.Empty;
            string OutputPath = string.Empty;
            bool uiNext = false;
            bool ucNext = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (uiNext)
                {
                    uiNext = false;
                    if (!short.TryParse(args[i], out config.TmxUserId))
                    {
                        Console.WriteLine("Bad Args!");
                        Info();
                    }
                }
                else if (ucNext)
                {
                    ucNext = false;
                    config.TmxUserComment = args[i];
                }
                else
                {
                    string a = args[i].ToLower();
                    if (a == "-so")
                        config.Solidify = !config.Solidify;
                    else if (a == "-id")
                        config.ConvertToIndexed = !config.ConvertToIndexed;
                    else if (a == "-fc")
                        config.ConvertToFullColor = !config.ConvertToFullColor;
                    else if (a == "-ui")
                        uiNext = true;
                    else if (a == "-uc")
                        ucNext = true;
                    else if (Enum.TryParse(a, out ImgType nExt))
                        config.DefaultOutputFormat = nExt;
                    else if (InputPath == string.Empty)
                        InputPath = args[i];
                    else
                        OutputPath = args[i];
                }
            }
            if (InputPath == string.Empty)
                Info();
            if (OutputPath == string.Empty)
                OutputPath = OutputPath = $"{Path.GetDirectoryName(InputPath)}\\{Path.GetFileNameWithoutExtension(InputPath)}.{config.DefaultOutputFormat}";

            if (OutputPath.IndexOf('.') == -1)
                OutputPath += $".{config.DefaultOutputFormat}";
            if (!File.Exists(InputPath))
            {
                Console.WriteLine($"File: {InputPath} does not exist!");
                Console.WriteLine("\nPress any key to exit");
                Console.ReadKey();
                return;
            }
            

            string se = Path.GetExtension(InputPath)[1..].ToLower();

            if (!Enum.TryParse(se, out ImgType Ext))
                throw new Exception($"Unsupported extension: {se}");

            Console.WriteLine($"Importing: {Path.GetFullPath(InputPath)}...");
            Bitmap NewImage; 
            PixelFormat ExportPixelFormat;
            using (Bitmap image = ImportBitmap(args[0], Ext))
            {
                ExportPixelFormat = image.PixelFormat; //Create a new image to allow for easier editing and free the lock on the input file
                NewImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                BitmapData data = NewImage.LockBits(new Rectangle(0, 0, NewImage.Width, NewImage.Height), ImageLockMode.WriteOnly, NewImage.PixelFormat);
                List<Color> Colors = GetPixels(image);
                if (config.SwapRedAndBlue)
                {
                    Parallel.For(0, Colors.Count, i =>
                    {
                         Colors[i] = Color.FromArgb(Colors[i].A, Colors[i].B, Colors[i].G, Colors[i].R);
                    });
                }

                byte[] pixels = new byte[Colors.Count * 4];
                for (int i = 0; i < Colors.Count; i++)
                {
                    int offset = i * 4;
                    pixels[offset] = Colors[i].B;
                    pixels[offset + 1] = Colors[i].G;
                    pixels[offset + 2] = Colors[i].R;
                    pixels[offset + 3] = Colors[i].A;
                }
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                NewImage.UnlockBits(data);
                image.Dispose();
            }
            if (Ext == ImgType.png) //Workaround for System.Drawing converting pngs to 32 bits on import
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(InputPath, FileMode.Open)))
                {
                    reader.BaseStream.Seek(12, SeekOrigin.Begin);
                    if (reader.ReadInt32() == 0x52444849) //IHDR
                    {
                        reader.BaseStream.Seek(9, SeekOrigin.Current);
                        if (reader.ReadByte() == 3)
                        {
                            ExportPixelFormat = PixelFormat.Format8bppIndexed;
                        }
                    }
                    
                }
            }

            if (config.Solidify == true)
                NewImage = Solidify(NewImage);

            if (config.ConvertToFullColor == true)
                ExportPixelFormat = PixelFormat.Format32bppArgb;

            if (config.ConvertToIndexed == true)
                ExportPixelFormat = PixelFormat.Format8bppIndexed;

            if (config.ConvertToFullColor == true)
                ExportPixelFormat = PixelFormat.Format32bppArgb;

            if (ExportPixelFormat.HasFlag(PixelFormat.Indexed))
                NewImage = LimitColors(NewImage);

            string se2 = Path.GetExtension(OutputPath)[1..].ToLower();
            if (!Enum.TryParse(se2, out ImgType OExt))
                OExt = config.DefaultOutputFormat;

            ExportBitmap(OutputPath,NewImage,OExt, config.TmxUserId, config.TmxUserComment, config.TmxUserTextureId, config.TmxUserClutId, config.TgaFlipHorizontal, config.TgaFlipVertical);
            Console.WriteLine($"{OExt.ToString().ToUpper()} file exported to: {Path.GetFullPath(OutputPath)}");
        }
        static void Info()
        {
            Console.WriteLine("GJConv 0.1 by Pioziomgames");
            Console.WriteLine("\nUsage:");
            Console.WriteLine("\tGJConv.exe {args} {inputfile} {outputfile}(optional) {outputformat}(optional)");
            Console.WriteLine("\nAll arguments are changable in the config");
            Console.WriteLine("Inputing Yes/No arguments will swap their value compared to the config");
            Console.WriteLine("\nArguments:");
            Console.WriteLine("\t-so         \tSolidify              (" + (config.Solidify ? "on" : "off") + " by default)");
            Console.WriteLine("\t-rb         \tSwap Red and Blue     (" + (config.SwapRedAndBlue ? "on" : "off") + " by default)");
            Console.WriteLine("\t-id         \tConvert to indexed    (" + (config.ConvertToIndexed ? "on" : "off") + " by default)");
            Console.WriteLine("\t-fc         \tConvert to full color (" + (config.ConvertToFullColor ? "on" : "off") + " by default)");
            Console.WriteLine("\t-ui {short} \tTmx user id           (" + config.TmxUserId + " by default)");
            Console.WriteLine("\t-uc {string}\tTmx user comment      (\"" + config.TmxUserComment + "\" by default)");

            Console.WriteLine("\nPress any key to exit");
            Console.ReadKey();
            System.Environment.Exit(0);
        }
        static string CheckMagic(string Path, string Extension)
        {
            byte[] file = File.ReadAllBytes(Path);

            if (file[0] == 'T' && file[1] == 'I' && file[2] == 'M' && file[3] == '2')
                Extension = "tm2";
            else if (file[0] == 'M' && file[1] == 'I' && file[2] == 'G')
                Extension = "gim";
            else if (file[0] == 'T' && file[1] == 'M' && file[3] == 'X')
                Extension = "tmx";
            return Extension;
        }
        
    }
}

