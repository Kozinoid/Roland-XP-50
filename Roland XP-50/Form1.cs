using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MIDI;
using System.Runtime.InteropServices;
using PIC18F4550Controller;
using System.IO;
using System.Diagnostics;

namespace Roland_XP_50
{
    public partial class Form1 : Form
    {
        private class PatchSet
        {
            public uint[] Commands = new uint[3];

            public PatchSet()
            {
                Commands[0] = 0x5100B0;
                Commands[1] = 0x0020B0;
                Commands[2] = 0xC0;
            }
        }

        private class PadSet
        {
            public PatchSet[] Pads = new PatchSet[4];

            public PadSet()
            {
                Pads[0] = new PatchSet();
                Pads[1] = new PatchSet();
                Pads[2] = new PatchSet();
                Pads[3] = new PatchSet();
            }
        }

        private MidiModule mm;
        private MIDIConnections mCon;
        private HotKeysForm hkf;
        private delegate void GetResHandler(MidiEventArgs mea);
        private GetResHandler myGetRes = null;
        private int currentPad = 1;
        private PatchSet[] currentPadSet = new PatchSet[10];
        private string defaultPath = "";
        private string defaultFilename = "";
        private bool opened = false;
        private Button[] buttonsArray = new Button[10];
        private PatchSet[ , ] instrumentsBase = new PatchSet[10, 6];
        private string[] groupNames = { "Pianos", "Organs", "Pads", "Strings", "Brasses", "Leads", "Bells", "Sequences", "El. Pianos", "Accordions"};
        private int currentGroup = 0;
        private System.Drawing.Text.InstalledFontCollection fontList = new System.Drawing.Text.InstalledFontCollection();
        private DataGridViewRow lastSelectedRow = null;
        string nullFile = "";

        public Form1()
        {
            InitializeComponent();

            buttonsArray[0] = bt_Pad0;
            buttonsArray[1] = bt_Pad1;
            buttonsArray[2] = bt_Pad2;
            buttonsArray[3] = bt_Pad3;
            buttonsArray[4] = bt_Pad4;
            buttonsArray[5] = bt_Pad5;
            buttonsArray[6] = bt_Pad6;
            buttonsArray[7] = bt_Pad7;
            buttonsArray[8] = bt_Pad8;
            buttonsArray[9] = bt_Pad9;

            for (int i = 0; i < 10; i++)
            {
                currentPadSet[i] = new PatchSet();
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    instrumentsBase[i, j] = new PatchSet();
                }
            }

            hkf = new HotKeysForm();

            defaultPath = Path.GetDirectoryName(Application.ExecutablePath);
            defaultFilename = Path.Combine(defaultPath, "default");

            mm = new MidiModule();
            mCon = new MIDIConnections(mm);
            if (mCon.ShowDialog() == DialogResult.Cancel)
            {
                // Отменить открытие программы
                opened = false;
                Environment.Exit(0);
            }
            else
            {
                mm.MidiEventRecieve += new EventHandler(mm_MidiEventRecieve);
                myGetRes = new GetResHandler(GetMIDIEvent);
                mm.InStart();

                if (File.Exists(defaultFilename))
                {
                    OpenAll(defaultFilename);
                }
                SelectCurrentGroup();
                RefreshAllBanks();
                opened = true;
            }

            nullFile = Path.Combine(Path.Combine(defaultPath, "Notes"), "Null.jpg");
        }

