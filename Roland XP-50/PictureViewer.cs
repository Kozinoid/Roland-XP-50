using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Roland_XP_50
{
    public partial class PictureViewer : UserControl
    {
        private string currentPicture = "";
        public string CurrentPicture
        {
            set
            {
                currentPicture = value;
                if (File.Exists(currentPicture))
                {
                    pictureBox1.Load(currentPicture);
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
        }

        public PictureViewer()
        {
            InitializeComponent();
        }
    }
}
