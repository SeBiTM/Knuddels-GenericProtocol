using System;
using System.IO;
using  Knuddels.Network.Generic.IStream;
using System.Text;
using  Network.Opcodes;

namespace  Knuddels.Network.Generic.DataStream
{
    internal class DataInputStream : MemoryStream, IStreamInput
    {
        #region " KONSTRUKTOR "

        public DataInputStream(byte[] pByteData) : base(pByteData)
        {
            
        }

        #endregion

        #region " READ "

        #region " READ | BYTE "

        public int Read()
        {
            return base.ReadByte();
        }

        public new byte ReadByte()
        {
            return (byte) base.ReadByte();
        }

        #endregion

        #region " READUNSIGNED "

        #region " READUNSIGNEDBYTE "

        public int ReadUnsignedByte()
        {
            int ch = ReadByte();
            if (ch < 0)
                throw new Exception("EOFException");
            return ch;
        }

        #endregion

        #region " READUNSIGNEDSHORT "

        public int ReadUnsignedShort()
        {
            return ReadShort() & 0xffff;
        }

        #endregion

        #endregion

        #region " READBOOLEAN "

        public bool ReadBoolean()
        {
            int ch = ReadByte();
            if (ch < 0)
                throw new Exception("EOFException");
            return (ch != 0);
        }

        #endregion

        #region " READCHAR "

        public char ReadChar()
        {
            int ch1 = ReadByte();
            int ch2 = ReadByte();
            if ((ch1 | ch2) < 0)
                throw new Exception("EOFException");
            return (char) ((ch1 << 8) + (ch2 << 0));
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

        public void ReadFully(byte[] pByteArray, int pStartPosition, int pLength)
        {
            int n = 0;
            while (n < pLength)
            {
                int count = Read(pByteArray, pStartPosition + n, pLength - n);
                if (count < 0)
                    throw new Exception("EOFException");
                n += count;
            }
        }

        public void ReadFully(byte[] pByteArray)
        {
            ReadFully(pByteArray, 0, pByteArray.Length);
        }

        #endregion

        #region " READINT "

        public int ReadInt()
        {
            int ch1 = ReadByte();
            int ch2 = ReadByte();
            int ch3 = ReadByte();
            int ch4 = ReadByte();
            if ((ch1 | ch2 | ch3 | ch4) < 0)
                throw new Exception("EOFException");
            return ((ch1 << 24) + (ch2 << 16) + (ch3 << 8) + (ch4 << 0));
        }

        #endregion

        #region " READLONG "

        public long ReadLong()
        {
            return (((long) ReadByte() << 56) + ((long) (ReadByte() & 255) << 48) + ((long) (ReadByte() & 255) << 40) +
                    ((long) (ReadByte() & 255) << 32) + ((long) (ReadByte() & 255) << 24) + ((ReadByte() & 255) << 16) +
                    ((ReadByte() & 255) << 8) + ((ReadByte() & 255) << 0));
        }

        #endregion

        #region " READSHORT "

        public short ReadShort()
        {
            int ch1 = Read();
            int ch2 = Read();
            if ((ch1 | ch2) < 0)
                throw new Exception("EOFException");
            return (short) ((ch1 << 8) + ch2);
        }

        #endregion

        #region " READUTF "

        public String ReadUTF()
        {
            /* Länge der Zeichenkette */
            int utfLength = ReadUnsignedShort();
            // ------------------------------
            var byteArray = new byte[utfLength];
            var charArray = new char[utfLength];
            // ------------------------------
            int c;
            int count = 0;
            int chararrCount = 0;
            // ------------------------------
            ReadFully(byteArray, 0, utfLength);
            // ------------------------------
            while (count < utfLength)
            {
                c = byteArray[count] & 0xff;
                if (c > 127)
                    break;
                count++;
                charArray[chararrCount++] = (char) c;
            }
            // ------------------------------
            #region " WHILE "
            while (count < utfLength)
            {
                c = byteArray[count] & 0xff;
                switch (c >> 4)
                {
                    case 0:case 1:case 2:case 3:case 4:case 5:case 6:
                    case 7:
                        count++;
                        charArray[chararrCount++] = (char) c;
                        break;
                    case 12:
                    case 13:
                        count += 2;
                        if (count > utfLength)
                            throw new Exception("[Error] malformed input: partial character at end");
                        int char2 = byteArray[count - 1];
                        if ((char2 & 0xC0) != 0x80)
                            throw new Exception("[Error]  malformed input around byte " + count);
                        charArray[chararrCount++] = (char) (((c & 0x1F) << 6) | (char2 & 0x3F));
                        break;
                    case 14:
                        count += 3;
                        if (count > utfLength)
                            throw new Exception("[Error] malformed input: partial character at end");
                        char2 = byteArray[count - 2];
                        int char3 = byteArray[count - 1];
                        if (((char2 & 0xC0) != 0x80) || ((char3 & 0xC0) != 0x80))
                            throw new Exception("[Error] malformed input around byte " + (count - 1));
                        charArray[chararrCount++] = (char) (((c & 0x0F) << 12) |
                                                          ((char2 & 0x3F) << 6) |
                                                          ((char3 & 0x3F) << 0));
                        break;
                    default:
                        throw new Exception("[Error] malformed input around byte " + count);
                }
            }
            #endregion
            // ------------------------------
            return new String(charArray);
        }

        #endregion

        #endregion
    }
}