using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Roland_XP_50
{
    public partial class TrackListForm : Form
    {
        public string Text
        {
            set { textBox1.Text = value; }
        }
        public TrackListForm()
        {
            InitializeComponent();
        }
    }
}
