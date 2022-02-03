using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Roland_XP_50
{
    public partial class HotKeysForm : Form
    {
        public struct HotBankStruct
        {
            public int BankNumber;
            public int KeyNumber;
        }

        public HotBankStruct[] HotKeys = new HotBankStruct[10];

        public HotKeysForm()
        {
            InitializeComponent();

            for (int i = 0; i < 10; i++)
            {
                HotKeys[i].BankNumber = 0;
                HotKeys[i].KeyNumber = 0;
            }
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                listBox1.SelectedIndex = HotKeys[0].BankNumber;
                listBox2.SelectedIndex = HotKeys[0].KeyNumber;
            }
            if (radioButton2.Checked)
            {
                listBox1.SelectedIndex = HotKeys[1].BankNumber;
                listBox2.SelectedIndex = HotKeys[1].KeyNumber;
            }
            if (radioButton3.Checked)
            {
                listBox1.SelectedIndex = HotKeys[2].BankNumber;
                listBox2.SelectedIndex = HotKeys[2].KeyNumber;
            }
            if (radioButton4.Checked)
            {
                listBox1.SelectedIndex = HotKeys[3].BankNumber;
                listBox2.SelectedIndex = HotKeys[3].KeyNumber;
            }
            if (radioButton5.Checked)
            {
                listBox1.SelectedIndex = HotKeys[4].BankNumber;
                listBox2.SelectedIndex = HotKeys[4].KeyNumber;
            }
            if (radioButton6.Checked)
            {
                listBox1.SelectedIndex = HotKeys[5].BankNumber;
                listBox2.SelectedIndex = HotKeys[5].KeyNumber;
            }
            if (radioButton7.Checked)
            {
                listBox1.SelectedIndex = HotKeys[6].BankNumber;
                listBox2.SelectedIndex = HotKeys[6].KeyNumber;
            }
            if (radioButton8.Checked)
            {
                listBox1.SelectedIndex = HotKeys[7].BankNumber;
                listBox2.SelectedIndex = HotKeys[7].KeyNumber;
            }
            if (radioButton9.Checked)
            {
                listBox1.SelectedIndex = HotKeys[8].BankNumber;
                listBox2.SelectedIndex = HotKeys[8].KeyNumber;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                HotKeys[0].BankNumber = listBox1.SelectedIndex;
            }
            if (radioButton2.Checked)
            {
                HotKeys[1].BankNumber = listBox1.SelectedIndex;
            }
            if (radioButton3.Checked)
            {
                HotKeys[2].BankNumber = listBox1.SelectedIndex;
            }
            if (radioButton4.Checked)
            {
                HotKeys[3].BankNumber = listBox1.SelectedIndex;
            }
            if (radioButton5.Checked)
            {
                HotKeys[4].BankNumber = listBox1.SelectedIndex;
            }
            if (radioButton6.Checked)
            {
                HotKeys[5].BankNumber = listBox1.SelectedIndex;
            }
            if (radioButton7.Checked)
            {
                HotKeys[6].BankNumber = listBox1.SelectedIndex;
            }
            if (radioButton8.Checked)
            {
                HotKeys[7].BankNumber = listBox1.SelectedIndex;
            }
            if (radioButton9.Checked)
            {
                HotKeys[8].BankNumber = listBox1.SelectedIndex;
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                HotKeys[0].KeyNumber = listBox2.SelectedIndex;
            }
            if (radioButton2.Checked)
            {
                HotKeys[1].KeyNumber = listBox2.SelectedIndex;
            }
            if (radioButton3.Checked)
            {
                HotKeys[2].KeyNumber = listBox2.SelectedIndex;
            }
            if (radioButton4.Checked)
            {
                HotKeys[3].KeyNumber = listBox2.SelectedIndex;
            }
            if (radioButton5.Checked)
            {
                HotKeys[4].KeyNumber = listBox2.SelectedIndex;
            }
            if (radioButton6.Checked)
            {
                HotKeys[5].KeyNumber = listBox2.SelectedIndex;
            }
            if (radioButton7.Checked)
            {
                HotKeys[6].KeyNumber = listBox2.SelectedIndex;
            }
            if (radioButton8.Checked)
            {
                HotKeys[7].KeyNumber = listBox2.SelectedIndex;
            }
            if (radioButton9.Checked)
            {
                HotKeys[8].KeyNumber = listBox2.SelectedIndex;
            }
        }
    }
}