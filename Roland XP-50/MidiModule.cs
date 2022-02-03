using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MIDI
{
    //*********************  Описание делегата  **************************************
    public delegate void MidiReciever(
        IntPtr hMidiIn,
        uint wMsg,
        uint dwInstance,
        uint dwParam1,
        uint dwParam2
    );

    public class MidiEventArgs : EventArgs
    {
        public uint dwParam1;
        public uint dwParam2;

        public MidiEventArgs()
        {

        }

        public MidiEventArgs(uint b1, uint b2)
        {
            dwParam1 = b1;
            dwParam2 = b2;
        }
    }

    public class MidiModule
    {
        //**************' MIDI input device capabilities structure  ******************
        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIINCAPS
        {
            public ushort       wMid;
            public ushort       wPid;
            public uint         vDriverVersion;     // MMVERSION
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string       szPname;
            public uint         dwSupport;
        }

        //**************' MIDI output device capabilities structure  *****************
        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIOUTCAPS
        { 
            public ushort       wMid; 
            public ushort       wPid; 
            public uint         vDriverVersion; 
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string       szPname;
            public ushort       wTechnology; 
            public ushort       wVoices; 
            public ushort       wNotes; 
            public ushort       wChannelMask; 
            public uint         dwSupport; 
        }

        //*************************  Импорт функций  *********************************
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiInGetNumDevs();
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiInGetDevCaps(uint uDeviceID, out MIDIINCAPS caps, 
            uint cbMidiInCaps);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll")]
        private static extern uint midiInOpen(ref IntPtr lphMidin, uint uDeviceID, MidiReciever dwCallback, 
            IntPtr dwInstance, uint dwFlags);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiInClose(IntPtr hMidiIn);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiInGetErrorText(uint err_Renamed, string lpText, uint uSize);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiInStart(IntPtr hMidiIn);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiInStop(IntPtr hMidiIn);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiInReset(IntPtr hMidiIn);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiOutGetNumDevs();
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiOutGetDevCaps(uint uDeviceID, out MIDIOUTCAPS caps,
            uint cbMidiOutCaps);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll")]
        private static extern uint midiOutOpen(ref IntPtr lphMidout, uint uOutDeviceID, IntPtr dwCallback,
            IntPtr dwInstance, uint dwFlags);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiOutClose(IntPtr hMidiIn);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiOutGetErrorText(uint err_Renamed, string lpText, uint uSize);
        //----------------------------------------------------------------------------
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);
        //----------------------------------------------------------------------------

        //*******************************  Delegate  *********************************
        public event EventHandler MidiEventRecieve = null;

        //******************************  Обработчик событий  ************************
        public void MidiEvent(
            IntPtr hMidiIn,
            uint wMsg,
            uint dwInstance,
            uint dwParam1,
            uint dwParam2)
        {
            MidiEventArgs mea = new MidiEventArgs(dwParam1, dwParam2);

            if (MidiEventRecieve != null) MidiEventRecieve(this, mea);
        }

        //****************************************************************************

        //*************************  Input functions  ********************************
        private uint MIDIInGetNumDevs()
        {
            return midiInGetNumDevs();
        }

        private uint MIDIInGetDevCaps(uint uDeviceID, out MIDIINCAPS lpCaps, uint uSize)
        {
            return midiInGetDevCaps(uDeviceID, out lpCaps, uSize);
        }

        private uint MIDIInOpen(ref IntPtr lphMidiIn, uint uInDeviceID, MidiReciever dwCallback,
            IntPtr dwInstance, uint dwFlags)
        {
            return midiInOpen(ref lphMidiIn, uInDeviceID, dwCallback, dwInstance, dwFlags);
        }

        private uint MIDIInClose(IntPtr hMidiIn)
        {
            return midiInClose(hMidiIn);
        }

        private uint MIDIInGetErrorText(uint err_Renamed, string lpText, uint uSize)
        {
            return midiInGetErrorText(err_Renamed, lpText, uSize);
        }

        private uint MIDIInStart(IntPtr hMidiIn)
        {
            return midiInStart(hMidiIn);
        }

        private uint MIDIInStop(IntPtr hMidiIn)
        {
            return midiInStop(hMidiIn);
        }

        private uint MIDIInReset(IntPtr hMidiIn)
        {
            return midiInReset(hMidiIn);
        }
        //*****************************************************************************

        //*************************  Output functions  ********************************
        public uint MIDIOutGetNumDevs()
        {
            return midiOutGetNumDevs();
        }

        private uint MIDIOutGetDevCaps(uint uOutDeviceID, out MIDIOUTCAPS lpCaps, uint uSize)
        {
            return midiOutGetDevCaps(uOutDeviceID, out lpCaps, uSize);
        }

        private uint MIDIOutOpen(ref IntPtr lphMidiOut, uint uDeviceID, IntPtr dwCallback,
            IntPtr dwInstance, uint dwFlags)
        {
            return midiOutOpen(ref lphMidiOut, uDeviceID, dwCallback, dwInstance, dwFlags);
        }

        private uint MIDIOutClose(IntPtr hMidiOut)
        {
            return midiOutClose(hMidiOut);
        }

        private uint MIDIOutGetErrorText(uint err_Renamed, string lpText, uint uSize)
        {
            return midiOutGetErrorText(err_Renamed, lpText, uSize);
        }

        private uint MIDIOutSendMessage(IntPtr hMidiOut, uint dwMsg)
        {
            return midiOutShortMsg(hMidiOut, dwMsg);
        }

        //******************************************************************************

        //******************************************************************************
        //*********************************  MIDI IN  **********************************
        //******************************************************************************

        //**********************************  Globals  *********************************
        private uint NumInDev;
        public MIDIINCAPS midiInCaps;
        private uint uInDeviceID;
        private IntPtr hMidiIn;
        MidiReciever callbackreciever;

        //*********************************  Инициализация Midi-In  ********************
        public int InInit()
        {
            callbackreciever = new MidiReciever(MidiEvent);

            // Количество девайсов
            NumInDev = MIDIInGetNumDevs();
            if (NumInDev == 0)
            {
                MessageBox.Show("Midi-in device not found!");
                return -1;
            }

            return (int)NumInDev;
        }

        //*****************************  Информация об устройстве  *********************
        public int InGetCaps(uint dev)
        {
            uInDeviceID = dev;      // Можно сделать выбор!
            midiInCaps = new MIDIINCAPS();
            uint res = MIDIInGetDevCaps(uInDeviceID, out midiInCaps, (uint)Marshal.SizeOf(midiInCaps));
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIInGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }
            return 0;
        }

        //*********************************  Открытие MIDI-In  *************************
        public int InOpen()
        {
            uint res = MIDIInOpen(ref hMidiIn, uInDeviceID, callbackreciever, IntPtr.Zero, CALLBACK_FUNCTION);
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIInGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }
            return 0;
        }

        //*********************************  Закрытие MIDI-In  *************************
        public int InClose()
        {
            uint res = MIDIInClose(hMidiIn);
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIInGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }

            return 0;
        }

        //*********************************  Сброс MIDI-In  ****************************
        public int InReset()
        {
            uint res = MIDIInReset(hMidiIn);
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIInGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }

            return 0;
        }

        //*********************************  Старт MIDI-In  ****************************
        public int InStart()
        {
            uint res = MIDIInStart(hMidiIn);
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIInGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }

            return 0;
        }

        //*********************************  Стоп MIDI-In  *****************************
        public int InStop()
        {
            uint res = MIDIInStop(hMidiIn);
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIInGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }

            return 0;
        }

        //******************************************************************************
        //*********************************  MIDI OUT  *********************************
        //******************************************************************************

        //**********************************  Globals  *********************************
        private uint NumOutDev;
        public MIDIOUTCAPS midiOutCaps;
        private uint uOutDeviceID;
        private IntPtr hMidiOut;

        //*********************************  Инициализация Midi-Out  *******************
        public int OutInit()
        {
            //callbackreciever = new MidiReciever(MidiEvent);

            // Количество девайсов
            NumOutDev = MIDIOutGetNumDevs();
            if (NumOutDev == 0)
            {
                MessageBox.Show("Midi-out device not found!");
                return -1;
            }

            return (int)NumOutDev;
        }

        //****************************  Информация об устройстве  **********************
        public int OutGetCaps(uint dev)
        {
            // Информация об устройстве
            uOutDeviceID = dev;      // Можно сделать выбор!
            midiOutCaps = new MIDIOUTCAPS();
            uint res = MIDIOutGetDevCaps(uOutDeviceID, out midiOutCaps, (uint)Marshal.SizeOf(midiOutCaps));
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIOutGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }
            return 0;
        }

        //*********************************  Открытие MIDI-Out  ************************
        public int OutOpen()
        {
            uint res = MIDIOutOpen(ref hMidiOut, uOutDeviceID, IntPtr.Zero, IntPtr.Zero, CALLBACK_NULL);
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIOutGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }
            return 0;
        }

        //*********************************  Закрытие MIDI-Out  ************************
        public int OutClose()
        {
            uint res = MIDIOutClose(hMidiOut);
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIOutGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }

            return 0;
        }

        //********************************* Послать сообщение ***************************
        public int OutSendMessage(uint msg)
        {
            uint res = MIDIOutSendMessage(hMidiOut, msg);
            if (res != MMSYSERR_NOERROR)
            {
                string str = "";
                MIDIOutGetErrorText(res, str, 255);
                MessageBox.Show("Error = " + str);
                return -1;
            }
            return 0;
        }


        //******************************************************************************
        //************************' Callback Function constants  ***********************
        //******************************************************************************
        public const uint CALLBACK_NULL = 0;                    //?????????????????????????
        public const uint CALLBACK_FUNCTION = 0x30000;          //' dwCallback is a FARPROC
        public const short MIM_OPEN = 0x3C1;                   //' MIDI In Port Opened
        public const short MIM_CLOSE = 0x3C2;                  //' MIDI In Port Closed
        public const short MIM_DATA = 0x3C3;                   //' MIDI In Short Data (e.g. Notes & CC)
        public const short MIM_LONGDATA = 0x3C4;               //' MIDI In Long Data (i.e. SYSEX)
        public const short MIM_ERROR = 0x3C5;                  //' MIDI In Error
        public const short MIM_LONGERROR = 0x3C6;              //' MIDI In Long Error
        public const short MIM_MOREDATA = 0x3CC;               //' MIDI Header Buffer is Full
        public const short MOM_OPEN = 0x3C7;                   //' MIDI Out Port Opened
        public const short MOM_CLOSE = 0x3C8;                  //' MIDI Out Port Closed
        public const short MOM_DONE = 0x3C9;                   //' MIDI Out Data sending completed
        public const short MOM_POSITIONCB = 0xCA;              //' MIDI Out Position requested

        //*************************' Midi Error Constants  *****************************
        public const short MMSYSERR_NOERROR = 0;
        public const short MMSYSERR_ERROR = 1;
        public const short MMSYSERR_BADDEVICEID = 2;
        public const short MMSYSERR_NOTENABLED = 3;
        public const short MMSYSERR_ALLOCATED = 4;
        public const short MMSYSERR_INVALHANDLE = 5;
        public const short MMSYSERR_NODRIVER = 6;
        public const short MMSYSERR_NOMEM = 7;
        public const short MMSYSERR_NOTSUPPORTED = 8;
        public const short MMSYSERR_BADERRNUM = 9;
        public const short MMSYSERR_INVALFLAG = 10;
        public const short MMSYSERR_INVALPARAM = 11;
        public const short MMSYSERR_HANDLEBUSY = 12;
        public const short MMSYSERR_INVALIDALIAS = 13;
        public const short MMSYSERR_BADDB = 14;
        public const short MMSYSERR_KEYNOTFOUND = 15;
        public const short MMSYSERR_READERROR = 16;
        public const short MMSYSERR_WRITEERROR = 17;
        public const short MMSYSERR_DELETEERROR = 18;
        public const short MMSYSERR_VALNOTFOUND = 19;
        public const short MMSYSERR_NODRIVERCB = 20;
        public const short MMSYSERR_LASTERROR = 20;
        public const short MIDIERR_UNPREPARED = 64; //' header not prepared
        public const short MIDIERR_STILLPLAYING = 65; //' still something playing
        public const short MIDIERR_NOMAP = 66; //' no current map
        public const short MIDIERR_NOTREADY = 67; //' hardware is still busy
        public const short MIDIERR_NODEVICE = 68; //' port no longer connected
        public const short MIDIERR_INVALIDSETUP = 69; //' invalid setup
        public const short MIDIERR_LASTERROR = 69; //' last error in range

        //*********************' Midi Header flags  ************************************
        public const short MHDR_DONE = 1; //' Set by the device driver to indicate that it is finished with the buffer and is returning it to the application.
        public const short MHDR_PREPARED = 2; //' Set by Windows to indicate that the buffer has been prepared
        public const short MHDR_INQUEUE = 4; //' Set by Windows to indicate that the buffer is queued for playback
        public const short MHDR_ISSTRM = 8; //' Set to indicate that the buffer is a stream buffer
    }
}
