using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GJ.IO.BitMapMethods;

namespace GJView
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
            Text = Program.WindowText + " - Help";
        }
    }
}
