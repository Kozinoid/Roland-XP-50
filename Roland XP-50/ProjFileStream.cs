using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PIC18F4550Controller
{
    public class ProjFileStream
    {
        private FileStream fs;

        //**************************  Конструктор  ******************************
        public ProjFileStream(string FileName, FileMode mode, FileAccess access)
        {
            fs = new FileStream(FileName, mode, access);
        }

        //*************************  Закрыть  ***********************************
        public void Close()
        {
            fs.Close();
        }

        //************************  Сохранить байт  *****************************
        public void WriteByte(byte bt)
        {
            fs.WriteByte(bt);
        }

        //************************  Читать байт  ********************************
        public byte ReadByte()
        {
            return (byte)fs.ReadByte();
        }

        //************************  Сохранить текст  ****************************
        public void WriteText(string str)
        {
            int length = str.Length;
            byte[] buf = new byte[length];
            buf = Encoding.Default.GetBytes(str);

            fs.Write(buf, 0, length);
        }

        //*************************  Сохранить строку  **************************
        public void WriteString(string str)
        {
            int length = str.Length;
            byte[] buf = new byte[length];
            buf = Encoding.Default.GetBytes(str);
            fs.WriteByte((byte)length);
            fs.Write(buf, 0, length);
        }

        //************************  Прочесть строку  ****************************
        public string ReadString()
        {
            string str = "";

            int length = fs.ReadByte();
            byte[] buf = new byte[length];
            fs.Read(buf, 0, length);
            str = Encoding.Default.GetString(buf);

            return str;
        }

        //-----------------------------------------------------------------------
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
        struct TestUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public int i;

            [System.Runtime.InteropServices.FieldOffset(0)]
            public byte b1;

            [System.Runtime.InteropServices.FieldOffset(1)]
            public byte b2;

            [System.Runtime.InteropServices.FieldOffset(2)]
            public byte b3;

            [System.Runtime.InteropServices.FieldOffset(3)]
            public byte b4;
        }

        //-----------------------------------------------------------------------
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
        struct UINTUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public uint i;

            [System.Runtime.InteropServices.FieldOffset(0)]
            public byte b1;

            [System.Runtime.InteropServices.FieldOffset(1)]
            public byte b2;

            [System.Runtime.InteropServices.FieldOffset(2)]
            public byte b3;

            [System.Runtime.InteropServices.FieldOffset(3)]
            public byte b4;
        }

        //************************  Сохранить int  ******************************
        public void WriteInt(int num)
        {
            TestUnion un = new TestUnion();
            un.i = num;
            fs.WriteByte(un.b1);
            fs.WriteByte(un.b2);
            fs.WriteByte(un.b3);
            fs.WriteByte(un.b4);
        }

        //************************  Прочесть int  *******************************
        public int ReadInt()
        {
            TestUnion un = new TestUnion();
            un.b1 = (byte)fs.ReadByte();
            un.b2 = (byte)fs.ReadByte();
            un.b3 = (byte)fs.ReadByte();
            un.b4 = (byte)fs.ReadByte();
            return un.i;
        }

        //************************  Сохранить uint  ******************************
        public void WriteUInt(uint num)
        {
            UINTUnion un = new UINTUnion();
            un.i = num;
            fs.WriteByte(un.b1);
            fs.WriteByte(un.b2);
            fs.WriteByte(un.b3);
            fs.WriteByte(un.b4);
        }

        //************************  Прочесть uint  *******************************
        public uint ReadUInt()
        {
            UINTUnion un = new UINTUnion();
            un.b1 = (byte)fs.ReadByte();
            un.b2 = (byte)fs.ReadByte();
            un.b3 = (byte)fs.ReadByte();
            un.b4 = (byte)fs.ReadByte();
            return un.i;
        }

        //***********************  Сохранить chunk  *****************************
        public void WriteChunk(string chunk)
        {
            string str;
            if (chunk.Length > 4)
            {
                str = chunk.Substring(0, 4);
            }
            else if (chunk.Length < 4)
            {
                str = chunk.PadRight(4, ' ');
            }
            else str = chunk;

            int length = str.Length;
            byte[] buf = new byte[length];
            buf = Encoding.Default.GetBytes(str);
            fs.Write(buf, 0, length);
        }

        //***********************  Прочесть chunk  ******************************
        public string ReadChunk()
        {
            string chunk = "";

            int length = 4;
            byte[] buf = new byte[length];
            fs.Read(buf, 0, length);
            chunk = Encoding.Default.GetString(buf);

            return chunk;
        }

        //**********************  Сохранить bool  *******************************
        public void WriteBool(bool fl)
        {
            if (fl) fs.WriteByte(1);
            else fs.WriteByte(0);
        }

        //***********************  Прочитать bool  ******************************
        public bool ReadBool()
        {
            bool res = false;
            if (fs.ReadByte() != 0) res = true;
            return res;
        }

        //*************************  Сохранить текст  ***************************
        public void WriteLirics(string str)
        {
            int length = str.Length;
            byte[] buf = new byte[length];
            buf = Encoding.Default.GetBytes(str);
            WriteInt(length);
            fs.Write(buf, 0, length);
        }

        //************************  Прочесть текст  *****************************
        public string ReadLirics()
        {
            string str = "";

            int length = ReadInt();
            byte[] buf = new byte[length];
            fs.Read(buf, 0, length);
            str = Encoding.Default.GetString(buf);

            return str;
        }

        //************************  Сохранить цвет  *******************************
        public void WriteColor(Color color)
        {
            WriteInt((int)color.R);
            WriteInt((int)color.G);
            WriteInt((int)color.B);
        }

        //************************  Прочитать цвет  *******************************
        public Color ReadColor()
        {
            int r = ReadInt();
            int g = ReadInt();
            int b = ReadInt();
            Color res = Color.FromArgb(r, g, b);
            return res;
        }
    }
}
