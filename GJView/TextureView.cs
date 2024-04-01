using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace GJView
{
    public partial class TextureView : UserControl
    {
        public Bitmap? CurrentTexture;
        public float TextureScale = 1;
        public float TextureOffsetX, TextureOffsetY = 0;
        public bool Filtering = true;
        private float ViewScale;
        private Point LastMouseLocation;
        private ColorPalette? RealPalette;
        private ColorPalette? OverwritePalette;
        public TextureView()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(brush, 0, 0, Width, Height);

            if (CurrentTexture == null)
                return;

            float BoxRatio = (float)Width / (float)Height;

            int TexWidth = CurrentTexture.Width;
            int TexHeight = CurrentTexture.Height;

            float TextureRatio = (float)TexWidth / (float)TexHeight;

            ViewScale = (TextureRatio >= BoxRatio) ? (float)Width / (float)TexWidth : (float)Height / (float)TexHeight;

            int ImageWidth = (int)MathF.Floor(ViewScale * (float)TexWidth * TextureScale);
            int ImageHeight = (int)MathF.Floor(ViewScale * (float)TexHeight * TextureScale);

            int XOffset = (int)MathF.Floor((Width - ImageWidth) / 2 + TextureOffsetX);
            int YOffset = (int)MathF.Floor((Height - ImageHeight) / 2 + TextureOffsetY);

            e.Graphics.InterpolationMode = Filtering ? InterpolationMode.Default : InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = Filtering ? PixelOffsetMode.Default : PixelOffsetMode.Half;

            using (Brush brush = new SolidBrush(ForeColor))
                e.Graphics.FillRectangle(brush, XOffset, YOffset, ImageWidth, ImageHeight);

            e.Graphics.DrawImage(CurrentTexture, XOffset, YOffset, ImageWidth, ImageHeight);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                ResetScale();
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                RealScale();
            else if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
                TextureOffsetX -= 5;
            else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
                TextureOffsetX += 5;
            else if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
                TextureOffsetY -= 5;
            else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
                TextureOffsetY += 5;
            else if (e.KeyCode == Keys.Z || e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
            {
                TextureScale += TextureScale * 0.1f;
                TextureOffsetX += TextureOffsetX * 0.1f;
                TextureOffsetY += TextureOffsetY * 0.1f;
            }
            else if (e.KeyCode == Keys.X || e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
            {
                TextureScale -= TextureScale * 0.1f;
                TextureOffsetX -= TextureOffsetX * 0.1f;
                TextureOffsetY -= TextureOffsetY * 0.1f;
            }
            else if (e.KeyCode == Keys.F)
                Filtering = !Filtering;
            else
                return;

            Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                TextureOffsetX += e.Location.X - LastMouseLocation.X;
                TextureOffsetY += e.Location.Y - LastMouseLocation.Y;
                Invalidate();
            }

            LastMouseLocation = e.Location;
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            TextureScale += TextureScale * (e.Delta * 0.001f);
            TextureOffsetX += TextureOffsetX * (e.Delta * 0.001f);
            TextureOffsetY += TextureOffsetY * (e.Delta * 0.001f);
            Invalidate();
        }
        public void RealScale()
        {
            ResetScale();
            if (CurrentTexture == null)
                return;
            TextureScale = 1 / ViewScale;
            Invalidate();
        }
        public void ResetScale()
        {
            TextureScale = 1;
            TextureOffsetX = TextureOffsetY = 0;
            Invalidate();
        }
        public void SetTexture(Bitmap? Image)
        {
            ResetScale();
            CurrentTexture = Image;
            RealPalette = CurrentTexture?.Palette;
            if (CurrentTexture != null && OverwritePalette != null)
                CurrentTexture.Palette = OverwritePalette;
            Invalidate();
        }
        public void SetPalette(ColorPalette? palette)
        {
            if (palette == null || palette?.Entries.Length == 0)
            {
                OverwritePalette = null;
                if (CurrentTexture != null && RealPalette != null)
                    CurrentTexture.Palette = RealPalette;
            }
            else
            {
                OverwritePalette = palette;
                if (OverwritePalette != null && CurrentTexture != null)
                    CurrentTexture.Palette = OverwritePalette;
            }
            Invalidate();
        }
    }
}
