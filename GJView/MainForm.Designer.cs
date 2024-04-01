namespace GJView
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openPaletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TexView = new GJView.TextureView();
            this.FileMenuStrip = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closePaletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.openPaletteToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.closePaletteToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.openToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(270, 26);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // openPaletteToolStripMenuItem
            // 
            this.openPaletteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.openPaletteToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.openPaletteToolStripMenuItem.Name = "openPaletteToolStripMenuItem";
            this.openPaletteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this.openPaletteToolStripMenuItem.Size = new System.Drawing.Size(270, 26);
            this.openPaletteToolStripMenuItem.Text = "Open Palette";
            this.openPaletteToolStripMenuItem.Click += openPaletteToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.closeToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(270, 26);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // TexView
            // 
            this.TexView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TexView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(44)))), ((int)(((byte)(45)))));
            this.TexView.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.TexView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TexView.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(50)))), ((int)(((byte)(51)))));
            this.TexView.Location = new System.Drawing.Point(0, 30);
            this.TexView.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.TexView.Name = "TexView";
            this.TexView.Size = new System.Drawing.Size(914, 570);
            this.TexView.TabIndex = 2;
            // 
            // FileMenuStrip
            // 
            this.FileMenuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.FileMenuStrip.ForeColor = System.Drawing.Color.White;
            this.FileMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.FileMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.FileMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.FileMenuStrip.Name = "FileMenuStrip";
            this.FileMenuStrip.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            this.FileMenuStrip.Size = new System.Drawing.Size(914, 30);
            this.FileMenuStrip.TabIndex = 1;
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += helpToolStripMenuItem_Click;
            // 
            // closePaletteToolStripMenuItem
            // 
            this.closePaletteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.closePaletteToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.closePaletteToolStripMenuItem.Name = "closePaletteToolStripMenuItem";
            this.closePaletteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.closePaletteToolStripMenuItem.Size = new System.Drawing.Size(270, 26);
            this.closePaletteToolStripMenuItem.Text = "Close Palette";
            this.closePaletteToolStripMenuItem.Click += closePaletteToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 600);
            this.Controls.Add(this.TexView);
            this.Controls.Add(this.FileMenuStrip);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "GJView";
            this.FileMenuStrip.ResumeLayout(false);
            this.FileMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private MenuStrip FileMenuStrip;
        private TextureView TexView;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem openPaletteToolStripMenuItem;
        private ToolStripMenuItem closePaletteToolStripMenuItem;
    }
}