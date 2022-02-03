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
    public partial class SearchForm : Form
    {
        public class TextChangedEventArgs:EventArgs
        {
            public string text;
        }

        public class SelectedItemChangedEventArgs : EventArgs
        {
            public DataGridViewRow row;
        }

        public delegate void TextChangedEventHandler(object sender, TextChangedEventArgs e);
        public delegate void SelectedItemChangedEventHandler(object sender, SelectedItemChangedEventArgs e);

        public event TextChangedEventHandler TextChangedEvent;
        public event SelectedItemChangedEventHandler SelectedItemChangedEvent;

        List<DataGridViewRow> items = new List<DataGridViewRow>();

        public SearchForm()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (TextChangedEvent != null) 
            {
                TextChangedEventArgs ea = new TextChangedEventArgs();
                ea.text = textBox1.Text;
                TextChangedEvent(this, ea);
            }
        }

        public void SetResult(List<DataGridViewRow> result)
        {
            items = result;
            listView1.Items.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                string name = ((string)items[i].Cells[1].Value).ToLower();
                string author = ((string)items[i].Cells[2].Value).ToLower();
                listView1.Items.Add(name + " - " + author);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedItemChangedEvent != null)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    SelectedItemChangedEventArgs ea = new SelectedItemChangedEventArgs();
                    ea.row = items[listView1.SelectedItems[0].Index];
                    SelectedItemChangedEvent(this, ea);
                }
            }
        }
    }
}
