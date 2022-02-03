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
    public partial class StatisticForm : Form
    {
        public ListView DisplayedList
        {
            get
            {
                return listView1;
            }
        }
        public StatisticForm()
        {
            InitializeComponent();
        }
    }
}
