namespace GJView
{
    partial class Help
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            InfoLbl = new Label();
            SuspendLayout();
            // 
            // InfoLbl
            // 
            InfoLbl.Dock = DockStyle.Fill;
            InfoLbl.ForeColor = Color.White;
            InfoLbl.Location = new Point(0, 0);
            InfoLbl.Name = "InfoLbl";
            InfoLbl.Size = new Size(239, 107);
            InfoLbl.TabIndex = 0;
            InfoLbl.Text = "wasd\\arrows\\hold mouse - move texture\r\n+ -\\z x\\scroll - zoom in\\out\r\nspace - reset texture position\r\nenter - display texture at real scale\r\nf - toggle filtering";
            InfoLbl.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Help
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(60, 44, 45);
            ClientSize = new Size(239, 107);
            Controls.Add(InfoLbl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Help";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "GJView";
            TopMost = true;
            ResumeLayout(false);
        }

        #endregion

        private Label InfoLbl;
    }
}