        private void mm_MidiEventRecieve(object sender, EventArgs e)
        {
            MidiEventArgs mea = (MidiEventArgs)e;
            if (myGetRes != null)
            {
                this.Invoke(myGetRes, mea);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (opened)
            {
                SaveAll(defaultFilename);
                mm.InStop();
                mm.MidiEventRecieve -= new EventHandler(mm_MidiEventRecieve);
                myGetRes = null;
                MessageBox.Show("Good bye! See you later!");
                mCon.CloseMIDI();
            }

            e.Cancel = false;
        }

        // Передача сообщений
        private void SendMIDIMessage(byte command, byte bt1, byte bt2, byte bt3)
        {
            if (cbTransmit.Checked)
            {
                byte[] data = new byte[4];
                uint msg = 0;

                data[0] = command;
                data[1] = bt1;
                data[2] = bt2;
                data[3] = bt3;
                msg = BitConverter.ToUInt32(data, 0);
                mm.OutSendMessage(msg);
            }
        }

        private void SendMIDIMessage(uint msg)
        {
            if (cbTransmit.Checked)
            {
                mm.OutSendMessage(msg);
            }
        }

        // Получение сообщений
        private void GetMIDIEvent(MidiEventArgs args)
        {

            uint command = args.dwParam1 & 0xFF;
            uint seccomm = args.dwParam1 & 0xFF00;
            if (command == 0xB0)
            {
                if (seccomm == 0x0000)
                {
                    currentPadSet[currentPad].Commands[0] = args.dwParam1;
                    RefreshBankName(currentPad);
                }
                else if (seccomm == 0x2000)
                {
                    currentPadSet[currentPad].Commands[1] = args.dwParam1;
                    RefreshBankName(currentPad);
                }
            }
            else if (command == 0xC0)
            {
                currentPadSet[currentPad].Commands[2] = args.dwParam1;
                RefreshBankName(currentPad);
            }
        }

        private void bt_Pad_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                if (buttonsArray[i].Equals(sender as Button))
                {
                    ChangeProgram(i);
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!editableToolStripMenuItem.Checked)
            {
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.A:
                            cbRecieve.Checked = !cbRecieve.Checked;
                            cbTransmit.Checked = !cbTransmit.Checked;
                            break;

                        case Keys.R:
                            cbRecieve.Checked = !cbRecieve.Checked;
                            break;

                        case Keys.T:
                            cbTransmit.Checked = !cbTransmit.Checked;
                            break;

                        case Keys.L:
                            SetNewNumeration();
                            break;
                    }
                }
                else
                {
                    switch (e.KeyCode)
                    {
                        case Keys.D1:
                            ChangeProgram(1);
                            break;

                        case Keys.D2:
                            ChangeProgram(2);
                            break;

                        case Keys.D3:
                            ChangeProgram(3);
                            break;

                        case Keys.D4:
                            ChangeProgram(4);
                            break;

                        case Keys.D5:
                            ChangeProgram(5);
                            break;

                        case Keys.D6:
                            ChangeProgram(6);
                            break;

                        case Keys.D7:
                            ChangeProgram(7);
                            break;

                        case Keys.D8:
                            ChangeProgram(8);
                            break;

                        case Keys.D9:
                            ChangeProgram(9);
                            break;

                        case Keys.D0:
                            ChangeProgram(0);
                            break;

                        case Keys.NumPad1:
                            KeyPadPushed(0);
                            break;

                        case Keys.NumPad2:
                            KeyPadPushed(1);
                            break;

                        case Keys.NumPad3:
                            KeyPadPushed(2);
                            break;

                        case Keys.NumPad4:
                            KeyPadPushed(3);
                            break;

                        case Keys.NumPad5:
                            KeyPadPushed(4);
                            break;

                        case Keys.NumPad6:
                            KeyPadPushed(5);
                            break;

                        case Keys.NumPad7:
                            KeyPadPushed(6);
                            break;

                        case Keys.NumPad8:
                            KeyPadPushed(7);
                            break;

                        case Keys.NumPad9:
                            KeyPadPushed(8);
                            break;
                    }
                }
            }
        }

        private void SetNewNumeration()
        {
            if ((MessageBox.Show("Are you shure?", "New numeration", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes))
                return;

            if (tabControl1.SelectedIndex == 0)
            {
                newNomerationToolStripMenuItem_Click(this, new EventArgs());
            }
            else
            {
                newNumerationToolStripMenuItem_Click(this, new EventArgs());
            }
        }

        private void KeyPadPushed(int number)
        {
            StoreCurrentGroup();
            currentGroup = hkf.HotKeys[number].BankNumber;
            SelectCurrentGroup();
            if (hkf.HotKeys[number].KeyNumber == 5)
            {
                ChangeProgram(0);
            }
            else
            {
                ChangeProgram(hkf.HotKeys[number].KeyNumber + 5);
            }
        }

        private void SelectPrev()
        {
            int ind = dataGridView1.Rows.Count - 1;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                ind = dataGridView1.SelectedRows[0].Index;
                ind--;
                if (ind < 0) ind = 0;
            }
            dataGridView1.Rows[ind].Selected = true;
        }

        private void SelectNext()
        {
            int ind = 0;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                ind = dataGridView1.SelectedRows[0].Index;
                ind++;
                if (ind == dataGridView1.Rows.Count) ind = dataGridView1.Rows.Count - 1;
            }
            dataGridView1.Rows[ind].Selected = true;
        }

        private void MoveSelectedUp()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow dgvr = dataGridView1.SelectedRows[0];
                int index = dataGridView1.Rows.IndexOf(dgvr);
                if (index > 0)
                {
                    dataGridView1.Rows.RemoveAt(index);
                    dataGridView1.Rows.Insert(index - 1, dgvr);
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[index - 1].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[index - 1].Cells[1];
                }
            }
        }

        private void MoveSelectedDown()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow dgvr = dataGridView1.SelectedRows[0];
                int index = dataGridView1.Rows.IndexOf(dgvr);
                if (index < dataGridView1.Rows.Count - 1)
                {
                    dataGridView1.Rows.RemoveAt(index);
                    dataGridView1.Rows.Insert(index + 1, dgvr);
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[index + 1].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[index + 1].Cells[1];
                }
            }
        }

        private void ChangeProgram(int padNumber)
        {
            currentPad = padNumber;
            PatchSet ps = currentPadSet[currentPad];
            SendMIDIMessage(ps.Commands[0]);
            SendMIDIMessage(ps.Commands[1]);
            SendMIDIMessage(ps.Commands[2]);
            for (int i = 0; i < 10; i++)
            {
                if (i == currentPad)
                {
                    buttonsArray[i].BackColor = Color.Yellow;
                }
                else
                {
                    buttonsArray[i].BackColor = SystemColors.ButtonFace;
                }
            }
        }

        private void RefreshBankName(int index)
        {
            if (!cbRecieve.Checked) return;
            
            PatchSet ps = currentPadSet[index];
            uint fst = ps.Commands[0] & 0xFF0000;
            uint scd = ps.Commands[1] & 0xFF0000;
            uint thd = ps.Commands[2] & 0xFF00;
            thd = thd / 0x100;

            string bankName = "";

            switch (fst)
            {
                case 0x500000:
                    if (scd == 0) bankName = "USER:" + string.Format("{0:000}", thd + 1);
                    break;

                case 0x510000:
                    switch (scd)
                    {
                        case 0x000000:
                            bankName = "A:" + string.Format("{0:000}", thd + 1);
                            break;
                        case 0x010000:
                            bankName = "B:" + string.Format("{0:000}", thd + 1);
                            break;
                        case 0x020000:
                            bankName = "C:" + string.Format("{0:000}", thd + 1);
                            break;
                        case 0x030000:
                            bankName = "GM:" + string.Format("{0:000}", thd + 1);
                            break;
                    }
                    break;

                case 0x540000:
                    switch (scd)
                    {
                        case 0x000000:
                            bankName = "Exp-A:" + string.Format("{0:000}", thd + 1);
                            break;
                        case 0x010000:
                            bankName = "Exp-A:" + string.Format("{0:000}", thd + 129);
                            break;
                        case 0x020000:
                            bankName = "Exp-B:" + string.Format("{0:000}", thd + 1);
                            break;
                        case 0x030000:
                            bankName = "Exp-B:" + string.Format("{0:000}", thd + 129);
                            break;
                        case 0x040000:
                            bankName = "Exp-C:" + string.Format("{0:000}", thd + 1);
                            break;
                        case 0x050000:
                            bankName = "Exp-C:" + string.Format("{0:000}", thd + 129);
                            break;
                        case 0x060000:
                            bankName = "Exp-D:" + string.Format("{0:000}", thd + 1);
                            break;
                        case 0x070000:
                            bankName = "Exp-D:" + string.Format("{0:000}", thd + 129);
                            break;
                    }
                    break;
            }

            buttonsArray[index].Text = bankName;
        }

        private void RefreshAllBanks()
        {
            RefreshBankName(0);
            RefreshBankName(1);
            RefreshBankName(2);
            RefreshBankName(3);
            RefreshBankName(4);
            RefreshBankName(5);
            RefreshBankName(6);
            RefreshBankName(7);
            RefreshBankName(8);
            RefreshBankName(9);
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (editableToolStripMenuItem.Checked)
            {
                int number = dataGridView1.Rows.Count;

                dataGridView1.Rows.Add(number + 1, "", "", "");
                PadSet ps = new PadSet();
                dataGridView1.Rows[number].Tag = ps;

                dataGridView1.Rows[number].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = number;
            }
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (editableToolStripMenuItem.Checked)
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    PadSet ps = new PadSet();

                    ps.Pads[0].Commands[0] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[0].Commands[0];
                    ps.Pads[0].Commands[1] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[0].Commands[1];
                    ps.Pads[0].Commands[2] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[0].Commands[2];

                    ps.Pads[1].Commands[0] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[1].Commands[0];
                    ps.Pads[1].Commands[1] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[1].Commands[1];
                    ps.Pads[1].Commands[2] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[1].Commands[2];

                    ps.Pads[2].Commands[0] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[2].Commands[0];
                    ps.Pads[2].Commands[1] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[2].Commands[1];
                    ps.Pads[2].Commands[2] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[2].Commands[2];

                    ps.Pads[3].Commands[0] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[3].Commands[0];
                    ps.Pads[3].Commands[1] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[3].Commands[1];
                    ps.Pads[3].Commands[2] = (dataGridView1.SelectedRows[0].Tag as PadSet).Pads[3].Commands[2];

                    int ind = dataGridView1.SelectedRows[0].Index + 1;

                    int number = dataGridView1.Rows.Count;

                    dataGridView1.Rows.Insert(ind, number + 1, "", "", "");
                    dataGridView1.Rows[ind].Tag = ps;
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                DeleteFromSongList();
            }
            else
            {
                DeleteFromPlayList();
            }
            
        }

        private void DeleteFromSongList()
        {
            if (editableToolStripMenuItem.Checked)
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    if (MessageBox.Show("Delete item?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        int number = (int)dataGridView1.SelectedRows[0].Cells[0].Value;
                        dataGridView1.SelectedRows[0].Tag = null;
                        dataGridView1.Rows.Remove(dataGridView1.SelectedRows[0]);
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            int newnum = (int)dataGridView1.Rows[i].Cells[0].Value;
                            if (newnum > number)
                            {
                                newnum--;
                                dataGridView1.Rows[i].Cells[0].Value = newnum;
                            }
                        }
                    }
                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you shure?", "New song list", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ClearAll();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you shure?", "Open song list", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "*.set | *.set";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    OpenAll(ofd.FileName);
                    RefreshAllBanks();
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.set | *.set";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SaveAll(sfd.FileName);
            }
        }

        private void storeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                PadSet ps = (PadSet)(dataGridView1.SelectedRows[0].Tag);

                ps.Pads[0].Commands[0] = currentPadSet[1].Commands[0];
                ps.Pads[0].Commands[1] = currentPadSet[1].Commands[1];
                ps.Pads[0].Commands[2] = currentPadSet[1].Commands[2];

                ps.Pads[1].Commands[0] = currentPadSet[2].Commands[0];
                ps.Pads[1].Commands[1] = currentPadSet[2].Commands[1];
                ps.Pads[1].Commands[2] = currentPadSet[2].Commands[2];

                ps.Pads[2].Commands[0] = currentPadSet[3].Commands[0];
                ps.Pads[2].Commands[1] = currentPadSet[3].Commands[1];
                ps.Pads[2].Commands[2] = currentPadSet[3].Commands[2];

                ps.Pads[3].Commands[0] = currentPadSet[4].Commands[0];
                ps.Pads[3].Commands[1] = currentPadSet[4].Commands[1];
                ps.Pads[3].Commands[2] = currentPadSet[4].Commands[2];
            }
        }

        string pictureFile = "";
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    lastSelectedRow = dataGridView1.SelectedRows[0];

                    if (dataGridView1.SelectedRows[0].Tag != null)
                    {
                        PadSet ps = (PadSet)(dataGridView1.SelectedRows[0].Tag);

                        currentPadSet[1].Commands[0] = ps.Pads[0].Commands[0];
                        currentPadSet[1].Commands[1] = ps.Pads[0].Commands[1];
                        currentPadSet[1].Commands[2] = ps.Pads[0].Commands[2];

                        currentPadSet[2].Commands[0] = ps.Pads[1].Commands[0];
                        currentPadSet[2].Commands[1] = ps.Pads[1].Commands[1];
                        currentPadSet[2].Commands[2] = ps.Pads[1].Commands[2];

                        currentPadSet[3].Commands[0] = ps.Pads[2].Commands[0];
                        currentPadSet[3].Commands[1] = ps.Pads[2].Commands[1];
                        currentPadSet[3].Commands[2] = ps.Pads[2].Commands[2];

                        currentPadSet[4].Commands[0] = ps.Pads[3].Commands[0];
                        currentPadSet[4].Commands[1] = ps.Pads[3].Commands[1];
                        currentPadSet[4].Commands[2] = ps.Pads[3].Commands[2];

                        ChangeProgram(currentPad);
                        RefreshAllBanks();

                        try
                        {
                            string jpgFile = Path.Combine(Path.Combine(defaultPath, "Notes"), GetCurrentSongName(lastSelectedRow) + ".jpg");
                            if (File.Exists(jpgFile))
                            {
                                pictureFile = jpgFile;
                                pictureViewer1.CurrentPicture = pictureFile;
                            }
                            else
                            {
                                pictureFile = "";
                                pictureViewer1.CurrentPicture = "";
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        Process proc;
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(pictureFile))
            {
                try
                {
                    proc = Process.Start("Photoshop.exe", pictureFile);
                }
                catch
                {
                    proc = Process.Start(pictureFile);
                }
                pictureViewer1.CurrentPicture = nullFile;
            }
        }

        private void SaveAll(string filename)
        {
            ProjFileStream pfs = new ProjFileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    pfs.WriteUInt(instrumentsBase[i, j].Commands[0]);
                    pfs.WriteUInt(instrumentsBase[i, j].Commands[1]);
                    pfs.WriteUInt(instrumentsBase[i, j].Commands[2]);
                }
            }

            for (int i = 0; i < 9; i++)
            {
                pfs.WriteInt(hkf.HotKeys[i].BankNumber);
                pfs.WriteInt(hkf.HotKeys[i].KeyNumber);
            }

            int num = dataGridView1.Rows.Count;
            pfs.WriteInt(num);

            for (int i = 0; i < num; i++)
            {
                pfs.WriteInt((int)dataGridView1.Rows[i].Cells[0].Value);
                pfs.WriteString((string)dataGridView1.Rows[i].Cells[1].Value);
                pfs.WriteString((string)dataGridView1.Rows[i].Cells[2].Value);
                pfs.WriteString((string)dataGridView1.Rows[i].Cells[3].Value);

                PadSet ps = (PadSet)(dataGridView1.Rows[i].Tag);

                pfs.WriteUInt(ps.Pads[0].Commands[0]);
                pfs.WriteUInt(ps.Pads[0].Commands[1]);
                pfs.WriteUInt(ps.Pads[0].Commands[2]);
                
                pfs.WriteUInt(ps.Pads[1].Commands[0]);
                pfs.WriteUInt(ps.Pads[1].Commands[1]);
                pfs.WriteUInt(ps.Pads[1].Commands[2]);

                pfs.WriteUInt(ps.Pads[2].Commands[0]);
                pfs.WriteUInt(ps.Pads[2].Commands[1]);
                pfs.WriteUInt(ps.Pads[2].Commands[2]);

                pfs.WriteUInt(ps.Pads[3].Commands[0]);
                pfs.WriteUInt(ps.Pads[3].Commands[1]);
                pfs.WriteUInt(ps.Pads[3].Commands[2]);
            }

            pfs.Close();
        }

        private void OpenAll(string filename)
        {
            ProjFileStream pfs = new ProjFileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    instrumentsBase[i, j].Commands[0] = pfs.ReadUInt();
                    instrumentsBase[i, j].Commands[1] = pfs.ReadUInt();
                    instrumentsBase[i, j].Commands[2] = pfs.ReadUInt();
                }
            }

            for (int i = 0; i < 9; i++)
            {
                hkf.HotKeys[i].BankNumber = pfs.ReadInt();
                hkf.HotKeys[i].KeyNumber = pfs.ReadInt();
            }

            ClearAll();

            int num = pfs.ReadInt();
            for (int i = 0; i < num; i++)
            {
                int index = pfs.ReadInt();
                string name = pfs.ReadString();
                string autor = pfs.ReadString();
                string key = pfs.ReadString();

                int number = dataGridView1.Rows.Add(index, name, autor, key);
                PadSet ps = new PadSet();

                ps.Pads[0].Commands[0] = pfs.ReadUInt();
                ps.Pads[0].Commands[1] = pfs.ReadUInt();
                ps.Pads[0].Commands[2] = pfs.ReadUInt();

                ps.Pads[1].Commands[0] = pfs.ReadUInt();
                ps.Pads[1].Commands[1] = pfs.ReadUInt();
                ps.Pads[1].Commands[2] = pfs.ReadUInt();

                ps.Pads[2].Commands[0] = pfs.ReadUInt();
                ps.Pads[2].Commands[1] = pfs.ReadUInt();
                ps.Pads[2].Commands[2] = pfs.ReadUInt();

                ps.Pads[3].Commands[0] = pfs.ReadUInt();
                ps.Pads[3].Commands[1] = pfs.ReadUInt();
                ps.Pads[3].Commands[2] = pfs.ReadUInt();

                dataGridView1.Rows[number].Tag = ps;
            }

            pfs.Close();
        }

        private void ClearAll()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Tag = null;
            }
            dataGridView1.Rows.Clear();
        }

        private void hotkeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hkf.ShowDialog();
        }

        private void editableToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (editableToolStripMenuItem.Checked)
            {
                tabControl1.SelectedIndex = 0;
                tabControl1.ItemSize = new Size(0, 1);
                dataGridView1.Columns[1].ReadOnly = false;
                dataGridView1.Columns[2].ReadOnly = false;
                dataGridView1.Columns[3].ReadOnly = false;
                this.Text = "XP-50: Edit mode";
            }
            else
            {
                tabControl1.ItemSize = new Size(96, 21);
                dataGridView1.Columns[1].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[3].ReadOnly = true;
                this.Text = "XP-50";
            }

            if (dataGridView1.SelectedRows.Count > 0)
            {
                lastSelectedRow = dataGridView1.SelectedRows[0];
            }
            else
            {
                lastSelectedRow = null;
            }
        }

        private void SelectCurrentGroup()
        {
            groupBox2.Text = groupNames[currentGroup];

            currentPadSet[0].Commands[0] = instrumentsBase[currentGroup, 0].Commands[0];
            currentPadSet[0].Commands[1] = instrumentsBase[currentGroup, 0].Commands[1];
            currentPadSet[0].Commands[2] = instrumentsBase[currentGroup, 0].Commands[2];

            currentPadSet[5].Commands[0] = instrumentsBase[currentGroup, 1].Commands[0];
            currentPadSet[5].Commands[1] = instrumentsBase[currentGroup, 1].Commands[1];
            currentPadSet[5].Commands[2] = instrumentsBase[currentGroup, 1].Commands[2];

            currentPadSet[6].Commands[0] = instrumentsBase[currentGroup, 2].Commands[0];
            currentPadSet[6].Commands[1] = instrumentsBase[currentGroup, 2].Commands[1];
            currentPadSet[6].Commands[2] = instrumentsBase[currentGroup, 2].Commands[2];

            currentPadSet[7].Commands[0] = instrumentsBase[currentGroup, 3].Commands[0];
            currentPadSet[7].Commands[1] = instrumentsBase[currentGroup, 3].Commands[1];
            currentPadSet[7].Commands[2] = instrumentsBase[currentGroup, 3].Commands[2];

            currentPadSet[8].Commands[0] = instrumentsBase[currentGroup, 4].Commands[0];
            currentPadSet[8].Commands[1] = instrumentsBase[currentGroup, 4].Commands[1];
            currentPadSet[8].Commands[2] = instrumentsBase[currentGroup, 4].Commands[2];

            currentPadSet[9].Commands[0] = instrumentsBase[currentGroup, 5].Commands[0];
            currentPadSet[9].Commands[1] = instrumentsBase[currentGroup, 5].Commands[1];
            currentPadSet[9].Commands[2] = instrumentsBase[currentGroup, 5].Commands[2];

            ChangeProgram(currentPad);
            RefreshAllBanks();
        }

        private void StoreCurrentGroup()
        {
            instrumentsBase[currentGroup, 0].Commands[0] = currentPadSet[0].Commands[0];
            instrumentsBase[currentGroup, 0].Commands[1] = currentPadSet[0].Commands[1];
            instrumentsBase[currentGroup, 0].Commands[2] = currentPadSet[0].Commands[2];

            instrumentsBase[currentGroup, 1].Commands[0] = currentPadSet[5].Commands[0];
            instrumentsBase[currentGroup, 1].Commands[1] = currentPadSet[5].Commands[1];
            instrumentsBase[currentGroup, 1].Commands[2] = currentPadSet[5].Commands[2];

            instrumentsBase[currentGroup, 2].Commands[0] = currentPadSet[6].Commands[0];
            instrumentsBase[currentGroup, 2].Commands[1] = currentPadSet[6].Commands[1];
            instrumentsBase[currentGroup, 2].Commands[2] = currentPadSet[6].Commands[2];

            instrumentsBase[currentGroup, 3].Commands[0] = currentPadSet[7].Commands[0];
            instrumentsBase[currentGroup, 3].Commands[1] = currentPadSet[7].Commands[1];
            instrumentsBase[currentGroup, 3].Commands[2] = currentPadSet[7].Commands[2];

            instrumentsBase[currentGroup, 4].Commands[0] = currentPadSet[8].Commands[0];
            instrumentsBase[currentGroup, 4].Commands[1] = currentPadSet[8].Commands[1];
            instrumentsBase[currentGroup, 4].Commands[2] = currentPadSet[8].Commands[2];

            instrumentsBase[currentGroup, 5].Commands[0] = currentPadSet[9].Commands[0];
            instrumentsBase[currentGroup, 5].Commands[1] = currentPadSet[9].Commands[1];
            instrumentsBase[currentGroup, 5].Commands[2] = currentPadSet[9].Commands[2];
        }

        private void pPianosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 0;
            SelectCurrentGroup();
        }

        private void oOrgansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 1;
            SelectCurrentGroup();
        }

        private void uPadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 2;
            SelectCurrentGroup();
        }

        private void sStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 3;
            SelectCurrentGroup();
        }

        private void bBrassesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 4;
            SelectCurrentGroup();
        }

        private void lLeadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 5;
            SelectCurrentGroup();
        }

        private void hBellsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 6;
            SelectCurrentGroup();
        }

        private void kSequensesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 7;
            SelectCurrentGroup();
        }

        private void eElPianosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 8;
            SelectCurrentGroup();
        }

        private void aAccordionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StoreCurrentGroup();
            currentGroup = 9;
            SelectCurrentGroup();
        }

        private void newNomerationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Cells[0].Value = (i + 1);
                }
            }
        }

        private void dataGridView1_MouseEnter(object sender, EventArgs e)
        {
            dataGridView1.Select();
        }

        private void attachPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "*.jpg | *.jpg";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string newPath = Path.Combine(Path.Combine(defaultPath, "Notes"), GetCurrentSongName(lastSelectedRow) + ".jpg");
                    if (File.Exists(newPath)) File.Delete(newPath);
                    File.Move(ofd.FileName, newPath);
                    pictureViewer1.CurrentPicture = newPath;
                }
            }
        }

        private void fromCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функция на стадии разработки...");
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string jpgFile = Path.Combine(Path.Combine(defaultPath, "Notes"), GetCurrentSongName(lastSelectedRow) + ".jpg");
                if (File.Exists(jpgFile))
                {
                    pictureViewer1.CurrentPicture = "";
                    File.Delete(jpgFile);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string GetCurrentSongName(DataGridViewRow row)
        {
            string part1 = row.Cells[1].Value as string;
            string part2 = row.Cells[2].Value as string;
            string newName = part2 + " - " + part1;

            return newName;
        }

        //-----------------------------  Search  ---------------------------------
        private SearchForm sForm = null;
        private bool sFormExist = false;
        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!sFormExist)
            {
                sFormExist = true;
                sForm = new SearchForm();
                sForm.FormClosed += sForm_FormClosed;
                sForm.TextChangedEvent += sForm_TextChangedEvent;
                sForm.SelectedItemChangedEvent += sForm_SelectedItemChangedEvent;
                sForm.Show(this);
            }
        }

        private void sForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            sForm.SelectedItemChangedEvent -= sForm_SelectedItemChangedEvent;
            sForm.TextChangedEvent -= sForm_TextChangedEvent;
            sForm.FormClosed -= sForm_FormClosed;
            sForm = null;
            sFormExist = false;
        }

        private void sForm_TextChangedEvent(object sender, SearchForm.TextChangedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                FingARow(dataGridView1, e.text);
            }
            else
            {
                FingARow(dataGridView2, e.text);
            }
        }

        void sForm_SelectedItemChangedEvent(object sender, SearchForm.SelectedItemChangedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                SelectRow(dataGridView1, e.row.Index);
            }
            else
            {
                SelectRow(dataGridView2, e.row.Index);
            }
        }

        List<DataGridViewRow> result = new List<DataGridViewRow>();
        private void FingARow(DataGridView dataGridView, string text)
        {
            int cnt = dataGridView.Rows.Count;
            result.Clear();
            int first = -1;

            for (int i = 0; i < cnt; i++)
            {
                string name = ((string)dataGridView.Rows[i].Cells[1].Value).ToLower();
                string author = ((string)dataGridView.Rows[i].Cells[2].Value).ToLower();
                text = text.ToLower();

                if ((name).Contains(text)
                    || (author).Contains(text))
                {
                    if (first < 0) first = i;
                    //dataGridView.Rows[i].Selected = true;
                    //dataGridView.FirstDisplayedScrollingRowIndex = i;
                    //break;

                    result.Add(dataGridView.Rows[i]);
                    
                }
            }

            if (first >= 0)
            {
                SelectRow(dataGridView, first);
                sForm.SetResult(result);
            }
        }

        private void SelectRow(DataGridView dataGridView, int ind)
        {
            dataGridView.Rows[ind].Selected = true;
            dataGridView.FirstDisplayedScrollingRowIndex = ind;
        }

        // ------------------------- Drag & Drop ---------------------------------
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;

        private void dataGridView1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty &&
                    !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {

                    // Proceed with the drag and drop, passing in the list item.                    
                    DragDropEffects dropEffect = dataGridView1.DoDragDrop(
                    dataGridView1.Rows[rowIndexFromMouseDown],
                    DragDropEffects.Move);
                }
            }
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            // Get the index of the item the mouse is below.
            rowIndexFromMouseDown = dataGridView1.HitTest(e.X, e.Y).RowIndex;
            if (rowIndexFromMouseDown != -1)
            {
                // Remember the point where the mouse down occurred. 
                // The DragSize indicates the size that the mouse can move 
                // before a drag event should be started.                
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                               e.Y - (dragSize.Height / 2)),
                                    dragSize);
            }
            else
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void dataGridView1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            // The mouse locations are relative to the screen, so they must be 
            // converted to client coordinates.
            Point clientPoint = dataGridView1.PointToClient(new Point(e.X, e.Y));

            // Get the row index of the item the mouse is below. 
            rowIndexOfItemUnderMouseToDrop =
                dataGridView1.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // If the drag operation was a move then remove and insert the row.
            if (e.Effect == DragDropEffects.Move)
            {
                DataGridViewRow rowToMove = e.Data.GetData(
                    typeof(DataGridViewRow)) as DataGridViewRow;
                dataGridView1.Rows.RemoveAt(rowIndexFromMouseDown);
                dataGridView1.Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);
                dataGridView1.Rows[rowIndexOfItemUnderMouseToDrop].Selected = true;
            }
        }
        //-------------------------------------------------------------------------

        //**************************************************************************
        //----------------------- Playlist Section --------------------------------
        private int playListPointer = 0;
        private void refreshSelectedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                if (dataGridView2.SelectedRows.Count > 0)
                {
                    int sel = dataGridView2.SelectedRows[0].Index;
                    int sn = GetRowIndex(GetCurrentSongName(dataGridView2.SelectedRows[0]));

                    if (sn >= 0)
                    {
                        RefreshItemBanks(sn, sel);
                    }
                }
            }
        }

        private void refreshAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                int sn = GetRowIndex(GetCurrentSongName(dataGridView2.Rows[i]));

                if (sn >= 0)
                {
                    RefreshItemBanks(sn, i);
                }
            }
        }

        private int GetRowIndex(string songName)
        {
            int res = -1;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (GetCurrentSongName(dataGridView1.Rows[i])  == songName)
                {
                    res = i;
                    break;
                }
            }
            return res;
        }

        private void RefreshItemBanks(int mainListIndex, int playListIndex)
        {
            if (dataGridView1.Rows[mainListIndex].Tag != null)
            {
                PadSet psMain = (PadSet)(dataGridView1.Rows[mainListIndex].Tag);
                if (dataGridView1.Rows[playListIndex].Tag != null)
                {
                    PadSet psPL = (PadSet)(dataGridView2.Rows[playListIndex].Tag);

                    psPL.Pads[0].Commands[0] = psMain.Pads[0].Commands[0];
                    psPL.Pads[0].Commands[1] = psMain.Pads[0].Commands[1];
                    psPL.Pads[0].Commands[2] = psMain.Pads[0].Commands[2];

                    psPL.Pads[1].Commands[0] = psMain.Pads[1].Commands[0];
                    psPL.Pads[1].Commands[1] = psMain.Pads[1].Commands[1];
                    psPL.Pads[1].Commands[2] = psMain.Pads[1].Commands[2];

                    psPL.Pads[2].Commands[0] = psMain.Pads[2].Commands[0];
                    psPL.Pads[2].Commands[1] = psMain.Pads[2].Commands[1];
                    psPL.Pads[2].Commands[2] = psMain.Pads[2].Commands[2];

                    psPL.Pads[3].Commands[0] = psMain.Pads[3].Commands[0];
                    psPL.Pads[3].Commands[1] = psMain.Pads[3].Commands[1];
                    psPL.Pads[3].Commands[2] = psMain.Pads[3].Commands[2];

                    dataGridView2_SelectionChanged(this, new EventArgs());

                    ChangeProgram(currentPad);
                    RefreshAllBanks();
                }
                //else
                //{

                //}
            }
        }

        private void addToListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView1.SelectedRows[0];
                int ind = dataGridView2.Rows.Add(row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value);
                PadSet ps = new PadSet();
                ps.Pads[0] = ((PadSet)row.Tag).Pads[0];
                ps.Pads[1] = ((PadSet)row.Tag).Pads[1];
                ps.Pads[2] = ((PadSet)row.Tag).Pads[2];
                ps.Pads[3] = ((PadSet)row.Tag).Pads[3];
                dataGridView2.Rows[ind].Tag = ps;
            }
        }

        private void DeleteFromPlayList()
        {
            if (tabControl1.SelectedIndex == 1)
            {
                if (dataGridView2.SelectedRows.Count > 0)
                {
                    if (MessageBox.Show("Delete item?", "Delete", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        dataGridView2.Rows.Remove(dataGridView2.SelectedRows[0]);
                    }
                }
            }
        }

        private void clearListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView2.Rows.Clear();
            tabControl1_SelectedIndexChanged(sender, e);
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                if (dataGridView2.SelectedRows.Count > 0)
                {
                    lastSelectedRow = dataGridView2.SelectedRows[0];

                    if (dataGridView2.SelectedRows[0].Tag != null)
                    {
                        PadSet ps = (PadSet)(dataGridView2.SelectedRows[0].Tag);

                        currentPadSet[1].Commands[0] = ps.Pads[0].Commands[0];
                        currentPadSet[1].Commands[1] = ps.Pads[0].Commands[1];
                        currentPadSet[1].Commands[2] = ps.Pads[0].Commands[2];

                        currentPadSet[2].Commands[0] = ps.Pads[1].Commands[0];
                        currentPadSet[2].Commands[1] = ps.Pads[1].Commands[1];
                        currentPadSet[2].Commands[2] = ps.Pads[1].Commands[2];

                        currentPadSet[3].Commands[0] = ps.Pads[2].Commands[0];
                        currentPadSet[3].Commands[1] = ps.Pads[2].Commands[1];
                        currentPadSet[3].Commands[2] = ps.Pads[2].Commands[2];

                        currentPadSet[4].Commands[0] = ps.Pads[3].Commands[0];
                        currentPadSet[4].Commands[1] = ps.Pads[3].Commands[1];
                        currentPadSet[4].Commands[2] = ps.Pads[3].Commands[2];

                        ChangeProgram(currentPad);
                        RefreshAllBanks();

                        string jpgFile = Path.Combine(Path.Combine(defaultPath, "Notes"), GetCurrentSongName(lastSelectedRow) + ".jpg");
                        if (File.Exists(jpgFile))
                        {
                            pictureViewer1.CurrentPicture = jpgFile;
                        }
                        else
                        {
                            pictureViewer1.CurrentPicture = "";
                        }
                    }
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                dataGridView1_SelectionChanged(sender, e);
            }
            else
            {
                dataGridView2_SelectionChanged(sender, e);
            }
        }

        private void newNumerationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    dataGridView2.Rows[i].Cells[0].Value = (i + 1);
                }
            }
        }

        // ------------------------- Drag$Drop Playlist ------------------------------
        private void dataGridView2_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty &&
                    !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {

                    // Proceed with the drag and drop, passing in the list item.                    
                    DragDropEffects dropEffect = dataGridView2.DoDragDrop(
                    dataGridView2.Rows[rowIndexFromMouseDown],
                    DragDropEffects.Move);
                }
            }
        }

        private void dataGridView2_MouseDown(object sender, MouseEventArgs e)
        {
            // Get the index of the item the mouse is below.
            rowIndexFromMouseDown = dataGridView2.HitTest(e.X, e.Y).RowIndex;
            if (rowIndexFromMouseDown != -1)
            {
                // Remember the point where the mouse down occurred. 
                // The DragSize indicates the size that the mouse can move 
                // before a drag event should be started.                
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                               e.Y - (dragSize.Height / 2)),
                                    dragSize);
            }
            else
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void dataGridView2_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dataGridView2_DragDrop(object sender, DragEventArgs e)
        {
            // The mouse locations are relative to the screen, so they must be 
            // converted to client coordinates.
            Point clientPoint = dataGridView2.PointToClient(new Point(e.X, e.Y));

            // Get the row index of the item the mouse is below. 
            rowIndexOfItemUnderMouseToDrop =
                dataGridView2.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // If the drag operation was a move then remove and insert the row.
            if (e.Effect == DragDropEffects.Move)
            {
                DataGridViewRow rowToMove = e.Data.GetData(
                    typeof(DataGridViewRow)) as DataGridViewRow;
                dataGridView2.Rows.RemoveAt(rowIndexFromMouseDown);
                dataGridView2.Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);
                dataGridView2.Rows[rowIndexOfItemUnderMouseToDrop].Selected = true;
            }
        }

        private void openPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.lst | *.lst";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                OpenPlayList(ofd.FileName);
            }
        }

        private void savePlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.lst | *.lst";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SavePlayList(sfd.FileName);
            }
        }

        private void SavePlayList(string filename)
        {
            ProjFileStream pfs = new ProjFileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            int num = dataGridView2.Rows.Count;
            pfs.WriteInt(num);

            for (int i = 0; i < num; i++)
            {
                pfs.WriteInt((int)dataGridView2.Rows[i].Cells[0].Value);
                pfs.WriteString((string)dataGridView2.Rows[i].Cells[1].Value);
                pfs.WriteString((string)dataGridView2.Rows[i].Cells[2].Value);
                pfs.WriteString((string)dataGridView2.Rows[i].Cells[3].Value);

                PadSet ps = (PadSet)(dataGridView2.Rows[i].Tag);

                pfs.WriteUInt(ps.Pads[0].Commands[0]);
                pfs.WriteUInt(ps.Pads[0].Commands[1]);
                pfs.WriteUInt(ps.Pads[0].Commands[2]);

                pfs.WriteUInt(ps.Pads[1].Commands[0]);
                pfs.WriteUInt(ps.Pads[1].Commands[1]);
                pfs.WriteUInt(ps.Pads[1].Commands[2]);

                pfs.WriteUInt(ps.Pads[2].Commands[0]);
                pfs.WriteUInt(ps.Pads[2].Commands[1]);
                pfs.WriteUInt(ps.Pads[2].Commands[2]);

                pfs.WriteUInt(ps.Pads[3].Commands[0]);
                pfs.WriteUInt(ps.Pads[3].Commands[1]);
                pfs.WriteUInt(ps.Pads[3].Commands[2]);
            }

            pfs.Close();
        }

        private void OpenPlayList(string filename)
        {
            ProjFileStream pfs = new ProjFileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            dataGridView2.Rows.Clear();

            int num = pfs.ReadInt();
            for (int i = 0; i < num; i++)
            {
                int index = pfs.ReadInt();
                string name = pfs.ReadString();
                string autor = pfs.ReadString();
                string key = pfs.ReadString();

                int number = dataGridView2.Rows.Add(index, name, autor, key);
                PadSet ps = new PadSet();

                ps.Pads[0].Commands[0] = pfs.ReadUInt();
                ps.Pads[0].Commands[1] = pfs.ReadUInt();
                ps.Pads[0].Commands[2] = pfs.ReadUInt();

                ps.Pads[1].Commands[0] = pfs.ReadUInt();
                ps.Pads[1].Commands[1] = pfs.ReadUInt();
                ps.Pads[1].Commands[2] = pfs.ReadUInt();

                ps.Pads[2].Commands[0] = pfs.ReadUInt();
                ps.Pads[2].Commands[1] = pfs.ReadUInt();
                ps.Pads[2].Commands[2] = pfs.ReadUInt();

                ps.Pads[3].Commands[0] = pfs.ReadUInt();
                ps.Pads[3].Commands[1] = pfs.ReadUInt();
                ps.Pads[3].Commands[2] = pfs.ReadUInt();

                dataGridView2.Rows[number].Tag = ps;
            }

            pfs.Close();
        }

        private void setCurrentSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                if (dataGridView2.SelectedRows.Count > 0)
                {
                    playListPointer = dataGridView2.SelectedRows[0].Index;
                    currentToolStripMenuItem.Text = "Current = " + playListPointer.ToString();
                }
            }
        }

        private void upSelectedToCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                if (dataGridView2.SelectedRows.Count > 0)
                {
                    int ind = dataGridView2.SelectedRows[0].Index;
                    if (ind > playListPointer)
                    {
                        DataGridViewRow rowToMove = dataGridView2.SelectedRows[0];
                        dataGridView2.Rows.RemoveAt(ind);
                        dataGridView2.Rows.Insert(playListPointer, rowToMove);
                        playListPointer++;
                        currentToolStripMenuItem.Text = "Current = " + playListPointer.ToString();

                        dataGridView2.Rows[playListPointer].Selected = true;
                    }
                }
            }
        }

        //***************************  Статистика  *****************************
        private void ShowStatistics()
        {
            int[] arr = new int[128];
            for (int i = 0; i < 128; i++)
            {
                arr[i] = 0;
            }

            int cnt = dataGridView1.Rows.Count;

            if (cnt > 0)
            {
                for (int i = 0; i < cnt; i++)
                {
                    if (dataGridView1.Rows[i].Tag != null)
                    {
                        PadSet pd = (PadSet)(dataGridView1.Rows[i].Tag);
                        for (int j = 0; j < 4; j++)
                        {
                            PatchSet ps = pd.Pads[j];
                            uint fst = ps.Commands[0] & 0xFF0000;
                            uint scd = ps.Commands[1] & 0xFF0000;
                            uint thd = ps.Commands[2] & 0xFF00;
                            thd = thd / 0x100;

                            if (fst == 0x500000)
                            {
                                arr[thd]++;
                            }
                        }
                    }
                }

                //--------------------------------------
                //          Вывод статистики            /
                //--------------------------------------

                StatisticForm sf = new StatisticForm();

                for (int i = 0; i < 128; i++)
                {
                    sf.DisplayedList.Items.Add("USER " + string.Format("{0:000}", i + 1) + " : " + arr[i].ToString());
                }

                sf.ShowDialog();
            }
        }

        private void statisticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowStatistics();
        }

        private void exportToTXTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count > 0)
            {
                string txt = "";
                foreach(DataGridViewRow row in dataGridView2.Rows)
                {
                    string str = row.Cells[1].Value + "-" + row.Cells[2].Value + "\r\n";
                    txt += str;
                }
                TrackListForm tlf = new TrackListForm();
                tlf.Text = txt;
                tlf.ShowDialog();
            }
        }

    }
}