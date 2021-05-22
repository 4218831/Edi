using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfEmbedWinForm
{
    public partial class WinFormControl1 : UserControl
    {
        public WinFormControl1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("button clicked.");
        }

        private void WinFormControl1_MouseMove(object sender, MouseEventArgs e)
        {
            Trace.WriteLine(string.Format("Mouse move {0}, {1})", e.X, e.Y));
        }
    }
}
