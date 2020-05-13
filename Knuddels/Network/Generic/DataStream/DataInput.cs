using System;
using System.Text;
using  Knuddels.Network.Generic.IStream;

namespace  Knuddels.Network.Generic.DataStream
{
    internal class DataInput : IStreamInput
    {
        #region " FELDER "

        /// <summary>
        /// Die empfangene Zeichenkette.
        /// </summary>
        private readonly string _String;

        /// <summary>
        /// Gibt die aktuelle Position in der empfangenen Zeichenkette zurück.
        /// </summary>
        private int _Position;

        #endregion

        #region " KONSTRUKTOR "

        public DataInput(string pString) : this(pString, 0)
        {
            
        }

        public DataInput(string pString, int pPosition)
        {
            _String = pString.Substring(pPosition);
            _Position = 0;
        }

        #endregion

        #region " READ "

        #region " READ | BYTE "

        // Applet: a()
        public int Read()
        {
            return _Position != _String.Length ? _String[_Position++] & 0xff : -1;
        }

        public byte ReadByte()
        {
            return (byte)_String[_Position++];
        }

        #endregion

        #region " READUNSIGNED | BYTE | SHORT "

        public int ReadUnsignedByte()
        {
            int ch = ReadByte();
            if (ch < 0)
                throw new Exception("EOFException");
            return ch;
        }

        public int ReadUnsignedShort()
        {
            return ReadShort() & 0xffff;
        }

        #endregion

        #region " READBOOLEAN "

        public bool ReadBoolean()
        {
            return ReadByte() == 1;
        }

        #endregion

        #region " READCHAR "

        public char ReadChar()
        {
            return _String[_Position++];
        }

        #endregion

        #region " READ | DOUBLE | FLOAT "

        public double ReadDouble()
        {
            return Convert.ToDouble(ReadLong());
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt()), 0);
        }

        #endregion

        #region " READFULLY "

        public void ReadFully(byte[] pByteArray)
        {
            ReadFully(pByteArray, 0, pByteArray.Length);
        }

        public void ReadFully(byte[] pByteArray, int pStartPosition, int pLength)
        {
            for (int i = 0; i < pLength; i++)
            {
                pByteArray[i] = ReadByte();
            }
        }

        #endregion

        #region " READINT "

        public int ReadInt()
        {
            int i = ReadByte() & 0xff;
            int j = ReadByte() & 0xff;
            int k = ReadByte() & 0xff;
            int l = ReadByte() & 0xff;
            return (i << 24) + (j << 16) + (k << 8) + l;
        }

        #endregion

        #region " READLONG "

        public long ReadLong()
        {
            return ((long)ReadByte() << 56)
                + ((long)(ReadByte() & 0xff) << 48)
                + ((long)(ReadByte() & 0xff) << 40)
                + ((long)(ReadByte() & 0xff) << 32)
                + ((long)(ReadByte() & 0xff) << 24)
                + ((ReadByte() & 0xff) << 16)
                + ((ReadByte() & 0xff) << 8)
                + (ReadByte() & 0xff);
        }

        #endregion

        #region " READSHORT "

        public short ReadShort()
        {
            int i = Read();
            int j = Read();
            return (short)((i << 8) + j);
        }

        #endregion

        #region " READUTF "

        public string ReadUTF()
        {
            /* Länge der Zeichenkette */
            int utflen = ReadUnsignedShort();
            // ------------------------------
            var bytearr = new byte[utflen];
            var chararr = new char[utflen];
            // ------------------------------
            int c;
            int count = 0;
            int chararrCount = 0;
            // ------------------------------
            ReadFully(bytearr, 0, utflen);
            // ------------------------------
            while (count < utflen)
            {
                c = bytearr[count] & 0xff;
                if (c > 127)
                    break;
                count++;
                chararr[chararrCount++] = (char)c;
            }
            // ------------------------------
            #region " WHILE "
            while (count < utflen)
            {
                c = bytearr[count] & 0xff;
                int char2;
                switch (c >> 4)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        count++;
                        chararr[chararrCount++] = (char)c;
                        break;
                    case 12:
                    case 13:
                        count += 2;
                        if (count > utflen)
                            throw new Exception("[Error] malformed input: partial character at end");
                        char2 = bytearr[count - 1];
                        if ((char2 & 0xC0) != 0x80)
                            throw new Exception("[Error]  malformed input around byte " + count);
                        chararr[chararrCount++] = (char)(((c & 0x1F) << 6) | (char2 & 0x3F));
                        break;
                    case 14:
                        count += 3;
                        if (count > utflen)
                            throw new Exception("[Error] malformed input: partial character at end");
                        char2 = bytearr[count - 2];
                        int char3 = bytearr[count - 1];

                        if (((char2 & 0xC0) != 0x80) || ((char3 & 0xC0) != 0x80))
                            throw new Exception("[Error] malformed input around byte " + (count - 1));
                        chararr[chararrCount++] = (char)(((c & 0x0F) << 12) |
                                                          ((char2 & 0x3F) << 6) |
                                                          ((char3 & 0x3F) << 0));
                        break;
                    default:
                        throw new Exception("[Error] malformed input around byte #" + count);
                }
            }
            #endregion
            // ------------------------------
            return new string(chararr, 0, utflen);
        }

        #endregion

        #region " READLINE // NotImplemented "

        public string ReadLine()
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region " ANDERES "

        /// <summary>
        /// Überspringt die angegebene Anzahl an Zeichen in der empfangenen Zeichenkette.
        /// </summary>
        /// <param name="pLength"></param>
        /// <returns></returns>
        public int SkipBytes(int pLength)
        {
            return -(_Position - Math.Min(_String.Length, _Position += pLength));
        }

        #endregion
    }
}