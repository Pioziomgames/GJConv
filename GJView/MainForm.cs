using static GJ.IO.BitMapMethods;

namespace GJView
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            FileMenuStrip.Renderer = new StripColorsRenderer();
            Text = Program.WindowText;
        }
        public MainForm(string Path)
            : this()
        {
            OpenFile(Path);
        }
        private void OpenFile(string path)
        {
            try
            {
                string se = Path.GetExtension(path)[1..].ToLower();

                if (!File.Exists(path))
                    return;
                if (!Enum.TryParse(se, out ImgType Ext))
                    return;
                Bitmap image = ImportBitmap(path, Ext);
                TexView.SetTexture(image);

                Text = $"{Program.WindowText} - {Path.GetFileName(path)} {image.Width}x{image.Height}";
            }
            catch { }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Supported Formats|*.";
            dialog.Filter += string.Join(";*.", (ImgType[])Enum.GetValues(typeof(ImgType)));
            
            if (dialog.ShowDialog() == DialogResult.OK)
                OpenFile(dialog.FileName);
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string path = ((string[])e.Data.GetData(DataFormats.FileDrop, false))[0];
                OpenFile(path);
            }
            catch { }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help NewHelp = new Help();
            NewHelp.ShowDialog();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Text = Program.WindowText;
            TexView.SetTexture(null);
        }
        private class StripColorsRenderer : ToolStripProfessionalRenderer
        {
            public StripColorsRenderer() : base(new StripColors()) { }
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
        }
        private class StripColors : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(60, 57, 57);
            public override Color MenuBorder => MenuItemSelected;
            public override Color MenuItemPressedGradientBegin => MenuItemSelected;
            public override Color MenuItemPressedGradientEnd => MenuItemSelected;
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(40, 37, 37);
            public override Color MenuItemSelectedGradientEnd => MenuItemSelectedGradientBegin;
            public override Color MenuItemBorder => MenuItemSelected;
            public override Color StatusStripBorder => MenuItemSelected;
            public override Color ToolStripBorder => MenuItemSelected;
            public override Color ToolStripDropDownBackground => MenuItemSelected;
            public override Color ImageMarginGradientBegin => MenuItemSelected;
            public override Color ImageMarginGradientMiddle => MenuItemSelected;
            public override Color ImageMarginGradientEnd => MenuItemSelected;
        }
    }
}