namespace GJView
{
    internal static class Program
    {
        public static string WindowText = $"GJView v.{Application.ProductVersion}";
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            if (args.Length > 0)
                Application.Run(new MainForm(args[args.Length-1]));
            else
                Application.Run(new MainForm());
        }
    }
}