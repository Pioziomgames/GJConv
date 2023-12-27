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
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();
            TexView = new TextureView();
            FileMenuStrip = new MenuStrip();
            helpToolStripMenuItem = new ToolStripMenuItem();
            FileMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, closeToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.BackColor = Color.FromArgb(35, 32, 32);
            openToolStripMenuItem.ForeColor = Color.White;
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openToolStripMenuItem.Size = new Size(146, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.BackColor = Color.FromArgb(35, 32, 32);
            closeToolStripMenuItem.ForeColor = Color.White;
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            closeToolStripMenuItem.Size = new Size(146, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // TexView
            // 
            TexView.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TexView.BackColor = Color.FromArgb(60, 44, 45);
            TexView.BackgroundImageLayout = ImageLayout.Stretch;
            TexView.Dock = DockStyle.Fill;
            TexView.ForeColor = Color.FromArgb(73, 50, 51);
            TexView.Location = new Point(0, 24);
            TexView.Margin = new Padding(4, 3, 4, 3);
            TexView.Name = "TexView";
            TexView.Size = new Size(800, 426);
            TexView.TabIndex = 2;
            // 
            // FileMenuStrip
            // 
            FileMenuStrip.BackColor = Color.FromArgb(35, 32, 32);
            FileMenuStrip.ForeColor = Color.White;
            FileMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, helpToolStripMenuItem });
            FileMenuStrip.Location = new Point(0, 0);
            FileMenuStrip.Name = "FileMenuStrip";
            FileMenuStrip.Size = new Size(800, 24);
            FileMenuStrip.TabIndex = 1;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.H;
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            helpToolStripMenuItem.Click += helpToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(TexView);
            Controls.Add(FileMenuStrip);
            Name = "MainForm";
            ShowIcon = false;
            Text = "GJView";
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
            FileMenuStrip.ResumeLayout(false);
            FileMenuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private MenuStrip FileMenuStrip;
        private TextureView TexView;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
    }
}