using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MIDI;

namespace Roland_XP_50
{
    public partial class MIDIConnections : Form
    {
        private MidiModule mm;

        public MIDIConnections(MidiModule mod)
        {
            InitializeComponent();
            mm = mod;
            OpenMIDI();
        }

        public void OpenMIDI()
        {
            int numInDevs = mm.InInit();
            int numOutDevs = mm.OutInit();

            lst_Input.Items.Clear();
            lst_Output.Items.Clear();

            for (uint i = 0; i < numInDevs; i++)
            {
                mm.InGetCaps(i);
                lst_Input.Items.Add(mm.midiInCaps.szPname);
            }

            for (uint i = 0; i < numOutDevs; i++)
            {
                mm.OutGetCaps(i);
                lst_Output.Items.Add(mm.midiOutCaps.szPname);
            }

            if (lst_Input.Items.Count > 0)
            {
                lst_Input.SelectedIndex = 0;
            }

            if (lst_Output.Items.Count > 0)
            {
                lst_Output.SelectedIndex = 0;
            }
        }

        public void CloseMIDI()
        {
            mm.InClose();
            mm.OutClose();
            mm = null;
        }

        private void bt_Ok_Click(object sender, EventArgs e)
        {
            int inDev = lst_Input.SelectedIndex;
            int outDev = lst_Output.SelectedIndex;
            if (inDev >= 0 && outDev >= 0)
            {
                mm.InGetCaps((uint)inDev);
                mm.InOpen();
                mm.OutGetCaps((uint)outDev);
                mm.OutOpen();
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}