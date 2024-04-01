using System.Drawing;
using static GJ.IO.BitMapMethods;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GJConv
{
    internal class Program
    {
        static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GJConv.cfg");
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
            bool fwNext = false;
            bool fhNext = false;
            
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
                    config.TmxUseFilenameForUserComment = false;
                    config.TmxUserComment = args[i];
                }
                else if (fwNext)
                {
                    if (!int.TryParse(args[i], out config.ForceWidth))
                    {
                        Console.WriteLine("Bad Args!");
                        Info();
                    }
                    fwNext = false;
                }
                else if (fhNext)
                {
                    if (!int.TryParse(args[i], out config.ForceHeight))
                    {
                        Console.WriteLine("Bad Args!");
                        Info();
                    }
                    fhNext = false;
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
                    else if (a == "-fw")
                        fwNext = true;
                    else if (a == "-fh")
                        fhNext = true;
                    else if (a == "-ls")
                        config.LinearScaling = !config.LinearScaling;
                    else if (a == "-f2")
                        config.ForcePowerOfTwo = !config.ForcePowerOfTwo;
                    else if (a == "-ls")
                        config.TmxUseFilenameForUserComment = !config.TmxUseFilenameForUserComment;
                    else if (a == "-po")
                        config.GimPSPOrder = !config.GimPSPOrder;
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

            if (!OutputPath.Contains('.'))
                OutputPath += $".{config.DefaultOutputFormat}";
            if (!File.Exists(InputPath))
            {
                Console.WriteLine($"File: {InputPath} does not exist!");
                Console.WriteLine("\nPress any key to exit");
                Console.ReadKey();
                return;
            }

            if (config.TmxUseFilenameForUserComment)
                config.TmxUserComment = Path.GetFileNameWithoutExtension(OutputPath);
            

            string se = InputPath.Contains('.') ? Path.GetExtension(InputPath)[1..].ToLower() : "";

            ImgType Ext = ImgType.png;
            Enum.TryParse(se, out Ext);
            //if (!Enum.TryParse(se, out ImgType Ext))
            //    throw new Exception($"Unsupported extension: {se}");

            Console.WriteLine($"Importing: {Path.GetFullPath(InputPath)}...");
            Bitmap NewImage; 
            PixelFormat ExportPixelFormat;
            using (Bitmap image = ImportBitmap(args[0]))
            {
                ExportPixelFormat = image.PixelFormat; //Create a new image to allow for easier editing and free the lock on the input file
                NewImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                BitmapData data = NewImage.LockBits(new Rectangle(0, 0, NewImage.Width, NewImage.Height), ImageLockMode.WriteOnly, NewImage.PixelFormat);
                Color[] Colors = GetPixels(image);
                if (config.SwapRedAndBlue)
                {
                    Parallel.For(0, Colors.Length, i =>
                    {
                         Colors[i] = Color.FromArgb(Colors[i].A, Colors[i].B, Colors[i].G, Colors[i].R);
                    });
                }

                byte[] pixels = new byte[Colors.Length * 4];
                for (int i = 0; i < Colors.Length; i++)
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
                using (BinaryReader reader = new(new FileStream(InputPath, FileMode.Open)))
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

            string se2 = Path.GetExtension(OutputPath)[1..].ToLower();
            if (!Enum.TryParse(se2, out ImgType OExt))
                OExt = config.DefaultOutputFormat;

            if (OExt == ImgType.txn)
            {
                bool TooBig = false;
                if (config.ForceWidth > 1024 || NewImage.Width > 1024)
                {
                    TooBig = true;
                    config.ForceWidth = 1024;
                }
                if (config.ForceHeight > 1024 || NewImage.Height > 1024)
                {
                    TooBig = true;
                    config.ForceHeight = 1024;
                }
                if (TooBig)
                {
                    Console.WriteLine("WARNING: Textures bigger than 1024 are not supported by the selected format");
                    Console.WriteLine("your image will be downscaled.");
                }
                if (!config.ForcePowerOfTwo && (((config.ForceWidth > 0 && config.ForceWidth % 2 != 0) || (config.ForceWidth % 2 != 2 && config.ForceWidth > 0)) || (NewImage.Height % 2 != 0 || NewImage.Width % 2 != 0)))
                {
                    Console.WriteLine("WARNING: Images that are not a power of 2 are not supported by the selected format");
                    Console.WriteLine("your image is going to be resized.");
                    config.ForcePowerOfTwo = true;
                }
            }


            if (config.ForceHeight > 0 || config.ForceWidth > 0)
            {
                if (config.ForceHeight <= 0)
                    config.ForceHeight = NewImage.Height;
                if (config.ForceWidth <= 0)
                    config.ForceWidth = NewImage.Width;

                NewImage = ResizeImage(NewImage, new Size(config.ForceWidth, config.ForceHeight), config.LinearScaling);
            }


            if (config.ForcePowerOfTwo == true)
                NewImage = ForcePowerOfTwo(NewImage, config.LinearScaling);

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

            ExportBitmap(OutputPath,NewImage,OExt, config.TmxUserId, config.TmxUserComment, config.TmxUserTextureId, config.TmxUserClutId, config.TgaFlipHorizontal, config.TgaFlipVertical, config.GimPSPOrder);
            Console.WriteLine($"{OExt.ToString().ToUpper()} file exported to: {Path.GetFullPath(OutputPath)}");
        }
        static void Info()
        {
            Console.WriteLine("GJConv 0.4 by Pioziomgames");
            Console.WriteLine("\nUsage:");
            Console.WriteLine("\tGJConv.exe {args} {inputfile} {outputfile}(optional) {outputformat}(optional)");
            Console.WriteLine("\nDefault values of all arguments are editable in the config file");
            Console.WriteLine("Inputing Yes/No arguments will swap their value compared to the one in the config file");
            Console.WriteLine("\nArguments:");
            Console.WriteLine("\t-so         \tSolidify image                        (" + (config.Solidify ? "on" : "off") + " by default)");
            Console.WriteLine("\t-rb         \tSwap Red and Blue channels            (" + (config.SwapRedAndBlue ? "on" : "off") + " by default)");
            Console.WriteLine("\t-id         \tConvert image to indexed              (" + (config.ConvertToIndexed ? "on" : "off") + " by default)");
            Console.WriteLine("\t-fc         \tConvert image to full color           (" + (config.ConvertToFullColor ? "on" : "off") + " by default)");
            Console.WriteLine("\t-ls         \tUse Linear filtering for scaling      (" + (config.LinearScaling ? "on" : "off") + " by default)");
            Console.WriteLine("\t-f2         \tForce size to be a power of two       (" + (config.ForcePowerOfTwo ? "on" : "off") + " by default)");
            Console.WriteLine("\t-fw {int}   \tForce image width                     (" + (config.ForceWidth > 0? $"{config.ForceWidth}" : "off") + " by default)");
            Console.WriteLine("\t-fh {int}   \tForce image height                    (" + (config.ForceHeight > 0 ? $"{config.ForceHeight}" : "off") + " by default)");
            Console.WriteLine("\t-ui {short} \tTmx user id                           (" + config.TmxUserId + " by default)");
            Console.WriteLine("\t-uc {string}\tTmx user comment (turns off filename) (\"" + config.TmxUserComment + "\" by default)");
            Console.WriteLine("\t-uf         \tTmx use filename for user comment     (" + (config.TmxUseFilenameForUserComment ? "on" : "off") + " by default)");
            Console.WriteLine("\t-po         \tGim use PSP pixel order               (" + (config.GimPSPOrder ? "on" : "off") + " by default)");

            Console.WriteLine("\nPress any key to exit");
            Console.ReadKey();
            System.Environment.Exit(0);
        }
    }
}

